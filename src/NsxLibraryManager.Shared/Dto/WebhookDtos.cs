using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Shared.Dto;

public class WebhookRequest
{
    /// <summary>
    /// Unique identifier for the webhook request
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// URI to send the webhook to
    /// </summary>
    public string WebhookUri { get; set; } = string.Empty;

    /// <summary>
    /// Http method to use for the webhook
    /// </summary>
    public WebhookHttpMethod HttpMethod { get; set; } = WebhookHttpMethod.Post;
    
    /// <summary>
    /// Payload to send with the webhook
    /// </summary>
    public object? Payload { get; set; }
    
    /// <summary>
    /// Type of webhook (e.g., "FileAdded", "ProcessCompleted")
    /// </summary>
    public string WebhookType { get; set; } = string.Empty;
    
    /// <summary>
    /// When the webhook request was created
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int Retries { get; set; }
}

public class WebhookStatus
{
    /// <summary>
    /// Unique identifier for the webhook request
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the webhook
    /// </summary>
    public WebhookStatusType Status { get; set; }
    
    /// <summary>
    /// Type of webhook
    /// </summary>
    public string WebhookType { get; set; } = string.Empty;
    
    /// <summary>
    /// When the webhook started processing
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// When the webhook completed
    /// </summary>
    public DateTime? CompletionTime { get; set; }
    
    /// <summary>
    /// Error message if the webhook failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// HTTP status code from the response
    /// </summary>
    public int? StatusCode { get; set; }
}

public class WebhookStatusUpdate
{
    /// <summary>
    /// Unique identifier for the webhook request
    /// </summary>
    public string WebhookId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of webhook
    /// </summary>
    public string WebhookType { get; set; } = string.Empty;
    
    /// <summary>
    /// URI the webhook is being sent to
    /// </summary>
    public string WebhookUri { get; set; } = string.Empty;
    
    
    /// <summary>
    /// Http method to use for the webhook
    /// </summary>
    public WebhookHttpMethod HttpMethod { get; set; } = WebhookHttpMethod.Post;

    
    /// <summary>
    /// When the webhook started processing
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// When the status was last updated
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}

public class WebhookCompletedStatus
{
    /// <summary>
    /// Unique identifier for the webhook request
    /// </summary>
    public string WebhookId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of webhook
    /// </summary>
    public string WebhookType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the webhook completed successfully
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// HTTP status code from the response
    /// </summary>
    public int? StatusCode { get; set; }
    
    /// <summary>
    /// Response content from the webhook call
    /// </summary>
    public string? Response { get; set; }
    
    /// <summary>
    /// Error message if the webhook failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// When the webhook started processing
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// When the webhook completed
    /// </summary>
    public DateTime CompletionTime { get; set; }
}

public class WebhookSetting
{
    /// <summary>
    /// Type of webhook (e.g., "FileAdded", "ProcessCompleted")
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the webhook
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// URI to send the webhook to
    /// </summary>
    public string Uri { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the webhook is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Optional headers to include with the webhook
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
    
    /// <summary>
    /// Optional description of the webhook
    /// </summary>
    public string? Description { get; set; }
}