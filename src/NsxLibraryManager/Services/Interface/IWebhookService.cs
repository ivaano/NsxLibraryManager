using Common.Services;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Services.Interface;

public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook of the specified type with the given payload
    /// </summary>
    /// <param name="webhookType">The type of webhook to send</param>
    /// <param name="payload">The payload to include in the request</param>
    /// <returns>The webhook request ID if successfully queued</returns>
    Task<Result<string>> SendWebhook(WebhookType webhookType, object payload);

    /// <summary>
    /// Removes a queued webhook request
    /// </summary>
    /// <param name="webhookId">The ID of the webhook request to remove</param>
    /// <returns>True if the webhook was found and removed, false otherwise</returns>
    Task<Result<bool>> RemoveQueuedWebhook(string webhookId);

    /// <summary>
    /// Gets the list of currently queued webhook requests
    /// </summary>
    /// <returns>List of webhook requests in the queue</returns>
    Task<Result<List<WebhookRequest>>> GetQueuedWebhooks();

    /// <summary>
    /// Gets the status of a webhook request
    /// </summary>
    /// <param name="webhookId">The ID of the webhook request</param>
    /// <returns>Status of the webhook request</returns>
    Task<Result<WebhookStatus>> GetWebhookStatus(string webhookId);

    /// <summary>
    /// Gets the webhook settings from user configuration
    /// </summary>
    /// <returns>List of webhook settings</returns>
    Task<Result<List<WebhookSetting>>> GetWebhookSettings();

}