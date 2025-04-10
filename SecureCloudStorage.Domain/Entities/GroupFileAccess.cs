using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace SecureCloudStorage.Domain;

[Table("GroupFileAccess")]

public class GroupFileAccess
{
    [Column("group_id")]
    public int GroupId { get; set; }
    public Group Group { get; set; }
    [Column("file_id")]    
    public int FileId { get; set; }
    public EncryptedFile File{get; set;}
}