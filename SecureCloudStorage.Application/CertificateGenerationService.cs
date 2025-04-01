using SecureCloudStorage.Domain;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class CertificateGenerationService {
    public UserCertificate GenerateUserCertificate(string email, string firstName, string lastName){
        //Generate a new 2048-bit RSA keypair to sign the certificate and encrypt/decrypt AES keys
        using var rsa = RSA.Create(2048);
        //Create a distinguished name
        var subject = new X500DistinguishedName($"CN={firstName} {lastName}, E={email}");
        //Create a certificate request using the subject info (email + name), the RSA key, SHA265 has algorithm, and PKCS#1 padding
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        //Declare this is not a Certificate Authority and it is a regular end-user certificate for encryption and decryption, for issuing certs
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        //Issue the certifcate signed by itself (not provided by external CA
        //Valid for 1 month
        var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddMonths(1));
        //Public certificate: DER-encoded file that contains public key + metadata
        var publicBytes = cert.Export(X509ContentType.Cert);//.cer
        var privateBytes = cert.Export(X509ContentType.Pkcs12, "secure-password");//.pfx password-protected for security
        return new UserCertificate{
            FullName = firstName + lastName,
            Email = email,
            PublicKey = publicBytes,
            PrivateKey = privateBytes
        };
    }
}