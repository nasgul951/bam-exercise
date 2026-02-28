using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Helpers;

namespace StargateAPI.Tests.Business.Commands;

public class CreatePersonPreProcessorTests
{
    private static CreatePersonPreProcessor CreateProcessor(StargateContext ctx) =>
        new(ctx);

    [Fact]
    public async Task Process_WhenNameIsNull_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var processor = CreateProcessor(ctx);
        var request = new CreatePerson { Name = null! };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            processor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenNameIsWhitespace_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var processor = CreateProcessor(ctx);
        var request = new CreatePerson { Name = "   " };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            processor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenNameExceeds200Characters_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        var processor = CreateProcessor(ctx);
        var request = new CreatePerson { Name = new string('A', 201) };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            processor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenNameIsExactly200Characters_DoesNotThrow()
    {
        using var ctx = DbContextFactory.Create();
        var processor = CreateProcessor(ctx);
        var request = new CreatePerson { Name = new string('A', 200) };

        var exception = await Record.ExceptionAsync(() =>
            processor.Process(request, CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Process_WhenPersonWithSameNameExists_ThrowsBadHttpRequestException()
    {
        using var ctx = DbContextFactory.Create();
        ctx.People.Add(new Person { Name = "Alice" });
        await ctx.SaveChangesAsync();

        var processor = CreateProcessor(ctx);
        var request = new CreatePerson { Name = "Alice" };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            processor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task Process_WhenPersonDoesNotExist_CompletesSuccessfully()
    {
        using var ctx = DbContextFactory.Create();
        var processor = CreateProcessor(ctx);
        var request = new CreatePerson { Name = "Bob" };

        var exception = await Record.ExceptionAsync(() =>
            processor.Process(request, CancellationToken.None));

        Assert.Null(exception);
    }
}
