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

        if (request.Order is null && request.Cursor is null)
        {
            list.Add("Order cannot be null if Cursor is null.");
        }
        
        return list;
    }
}