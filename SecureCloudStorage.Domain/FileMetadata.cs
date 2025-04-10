using System.Text.Json.Serialization;

namespace SecureCloudStorage.Domain;

public class FileMetadata
{
    public string? FileName {get; set;}
    public byte[] InitializationVector {get; set;} //AES IV
    public Dictionary<string, byte[]> EncryptedKeys {get; set;} //Per-user encrypted AES keys
    public string AesKeyPath {get; set;} 
}
