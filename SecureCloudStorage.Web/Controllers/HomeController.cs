using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using SecureCloudStorage.Web.Controllers;
using SecureCloudStorage.Application;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Web.Models;
using SecureCloudStorage.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context){
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> WipeEverything()
    {
        // 1. Delete files from disk
        var storageBase = Path.Combine(Directory.GetCurrentDirectory(), "../SecureCloudStorage.Infrastructure", "Storage");

        var dirs = new[] { "uploads", "metadata", "aeskeys", "certs-private", "certs"};
        foreach (var dir in dirs)
        {
            var fullPath = Path.Combine(storageBase, dir);
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true); 
            }
        }

        _context.UserFileAccesses.RemoveRange(_context.UserFileAccesses);
        _context.GroupFileAccesses.RemoveRange(_context.GroupFileAccesses);
        _context.GroupMembers.RemoveRange(_context.GroupMembers);
        _context.EncryptedFiles.RemoveRange(_context.EncryptedFiles);
        _context.Groups.RemoveRange(_context.Groups);
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();
        //clear everything before reset
        await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `User` AUTO_INCREMENT = 1;");
        await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `GroupMember` AUTO_INCREMENT = 1;");
        await _context.Database.ExecuteSqlRawAsync("ALTER TABLE `EncryptedFile` AUTO_INCREMENT = 1;");
        HttpContext.Session.Remove("User");
        HttpContext.Session.Remove("Email");
        HttpContext.Session.Remove("Id");
        TempData["Message"] = "☢️ Everything has been wiped!";
        
        return RedirectToAction("Index");
    }
     [HttpPost]
    public IActionResult LogOut()
    {
        HttpContext.Session.Remove("User");
        HttpContext.Session.Remove("Email");
        HttpContext.Session.Remove("Id");
        return RedirectToAction("Index", "Home");

    }
    public IActionResult Privacy()
    {
        return View();
    }

   
    /*
    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file){
        if (file == null || file.Length == 0){
            ViewBag.Message = "No file selected.";
            return View();
        }
        var filePath = Path.Combine("wwwroot/uploads", file.FileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        ViewBag.Message = $"File {file.FileName} uploaded successfully!";
        return View();
    }
    */
}
