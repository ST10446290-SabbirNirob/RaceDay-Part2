using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDayAPI.Tests;

// These are integration tests: they exercise real API routes and auth middleware.
// The database is isolated in memory and is not the Part 1 RaceDay database.
public sealed class ApiIntegrationTests
{
    private static readonly object Participant = new
    {
        firstName = "Daniel", lastName = "Peters",
        email = "daniel.test@example.com", password = "RaceDayTest123!",
        role = "Participant"
    };

    private static readonly object Organiser = new
    {
        firstName = "Sarah", lastName = "Jacobs",
        email = "sarah.test@example.com", password = "RaceDayTest123!",
        role = "Organiser"
    };

    [Fact]
    public async Task Registration_Rejects_DuplicateEmail_And_InvalidRole()
    {
        using var factory = new RaceDayApiFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/auth/register", Participant)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/register", Participant)).StatusCode);
        var invalid = new { firstName = "Invalid", lastName = "User", email = "invalid@example.com",
            password = "RaceDayTest123!", role = "Administrator" };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/register", invalid)).StatusCode);
    }

    [Fact]
    public async Task Organiser_Registration_IsAllowed_Without_Invitation()
    {
        using var factory = new RaceDayApiFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/auth/register", Organiser)).StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Fails_And_Session_Can_Logout()
    {
        using var factory = new RaceDayApiFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        await client.PostAsJsonAsync("/api/auth/register", Participant);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync("/api/auth/login", new
            { email = "daniel.test@example.com", password = "Wrong123!" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, await Login(client, "daniel.test@example.com"));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await SendProtected(client, HttpMethod.Post, "/api/auth/logout", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task EventCreation_Requires_Organiser_Role()
    {
        using var factory = new RaceDayApiFactory();
        using var participant = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        using var organiser = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        var ev = EventData();

        Assert.Equal(HttpStatusCode.Unauthorized,
            (await SendProtected(participant, HttpMethod.Post, "/api/events", ev)).StatusCode);

        await participant.PostAsJsonAsync("/api/auth/register", Participant);
        await Login(participant, "daniel.test@example.com");
        Assert.Equal(HttpStatusCode.Forbidden,
            (await SendProtected(participant, HttpMethod.Post, "/api/events", ev)).StatusCode);

        await organiser.PostAsJsonAsync("/api/auth/register", Organiser);
        await Login(organiser, "sarah.test@example.com");
        Assert.Equal(HttpStatusCode.Created,
            (await SendProtected(organiser, HttpMethod.Post, "/api/events", ev)).StatusCode);
    }

    [Fact]
    public async Task Participant_CanEnrol_And_DuplicateEntry_IsRejected()
    {
        using var factory = new RaceDayApiFactory();
        using var organiser = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        using var participant = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        await organiser.PostAsJsonAsync("/api/auth/register", Organiser);
        await participant.PostAsJsonAsync("/api/auth/register", Participant);
        await Login(organiser, "sarah.test@example.com");
        await Login(participant, "daniel.test@example.com");

        using var eventReply = await SendProtected(organiser, HttpMethod.Post, "/api/events", EventData());
        Assert.Equal(HttpStatusCode.Created, eventReply.StatusCode);
        int eventId = await GetInt(eventReply, "eventID");
        var category = new { categoryName = "10 km Run", distanceKm = 10m,
            entryFee = 150m, maxParticipants = 100 };
        using var categoryReply = await SendProtected(organiser, HttpMethod.Post,
            $"/api/events/{eventId}/categories", category);
        Assert.Equal(HttpStatusCode.Created, categoryReply.StatusCode);
        int categoryId = await GetInt(categoryReply, "categoryID");

        var entry = new { eventID = eventId, categoryID = categoryId };
        Assert.Equal(HttpStatusCode.Created,
            (await SendProtected(participant, HttpMethod.Post, "/api/enrolments", entry)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,
            (await SendProtected(participant, HttpMethod.Post, "/api/enrolments", entry)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await participant.GetAsync("/api/enrolments/my")).StatusCode);
    }

    private static object EventData() => new
    {
        eventName = "RaceDay Integration Test Run",
        description = "Created automatically during tests",
        eventDate = DateTime.Now.AddDays(30),
        location = "Cape Town",
        eventType = "Running",
        registrationDeadline = DateTime.Now.AddDays(20)
    };

    private static async Task<HttpStatusCode> Login(HttpClient client, string email)
    {
        var reply = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "RaceDayTest123!" });
        return reply.StatusCode;
    }

    private static Task<HttpResponseMessage> SendProtected(
        HttpClient client, HttpMethod method, string url, object payload)
    {
        var message = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(payload)
        };
        message.Headers.Add("X-RaceDay-Request", "1");
        return client.SendAsync(message);
    }

    private static async Task<int> GetInt(HttpResponseMessage message, string propertyName)
    {
        using var doc = JsonDocument.Parse(await message.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty(propertyName).GetInt32();
    }
}
