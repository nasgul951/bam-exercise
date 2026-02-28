using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests.Helpers;

internal static class DbContextFactory
{
    internal static StargateContext Create()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new StargateContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
