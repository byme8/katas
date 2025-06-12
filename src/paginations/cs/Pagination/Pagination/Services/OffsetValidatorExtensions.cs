namespace Pagination.Services;

public static class OffsetValidatorExtensions
{
    public static IReadOnlyList<string> Validate(this OffsetPaginationRequest<CommentSortField> request)
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

        if (request.OrderBy == CommentSortField.None)
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