namespace SecureCloudStorage.Domain;
using System.ComponentModel.DataAnnotations.Schema;

[Table("EncryptedFile")]
public class EncryptedFile
{
    [Column("file_id")]
    public int Id { get; set; }
    [Column("file_name")]
    public string FileName { get; set; }
    [Column("encrypted_path")]
    public string EncryptedPath { get; set; }
    [Column("meta_data")]
    public string MetadataPath { get; set; }
    [Column("upload_ad")]
    public DateTime UploadedAt { get; set; }
    [Column("uploader_id")]
    public int UploaderId { get; set; }
    
    public User Uploader { get; set; }
    public ICollection<UserFileAccess> AccessList { get; set; }
}
