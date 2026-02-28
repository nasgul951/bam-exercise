using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Queries;

public class GetPeopleHandlerTests
{
    private static GetPeopleHandler CreateHandler(StargateContext ctx) =>
        new(ctx);

    private static async Task<Person> SeedPerson(StargateContext ctx, string name,
        string? rank = null, string? dutyTitle = null)
    {
        var person = new Person { Name = name };
        ctx.People.Add(person);
        await ctx.SaveChangesAsync();

        if (rank != null && dutyTitle != null)
        {
            ctx.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = rank,
                CurrentDutyTitle = dutyTitle,
                CareerStartDate = new DateTime(2020, 1, 1)
            });
            await ctx.SaveChangesAsync();
        }

        return person;
    }

    [Fact]
    public async Task Handle_WhenNoPeople_ReturnsEmptyPagedResult()
    {
        using var ctx = DbContextFactory.Create();
        var result = await CreateHandler(ctx).Handle(
            new GetPeople { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Empty(result.People.Items);
        Assert.Equal(0, result.People.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenPeopleExist_ReturnsCorrectProjection()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPerson(ctx, "Neil Armstrong", rank: "Colonel", dutyTitle: "Commander");

        var result = await CreateHandler(ctx).Handle(
            new GetPeople { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Single(result.People.Items);
        var item = result.People.Items[0];
        Assert.Equal("Neil Armstrong", item.Name);
        Assert.Equal("Colonel", item.CurrentRank);
        Assert.Equal("Commander", item.CurrentDutyTitle);
    }

    [Fact]
    public async Task Handle_WhenPersonHasNoAstronautDetail_ProjectsNullAstronautFields()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPerson(ctx, "John Doe");

        var result = await CreateHandler(ctx).Handle(
            new GetPeople { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Single(result.People.Items);
        var item = result.People.Items[0];
        Assert.Null(item.CareerStartDate);
        Assert.Null(item.CareerEndDate);
    }

    [Fact]
    public async Task Handle_WhenMultiplePeopleExist_RespectsPageSize()
    {
        using var ctx = DbContextFactory.Create();
        for (int i = 1; i <= 15; i++)
            await SeedPerson(ctx, $"Person {i}");

        var result = await CreateHandler(ctx).Handle(
            new GetPeople { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(10, result.People.Items.Count);
        Assert.Equal(15, result.People.TotalCount);
    }

    [Fact]
    public async Task Handle_WhenOnSecondPage_RespectsPageNumber()
    {
        using var ctx = DbContextFactory.Create();
        for (int i = 1; i <= 15; i++)
            await SeedPerson(ctx, $"Person {i}");

        var result = await CreateHandler(ctx).Handle(
            new GetPeople { PageNumber = 2, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(5, result.People.Items.Count);
        Assert.Equal(15, result.People.TotalCount);
    }
}
