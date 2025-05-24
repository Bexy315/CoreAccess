namespace CoreAccess.WebAPI.Model;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public static class CoreAccessClaimType
{
    public const string UserId = "coreaccess:user_id";
    public const string UserName = "coreaccess:username";
    public const string Role = "coreaccess:role";
    public const string TokenId = "coreaccess:token_id";
}