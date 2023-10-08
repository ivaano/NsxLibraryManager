using System.Net;

namespace Nsxrenamer.Exceptions;

public class SimpleHttpResponseException : Exception
{
    public HttpStatusCode StatusCode { get; private set; }

    public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
    {
        StatusCode = statusCode;
    }
}