using MediatR;
using StargateAPI.Business.Common;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries;

public class GetLogs : PagedRequest, IRequest<GetLogsResult> { }

public class GetLogsHandler : IRequestHandler<GetLogs, GetLogsResult>
{
    private readonly StargateContext _context;
    public GetLogsHandler(StargateContext context) => _context = context;

    public async Task<GetLogsResult> Handle(GetLogs request, CancellationToken cancellationToken)
    {
        var query = _context.LogEntries.OrderByDescending(l => l.Timestamp);
        return new GetLogsResult
        {
            Logs = await query.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken)
        };
    }
}

public class GetLogsResult : BaseResponse
{
    public PagedResult<LogEntry> Logs { get; set; } = new();
}

