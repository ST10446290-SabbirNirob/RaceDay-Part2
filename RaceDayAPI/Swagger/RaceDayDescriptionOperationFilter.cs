using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RaceDayAPI.Swagger;

// Adds short, readable Swagger descriptions for every RaceDay endpoint.
// Request body fields are automatically documented from the DTO classes.
public sealed class RaceDayDescriptionOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["POST api/auth/register"] = "Register a new Participant or an invited Organiser. Passwords are hashed before storage.",
        ["POST api/auth/login"] = "Verify email and password and create a server-side login session.",
        ["GET api/auth/me"] = "Read the account of the currently logged-in user.",
        ["POST api/auth/logout"] = "Clear the active login session.",
        ["GET api/users/profile"] = "Read the authenticated user's profile without exposing the password hash.",
        ["PUT api/users/profile"] = "Update your own first name, last name and email; role cannot be changed here.",
        ["GET api/events"] = "List public RaceDay events ordered by date.",
        ["GET api/events/mine"] = "List events owned by the current Organiser.",
        ["GET api/events/{id}"] = "Get one event with its categories and routes.",
        ["POST api/events"] = "Create an event for the signed-in Organiser.",
        ["PUT api/events/{id}"] = "Edit an event owned by the signed-in Organiser.",
        ["DELETE api/events/{id}"] = "Delete your event only if no dependent categories, routes or enrolments exist.",
        ["GET api/events/{eventId}/categories"] = "List categories and distances for a given event.",
        ["GET api/categories/{id}"] = "Get details about one event category.",
        ["POST api/events/{eventId}/categories"] = "Create a category for an event that you organise.",
        ["PUT api/categories/{id}"] = "Update an existing category belonging to your event.",
        ["DELETE api/categories/{id}"] = "Delete a category only if it has no enrolments.",
        ["GET api/events/{eventId}/routes"] = "List route information for an event.",
        ["GET api/routes/{id}"] = "Get details for a route.",
        ["POST api/events/{eventId}/routes"] = "Create route information for an event you organise.",
        ["PUT api/routes/{id}"] = "Edit route information for your event.",
        ["DELETE api/routes/{id}"] = "Remove a route from your event.",
        ["POST api/enrolments"] = "Enter an event as a Participant and select an available category.",
        ["GET api/enrolments/my"] = "List your own enrolments and their statuses.",
        ["GET api/events/{eventId}/enrolments"] = "List enrolments for an event you organise.",
        ["PUT api/enrolments/{id}"] = "Update an enrolment's status as the owning Organiser.",
        ["DELETE api/enrolments/{id}"] = "Remove an eligible enrolment that belongs to you or your event.",
        ["POST api/results"] = "Capture the finish time and positions for an enrolment after its event.",
        ["GET api/results/my"] = "Show the signed-in Participant's personal results.",
        ["GET api/events/{eventId}/results"] = "View race results for a specific event.",
        ["PUT api/results/{id}"] = "Correct the finish time or positions for your event.",
        ["DELETE api/results/{id}"] = "Remove an incorrect result for your event."
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string key = $"{context.ApiDescription.HttpMethod} {context.ApiDescription.RelativePath}";
        // RelativePath sometimes has route constraints such as :int.
        key = key.Replace(":int", "", StringComparison.OrdinalIgnoreCase);
        if (!Descriptions.TryGetValue(key, out string? purpose)) return;

        operation.Summary = purpose;
        operation.Description = purpose + " The HTTP response indicates whether the request succeeded " +
            "or failed validation, authentication, permission or resource checks.";

        operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Invalid input or request." });
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "User must log in." });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Role or ownership does not allow this operation." });
        operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Requested record not found." });
        operation.Responses.TryAdd("409", new OpenApiResponse { Description = "Conflicting or dependent record." });
    }
}
