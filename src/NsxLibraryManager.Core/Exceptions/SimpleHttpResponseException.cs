using System.Net;

namespace NsxLibraryManager.Core.Exceptions;

public class SimpleHttpResponseException : Exception
{
    public HttpStatusCode StatusCode { get; private set; }

    public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
    {
        StatusCode = statusCode;
    }
}