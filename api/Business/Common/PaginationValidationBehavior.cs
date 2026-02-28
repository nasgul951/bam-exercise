using MediatR;

namespace StargateAPI.Business.Common;

public class PaginationValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is PagedRequest paged)
        {
            if (paged.PageNumber < 1)
                throw new BadHttpRequestException("pageNumber must be greater than or equal to 1.");
            if (paged.PageSize < 1)
                throw new BadHttpRequestException("pageSize must be greater than or equal to 1.");
            if (paged.PageSize > PagedRequest.MaxPageSize)
                throw new BadHttpRequestException(
                    $"pageSize must not exceed {PagedRequest.MaxPageSize}.");
        }

        return next();
    }
}
