using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BiteForm.Api.Endpoints;
using BiteForm.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace BiteForm.Tests.Integration;

[TestFixture]
public class ApiIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = string.Empty,
                        ["UseInMemoryDb"] = "true"
                    });
                });
                // No need to override DbContext here; Infra uses InMemory when UseInMemoryDb=true
            });
    }

    [TearDown]
    public void TearDown() => _factory.Dispose();

    [Test]
    public async Task Forms_CRUD_Works()
    {
        var client = _factory.CreateClient();
        var tenant = "it_tenant";

        // Create form
        var create = new FormsEndpoints.CreateFormRequest(tenant, "Contact", "desc");
        var resp = await client.PostAsJsonAsync("/api/v1/forms", create);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var formId = created.GetProperty("id").GetGuid();

        // Update directly
        var update = new FormsEndpoints.UpdateFormRequest(tenant, "Contact Updated", "Updated");
        var updResp = await client.PutAsJsonAsync($"/api/v1/forms/{formId}", update);
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var upd = await updResp.Content.ReadFromJsonAsync<JsonElement>();
        upd.GetProperty("name").GetString().Should().Be("Contact Updated");

        // Delete
        var del = await client.DeleteAsync($"/api/v1/forms/{formId}?tenantId={tenant}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get returns problem+json 404
        var notFound = await client.GetAsync($"/api/v1/forms/{formId}?tenantId={tenant}");
        notFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
        notFound.Content.Headers.ContentType!.MediaType.Should().Contain("problem+json");
    }

    [Test]
    public async Task Fields_CRUD_And_Conflict_Works()
    {
        var client = _factory.CreateClient();
        var tenant = "t_fields";
        // Create form
        var formResp = await client.PostAsJsonAsync("/api/v1/forms", new FormsEndpoints.CreateFormRequest(tenant, "F", null));
        var formId = (await formResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // Create first field via absolute route
        var createField = await client.PostAsJsonAsync($"/api/v1/forms/{formId}/fields?tenantId={tenant}", new FormsEndpoints.CreateFieldRequest("email", "Email", "text", true, 1));
        createField.EnsureSuccessStatusCode();
        // Create second field
        await client.PostAsJsonAsync($"/api/v1/forms/{formId}/fields?tenantId={tenant}", new FormsEndpoints.CreateFieldRequest("name", "Name", "text", false, 2));

        // List fields
        var list = await client.GetFromJsonAsync<JsonElement>($"/api/v1/forms/{formId}/fields?tenantId={tenant}&page=1&pageSize=10");
        list.GetProperty("items").EnumerateArray().Should().HaveCount(2);

        // Get one
        var firstId = list.GetProperty("items").EnumerateArray().First().GetProperty("id").GetGuid();
        var one = await client.GetFromJsonAsync<JsonElement>($"/api/v1/forms/{formId}/fields/{firstId}?tenantId={tenant}");
        one.GetProperty("formId").GetGuid().Should().Be(formId);

        // Update field label
        var upd = new FormsEndpoints.UpdateFieldRequest(null, "Email Address", null, null, null);
        var updResp = await client.PutAsJsonAsync($"/api/v1/forms/{formId}/fields/{firstId}?tenantId={tenant}", upd);
        updResp.EnsureSuccessStatusCode();

        // Conflict on key change to existing key
        var keyConflict = new FormsEndpoints.UpdateFieldRequest("name", null, null, null, null);
        var conflictResp = await client.PutAsJsonAsync($"/api/v1/forms/{formId}/fields/{firstId}?tenantId={tenant}", keyConflict);
        conflictResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        conflictResp.Content.Headers.ContentType!.MediaType.Should().Contain("problem+json");

        // Delete
        var del = await client.DeleteAsync($"/api/v1/forms/{formId}/fields/{firstId}?tenantId={tenant}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Submissions_Create_List_Responses_And_Validation()
    {
        var client = _factory.CreateClient();
        var tenant = "t_sub";
        // Form
        var formResp = await client.PostAsJsonAsync("/api/v1/forms", new FormsEndpoints.CreateFormRequest(tenant, "S", null));
        var formId = (await formResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        // Seed fields directly to avoid routing ambiguity under TestServer
        Guid f1, f2;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            f1 = Guid.NewGuid();
            f2 = Guid.NewGuid();
            db.Fields.Add(new BiteForm.Domain.Entities.FormField { Id = f1, FormId = formId, Key = "q1", Label = "Q1", Type = "text", Required = true, Order = 1 });
            db.Fields.Add(new BiteForm.Domain.Entities.FormField { Id = f2, FormId = formId, Key = "q2", Label = "Q2", Type = "text", Required = false, Order = 2 });
            await db.SaveChangesAsync();
        }

        // Submission with responses
        var subBody = new SubmissionsEndpoints.CreateSubmissionRequest("user@example.com",
            new List<SubmissionsEndpoints.CreateResponseItem>
            {
                new(f1, "hello"), new(f2, "world")
            });
        var sub = await client.PostAsJsonAsync($"/api/v1/forms/{formId}/submissions?tenantId={tenant}", subBody);
        sub.StatusCode.Should().Be(HttpStatusCode.Created);
        var subId = (await sub.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        // List responses paged
        var list = await client.GetFromJsonAsync<JsonElement>($"/api/v1/forms/{formId}/submissions/{subId}/responses?tenantId={tenant}&page=1&pageSize=10");
        list.GetProperty("items").EnumerateArray().Should().HaveCount(2);

        // Duplicate response -> 409 problem
        var dupResp = await client.PostAsJsonAsync($"/api/v1/forms/{formId}/submissions/{subId}/responses?tenantId={tenant}", new SubmissionsEndpoints.CreateResponseRequest(f1, "x"));
        dupResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        dupResp.Content.Headers.ContentType!.MediaType.Should().Contain("problem+json");

        // Invalid field id -> 400 validation problem
        var bad = await client.PostAsJsonAsync($"/api/v1/forms/{formId}/submissions/{subId}/responses?tenantId={tenant}", new SubmissionsEndpoints.CreateResponseRequest(Guid.NewGuid(), "x"));
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        bad.Content.Headers.ContentType!.MediaType.Should().Contain("problem+json");
    }

    [Test]
    public async Task UnknownRoute_Returns_ProblemJson_404()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/nope/not-here");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        resp.Content.Headers.ContentType!.MediaType.Should().Contain("problem+json");
    }
}
