namespace SecureCloudStorage.Web.Models;

public class EditGroupViewModel
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }

    public string NewGroupName { get; set; }
    public string MemberEmails {get; set;}

    public string NewMemberEmails {get; set;}
    public string AdminEmails {get; set;}

    public string NewAdminEmails {get; set;}

}
