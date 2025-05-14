using System.Collections.Concurrent;
using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Services;

public class WebhookStateService
{
    private readonly ILogger<WebhookStateService> _logger;
    private readonly ConcurrentDictionary<string, WebhookStatusUpdate> _currentWebhooks = new();
    private readonly ConcurrentQueue<WebhookCompletedStatus> _completedWebhooks = new();
    private const int MaxCompletedWebhooksToKeep = 100;

    public WebhookStateService(ILogger<WebhookStateService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<WebhookStatusUpdate> CurrentWebhooks => _currentWebhooks.Values;
    
    public IEnumerable<WebhookCompletedStatus> CompletedWebhooks => _completedWebhooks;

    public void UpdateStatus(WebhookStatusUpdate update)
    {
        _currentWebhooks[update.WebhookId] = update;
        _logger.LogDebug("Updated webhook status: {webhookId}, {webhookType}", 
            update.WebhookId, update.WebhookType);
    }

    public void CompleteWebhook(WebhookCompletedStatus status)
    {
        _currentWebhooks.TryRemove(status.WebhookId, out _);
        _completedWebhooks.Enqueue(status);
        
        // Ensure we don't keep too many completed webhooks in memory
        while (_completedWebhooks.Count > MaxCompletedWebhooksToKeep)
        {
            _completedWebhooks.TryDequeue(out _);
        }
        
        _logger.LogInformation("Completed webhook: {webhookId}, {webhookType}, Success: {success}", 
            status.WebhookId, status.WebhookType, status.Success);
    }

    public void ClearCompletedWebhooks()
    {
        while (_completedWebhooks.TryDequeue(out _)) { }
        _logger.LogInformation("Cleared completed webhooks history");
    }
}