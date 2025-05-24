namespace CoreAccess.WebAPI.Model;

public class AppSetting
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public bool IsEncrypted { get; set; }
}
