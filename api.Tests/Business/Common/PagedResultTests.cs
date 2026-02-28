using StargateAPI.Business.Common;

namespace StargateAPI.Tests.Business.Common;

public class PagedResultTests
{
    private static PagedResult<int> Make(int totalCount, int pageSize, int pageNumber) =>
        new() { TotalCount = totalCount, PageSize = pageSize, PageNumber = pageNumber };

    [Fact]
    public void TotalPages_WhenCountDivisibleByPageSize_ReturnsExactQuotient()
    {
        var result = Make(20, 10, 1);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenCountNotDivisibleByPageSize_RoundsUp()
    {
        var result = Make(21, 10, 1);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenCountIsZero_ReturnsZero()
    {
        var result = Make(0, 10, 1);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_WhenPageNumberIsOne_ReturnsFalse()
    {
        var result = Make(20, 10, 1);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_WhenPageNumberIsGreaterThanOne_ReturnsTrue()
    {
        var result = Make(20, 10, 2);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ReturnsFalse()
    {
        // 10 count, page size 10, page 1 → TotalPages=1, no next page
        var result = Make(10, 10, 1);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_WhenNotOnLastPage_ReturnsTrue()
    {
        // 20 count, page size 10, page 1 → TotalPages=2, has next
        var result = Make(20, 10, 1);
        Assert.True(result.HasNextPage);
    }
}
