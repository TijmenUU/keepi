using System.Text.Json;
using Bogus;
using Keepi.Api.Projects.Create;
using Keepi.Api.Projects.GetAll;
using Keepi.Api.Projects.Update;
using Keepi.Api.UserEntries.GetExport;
using Keepi.Api.UserEntries.GetWeek;
using Keepi.Api.UserEntries.UpdateWeek;
using Keepi.Api.UserInvoiceItemCustomizations.UpdateAll;
using Keepi.Api.UserProjects.GetAll;
using Keepi.Api.Users.Get;
using Keepi.Api.Users.GetAll;

namespace Keepi.Web.Integration.Tests;

public class KeepiClient
{
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public KeepiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        jsonSerializerOptions = options;
    }

    public static async Task<KeepiClient> CreateWithRandomUser(HttpClient httpClient)
    {
        var faker = new Faker(locale: "nl");
        return await CreateWithUser(
            httpClient: httpClient,
            fullName: faker.Person.FullName,
            subjectClaim: Guid.NewGuid().ToString()
        );
    }

    public static async Task<KeepiClient> CreateWithUser(
        HttpClient httpClient,
        string fullName,
        string subjectClaim
    )
    {
        var keepiClient = new KeepiClient(httpClient: httpClient);
        keepiClient.ConfigureUser(name: fullName, subjectClaim: subjectClaim);

        return keepiClient;
    }

    public void ConfigureUser(string name, string subjectClaim)
    {
        httpClient.DefaultRequestHeaders.Add("X-User-Name", name);
        httpClient.DefaultRequestHeaders.Add("X-User-Subject-Claim", subjectClaim);
    }

    public async Task<GetAllUsersResponse> GetAllUsers()
    {
        return await MakeDebuggableRequestWithResponse<GetAllUsersResponse>(
            path: "/api/users",
            method: HttpMethod.Get
        );
    }

    public async Task<GetUserResponse> GetUser()
    {
        return await MakeDebuggableRequestWithResponse<GetUserResponse>(
            path: "/api/user",
            method: HttpMethod.Get
        );
    }

    public async Task<GetAllProjectsResponse> GetAllProjects()
    {
        return await MakeDebuggableRequestWithResponse<GetAllProjectsResponse>(
            path: "/api/projects",
            method: HttpMethod.Get
        );
    }

    public async Task<GetAllProjectsResponseProject> GetProject(int projectId)
    {
        var response = await GetAllProjects();
        response.Projects.ShouldContain(p => p.Id == projectId);

        return response.Projects.Single(p => p.Id == projectId);
    }

    public async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request)
    {
        return await MakeDebuggableRequestWithResponse<CreateProjectResponse>(
            path: "/api/projects",
            jsonBody: request,
            method: HttpMethod.Post
        );
    }

    public async Task UpdateProject(int projectId, UpdateProjectRequest request)
    {
        var (response, _) = await MakeDebuggableRequest(
            path: $"/api/projects/{projectId}",
            jsonBody: request,
            method: HttpMethod.Put
        );
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    public async Task DeleteProject(int projectId)
    {
        var (response, _) = await MakeDebuggableRequest(
            path: $"/api/projects/{projectId}",
            method: HttpMethod.Delete
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task<GetUserProjectsResponse> GetUserProjects()
    {
        return await MakeDebuggableRequestWithResponse<GetUserProjectsResponse>(
            "/api/user/projects",
            method: HttpMethod.Get
        );
    }

    public async Task<GetWeekUserEntriesResponse> GetUserWeekEntries(int year, int weekNumber)
    {
        return await MakeDebuggableRequestWithResponse<GetWeekUserEntriesResponse>(
            path: $"/api/user/entries/year/{year}/week/{weekNumber}",
            method: HttpMethod.Get
        );
    }

    public async Task UpdateUserWeekEntries(
        int year,
        int weekNumber,
        PutUpdateWeekUserEntriesRequest request
    )
    {
        var (response, _) = await MakeDebuggableRequest(
            path: $"/api/user/entries/year/{year}/week/{weekNumber}",
            jsonBody: request,
            method: HttpMethod.Put
        );

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        response.Headers.Location?.OriginalString.ShouldBe(
            $"/api/user/entries/year/{year}/week/{weekNumber}"
        );
    }

    public async Task UpdateUserInvoiceItemCustomizations(
        UpdateAllUserInvoiceItemCustomizationsRequest request
    )
    {
        var (response, _) = await MakeDebuggableRequest(
            path: "/api/user/invoiceitemcustomizations",
            jsonBody: request,
            method: HttpMethod.Put
        );
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    public async Task<Stream> GetUserEntriesExportStream(DateOnly start, DateOnly stop)
    {
        var response = await httpClient.PostAsJsonAsync(
            requestUri: "/api/user/entries/export",
            value: new GetUserEntriesExportEndpointRequest { Start = start, Stop = stop },
            options: jsonSerializerOptions
        );
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    private Task<TResponse> MakeDebuggableRequestWithResponse<TResponse>(
        string path,
        HttpMethod method
    )
        where TResponse : class =>
        MakeDebuggableRequestWithResponse<TResponse>(path: path, jsonBody: null, method: method);

    private async Task<TResponse> MakeDebuggableRequestWithResponse<TResponse>(
        string path,
        object? jsonBody,
        HttpMethod method
    )
        where TResponse : class
    {
        var (response, body) = await MakeDebuggableRequest(
            path: path,
            jsonBody: jsonBody,
            method: method
        );

        response.EnsureSuccessStatusCode();
        body.ShouldNotBeNull();

        var result = JsonSerializer.Deserialize<TResponse>(
            json: body,
            options: jsonSerializerOptions
        );
        result.ShouldNotBeNull();

        return result;
    }

    private Task<(HttpResponseMessage response, string body)> MakeDebuggableRequest(
        string path,
        HttpMethod method
    ) => MakeDebuggableRequest(path: path, jsonBody: null, method: method);

    private async Task<(HttpResponseMessage response, string body)> MakeDebuggableRequest(
        string path,
        object? jsonBody,
        HttpMethod method
    )
    {
        var request = new HttpRequestMessage(method: method, requestUri: path);
        if (jsonBody != null)
        {
            request.Content = new StringContent(
                content: JsonSerializer.Serialize(value: jsonBody, options: jsonSerializerOptions),
                mediaType: new(mediaType: "application/json")
            );
        }

        var response = await httpClient.SendAsync(request: request);
        var body = await response.Content.ReadAsStringAsync();

        return (response, body);
    }
}
