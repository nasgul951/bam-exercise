using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Commands;

public class CreateAstronautDutyHandlerTests
{
    private static readonly DateTime StartDate = new(2024, 1, 1);

    private static CreateAstronautDutyHandler CreateHandler(StargateContext ctx) =>
        new(ctx);

    private static async Task<Person> SeedPerson(StargateContext ctx, string name = "Alice")
    {
        var person = new Person { Name = name };
        ctx.People.Add(person);
        await ctx.SaveChangesAsync();
        return person;
    }

    private static CreateAstronautDuty MakeRequest(string name, string dutyTitle = "Commander",
        string rank = "Colonel", DateTime? startDate = null) => new()
    {
        Name = name,
        DutyTitle = dutyTitle,
        Rank = rank,
        DutyStartDate = startDate ?? StartDate
    };

    [Fact]
    public async Task Handle_WhenFirstDutyForPerson_CreatesAstronautDetail()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        var handler = CreateHandler(ctx);
        var request = MakeRequest(person.Name);

        await handler.Handle(request, CancellationToken.None);

        var detail = await ctx.AstronautDetails.FirstOrDefaultAsync(d => d.PersonId == person.Id);
        Assert.NotNull(detail);
        Assert.Equal(StartDate.Date, detail.CareerStartDate);
    }

    [Fact]
    public async Task Handle_WhenFirstDutyIsRetired_SetsCareerEndDateToStartDate()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        var handler = CreateHandler(ctx);
        var request = MakeRequest(person.Name, dutyTitle: "RETIRED");

        await handler.Handle(request, CancellationToken.None);

        var detail = await ctx.AstronautDetails.FirstAsync(d => d.PersonId == person.Id);
        Assert.Equal(StartDate.Date, detail.CareerEndDate);
    }

    [Fact]
    public async Task Handle_WhenSubsequentDuty_UpdatesExistingAstronautDetailNotCreatesNew()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        // Pre-existing detail
        ctx.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "Major",
            CurrentDutyTitle = "Pilot",
            CareerStartDate = StartDate.AddDays(-10)
        });
        await ctx.SaveChangesAsync();

        var handler = CreateHandler(ctx);
        var request = MakeRequest(person.Name, dutyTitle: "Commander", rank: "Colonel",
            startDate: StartDate);

        await handler.Handle(request, CancellationToken.None);

        var details = await ctx.AstronautDetails.Where(d => d.PersonId == person.Id).ToListAsync();
        Assert.Single(details);
        Assert.Equal("Commander", details[0].CurrentDutyTitle);
        Assert.Equal("Colonel", details[0].CurrentRank);
    }

    [Fact]
    public async Task Handle_WhenSubsequentDutyIsRetired_SetsCareerEndDateToStartDateMinusOne()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        ctx.AstronautDetails.Add(new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = "Major",
            CurrentDutyTitle = "Pilot",
            CareerStartDate = StartDate.AddDays(-10)
        });
        await ctx.SaveChangesAsync();

        var handler = CreateHandler(ctx);
        var request = MakeRequest(person.Name, dutyTitle: "RETIRED", startDate: StartDate);

        await handler.Handle(request, CancellationToken.None);

        var detail = await ctx.AstronautDetails.FirstAsync(d => d.PersonId == person.Id);
        Assert.Equal(StartDate.AddDays(-1).Date, detail.CareerEndDate);
    }

    [Fact]
    public async Task Handle_WhenPreviousDutyExists_ClosesPreviousDutyEndDate()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        var existingDuty = new AstronautDuty
        {
            PersonId = person.Id,
            Rank = "Major",
            DutyTitle = "Pilot",
            DutyStartDate = StartDate.AddDays(-5)
        };
        ctx.AstronautDuties.Add(existingDuty);
        await ctx.SaveChangesAsync();

        var handler = CreateHandler(ctx);
        var request = MakeRequest(person.Name, startDate: StartDate);

        await handler.Handle(request, CancellationToken.None);

        var closedDuty = await ctx.AstronautDuties.FindAsync(existingDuty.Id);
        Assert.NotNull(closedDuty!.DutyEndDate);
        Assert.Equal(StartDate.AddDays(-1).Date, closedDuty.DutyEndDate);
    }

    [Fact]
    public async Task Handle_WhenNoPreviousDutyExists_NewDutyHasNullEndDate()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        var handler = CreateHandler(ctx);

        var result = await handler.Handle(MakeRequest(person.Name), CancellationToken.None);

        var duty = await ctx.AstronautDuties.FindAsync(result.Id);
        Assert.Null(duty!.DutyEndDate);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsNewDutyId()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        var handler = CreateHandler(ctx);

        var result = await handler.Handle(MakeRequest(person.Name), CancellationToken.None);

        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_NewDutyHasCorrectFields()
    {
        using var ctx = DbContextFactory.Create();
        var person = await SeedPerson(ctx);
        var handler = CreateHandler(ctx);
        var request = MakeRequest(person.Name, dutyTitle: "Commander", rank: "Colonel",
            startDate: StartDate);

        var result = await handler.Handle(request, CancellationToken.None);

        var duty = await ctx.AstronautDuties.FindAsync(result.Id);
        Assert.NotNull(duty);
        Assert.Equal(person.Id, duty.PersonId);
        Assert.Equal("Commander", duty.DutyTitle);
        Assert.Equal("Colonel", duty.Rank);
        Assert.Equal(StartDate.Date, duty.DutyStartDate);
    }
}
