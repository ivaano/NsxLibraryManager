namespace NsxLibraryManager.Contracts;

public class ErrorResponse
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; }  = string.Empty;
    public IEnumerable<string> Errors { get; set; }  = [];
    public DateTime Timestamp { get; set; }

    public static ErrorResponse FromError(string message, IEnumerable<string> errors, string code = "UNKNOWN_ERROR")
    {
        return new ErrorResponse
        {
            ErrorMessage = message,
            ErrorCode = code,
            Errors = errors,
            Timestamp = DateTime.Now
        };
    }
}