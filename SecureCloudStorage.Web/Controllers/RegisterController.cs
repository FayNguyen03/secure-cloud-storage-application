using Microsoft.AspNetCore.Mvc;
using System.IO;
using SecureCloudStorage.Application;
using SecureCloudStorage.Web.Models;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace SecureCloudStorage.Web.Controllers{
    public class RegisterController: Controller{
        private readonly CertificateGenerationService _certGen;
        private readonly AppDbContext _context;

        public RegisterController(CertificateGenerationService certGen, AppDbContext context){
            _certGen = certGen;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model){

            var storageBase = Path.Combine(Directory.GetCurrentDirectory(), "../SecureCloudStorage.Infrastructure", "Storage");
            
            var certPath = Path.Combine(storageBase, "certs", $"{model.Email}.cer");
            
            var certPrivacyPath = Path.Combine(storageBase, "certs-private", $"{model.Email}.pfx");

            var userCert = _certGen.GenerateUserCertificate(model.Email, model.FirstName, model.LastName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(certPath)!);

            System.IO.File.WriteAllBytes(certPath, userCert.PublicKey);

            Directory.CreateDirectory(Path.GetDirectoryName(certPrivacyPath)!);

            System.IO.File.WriteAllBytes(certPrivacyPath, userCert.PrivateKey);
            
            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PublicKey = userCert.PublicKey,
                Password = model.Password
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("RegisterSuccessfully");
        }
        public IActionResult RegisterSuccessfully() => View();
    }     
}

