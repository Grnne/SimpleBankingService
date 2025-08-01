using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Simple_Account_Service.Application.ForFakesAndDummies;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> GetToken()
    {
        var client = httpClientFactory.CreateClient();

        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "my_client"),
            new KeyValuePair<string, string>("username", "test"),
            new KeyValuePair<string, string>("password", "password")
        };

        var response = await client.PostAsync("http://keycloak:8080/realms/my_realm/protocol/openid-connect/token", new FormUrlEncodedContent(data));

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();

        return Content("Bearer " + accessToken!);
    }
}