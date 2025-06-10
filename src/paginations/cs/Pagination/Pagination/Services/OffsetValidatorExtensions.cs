namespace Pagination.Services;

public static class OffsetValidatorExtensions
{
    public static IReadOnlyList<string> Validate(this OffsetPaginationRequest<CommentSortField, SortDirection> request)
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

        if (request.Ordering is null || request.Ordering.Length == 0)
        {
            list.Add("Ordering cannot be null or empty.");
        }
        else
        {
            foreach (var order in request.Ordering)
            {
                if (order.OrderBy == CommentSortField.None)
                {
                    list.Add("OrderBy cannot be None.");
                }

                if (order.Direction == SortDirection.None)
                {
                    list.Add("Direction cannot be None.");
                }
            }
        }

        return list;
    }
}