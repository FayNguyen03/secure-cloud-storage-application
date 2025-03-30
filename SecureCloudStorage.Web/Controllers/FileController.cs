using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SecureCloudStorage.Application;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Web.Models;


namespace SecureCloudStorage.Web.Controllers;

public class FilesController : Controller
{
    private readonly IEncryptionService _encryptionService;

    public FilesController(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(EncryptedFileViewModel model)
    {
        if (model.File == null || model.RecipientEmails.Count == 0)
            return View(model);

        using var ms = new MemoryStream();
        await model.File.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        var recipients = model.RecipientEmails.Select(email => new UserCertificate
        {
            Email = email,
            PublicKey = System.IO.File.ReadAllBytes($"certs/{email}.cer")
        }).ToList();

        var (encryptedFile, metadata) = _encryptionService.EncryptFile(fileBytes, recipients);

        System.IO.File.WriteAllBytes($"uploads/{model.File.FileName}.enc", encryptedFile);
        System.IO.File.WriteAllText($"uploads/{model.File.FileName}.meta.json", JsonSerializer.Serialize(metadata));

        return RedirectToAction("UploadSuccess");
    }

    public IActionResult UploadSuccess() => View();
}

