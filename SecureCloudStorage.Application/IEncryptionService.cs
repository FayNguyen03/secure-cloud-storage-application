using SecureCloudStorage.Domain;
namespace SecureCloudStorage.Application;

public interface IEncryptionService
{
    (byte[] EncryptedFile, FileMetadata Metadata) EncryptFile(string fileName, byte[] fileData, List<User> recipients);

    public byte[] DecryptAESKey(byte[] encryptedAesKey, byte[]  privateKey);
    byte[] DecryptFile(byte[] encryptedData, byte[] initializationVector, byte[] encryptedAesKey, byte[] privateKey);

    public byte[] LoadDecryptedAESKey(string aesKeyFilePath, byte[] masterKey);

    public void SaveEncryptedAESKey(byte[] aesKey, string aesKeyFilePath, byte[] masterKey);
}
