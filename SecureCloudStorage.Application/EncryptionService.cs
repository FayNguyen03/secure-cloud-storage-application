using SecureCloudStorage.Domain;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
namespace SecureCloudStorage.Application;

public class EncryptionService: IEncryptionService{
    public (byte[] EncryptedFile, FileMetadata Metadata) EncryptFile(byte[] fileData, List<User> recipients){
        using var aes = Aes.Create();
        
        aes.GenerateIV();
        aes.GenerateKey();

        //Encrypt the file 
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(fileData, 0, fileData.Length);
        cs.FlushFinalBlock();

        //Encrypt RSA key for each user
        var encryptedKeys = new Dictionary<string, byte[]>();
        foreach (var user in recipients){
            var cert = new X509Certificate2(user.PublicKey);
            using var rsa = cert.GetRSAPublicKey();
            var encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);
            encryptedKeys[user.Email] = encryptedKey;
        }

        return (ms.ToArray(), new FileMetadata{
            FileName = "encryptedFile.dat",
            InitializationVector = aes.IV,
            EncryptedKeys = encryptedKeys
        });
    }

    public byte[]DecryptFile (byte[] encryptedData, byte[] initializationVector, byte[] encryptedAesKey, byte[] privateKey){
        return null;
    }
}