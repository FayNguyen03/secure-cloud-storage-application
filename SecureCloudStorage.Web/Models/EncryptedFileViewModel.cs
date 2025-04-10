using SecureCloudStorage.Domain;

namespace SecureCloudStorage.Web.Models;

public class EncryptedFileViewModel
{
    //C# representation of the file used to process or save the file
    public IFormFile File { get; set; }

    public string RecipientEmails {get; set;}

    public List<int> SelectedGroupIds { get; set; } = new();
    public List<Group> AvailableGroups { get; set; } = new();

}
