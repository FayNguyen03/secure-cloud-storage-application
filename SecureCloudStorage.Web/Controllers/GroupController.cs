using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SecureCloudStorage.Application;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Web.Models;
using SecureCloudStorage.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace SecureCloudStorage.Web.Controllers{

public class GroupController : Controller
{
    private readonly AppDbContext _context;
    public GroupController(AppDbContext context)
    {
        _context = context;
    }
    //GET /Files/Upload
    //return the upload form to the user
    [HttpGet]
    public IActionResult AddGroup() => View();

    //POST /Files/Upload
    //handle the file submission when the user submits the upload form
    [HttpPost]
    public async Task<IActionResult> AddGroup(AddGroupViewModel model)
    {
        var curr_user =_context.Users
            .Where(u => u.Email == HttpContext.Session.GetString("Email"))
            .ToList();
        if (model.MemberEmails.Length == 0 || model.MemberEmails.Length == 0 )
            return View(model);

        var emails = model.MemberEmails.Split(new[] {',', ';', '\n'}, StringSplitOptions.RemoveEmptyEntries).Select(emails =>emails.Trim()).ToList();
        var adminemails = model.AdminEmails.Split(new[] {',', ';', '\n'}, StringSplitOptions.RemoveEmptyEntries).Select(emails =>emails.Trim()).ToList();
        //return error if the emails are not in the database
        var members = _context.Users
            .Where(u => emails.Contains(u.Email))
            .ToList();
        var admins = _context.Users
            .Where(u => adminemails.Contains(u.Email))
            .ToList();    
        members = members.Concat(curr_user).GroupBy(u => u.Id).Select(g=>g.First()).ToList();
        admins = admins.Concat(curr_user).GroupBy(u => u.Id).Select(g=>g.First()).ToList();
        foreach(var user in admins){
            if(!members.Contains(user)) admins.Remove(user);
        }
        var foundEmails = members.Select(r => r.Email).ToHashSet();
        var missingEmails = emails.Where(e => !foundEmails.Contains(e)).ToList();
        members = members.Concat(curr_user).
                                GroupBy(u => u.Email).
                                Select(g => g.First()).
                                ToList();
        if (missingEmails.Any())
        {
            ViewBag.MissingRecipients = "Emails not exist: " + string.Join(", ", missingEmails);
        }
        await _context.SaveChangesAsync();
        //Add a new group
        _context.Groups.Add(new Group{
                Name = model.GroupName
            });
        await _context.SaveChangesAsync();
        var currGroup = _context.Groups.Where(u => u.Name == model.GroupName).ToList()[0];
        foreach (var member in members)
        {
            _context.GroupMembers.Add(new GroupMember
                {
                    UserId = member.Id,
                    GroupId =  currGroup.Id,
                    Admin = admins.Contains(member)
                });
        }
        await _context.SaveChangesAsync();
        
        //show the upload successfully page
        
        return RedirectToAction("DisplayGroup");
        }

    public IActionResult DisplayGroup(){
        var curr_user = _context.Users
            .Where(u => u.Email == HttpContext.Session.GetString("Email"))
            .ToList();
    
        var memberList = _context.Groups.Include(g => g.GroupMembers).Select(group => new{
                        GroupName = group.Name,
                        Members = group.GroupMembers.Select(m => new{
                            First = m.User.FirstName, Last = m.User.LastName, Email = m.User.Email
                        })
        });
        var admin = _context.GroupMembers.Where(member => member.UserId == curr_user[0].Id).Select(group => group.GroupId).ToList();
        var memberEmails = new Dictionary<string, string>();
        foreach (var group in memberList){
            var tempString = "";
            foreach(var member in group.Members){
                tempString += member.First + " " + member.Last + " (" + member.Email + ")\n";  
            }
            memberEmails[group.GroupName] = tempString;
        }
        var groups = _context.Groups.Select(group => new DisplayGroupViewModel
                            {
                                GroupName = group.Name,
                                MemberEmails = memberEmails[group.Name],
                                Admin = admin.Contains(group.Id),
                                Id = group.Id
                            }).ToList();
        return View(groups);
    }
    
    public IActionResult EditGroup(int id){
        var curr_user = _context.Users
            .Where(u => u.Email == HttpContext.Session.GetString("Email"))
            .ToList();
    
        var memberList = _context.Groups.Include(g => g.GroupMembers).Select(group => new{
                        GroupName = group.Name,
                        Members = group.GroupMembers.Select(m => new{
                            First = m.User.FirstName, Last = m.User.LastName, Email = m.User.Email
                        })
        });
        var admin = _context.GroupMembers.Where(member => member.UserId == curr_user[0].Id).Select(group => group.GroupId).ToList();
        var memberEmails = new Dictionary<string, string>();
        foreach (var group in memberList){
            var tempString = "";
            foreach(var member in group.Members){
                tempString += member.First + " " + member.Last + " (" + member.Email + ")\n";  
            }
            memberEmails[group.GroupName] = tempString;
        }
        var groups = _context.Groups.Select(group => new DisplayGroupViewModel
                            {
                                GroupName = group.Name,
                                MemberEmails = memberEmails[group.Name],
                                Admin = admin.Contains(group.Id),
                                Id = group.Id
                            }).ToList();
        return View(groups);
    }
       
}
}
