using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
//an API that supports user interface (UI) login functionality.
using Microsoft.EntityFrameworkCore;
namespace SecureCloudStorage.Domain;
[Table("User")]
public class User
{
    [Column("user_id")]
    public int Id { get; set; }
    [Column("email")]    
    public string Email { get; set; }
    [Column("first_name")]
    public string FirstName { get; set; }
    [Column("last_name")]
    public string LastName { get; set; }
    [Column("public_key")]
    public byte[] PublicKey { get; set; }
    [Column("password_acc")]
    public string Password {get; set;}
    public ICollection<UserFileAccess> FileAccesses { get; set; }
    public ICollection<GroupMember> GroupMembers { get; set; }

}