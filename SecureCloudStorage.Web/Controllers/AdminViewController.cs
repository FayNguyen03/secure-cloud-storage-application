using Microsoft.AspNetCore.Mvc;
using System.IO;
using SecureCloudStorage.Application;
using SecureCloudStorage.Web.Models;

namespace SecureCloudStorage.Web.Controllers{
    public class AdminViewController: Controller{
        private readonly CertificateGenerationService _certGen;

        public AdminViewController(CertificateGenerationService certGen){
            _certGen = certGen;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(AdminRegisterViewMode model){
            var userCert = _certGen.GenerateUserCertificate(model.Email, model.FirstName, model.LastName);
            
            System.IO.File.WriteAllBytes($"wwwroot/certs/{model.Email}.cer", userCert.PublicKey);

            
            System.IO.File.WriteAllBytes($"wwwroot/certs-private/{model.Email}.pfx", userCert.PrivateKey);
            
            return RedirectToAction("RegisterSuccessfully");
        }
        public IActionResult RegisterSuccessfully() => View();
    }     
}

