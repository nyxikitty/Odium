using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class VRChatApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    public VRChatApiClient()
    {
        var handler = new HttpClientHandler()
        {
            UseCookies = false // Manual cookie handling for better control
        };

        _httpClient = new HttpClient(handler);

        // Set default headers
        _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,ja;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Priority", "u=1, i");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Arch", "\"x86\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Bitness", "\"64\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Full-Version", "\"138.0.3351.95\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Full-Version-List", "\"Not)A;Brand\";v=\"8.0.0.0\", \"Chromium\";v=\"138.0.7204.158\", \"Microsoft Edge\";v=\"138.0.3351.95\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Mobile", "?0");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Model", "\"\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Platform", "\"Windows\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-CH-UA-Platform-Version", "\"10.0.0\"");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36 Edg/138.0.0.0");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
    }

    public async Task<string> GetUserDataAsync(string userId)
    {
        return await GetUserDataAsync(userId, CancellationToken.None);
    }

    public async Task<string> GetUserDataAsync(string userId, CancellationToken cancellationToken)
    {
        var url = string.Format("https://vrchat.com/api/1/users/{0}", userId);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        try
        {
            request.Headers.Add("If-None-Match", "W/\"W0Yp21IbfuZZ4epN3IuP_Ivx3lg\"");
            request.Headers.Add("Referer", string.Format("https://vrchat.com/home/user/{0}", userId));
            request.Headers.Add("Cookie", "_gcl_au=1.1.338008043.1752998232; _ga=GA1.1.300768148.1752998233; _gsid=c1f98b2307bd4df0802f8ce245cd2d5f; _ga_NNJWHCM5C3=GS2.1.s1753140772$o7$g0$t1753140833$j60$l0$h0; cf_clearance=GUU2lfeuCXSnIU6RjtcwfB16vEv6skN6_..R0Fvr_Jc-1753144275-1.2.1.1-iOxTVITnn4E87edXDmB.PSdAPMj_CW0cFZJ6YOpfWUPho.sm_BFOBg6535b2B9qawZHOmgbgYZQL7pVMfHmNq2L2sWaHUCDIJtIsBdnt3nAWrVsHCfb6eE9AieJPTl0h.0bHUJ5CU58asydAjMtSs.RgtaCl9Avr0YUwJSUa_IgemEi2x6xSjwwY_x05wvAHIOf4_k0Xrb3Bqdu4CzsCCwSE5N791MAaRC9NESBWEZ0; auth=authcookie_42caeaaa-c1c9-456d-bb13-06cfa63ce4de");

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            finally
            {
                response.Dispose();
            }
        }
        finally
        {
            request.Dispose();
        }
    }

    public async Task<string> GetUserDataWithRetryAsync(string userId, int timeoutMs = 10000, int maxRetries = 3)
    {
        return await GetUserDataWithRetryAsync(userId, timeoutMs, maxRetries, CancellationToken.None);
    }

    public async Task<string> GetUserDataWithRetryAsync(string userId, int timeoutMs, int maxRetries, CancellationToken cancellationToken)
    {
        var timeoutCts = new CancellationTokenSource(timeoutMs);
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await GetUserDataAsync(userId, combinedCts.Token).ConfigureAwait(false);
                }
                catch (Exception ex) when (attempt < maxRetries && (ex is HttpRequestException || ex is TaskCanceledException))
                {
                    // Exponential backoff: wait 1s, then 2s, then 4s
                    var delay = 1000 * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }

            // Final attempt without catching exceptions
            return await GetUserDataAsync(userId, combinedCts.Token).ConfigureAwait(false);
        }
        finally
        {
            timeoutCts.Dispose();
            combinedCts.Dispose();
        }
    }

    // Get just the date_joined field
    public async Task<DateTime?> GetUserDateJoinedAsync(string userId)
    {
        var jsonResponse = await GetUserDataAsync(userId);
        var userData = JObject.Parse(jsonResponse);

        var dateJoinedStr = userData["date_joined"]?.ToString();
        if (string.IsNullOrEmpty(dateJoinedStr))
            return null;

        if (DateTime.TryParse(dateJoinedStr, out DateTime dateJoined))
            return dateJoined;

        return null;
    }

    // Get specific field using JObject (flexible approach)
    public async Task<T> GetUserFieldAsync<T>(string userId, string fieldName)
    {
        var jsonResponse = await GetUserDataAsync(userId);
        var userData = JObject.Parse(jsonResponse);

        var field = userData[fieldName];
        if (field == null)
            return default(T);

        return field.ToObject<T>();
    }

    // Synchronous wrapper for scenarios where async isn't available
    public string GetUserData(string userId, int timeoutMs = 10000)
    {
        try
        {
            return GetUserDataAsync(userId).GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            // Unwrap AggregateException for cleaner error handling
            throw ex.InnerException ?? ex;
        }
    }

    // Synchronous date_joined getter
    public DateTime? GetUserDateJoined(string userId)
    {
        try
        {
            return GetUserDateJoinedAsync(userId).GetAwaiter().GetResult();
        }
        catch (AggregateException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
            _disposed = true;
        }
    }
}