namespace SecureCloudStorage.Domain;
using System.ComponentModel.DataAnnotations.Schema;
[Table("UserFileAccess")]
public class UserFileAccess
{
    [Column("user_id")]
    public int UserId { get; set; }
    public User User { get; set; }
    [Column("file_id")]
    public int FileId { get; set; }
    public EncryptedFile File { get; set; }
    
    [Column("encrypted_aes")]
    public byte[] EncryptedAesKey { get; set; }
}
