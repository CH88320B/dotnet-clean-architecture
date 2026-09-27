namespace CleanArch.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToList(), Page, PageSize, TotalCount);
}

public enum SortDirection
{
    Asc,
    Desc,
}

/// <summary>Filtering, sorting and paging options for the product list endpoint.</summary>
public sealed record ProductQuery
{
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    /// <summary>Case-insensitive match on name or SKU.</summary>
    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public bool? InStock { get; init; }

    /// <summary>One of: name, price, stock, createdAt.</summary>
    public string SortBy { get; init; } = "createdAt";

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
