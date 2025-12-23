using System.Net;
using System.Net.Http.Json;
using Calendar.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Calendar.Tests.Api;

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Then_List_ShouldReturnEvent()
    {
        var now = DateTimeOffset.UtcNow;
        var req = new CreateEventRequest(
            "Demo",
            "Test",
            now.AddHours(1),
            now.AddHours(2),
            new List<CreateAttendeeRequest> { new("Gustavo", "gustavo@test.com") }
        );

        var create = await _client.PostAsJsonAsync("/api/events", req);
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await create.Content.ReadFromJsonAsync<EventDto>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Demo");

        var list = await _client.GetAsync("/api/events");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await list.Content.ReadFromJsonAsync<List<EventDto>>();
        items!.Any(e => e.Id == created.Id).Should().BeTrue();
    }
}
