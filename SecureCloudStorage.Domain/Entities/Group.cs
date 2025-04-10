using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace SecureCloudStorage.Domain;
[Table("GroupMember")]
public class Group
{
    [Column("group_id")]
    public int Id { get; set; }
    [Column("group_name")]    
    public string Name { get; set; }
    public ICollection<GroupMember> GroupMembers { get; set; }
    public ICollection<GroupFileAccess> GroupAccessList { get; set; }
}