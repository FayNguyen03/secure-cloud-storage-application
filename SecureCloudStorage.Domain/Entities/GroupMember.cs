using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace SecureCloudStorage.Domain;
[Table("UserMember")]
public class GroupMember
{
    [Column("group_id")]
    public int GroupId { get; set; }
    [Column("user_id")]    
    public string UserId { get; set; }
}