namespace NsxLibraryManager.Shared.Enums;

public enum WebhookStatusType
{
    /// <summary>
    /// The webhook is queued and waiting to be processed
    /// </summary>
    Queued,
    
    /// <summary>
    /// The webhook is currently being processed
    /// </summary>
    InProgress,
    
    /// <summary>
    /// The webhook has been successfully processed
    /// </summary>
    Completed,
    
    /// <summary>
    /// The webhook processing failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// The webhook request could not be found
    /// </summary>
    NotFound
}