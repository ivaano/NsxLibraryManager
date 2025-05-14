using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Services;

public class WebhookBackgroundService : BackgroundService
{
    private readonly ILogger<WebhookBackgroundService> _logger;

    private readonly ConcurrentQueue<WebhookRequest> _webhookQueue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly WebhookStateService _stateService;
    private readonly HttpClient _httpClient;

    public WebhookBackgroundService(
        ILogger<WebhookBackgroundService> logger,
        WebhookStateService stateService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _stateService = stateService;
        _httpClient = httpClientFactory.CreateClient("webhook");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessWebhookQueue(stoppingToken);

                await Task.WhenAny(
                    Task.Delay(5000, stoppingToken),
                    _signal.WaitAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown, don't log an error
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during webhook operation");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
    
    public string QueueWebhookRequest(string webhookUri, object payload, WebhookType webhookType)
    {
        var request = new WebhookRequest
        {
            Id = Guid.NewGuid().ToString(),
            WebhookUri = webhookUri,
            Payload = payload,
            WebhookType = Enum.GetName(typeof(WebhookType), webhookType) ?? "Unknown",
            Timestamp = DateTime.Now,
            Retries = 0
        };
        
        try
        {
            _webhookQueue.Enqueue(request);
            _signal.Release();
            _logger.LogInformation("Queued webhook request: {webhookType}, ID: {id}", webhookType, request.Id);
            return request.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing webhook request: {webhookType}", webhookType);
            throw;
        }
    }

    public bool RemoveQueuedWebhookRequest(string requestId)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            _logger.LogWarning("Attempted to remove queued webhook with null or empty ID");
            return false;
        }

        var found = false;
        var tempQueue = new ConcurrentQueue<WebhookRequest>();
    
        while (_webhookQueue.TryDequeue(out var request))
        {
            if (request.Id != requestId)
            {
                tempQueue.Enqueue(request);
            }
            else
            {
                found = true;
                _logger.LogInformation("Removed queued webhook request: {webhookType}, ID: {id}", 
                    request.WebhookType, request.Id);
            }
        }
    
        while (tempQueue.TryDequeue(out var request))
        {
            _webhookQueue.Enqueue(request);
        }
    
        if (!_webhookQueue.IsEmpty && found)
        {
            _signal.Release();
        }
    
        return found;
    }

    public List<WebhookRequest> GetWebhookQueue()
    {
        return _webhookQueue.ToList();
    }
    
    public WebhookStatus GetWebhookStatus(string id)
    {
        // Check if it's in the state service (in progress or completed)
        var activeWebhook = _stateService.CurrentWebhooks
            .FirstOrDefault(t => t.WebhookId == id);
            
        if (activeWebhook != null)
        {
            return new WebhookStatus
            {
                Id = id,
                Status = WebhookStatusType.InProgress,
                WebhookType = activeWebhook.WebhookType,
                StartTime = activeWebhook.StartTime
            };
        }
        
        var completedWebhook = _stateService.CompletedWebhooks
            .FirstOrDefault(t => t.WebhookId == id);
            
        if (completedWebhook != null)
        {
            return new WebhookStatus
            {
                Id = id,
                Status = completedWebhook.Success ? 
                    WebhookStatusType.Completed : WebhookStatusType.Failed,
                WebhookType = completedWebhook.WebhookType,
                StartTime = completedWebhook.StartTime,
                CompletionTime = completedWebhook.CompletionTime,
                ErrorMessage = completedWebhook.ErrorMessage,
                StatusCode = completedWebhook.StatusCode
            };
        }
        
        if (_webhookQueue.Any(r => r.Id == id))
        {
            return new WebhookStatus
            {
                Id = id,
                Status = WebhookStatusType.Queued,
                WebhookType = _webhookQueue.First(r => r.Id == id).WebhookType
            };
        }
        
        return new WebhookStatus
        {
            Id = id,
            Status = WebhookStatusType.NotFound
        };
    }
    
    private async Task ProcessWebhookQueue(CancellationToken stoppingToken)
    {
        while (_webhookQueue.TryDequeue(out var request) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendWebhookRequest(request, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook request: {id}", request.Id);
                
                // Check if we should retry
                if (request.Retries < 3) // Allow up to 3 retries
                {
                    request.Retries++;
                    _webhookQueue.Enqueue(request); // Put it back in the queue
                    _logger.LogInformation("Requeueing webhook request for retry: {id}, attempt {attempt}", 
                        request.Id, request.Retries);
                }
                else
                {
                    // Report failure after all retries are exhausted
                    _stateService.CompleteWebhook(new WebhookCompletedStatus
                    {
                        WebhookId = request.Id,
                        WebhookType = request.WebhookType,
                        Success = false,
                        ErrorMessage = ex.Message,
                        StartTime = request.Timestamp,
                        CompletionTime = DateTime.Now
                    });
                }
            }
        }
    }
    
    private async Task SendWebhookRequest(WebhookRequest request, CancellationToken stoppingToken)
    {
        // Start the webhook status
        _stateService.UpdateStatus(new WebhookStatusUpdate
        {
            WebhookId = request.Id,
            WebhookType = request.WebhookType,
            WebhookUri = request.WebhookUri,
            StartTime = DateTime.Now,
            LastUpdateTime = DateTime.Now
        });

        try
        {
            _logger.LogInformation("Starting webhook request: {WebhookType}, ID: {Id}", 
                request.WebhookType, request.Id);

            var content = new StringContent(
                JsonSerializer.Serialize(request.Payload), 
                Encoding.UTF8, 
                "application/json");
            
            var response = await _httpClient.PostAsync(request.WebhookUri, content, stoppingToken);
            
            var statusCode = (int)response.StatusCode;
            var success = response.IsSuccessStatusCode;
            var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);

            _stateService.CompleteWebhook(new WebhookCompletedStatus
            {
                WebhookId = request.Id,
                WebhookType = request.WebhookType,
                Success = success,
                StatusCode = statusCode,
                Response = responseBody,
                StartTime = request.Timestamp,
                CompletionTime = DateTime.Now
            });

            _logger.LogInformation("Completed webhook request: {webhookType}, result: {statusCode}",
                request.WebhookType, statusCode);
            
            if (!success)
            {
                _logger.LogWarning("Webhook request failed with status code {statusCode}: {response}",
                    statusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook request: {id}", request.Id);
            throw;
        }
    }
}