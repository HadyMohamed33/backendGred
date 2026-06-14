using AlNady.Application.Features.Auth.Commands;
using AlNady.Application.Features.Auth.Validators;
using FluentValidation.TestHelper;
using AlNady.Domain.Enums;
using Xunit;

namespace AlNady.UnitTests.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var cmd = new RegisterCommand("", "Password1!", "John Doe", null, UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var cmd = new RegisterCommand("not-an-email", "Password1!", "John", null, UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Too_Short()
    {
        var cmd = new RegisterCommand("test@mail.com", "Pass1!", "John", null, UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Lacks_Uppercase()
    {
        var cmd = new RegisterCommand("test@mail.com", "password1!", "John", null, UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Lacks_Special_Char()
    {
        var cmd = new RegisterCommand("test@mail.com", "Password1", "John", null, UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Have_Error_When_FullName_Is_Empty()
    {
        var cmd = new RegisterCommand("test@mail.com", "Password1!", "", null, UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Should_Pass_Validation_With_Valid_Data()
    {
        var cmd = new RegisterCommand("test@mail.com", "Password1!", "John Doe", "+201234567890", UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Is_Invalid()
    {
        var cmd = new RegisterCommand("test@mail.com", "Password1!", "John", "invalid-phone", UserRole.Player, null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }
}
