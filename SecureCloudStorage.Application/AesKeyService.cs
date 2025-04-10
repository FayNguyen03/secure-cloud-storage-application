using Microsoft.Extensions.Configuration;
namespace  SecureCloudStorage.Application{

public class AESKeyService
{
    private readonly byte[] _masterKey;

    public AESKeyService(IConfiguration configuration)
    {
        var base64Key = configuration["Encryption:MasterKey"];
        _masterKey = Convert.FromBase64String(base64Key); 
    }

    public byte[] GetMasterKey() => _masterKey;
}
}