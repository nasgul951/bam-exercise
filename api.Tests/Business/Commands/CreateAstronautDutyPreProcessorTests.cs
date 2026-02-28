using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Commands;

public class CreateAstronautDutyPreProcessorTests
{
    private static readonly DateTime BaseDate = new(2024, 1, 1);

    private static CreateAstronautDutyPreProcessor CreateProcessor(StargateContext ctx) =>
        new(ctx);

    private static CreateAstronautDuty ValidRequest(string name = "Alice") => new()
    {
        Name = name,
        Rank = "Colonel",
        DutyTitle = "Commander",
        DutyStartDate = BaseDate
    };

    private static async Task<Person> SeedPerson(StargateContext ctx, string name = "Alice")
    {
        var person = new Person { Name = name };
        ctx.People.Add(person);
        await ctx.SaveChangesAsync();
        return person;
    }

    [Fact]
    public async Task Process_WhenNameIsEmpty_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var request = ValidRequest();
        request.Name = "";

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenRankIsEmpty_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var request = ValidRequest();
        request.Rank = "";

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenDutyTitleIsEmpty_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var request = ValidRequest();
        request.DutyTitle = "";

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenPersonDoesNotExist_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        // No person seeded

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(ValidRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenDutyIsDuplicate_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        ctx.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            DutyTitle = "Commander",
            DutyStartDate = BaseDate,
            Rank = "Colonel"
        });
        await ctx.SaveChangesAsync();

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(ValidRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenStartDateEqualsMostRecentDuty_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        ctx.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            DutyTitle = "Pilot",
            DutyStartDate = BaseDate,
            Rank = "Major"
        });
        await ctx.SaveChangesAsync();

        // Same start date as existing duty → should fail
        var request = ValidRequest();
        request.DutyStartDate = BaseDate;

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenStartDateBeforeMostRecentDuty_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        ctx.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            DutyTitle = "Pilot",
            DutyStartDate = BaseDate,
            Rank = "Major"
        });
        await ctx.SaveChangesAsync();

        var request = ValidRequest();
        request.DutyStartDate = BaseDate.AddDays(-1);

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            CreateProcessor(ctx).Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenStartDateAfterMostRecentDuty_CompletesSuccessfully()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        ctx.AstronautDuties.Add(new AstronautDuty
        {
            PersonId = person.Id,
            DutyTitle = "Pilot",
            DutyStartDate = BaseDate,
            Rank = "Major"
        });
        await ctx.SaveChangesAsync();

        var request = ValidRequest();
        request.DutyStartDate = BaseDate.AddDays(1);

        var exception = await Record.ExceptionAsync(() =>
            CreateProcessor(ctx).Process(request, CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Process_WhenPersonHasNoPreviousDuties_CompletesSuccessfully()
    {
        using var ctx = DbContextFactory.Create();
        await SeedPerson(ctx);

        var exception = await Record.ExceptionAsync(() =>
            CreateProcessor(ctx).Process(ValidRequest(), CancellationToken.None));

        Assert.Null(exception);
    }
}
