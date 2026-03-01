using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Queries;

public class GetLogsHandlerTests
{
    private static GetLogsHandler CreateHandler(StargateContext ctx) => new(ctx);

    private static async Task<LogEntry> SeedLog(StargateContext ctx, string message, DateTime timestamp)
    {
        var entry = new LogEntry
        {
            Category = "TestCategory",
            LogLevel = "Information",
            Message = message,
            Timestamp = timestamp
        };
        ctx.LogEntries.Add(entry);
        await ctx.SaveChangesAsync();
        return entry;
    }

    [Fact]
    public async Task Handle_WhenNoLogs_ReturnsEmptyPagedResult()
    {
        using var ctx = DbContextFactory.Create();
        var result = await CreateHandler(ctx).Handle(
            new GetLogs { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Empty(result.Logs.Items);
        Assert.Equal(0, result.Logs.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenLogsExist_ReturnsLogsDescendingByTimestamp()
    {
        using var ctx = DbContextFactory.Create();
        var older = await SeedLog(ctx, "Older message", new DateTime(2025, 1, 1));
        var newer = await SeedLog(ctx, "Newer message", new DateTime(2025, 6, 1));

        var result = await CreateHandler(ctx).Handle(
            new GetLogs { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(2, result.Logs.TotalCount);
        Assert.Equal("Newer message", result.Logs.Items[0].Message);
        Assert.Equal("Older message", result.Logs.Items[1].Message);
    }

    [Fact]
    public async Task Handle_WhenMultipleLogsExist_RespectsPageSize()
    {
        using var ctx = DbContextFactory.Create();
        for (int i = 1; i <= 15; i++)
            await SeedLog(ctx, $"Log {i}", new DateTime(2025, 1, i));

        var result = await CreateHandler(ctx).Handle(
            new GetLogs { PageNumber = 1, PageSize = 6 }, CancellationToken.None);

        Assert.Equal(6, result.Logs.Items.Count);
        Assert.Equal(15, result.Logs.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenOnSecondPage_RespectsPageNumber()
    {
        using var ctx = DbContextFactory.Create();
        for (int i = 1; i <= 15; i++)
            await SeedLog(ctx, $"Log {i}", new DateTime(2025, 1, i));

        var result = await CreateHandler(ctx).Handle(
            new GetLogs { PageNumber = 2, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(5, result.Logs.Items.Count);
        Assert.Equal(15, result.Logs.TotalCount);
    }
}
