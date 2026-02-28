using StargateAPI.Business.Data;
using StargateAPI.Business.Exceptions;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Queries;

public class GetPersonByNameHandlerTests
{
    private static GetPersonByNameHandler CreateHandler(StargateContext ctx) =>
        new(ctx);

    [Fact]
    public async Task Handle_WhenPersonExists_ReturnsCorrectPersonAstronaut()
    {
        using var ctx = DbContextFactory.Create();
        var person = new Person { Name = "Neil Armstrong" };
        ctx.People.Add(person);
        await ctx.SaveChangesAsync();
        ctx.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "Colonel",
            CurrentDutyTitle = "Commander",
            CareerStartDate = new DateTime(2020, 1, 1)
        });
        await ctx.SaveChangesAsync();

        var result = await CreateHandler(ctx).Handle(
            new GetPersonByName { Name = "Neil Armstrong" }, CancellationToken.None);

        Assert.NotNull(result.Person);
        Assert.Equal("Neil Armstrong", result.Person.Name);
        Assert.Equal("Colonel", result.Person.CurrentRank);
        Assert.Equal("Commander", result.Person.CurrentDutyTitle);
    }

    [Fact]
    public async Task Handle_WhenPersonExistsWithNoAstronautDetail_ReturnsPersonWithNullAstronautFields()
    {
        using var ctx = DbContextFactory.Create();
        ctx.People.Add(new Person { Name = "John Doe" });
        await ctx.SaveChangesAsync();

        var result = await CreateHandler(ctx).Handle(
            new GetPersonByName { Name = "John Doe" }, CancellationToken.None);

        Assert.NotNull(result.Person);
        Assert.Equal("John Doe", result.Person.Name);
        // No AstronautDetail → navigation property is null, projected nullable fields are null
        Assert.Null(result.Person.CareerStartDate);
        Assert.Null(result.Person.CareerEndDate);
    }

    [Fact]
    public async Task Handle_WhenPersonDoesNotExist_ThrowsNotFoundException()
    {
        using var ctx = DbContextFactory.Create();

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await CreateHandler(ctx).Handle(
                new GetPersonByName { Name = "Nobody" }, CancellationToken.None);
        });
    }

    [Fact]
    public async Task Handle_WhenPersonExists_ResultSuccessIsTrue()
    {
        using var ctx = DbContextFactory.Create();
        ctx.People.Add(new Person { Name = "Buzz Aldrin" });
        await ctx.SaveChangesAsync();

        var result = await CreateHandler(ctx).Handle(
            new GetPersonByName { Name = "Buzz Aldrin" }, CancellationToken.None);

        Assert.True(result.Success);
    }
}
