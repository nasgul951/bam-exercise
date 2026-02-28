using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using StargateAPI.Business.Common;
using StargateAPI.Business.Queries;

namespace StargateAPI.Tests.Business.Common;

public class PaginationValidationBehaviorTests
{
    private readonly PaginationValidationBehavior<GetPeople, GetPeopleResult> _behavior = new();

    private static RequestHandlerDelegate<GetPeopleResult> NextDelegate() =>
        () => Task.FromResult(new GetPeopleResult());

    [Fact]
    public async Task Handle_WhenRequestIsNotPagedRequest_CallsNext()
    {
        // Use a non-paged behavior instance; we use a stub IRequest<Unit>
        var behavior = new PaginationValidationBehavior<NonPagedStub, Unit>();
        var called = false;
        RequestHandlerDelegate<Unit> next = () => { called = true; return Task.FromResult(Unit.Value); };

        await behavior.Handle(new NonPagedStub(), next, CancellationToken.None);

        Assert.True(called);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WhenPageNumberIsInvalid_ThrowsBadHttpRequestException(int pageNumber)
    {
        var request = new GetPeople { PageNumber = pageNumber, PageSize = 10 };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            _behavior.Handle(request, NextDelegate(), CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WhenPageSizeIsInvalid_ThrowsBadHttpRequestException(int pageSize)
    {
        var request = new GetPeople { PageNumber = 1, PageSize = pageSize };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            _behavior.Handle(request, NextDelegate(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPageSizeExceedsMaximum_ThrowsBadHttpRequestException()
    {
        var request = new GetPeople { PageNumber = 1, PageSize = PagedRequest.MaxPageSize + 1 };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            _behavior.Handle(request, NextDelegate(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPageSizeEqualsMaximum_CallsNext()
    {
        var request = new GetPeople { PageNumber = 1, PageSize = PagedRequest.MaxPageSize };
        var called = false;
        RequestHandlerDelegate<GetPeopleResult> next = () =>
        {
            called = true;
            return Task.FromResult(new GetPeopleResult());
        };

        await _behavior.Handle(request, next, CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CallsNext()
    {
        var request = new GetPeople { PageNumber = 1, PageSize = 10 };
        var called = false;
        RequestHandlerDelegate<GetPeopleResult> next = () =>
        {
            called = true;
            return Task.FromResult(new GetPeopleResult());
        };

        await _behavior.Handle(request, next, CancellationToken.None);

        Assert.True(called);
    }

    // Stub for the non-paged test
    private class NonPagedStub : IRequest<Unit> { }
}
