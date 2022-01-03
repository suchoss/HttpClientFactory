using System.Net;

namespace HttpClientFactory;

public record TurboResult<T>(T Result, bool IsSuccessful, HttpStatusCode StatusCode, string? ReasonPhrase);
