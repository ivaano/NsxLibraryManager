namespace NsxLibraryManager.Models;

public class ErrorResponse
{
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public IEnumerable<string> Errors { get; set; }
    public DateTime Timestamp { get; set; }

    public static ErrorResponse FromError(string message, IEnumerable<string> errors, string code = "UNKNOWN_ERROR")
    {
        return new ErrorResponse
        {
            ErrorMessage = message,
            ErrorCode = code,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }
}