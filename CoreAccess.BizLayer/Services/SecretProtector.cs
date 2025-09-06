using Microsoft.AspNetCore.DataProtection;

namespace CoreAccess.BizLayer.Services;

public interface ISecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedData);
}

public class SecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private readonly IDataProtector _protector = provider.CreateProtector("CoreAccess.Settings.v1");

    public string Protect(string plaintext) => _protector.Protect(plaintext);
    public string Unprotect(string protectedData) => _protector.Unprotect(protectedData);
}