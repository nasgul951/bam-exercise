using StargateAPI.Business.Data;
using StargateAPI.Business.Exceptions;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Queries;

public class GetAstronautDutiesByNameHandlerTests
{
    private static GetAstronautDutiesByNameHandler CreateHandler(StargateContext ctx) =>
        new(ctx);

    private static async Task<Person> SeedPerson(StargateContext ctx, string name)
    {
        var person = new Person { Name = name };
        ctx.People.Add(person);
        await ctx.SaveChangesAsync();
        return person;
    }

    [Fact]
    public async Task Handle_WhenPersonDoesNotExist_ThrowsNotFoundException()
    {
        using var ctx = DbContextFactory.Create();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler(ctx).Handle(
                new GetAstronautDutiesByName { Name = "Ghost" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPersonExistsWithNoDuties_ReturnsEmptyDutiesList()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPerson(ctx, "Alice");

        var result = await CreateHandler(ctx).Handle(
            new GetAstronautDutiesByName { Name = "Alice" }, CancellationToken.None);

        Assert.Empty(result.AstronautDuties);
    }

    [Fact]
    public async Task Handle_WhenPersonHasMultipleDuties_ReturnsDutiesDescendingByStartDate()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx, "Alice");
        ctx.AstronautDuties.AddRange(
            new AstronautDuty { PersonId = person.Id, DutyTitle = "Pilot", Rank = "Major", DutyStartDate = new DateTime(2022, 1, 1) },
            new AstronautDuty { PersonId = person.Id, DutyTitle = "Commander", Rank = "Colonel", DutyStartDate = new DateTime(2024, 1, 1) },
            new AstronautDuty { PersonId = person.Id, DutyTitle = "Trainee", Rank = "Lieutenant", DutyStartDate = new DateTime(2020, 1, 1) }
        );
        await ctx.SaveChangesAsync();

        var result = await CreateHandler(ctx).Handle(
            new GetAstronautDutiesByName { Name = "Alice" }, CancellationToken.None);

        Assert.Equal(3, result.AstronautDuties.Count);
        Assert.Equal(new DateTime(2024, 1, 1), result.AstronautDuties[0].DutyStartDate);
        Assert.Equal(new DateTime(2022, 1, 1), result.AstronautDuties[1].DutyStartDate);
        Assert.Equal(new DateTime(2020, 1, 1), result.AstronautDuties[2].DutyStartDate);
    }

    [Fact]
    public async Task Handle_WhenMultiplePeopleExist_ReturnsOnlyDutiesForRequestedPerson()
    {
        using var ctx = DbContextFactory.Create();
        var alice = await SeedPerson(ctx, "Alice");
        var bob = await SeedPerson(ctx, "Bob");

        ctx.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = alice.Id,
            DutyTitle = "Commander",
            Rank = "Colonel",
            DutyStartDate = new DateTime(2024, 1, 1)
        });
        ctx.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = bob.Id,
            DutyTitle = "Pilot",
            Rank = "Major",
            DutyStartDate = new DateTime(2023, 1, 1)
        });
        await ctx.SaveChangesAsync();

        var result = await CreateHandler(ctx).Handle(
            new GetAstronautDutiesByName { Name = "Alice" }, CancellationToken.None);

        Assert.Single(result.AstronautDuties);
        Assert.Equal("Commander", result.AstronautDuties[0].DutyTitle);
    }

    [Fact]
    public async Task Handle_WhenPersonExists_ReturnsCorrectPersonAstronaut()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx, "Alice");
        ctx.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "Colonel",
            CurrentDutyTitle = "Commander",
            CareerStartDate = new DateTime(2020, 1, 1)
        });
        await ctx.SaveChangesAsync();

        var result = await CreateHandler(ctx).Handle(
            new GetAstronautDutiesByName { Name = "Alice" }, CancellationToken.None);

        Assert.Equal("Alice", result.Person.Name);
        Assert.Equal("Colonel", result.Person.CurrentRank);
    }
}
