namespace Pagination.Services;

public static class CursorValidatorExtensions
{
    public static IReadOnlyList<string> Validate<TOrderBy>(this CursorPaginationRequest<TOrderBy> request)
        where TOrderBy : Enum
    {
        var list = new List<string>();

        if (request.Size <= 0)
        {
            list.Add("Size must be greater than 0.");
        }

        if (request.Size > 1000)
        {
            list.Add("Size cannot be greater than 1000.");
        }

        if (request.OrderBy != null && request.OrderBy.ToString() == "None")
        {
            list.Add("OrderBy cannot be None.");
        }

        if (request.Direction == SortDirection.None)
        {
            list.Add("Direction cannot be None.");
        }

        return list;
    }
}