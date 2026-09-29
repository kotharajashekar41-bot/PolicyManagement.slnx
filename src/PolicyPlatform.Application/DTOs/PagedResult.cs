namespace PolicyPlatform.Application.DTOs;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)Size);
}
