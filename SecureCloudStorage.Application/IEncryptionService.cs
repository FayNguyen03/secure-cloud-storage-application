using SecureCloudStorage.Domain;
namespace SecureCloudStorage.Application;

public interface IEncryptionService
{
    (byte[] EncryptedFile, FileMetadata Metadata) EncryptFile(byte[] fileData, List<UserCertificate> recipients);
    byte[] DecryptFile(byte[] encryptedData, byte[] initializationVector, byte[] encryptedAesKey, byte[] privateKey);
}
