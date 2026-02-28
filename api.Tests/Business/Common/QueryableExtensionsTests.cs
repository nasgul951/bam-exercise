using StargateAPI.Business.Common;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Common;

public class QueryableExtensionsTests
{
    private static async Task SeedPeople(StargateContext ctx, int count)
    {
        for (int i = 1; i <= count; i++)
            ctx.People.Add(new Person { Name = $"Person {i}" });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task ToPagedListAsync_WhenSourceIsEmpty_ReturnsEmptyResult()
    {
        using var ctx = DbContextFactory.Create();

        var result = await ctx.People.ToPagedListAsync(1, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task ToPagedListAsync_WhenPageOneOfMany_ReturnsCorrectSlice()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPeople(ctx, 25);

        var result = await ctx.People.ToPagedListAsync(1, 10);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
    }

    [Fact]
    public async Task ToPagedListAsync_WhenLastPartialPage_ReturnsRemainder()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPeople(ctx, 25);

        var result = await ctx.People.ToPagedListAsync(3, 10);

        Assert.Equal(5, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
    }

    [Fact]
    public async Task ToPagedListAsync_WhenPageBeyondTotal_ReturnsEmptyItems()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPeople(ctx, 5);

        var result = await ctx.People.ToPagedListAsync(3, 10);

        Assert.Empty(result.Items);
        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task ToPagedListAsync_WhenExactlyOnePage_ReturnsAllItemsWithNoNextPage()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPeople(ctx, 10);

        var result = await ctx.People.ToPagedListAsync(1, 10);

        Assert.Equal(10, result.Items.Count);
        Assert.False(result.HasNextPage);
    }
}
