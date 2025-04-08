using Microsoft.AspNetCore.Mvc;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Infrastructure;
using SecureCloudStorage.Web.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

public class SigninController : Controller
{
    private readonly AppDbContext _context;

    public SigninController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public IActionResult Signin() => View();

    [HttpPost]
    public IActionResult Signin(SigninViewModel model)
    {
        //find the first compatible user
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

        if (user == null)
        {
            ViewBag.error = "Invalid email or password";
            return View();
        }

        // Check password
        bool isPasswordValid = (model.Password == user.Password);

        if (!isPasswordValid)
        {
            ViewBag.error = "Invalid email or password";
            return View(model);
        }

        HttpContext.Session.SetString("User", user.FirstName);
        HttpContext.Session.SetString("Email", user.Email);
        HttpContext.Session.SetInt32("Id", user.Id);

        return RedirectToAction("Index", "Home");
    }


    
}
