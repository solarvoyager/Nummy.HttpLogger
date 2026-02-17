# Nummy.HttpLogger - Improvement Plan

## Package Summary
HTTP request/response logging middleware for ASP.NET Core. Intercepts HTTP traffic, captures request/response bodies and headers, and sends them to a remote Nummy service via HTTP POST.

---

## Critical Issues

### 1. HttpClient created from Singleton holds stale connections
- **File:** `Data/Services/NummyHttpLoggerService.cs:9`
- **Problem:** `NummyHttpLoggerService` is registered as **Singleton** but creates an `HttpClient` once in the constructor via `clientFactory.CreateClient()`. The `IHttpClientFactory` is designed to be called per-use so that handler rotation and DNS changes are respected. Storing the client in a field for the lifetime of the app defeats the purpose of `IHttpClientFactory` and can lead to stale DNS entries and socket exhaustion over long-running deployments.
- **Fix:** Store `IHttpClientFactory` and call `CreateClient()` per request, or switch to a typed client with proper lifetime management. Alternatively, keep the singleton but create the client per call:
  ```csharp
  internal class NummyHttpLoggerService(IHttpClientFactory clientFactory) : INummyHttpLoggerService
  {
      public async Task LogRequestAsync(NummyRequestLog requestLog)
      {
          using var client = clientFactory.CreateClient(NummyConstants.ClientName);
          await client.PostAsJsonAsync(NummyConstants.RequestLogAddUrl, requestLog);
      }
  }
  ```

### 2. Response body stream replacement is unsafe for certain responses
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:46-47`
- **Problem:** The middleware unconditionally replaces `context.Response.Body` with a `MemoryStream` buffer even when response logging is disabled (`EnableResponseLogging = false`). This means **every request** pays the cost of buffering the entire response body into memory, even when not needed. For large responses (file downloads, streaming), this can cause **OutOfMemoryException** and defeats streaming semantics (the entire response must complete before any bytes reach the client).
- **Fix:** Only replace the response body when `EnableResponseLogging` is true:
  ```csharp
  if (options.Value.EnableResponseLogging)
  {
      var originalResponseBody = context.Response.Body;
      await using var bufferStream = new MemoryStream();
      context.Response.Body = bufferStream;
      try
      {
          await next(context);
          // ... log response ...
      }
      finally
      {
          context.Response.Body = originalResponseBody;
      }
  }
  else
  {
      await next(context);
  }
  ```

### 3. No exception handling on HTTP calls to Nummy service
- **File:** `Data/Services/NummyHttpLoggerService.cs:13,18`
- **Problem:** `PostAsJsonAsync` calls can throw `HttpRequestException`, `TaskCanceledException` (timeout), `OperationCanceledException`, etc. While the middleware does `try/catch` around some paths, `InsertLogAsync` itself has no protection. If the Nummy service is down or slow, the 30-second timeout will block the request pipeline for that duration before the exception propagates.
- **Fix:** Add fire-and-forget semantics or a short timeout specifically for logging calls. Consider using `Task.Run` with a background queue to decouple logging from request processing entirely.

### 4. Full response body is read into a string regardless of size
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:149,222`
- **Problem:** `reader.ReadToEndAsync()` reads the entire body into a `string` before truncating. For a 500MB response, this allocates 500MB+ of string memory even though only `MaxBodyLength` (default 32KB) will be logged. This is a **memory bomb** for large responses.
- **Fix:** Read only up to `MaxBodyLength` bytes instead of the full body:
  ```csharp
  var buffer = new char[max + 1];
  var charsRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
  var toLog = charsRead > max
      ? new string(buffer, 0, max) + "...(truncated)"
      : new string(buffer, 0, charsRead);
  ```

---

## Performance Issues

### 5. Synchronous LINQ `.Any()` on every request for path exclusion
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:20-22`
- **Problem:** `ExcludeContainingPaths` is a `HashSet<string>` but iterated with `.Any()` and `Contains` substring matching, so the `HashSet` provides no performance benefit. For large exclusion lists this is O(n) per request.
- **Fix:** This is minor but could use a simple list since `HashSet` semantics aren't leveraged. Consider documenting that this is a substring match, not exact match.

### 6. Request body `ReadToEndAsync` reads entire body before truncation
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:149`
- **Problem:** Same pattern as issue #4 but for request bodies. Reads entire request body into memory before truncating to `MaxBodyLength`.
- **Fix:** Read only `MaxBodyLength + 1` characters to detect truncation.

### 7. Header masking uses `.Contains()` on HashSet — case sensitivity mismatch
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:111`
- **Problem:** `options.MaskHeaders.Contains(key)` performs a case-sensitive lookup by default. HTTP headers are case-insensitive. If a user configures `"authorization"` but the header arrives as `"Authorization"`, it won't be masked — **leaking sensitive data**.
- **Fix:** Initialize `MaskHeaders` with `StringComparer.OrdinalIgnoreCase`:
  ```csharp
  public HashSet<string> MaskHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);
  ```

---

## Thread Safety Issues

### 8. Singleton service holding a single HttpClient instance
- **File:** `Data/Services/NummyHttpLoggerService.cs:9`
- **Problem:** While `HttpClient` is thread-safe for concurrent `SendAsync` calls, the pattern of caching it from the factory in a singleton means DNS changes are never picked up (see #1). This is a combined thread-safety/reliability concern.

---

## Reliability Issues

### 9. Silent exception swallowing with no diagnostics
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:39-42,69-72,173-176,244-246`
- **Problem:** Multiple `catch` blocks silently swallow exceptions with empty catch bodies. There is no logging, no diagnostics event, no counter — failures are completely invisible. If the Nummy service goes down, the operator has no way to know that logging has silently stopped.
- **Fix:** Inject `ILogger<NummyHttpLoggerMiddleware>` and log at `Debug` or `Warning` level. This lets operators enable diagnostics when needed without impacting normal operation.

### 10. `IsBinaryContentType` logic has false positives
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:100`
- **Problem:** `contentType.Contains("application")` will match `application/json` and `application/xml`, but these are explicitly handled earlier. However, `application/x-www-form-urlencoded` (form posts) will be treated as binary, so form bodies won't be logged. The `pdf/` prefix check (line 98) is wrong — PDF content type is `application/pdf`, not `pdf/...`.
- **Fix:** Restructure the check to whitelist text-like types rather than blacklist:
  ```csharp
  private static bool IsBinaryContentType(string? contentType)
  {
      if (string.IsNullOrEmpty(contentType)) return false;
      contentType = contentType.ToLowerInvariant();
      // Whitelist known text types
      if (contentType.StartsWith("text/")) return false;
      if (contentType.Contains("json") || contentType.Contains("xml") ||
          contentType.Contains("html") || contentType.Contains("form-urlencoded")) return false;
      return true; // everything else is binary
  }
  ```

### 11. Missing `CancellationToken` propagation
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs` (throughout)
- **Problem:** None of the HTTP calls to the Nummy service pass `CancellationToken`. If a client disconnects mid-request, the middleware still waits for the logging HTTP call to complete (up to 30s timeout). This wastes server resources.
- **Fix:** Pass `context.RequestAborted` to logging service calls and propagate through to `PostAsJsonAsync`.

### 12. `EnableBuffering()` called after potential binary check skip
- **File:** `Middleware/NummyHttpLoggerMiddleware.cs:143`
- **Problem:** `EnableBuffering()` is only called when the content is not binary. If a non-binary request follows a binary one in pipelined connections, this is fine. But if downstream middleware relies on `EnableBuffering()` being called, it may not be available for binary-content requests. This is a minor edge case.

---

## Code Quality

### 13. Typo in folder name `Entitites`
- **File:** `Data/Entitites/` directory
- **Problem:** "Entitites" should be "Entities". This is in the namespace so changing it is a breaking change for the next major version.

### 14. Options properties lack `required` keyword or null warnings
- **File:** `Utils/NummyHttpLoggerOptions.cs:9-10`
- **Problem:** `ApplicationId` and `NummyServiceUrl` are non-nullable `string` without `required` keyword. The compiler will warn about uninitialized non-nullable properties. While validation catches this at runtime, the API surface should communicate the requirement.
- **Fix:** Add `required` keyword or initialize with `string.Empty` and document that validation will reject empty values.
