namespace SecureCloudStorage.Domain;

public class UserCertificate
{
    public string Email {get; set;}
    public string FullName {get; set;}
    public string Username {get; set;}
    public byte[] PublicKey {get; set;} 
    public byte[] PrivateKey {get; set;} 
}
