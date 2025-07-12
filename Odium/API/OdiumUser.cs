using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Odium.Odium;

public class OdiumUserService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public OdiumUserService(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<int> GetUserCountAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/odium/users/list");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<object>>(jsonContent);

                var userCount = users?.Count ?? 0;

                AssignedVariables.odiumUsersCount = userCount;

                return userCount;
            }
            else
            {
                Console.WriteLine($"Error: HTTP {response.StatusCode} - {response.ReasonPhrase}");
                AssignedVariables.odiumUsersCount = 0;
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred while fetching user count: {ex.Message}");
            AssignedVariables.odiumUsersCount = 0;
            return 0;
        }
    }
}