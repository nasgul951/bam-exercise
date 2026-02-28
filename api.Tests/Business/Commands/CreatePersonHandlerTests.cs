using StargateAPI.Business.Commands;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Commands;

public class CreatePersonHandlerTests
{
    [Fact]
    public async Task Handle_WhenValidRequest_PersistsPersonToDatabase()
    {
        using var ctx = DbContextFactory.Create();
        var handler = new CreatePersonHandler(ctx);
        var request = new CreatePerson { Name = "Neil Armstrong" };

        await handler.Handle(request, CancellationToken.None);

        Assert.Single(ctx.People);
        Assert.Equal("Neil Armstrong", ctx.People.First().Name);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsResultWithGeneratedId()
    {
        using var ctx = DbContextFactory.Create();
        var handler = new CreatePersonHandler(ctx);
        var request = new CreatePerson { Name = "Buzz Aldrin" };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsSuccessTrue()
    {
        using var ctx = DbContextFactory.Create();
        var handler = new CreatePersonHandler(ctx);
        var request = new CreatePerson { Name = "Yuri Gagarin" };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Success);
    }
}
