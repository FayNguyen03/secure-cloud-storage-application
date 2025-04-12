using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using SecureCloudStorage.Domain;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
namespace SecureCloudStorage.Application;


public class EncryptionService: IEncryptionService{
    private readonly AESKeyService _aes_service;
    
    private readonly  IConfiguration configuration;
    private readonly string _secure_password ="Stolaf";
    public EncryptionService(AESKeyService aes_service){
        _aes_service = aes_service;
    }
    public (byte[] EncryptedFile, FileMetadata Metadata) EncryptFile(string fileName, byte[] fileData, List<User> recipients){
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

        var storageBase = Path.Combine(Directory.GetCurrentDirectory(), "../SecureCloudStorage.Infrastructure", "Storage");
        var aesKeyPath =  Path.Combine(storageBase, "aeskeys", $"{fileName}.enc");
        
        SaveEncryptedAESKey(aes.Key, aesKeyPath, _aes_service.GetMasterKey());

        return (ms.ToArray(), new FileMetadata{
            FileName = fileName,
            InitializationVector = aes.IV,
            EncryptedKeys = encryptedKeys,
            AesKeyPath = aesKeyPath       
        });
    }

    public byte[] DecryptAESKey(byte[] encryptedKey, byte[] privateKey)
    {
        var cert = new X509Certificate2(privateKey, _secure_password, X509KeyStorageFlags.Exportable);

        using var rsa = cert.GetRSAPrivateKey();
        return rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
    }

    public byte[]DecryptFile (byte[] encryptedData, byte[] initializationVector, byte[] encryptedAesKey, byte[] privateKey){
        using var aes = Aes.Create();
        aes.Key = DecryptAESKey(encryptedAesKey, privateKey);
        aes.IV = initializationVector;

        using var ms = new MemoryStream(encryptedData);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var output = new MemoryStream();
        cs.CopyTo(output);
        return output.ToArray();
    }

    public void SaveEncryptedAESKey(byte[] aesKey, string aesKeyFilePath, byte[] masterKey)
    {
        using var aes = Aes.Create();
        aes.Key = masterKey;
        aes.IV = new byte[16]; 

        using var encryptor = aes.CreateEncryptor();
        var encryptedKey = encryptor.TransformFinalBlock(aesKey, 0, aesKey.Length);

        Directory.CreateDirectory(Path.GetDirectoryName(aesKeyFilePath)!);
        using var fs = new FileStream(aesKeyFilePath, FileMode.Create, FileAccess.Write);
        fs.Write(aes.IV, 0, aes.IV.Length);
        fs.Write(encryptedKey, 0, encryptedKey.Length);
    }

    public byte[] LoadDecryptedAESKey(string aesKeyFilePath, byte[] masterKey)
    {
        var allBytes = System.IO.File.ReadAllBytes(aesKeyFilePath);
        var iv = allBytes.Take(16).ToArray();
        var encryptedKey = allBytes.Skip(16).ToArray();

        using var aes = Aes.Create();
        aes.Key = masterKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(encryptedKey, 0, encryptedKey.Length);
    }

}