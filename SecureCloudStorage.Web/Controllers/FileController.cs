using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SecureCloudStorage.Application;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Web.Models;
using SecureCloudStorage.Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace SecureCloudStorage.Web.Controllers{

public class FileController : Controller
{
    private readonly IEncryptionService _encryptionService;
    private readonly AppDbContext _context;
    public FileController(IEncryptionService encryptionService, AppDbContext context)
    {
        _encryptionService = encryptionService;
        _context = context;
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
        var uploader = _context.Users
            .Where(u => u.Email == HttpContext.Session.GetString("Email")).ToList();

        var storageBase = Path.Combine(Directory.GetCurrentDirectory(), "../SecureCloudStorage.Infrastructure", "Storage");

        //check whether a file is submitted and a recipient email is inputted
        if (model.Files.Count == 0 || model.RecipientEmails.Length == 0)
            return View(model);

        var emails = model.RecipientEmails.Split(new[] {',', ';', '\n'}, StringSplitOptions.RemoveEmptyEntries).Select(emails =>emails.Trim()).ToList();
        //return error if the emails are not in the database
        var recipients = _context.Users
            .Where(u => emails.Contains(u.Email))
            .ToList();
        var foundEmails = recipients.Select(r => r.Email).ToHashSet();
        var missingEmails = emails.Where(e => !foundEmails.Contains(e)).ToList();
        recipients = recipients.Concat(uploader).
                                GroupBy(u => u.Email).
                                Select(g => g.First()).
                                ToList();
        if (missingEmails.Any())
        {
            ViewBag.MissingRecipients = string.Join(", ", missingEmails);
        }
        foreach (var file in model.Files){

            var filePath = Path.Combine(storageBase, "uploads", $"{file.FileName}.enc");
            
            var metadataPath = Path.Combine(storageBase, "metadata", $"{file.FileName}.meta.json");
            //read file into bytes
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            
            var (encryptedFile, metadata) = _encryptionService.EncryptFile(fileBytes, recipients);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            //encrypt the file and write it into .enc file
            System.IO.File.WriteAllBytes(filePath, encryptedFile);

            Directory.CreateDirectory(Path.GetDirectoryName(metadataPath)!);
            //write metadata (IV + per-user keys) as .meta.json
            System.IO.File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata));
            var encryptedFileEntry = new EncryptedFile
            {
                FileName = file.FileName,
                EncryptedPath = filePath,
                MetadataPath = metadataPath,
                UploadedAt = DateTime.UtcNow,
                UploaderId = (int)uploader[0].Id
            };
            _context.EncryptedFiles.Add(encryptedFileEntry);
            await _context.SaveChangesAsync();
            foreach (var recipient in recipients)
            {
                _context.UserFileAccesses.Add(new UserFileAccess
                    {
                        UserId = recipient.Id,
                        FileId = encryptedFileEntry.Id,
                        EncryptedAesKey = metadata.EncryptedKeys[recipient.Email]
                    });
        }
            await _context.SaveChangesAsync();
            }
        //show the upload successfully page
        
        return RedirectToAction("UploadSuccessfully");
    }

    public IActionResult DisplayFiles(){
        var userId = HttpContext.Session.GetInt32("UserID");
        var files = _context.EncryptedFiles.Include(f => f.Uploader).
                                            Include(f => f.AccessList).
                                            Select(file => new FileDisplayViewModel
                                            {
                                                FileId = file.Id,
                                                FileName = file.FileName,
                                                UploadedAt = file.UploadedAt,
                                                UploaderName = file.Uploader.FirstName + " " + file.Uploader.LastName,
                                                Downloadable = (userId != null) && file.AccessList.Any(a => a.UserId == userId)
                                            }).ToList();
        return View(files);
    }
    public IActionResult UploadSuccessfully() => View();
}

}