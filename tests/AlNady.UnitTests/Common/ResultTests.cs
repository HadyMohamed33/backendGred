using AlNady.Shared.Common;

namespace AlNady.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_Should_Set_IsSuccess_True()
    {
        var result = Result<string>.Success("data");
        Assert.True(result.IsSuccess);
        Assert.Equal("data", result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public void Failure_Should_Set_IsSuccess_False()
    {
        var result = Result<string>.Failure("error message");
        Assert.False(result.IsSuccess);
        Assert.Equal("error message", result.ErrorMessage);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void NotFound_Should_Return_404()
    {
        var result = Result<string>.NotFound("Not found");
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public void Unauthorized_Should_Return_401()
    {
        var result = Result<string>.Unauthorized();
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public void Conflict_Should_Return_409()
    {
        var result = Result<string>.Conflict("Conflict");
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public void Failure_With_Errors_List_Should_Join_Errors()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var result = Result<string>.Failure(errors);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Error 1", result.ErrorMessage);
    }

    [Fact]
    public void PagedResult_Should_Calculate_TotalPages()
    {
        var paged = PagedResult<int>.Create(new[] { 1, 2, 3 }, 25, 1, 10);
        Assert.Equal(3, paged.TotalPages);
        Assert.True(paged.HasNextPage);
        Assert.False(paged.HasPreviousPage);
    }

    [Fact]
    public void PagedResult_LastPage_Should_Not_HaveNextPage()
    {
        var paged = PagedResult<int>.Create(new[] { 1, 2, 3 }, 25, 3, 10);
        Assert.False(paged.HasNextPage);
        Assert.True(paged.HasPreviousPage);
    }
}
