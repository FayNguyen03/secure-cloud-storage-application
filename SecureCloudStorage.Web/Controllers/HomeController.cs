using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

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
}
