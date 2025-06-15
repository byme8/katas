namespace Pagination.Services;

public static class OffsetValidatorExtensions
{
    public static IReadOnlyList<string> Validate<TOrderBy>(this OffsetPaginationRequest<TOrderBy> request)
        where TOrderBy : Enum
    {
        var list = new List<string>();
        
        if (request.Page <= 0)
        {
            list.Add("Page must be greater than 0.");
        }

        if (request.Size <= 0)
        {
            list.Add("Size must be greater than 0.");
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