namespace SecureCloudStorage.Web.Models;

public class EncryptedFileViewModel
{
    //C# representation of the file used to process or save the file
    public IFormFile File { get; set; }

    public List<string> RecipientEmails {get; set;}

}
