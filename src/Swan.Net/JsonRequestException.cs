namespace Swan.Net;

using System;
using System.Net.Http;

/// <summary>
/// Represents errors that occurs requesting a JSON file through HTTP.
/// </summary>
/// <seealso cref="System.Exception" />
[Serializable]
public class JsonRequestException
    : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRequestException" /> class.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="method">The method.</param>
    /// <param name="httpErrorCode">The HTTP error code.</param>
    /// <param name="errorContent">Content of the error.</param>
    public JsonRequestException(Uri url, HttpMethod method, int httpErrorCode = 500, string? errorContent = null)
        : base($"HTTP Request Error Url {url} Method {method} HTTP Status Code {httpErrorCode} Body Content {errorContent}")
    {
        Url = url;
        Method = method;
        HttpErrorCode = httpErrorCode;
        HttpErrorContent = errorContent;
    }

    /// <summary>
    /// Gets the URL.
    /// </summary>
    /// <value>
    /// The URL.
    /// </value>
    public Uri Url { get; }

    /// <summary>
    /// Gets the method.
    /// </summary>
    /// <value>
    /// The method.
    /// </value>
    public HttpMethod Method { get; }

    /// <summary>
    /// Gets the HTTP error code.
    /// </summary>
    /// <value>
    /// The HTTP error code.
    /// </value>
    public int HttpErrorCode { get; }

    /// <summary>
    /// Gets the content of the HTTP error.
    /// </summary>
    /// <value>
    /// The content of the HTTP error.
    /// </value>
    public string? HttpErrorContent { get; }

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrEmpty(HttpErrorContent)
        ? $"HTTP Response Status Code {HttpErrorCode} Error Message: {HttpErrorContent}"
        : base.ToString();
}
