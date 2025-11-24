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
        var response = await httpClient.GetFromJsonAsync<GetAllUsersResponse>(
            requestUri: "/api/users",
            options: jsonSerializerOptions
        );
        response.ShouldNotBeNull();

        return response;
    }

    public async Task<GetUserResponse> GetUser()
    {
        var response = await httpClient.GetFromJsonAsync<GetUserResponse>(
            requestUri: "/api/user",
            options: jsonSerializerOptions
        );
        response.ShouldNotBeNull();

        return response;
    }

    public async Task<GetAllProjectsResponse> GetAllProjects()
    {
        var response = await httpClient.GetFromJsonAsync<GetAllProjectsResponse>(
            requestUri: "/api/projects",
            options: jsonSerializerOptions
        );
        response.ShouldNotBeNull();

        return response;
    }

    public async Task<GetAllProjectsResponseProject> GetProject(int projectId)
    {
        return (await GetAllProjects()).Projects.Single(u => u.Id == projectId);
    }

    public async Task<CreateProjectResponse> CreateProject(CreateProjectRequest request)
    {
        var response = await httpClient.PostAsJsonAsync(
            requestUri: "/api/projects",
            value: request,
            options: jsonSerializerOptions
        );
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync<CreateProjectResponse>();
        responseModel.ShouldNotBeNull();

        return responseModel;
    }

    public async Task UpdateProject(int projectId, UpdateProjectRequest request)
    {
        var response = await httpClient.PutAsJsonAsync(
            requestUri: $"/api/projects/{projectId}",
            value: request,
            options: jsonSerializerOptions
        );
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    public async Task DeleteProject(int projectId)
    {
        var response = await httpClient.DeleteAsync(requestUri: $"/api/projects/{projectId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<GetUserProjectsResponse> GetUserProjects()
    {
        var response = await httpClient.GetFromJsonAsync<GetUserProjectsResponse>(
            requestUri: "/api/user/projects",
            options: jsonSerializerOptions
        );
        response.ShouldNotBeNull();

        return response;
    }

    public async Task<GetWeekUserEntriesResponse> GetUserWeekEntries(int year, int weekNumber)
    {
        var response = await httpClient.GetAsync(
            requestUri: $"/api/user/entries/year/{year}/week/{weekNumber}"
        );
        response.EnsureSuccessStatusCode();

        var responseModel = await response.Content.ReadFromJsonAsync<GetWeekUserEntriesResponse>(
            options: jsonSerializerOptions
        );
        responseModel.ShouldNotBeNull();

        return responseModel;
    }

    public async Task UpdateUserWeekEntries(
        int year,
        int weekNumber,
        PutUpdateWeekUserEntriesRequest request
    )
    {
        var response = await httpClient.PutAsJsonAsync(
            requestUri: $"/api/user/entries/year/{year}/week/{weekNumber}",
            value: request,
            options: jsonSerializerOptions
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
        var response = await httpClient.PutAsJsonAsync(
            requestUri: "/api/user/invoiceitemcustomizations",
            value: request,
            options: jsonSerializerOptions
        );
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    public async Task<Stream> GetUserEntriesExportStream(DateOnly start, DateOnly stop)
    {
        var httpResponse = await httpClient.PostAsJsonAsync(
            requestUri: "/api/user/entries/export",
            value: new GetUserEntriesExportEndpointRequest { Start = start, Stop = stop },
            options: jsonSerializerOptions
        );
        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadAsStreamAsync();
    }
}
