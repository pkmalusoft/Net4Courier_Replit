using Microsoft.AspNetCore.DataProtection;

namespace Net4Courier.Web.Services;

public interface ISecureStorageService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class SecureStorageService : ISecureStorageService
{
    private readonly IDataProtector _protector;

    public SecureStorageService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Net4Courier.ApiSettings.Secrets");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;
        
        try
        {
            return _protector.Protect(plainText);
        }
        catch
        {
            return plainText;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;
        
        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch
        {
            return cipherText;
        }
    }
}
