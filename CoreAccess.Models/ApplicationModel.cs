namespace CoreAccess.Models;

public class ApplicationSearchOptions
{
    public string Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ApplicationDto
{
    public string Id { get; set; }
    public string ClientId { get; set; }
    public string DisplayName { get; set; }
    public string ClientType { get; set; }
    public string? RedirectUris { get; set; }
    public string? PostLogoutRedirectUris { get; set; }
}

public class ApplicationDetailDto : ApplicationDto
{
    public string ApplicationType { get; set; }
    public string ConsentType { get; set; }
    public string ClientSecret { get; set; }
    public string? Permissions { get; set; }
    public string? Requirements { get; set; }
}

