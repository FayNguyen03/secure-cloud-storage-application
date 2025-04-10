using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace SecureCloudStorage.Domain;
[Table("UserMember")]
public class GroupMember
{
    [Column("group_id")]
    public int GroupId { get; set; }
    public Group Group { get; set; }
    [Column("user_id")]    
    public int UserId { get; set; }
    public User User { get; set; }
    [Column("admin")]  
    public bool Admin { get; set; }
}