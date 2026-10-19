using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using RaceDayAPI.DTOs.Auth;
using RaceDayAPI.DTOs.Events;
using RaceDayAPI.Models;
using Xunit;

namespace RaceDayAPI.Tests;

// True unit tests: validate small classes without starting the API or database.
public sealed class ValidationUnitTests
{
    [Fact]
    public void RegistrationDto_Rejects_Invalid_Email()
    {
        var data = new RegisterDto
        {
            FirstName = "Daniel", LastName = "Peters", Email = "not-an-email",
            Password = "RaceDayTest123!", Role = "Participant"
        };
        Assert.NotEmpty(Validate(data));
    }

    [Fact]
    public void RegistrationDto_Rejects_Password_Shorter_Than_Eight()
    {
        var data = new RegisterDto
        {
            FirstName = "Daniel", LastName = "Peters", Email = "daniel@example.com",
            Password = "short", Role = "Participant"
        };
        Assert.NotEmpty(Validate(data));
    }

    [Fact]
    public void EventDto_Rejects_Deadline_After_EventDate()
    {
        var data = new EventWriteDto
        {
            EventName = "City Run", Location = "Cape Town", EventType = "Running",
            EventDate = new DateTime(2027, 2, 1),
            RegistrationDeadline = new DateTime(2027, 2, 2)
        };
        Assert.NotEmpty(Validate(data));
    }

    [Fact]
    public void PasswordHasher_Does_Not_Save_Plaintext_And_Verifies_Correctly()
    {
        var user = new User { Email = "daniel@example.com", Role = "Participant" };
        var hasher = new PasswordHasher<User>();
        string hash = hasher.HashPassword(user, "RaceDayTest123!");
        Assert.NotEqual("RaceDayTest123!", hash);
        Assert.NotEqual(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, hash, "RaceDayTest123!"));
        Assert.Equal(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, hash, "wrong-password"));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, true);
        return errors;
    }
}
