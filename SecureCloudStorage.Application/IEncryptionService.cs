using SecureCloudStorage.Domain;
namespace SecureCloudStorage.Application;

public interface IEncryptionService
{
    (byte[] EncryptedFile, FileMetadata Metadata) EncryptFile(byte[] fileData, List<User> recipients);

    public byte[] DecryptAESKey(byte[] encryptedAesKey, byte[]  privateKey);
    byte[] DecryptFile(byte[] encryptedData, byte[] initializationVector, byte[] encryptedAesKey, byte[] privateKey);
}
