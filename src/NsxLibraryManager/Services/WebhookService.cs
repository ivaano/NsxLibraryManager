using Common.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Services;

public class WebhookService : IWebhookService
{
    private readonly WebhookBackgroundService _backgroundService;
    private readonly ILogger<WebhookService> _logger;
    private readonly ISettingsService _settingsService;

    public WebhookService(
        ISettingsService settingsService, 
        IServiceProvider serviceProvider,
        ILogger<WebhookService> logger)
    {
        _settingsService = settingsService;
        _backgroundService = serviceProvider.GetServices<IHostedService>().OfType<WebhookBackgroundService>().First();
        _logger = logger;
    }

    public Task<Result<string>> SendWebhook(WebhookType webhookType, object payload)
    {
        var userSettings = _settingsService.GetUserSettings();
        var webhookUrl = string.Empty;
        
        switch (webhookType)
        {
            case WebhookType.LibraryReload:
                if (!userSettings.LibraryReloadPostWebhook)
                {
                    _logger.LogInformation("Webhook {webhookType} is disabled in settings", webhookType);
                    return Task.FromResult(Result.Failure<string>($"Webhook {webhookType} is disabled in settings"));
                }

                webhookUrl = userSettings.LibraryReloadWebhookUrl;
                break;
            default:
                _logger.LogInformation("Unknown webhook type  {webhookType}", webhookType);
                return Task.FromResult(Result.Failure<string>($"Unknown webhook type {webhookType}"));
        }

        try
        {
            var requestId = _backgroundService.QueueWebhookRequest(
                webhookUrl, 
                payload, 
                webhookType);
                
            return Task.FromResult(Result.Success(requestId));
        }        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing webhook request");
            return Task.FromResult(Result.Failure<string>(ex.Message));
        }
    }

    public Task<Result<bool>> RemoveQueuedWebhook(string webhookId)
    {
        var result = _backgroundService.RemoveQueuedWebhookRequest(webhookId);
        return Task.FromResult(Result.Success(result));
    }

    public Task<Result<List<WebhookRequest>>> GetQueuedWebhooks()
    {
        var queuedWebhooks = _backgroundService.GetWebhookQueue();
        return Task.FromResult(Result.Success(queuedWebhooks));
    }
    
    public Task<Result<WebhookStatus>> GetWebhookStatus(string webhookId)
    {
        var status = _backgroundService.GetWebhookStatus(webhookId);
        return Task.FromResult(Result.Success(status));
    }
    
    public Task<Result<List<WebhookSetting>>> GetWebhookSettings()
    {
        //return Task.FromResult(Result.Success(_userSettings.WebhookSettings ?? new List<WebhookSetting>()));
        var taco = new WebhookSetting();
        return Task.FromResult(Result.Success(new List<WebhookSetting>()));
    }
    
}