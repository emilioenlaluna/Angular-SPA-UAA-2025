namespace API.UnitTests.Tests;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.UnitTests.Helpers;
using Newtonsoft.Json.Linq;
public class RegisterRequestTests
{
    private readonly string apiRoute = "api/users";
    private readonly HttpClient _client;
    private HttpResponseMessage httpResponse;
    private string requestUrl;
    private string loginObject;
    private HttpContent httpContent;
    public RegisterRequestTests()
    {
        _client = TestHelper.Instance.Client;
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnOk()
    {
        // Arrange
        var expectedStatusCode = "OK";
        requestUrl = "api/account/login";
        var loginRequest = new LoginRequest
        {
            Username = "arenita",
            Password = "123456"
        };
        loginObject = GetLoginObject(loginRequest);
        httpContent = GetHttpContent(loginObject);
        httpResponse = await _client.PostAsync(requestUrl, httpContent);
        var response = await httpResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userResponse.Token);
        requestUrl = $"{apiRoute}";
        // Act
        httpResponse = await _client.GetAsync(requestUrl);
        // Assert
        Assert.Equal(Enum.Parse<HttpStatusCode>(expectedStatusCode, true), httpResponse.StatusCode);
        Assert.Equal(expectedStatusCode, httpResponse.StatusCode.ToString());
    }
    [Theory]
    [InlineData("arenita")]
    public async Task GetByUsernameAsync_ShouldReturnOk(string username)
    {
        // Arrange
        var expectedStatusCode = "OK";
        requestUrl = "api/account/login";
        var loginRequest = new LoginRequest
        {
            Username = "arenita",
            Password = "123456"
        };
        loginObject = GetLoginObject(loginRequest);
        httpContent = GetHttpContent(loginObject);
        httpResponse = await _client.PostAsync(requestUrl, httpContent);
        var response = await httpResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userResponse.Token);
        requestUrl = $"{apiRoute}/{username}";
        // Act
        httpResponse = await _client.GetAsync(requestUrl);
        // Assert
        Assert.Equal(Enum.Parse<HttpStatusCode>(expectedStatusCode, true), httpResponse.StatusCode);
        Assert.Equal(expectedStatusCode, httpResponse.StatusCode.ToString());
    }
        [Theory]
    [InlineData("notExisting")]
    public async Task GetByUsernameAsync_ShouldReturnNotFound(string username)
    {
        // Arrange
        var expectedStatusCode = "NotFound";
        requestUrl = "api/account/login";
        var loginRequest = new LoginRequest
        {
            Username = "arenita",
            Password = "123456"
        };
        loginObject = GetLoginObject(loginRequest);
        httpContent = GetHttpContent(loginObject);
        httpResponse = await _client.PostAsync(requestUrl, httpContent);
        var response = await httpResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userResponse.Token);
        requestUrl = $"{apiRoute}/{username}";
        // Act
        httpResponse = await _client.GetAsync(requestUrl);
        // Assert
        Assert.Equal(Enum.Parse<HttpStatusCode>(expectedStatusCode, true), httpResponse.StatusCode);
        Assert.Equal(expectedStatusCode, httpResponse.StatusCode.ToString());
    }
      
    [Fact]
    public async Task UpdateUser_ShouldReturnNoContent_WhenValidRequest()
    {
        // Arrange: Autenticar al usuario
        requestUrl = "api/account/login";
        var loginRequest = new LoginRequest
        {
            Username = "arenita",
            Password = "123456"
        };
        loginObject = GetLoginObject(loginRequest);
        httpContent = GetHttpContent(loginObject);
        httpResponse = await _client.PostAsync(requestUrl, httpContent);
        httpResponse.EnsureSuccessStatusCode(); // Verificamos que el login fue exitoso
        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userResponse.Token);
        // Preparar los datos de actualización
        var memberUpdateRequest = new MemberUpdateRequest
        {
            Introduction = "New introduction",
            LookingFor = "Looking for new opportunities",
            Interests = "Reading, Traveling",
            City = "Mexico City",
            Country = "Mexico"
        };
        requestUrl = $"{apiRoute}";
        httpContent = GetHttpContent(JsonSerializer.Serialize(memberUpdateRequest));
        // Act: Enviar la solicitud PUT
        httpResponse = await _client.PutAsync(requestUrl, httpContent);
        // Assert: Verificar que la respuesta sea exitosa
        Assert.Equal(HttpStatusCode.NoContent, httpResponse.StatusCode);
    }
    
[Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserNotAuthenticated()
    {
        // Arrange: Preparar datos de actualización sin autenticar al cliente
        var memberUpdateRequest = new MemberUpdateRequest
        {
            Introduction = "Introduction without authentication",
            LookingFor = "Looking for nothing",
            Interests = "None",
            City = "Nowhere",
            Country = "NoCountry"
        };
        requestUrl = $"{apiRoute}";
        httpContent = GetHttpContent(JsonSerializer.Serialize(memberUpdateRequest));
        // Act: Enviar la solicitud PUT sin autenticación
        httpResponse = await _client.PutAsync(requestUrl, httpContent);
        // Assert: Verificar que la respuesta sea BadRequest (401 Unauthorized)
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }
    #region Private methods
    private static string GetLoginObject(LoginRequest loginDto)
    {
        var entityObject = new JObject()
        {
            { nameof(loginDto.Username), loginDto.Username },
            { nameof(loginDto.Password), loginDto.Password }
        };
        return entityObject.ToString();
    }
    private static StringContent GetHttpContent(string objectToCode) =>
        new(objectToCode, Encoding.UTF8, "application/json");
    #endregion
}