using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SecureCloudStorage.Application;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Web.Models;


namespace SecureCloudStorage.Web.Controllers{

public class FileController : Controller
{
    private readonly IEncryptionService _encryptionService;

    public FileController(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }
    //GET /Files/Upload
    //return the upload form to the user
    [HttpGet]
    public IActionResult Upload() => View();

    //POST /Files/Upload
    //handle the file submission when the user submits the upload form
    [HttpPost]
    public async Task<IActionResult> Upload(EncryptedFileViewModel model)
    {
        //check whether a file is submitted and a recipient email is inputted
        if (model.File == null || model.RecipientEmails.Count == 0)
            return View(model);

        //read file into bytes
        using var ms = new MemoryStream();
        await model.File.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        //for each recipient, load their public key from a .cer file stored in the certs/ folder then create a UserCertificate object
        var recipients = model.RecipientEmails.Select(email => new UserCertificate
        {
            Email = email,
            PublicKey = System.IO.File.ReadAllBytes($"wwwroot/certs/{email}.cer")
        }).ToList();

        //encrypt the file and write it into .enc file
        var (encryptedFile, metadata) = _encryptionService.EncryptFile(fileBytes, recipients);
        System.IO.File.WriteAllBytes($"wwwroot/uploads/{model.File.FileName}.enc", encryptedFile);
        //write metadata (IV + per-user keys) as .meta.json
        System.IO.File.WriteAllText($"wwwroot/uploads/{model.File.FileName}.meta.json", JsonSerializer.Serialize(metadata));
        //show the upload successfully page
        
        return RedirectToAction("UploadSuccess");
    }

    public IActionResult UploadSuccess() => View();
}

}