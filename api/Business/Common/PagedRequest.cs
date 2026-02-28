namespace StargateAPI.Business.Common;

public abstract class PagedRequest
{
    public const int MaxPageSize = 100;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
