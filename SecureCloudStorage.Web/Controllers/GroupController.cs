using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using SecureCloudStorage.Application;
using SecureCloudStorage.Domain;
using SecureCloudStorage.Web.Models;
using SecureCloudStorage.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;


namespace SecureCloudStorage.Web.Controllers{

public class GroupController : Controller
{
    private readonly AppDbContext _context;
    private readonly AESKeyService _aes_key_service;

    private readonly IEncryptionService _encryption_service;
    public GroupController(AppDbContext context,  AESKeyService aes_key_service, IEncryptionService encryption_service)
    {
        _context = context;
        _aes_key_service = aes_key_service;
        _encryption_service = encryption_service;
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
    

        var emails = new List<string>();
        var adminemails = new List<string>();
        if(!String.IsNullOrWhiteSpace(model.MemberEmails)) emails = model.MemberEmails.Split(new[] {',', ';', '\n'}, StringSplitOptions.RemoveEmptyEntries).Select(emails =>emails.Trim()).ToList();
        if(!String.IsNullOrWhiteSpace(model.AdminEmails)) adminemails = model.AdminEmails.Split(new[] {',', ';', '\n'}, StringSplitOptions.RemoveEmptyEntries).Select(emails =>emails.Trim()).ToList();
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
    
    private async Task GiveFileAccessToNewGroupMembers(List<User> newUsers, int groupId)
    {
        var storageBase = Path.Combine(Directory.GetCurrentDirectory(), "../SecureCloudStorage.Infrastructure", "Storage");
    
        // Get all files that the group has access to
        var fileIds = _context.GroupFileAccesses
            .Where(gfa => gfa.GroupId == groupId)
            .Select(gfa => gfa.FileId)
            .ToList();

        foreach (var fileId in fileIds)
        {
            var file = _context.EncryptedFiles.FirstOrDefault(f => f.Id == fileId);
            if (file == null) continue;

            // Read metadata file
            var metadataPath = file.MetadataPath;
            if (!System.IO.File.Exists(metadataPath)) continue;

            var jsonString = await System.IO.File.ReadAllTextAsync(metadataPath);
            var metadata = JsonSerializer.Deserialize<FileMetadata>(jsonString);
            if (metadata == null || metadata.EncryptedKeys == null) continue;

            foreach (var user in newUsers)
            {
                if (_context.UserFileAccesses.Any(u => u.FileId == file.Id && u.UserId == user.Id))
                    continue; 

                // Encrypt AES key with the new user's public key
                var cert = new X509Certificate2(user.PublicKey);
                using var rsa = cert.GetRSAPublicKey();
                var encryptedKey = rsa.Encrypt(_encryption_service.LoadDecryptedAESKey(metadata.AesKeyPath, _aes_key_service.GetMasterKey()), RSAEncryptionPadding.OaepSHA256);

                _context.UserFileAccesses.Add(new UserFileAccess
                {
                    FileId = file.Id,
                    UserId = user.Id,
                    EncryptedAesKey = encryptedKey
                });

                // Also update metadata (optional but nice to keep them in sync)
                metadata.EncryptedKeys[user.Email] = encryptedKey;
            }

            // Save updated metadata
            await System.IO.File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata));
        }

        await _context.SaveChangesAsync();
    }

    [HttpGet]
    public IActionResult EditGroup(int id, bool? editName, bool? editEmails, bool? editAdmins){
        var group = _context.Groups.Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User).FirstOrDefault(u => u.Id == id);
        var name = group.Name;
        var members = group.GroupMembers.Select(user => user.User.Email).ToList();
        var admin = group.GroupMembers.Where(member => member.Admin == true).Select(user => user.User.Email).ToList();
        var currGroup = new EditGroupViewModel
                            {
                                GroupId = group.Id,
                                GroupName = name,
                                MemberEmails = string.Join(" ", members),
                                AdminEmails =  string.Join(" ", admin)
                            };
        ViewBag.EditName = editName ?? false;
        ViewBag.EditEmails = editEmails ?? false;
        ViewBag.EditAdmins = editAdmins ?? false;
        return View(currGroup);
    }

    [HttpPost]
    public async Task<IActionResult> EditGroup(int id, EditGroupViewModel model)
    {
        var group = _context.Groups.FirstOrDefault(g => g.Id == id);
        var curr = _context.Users.FirstOrDefault(g => g.Email == HttpContext.Session.GetString("Email"));
        var currUsers = _context.GroupMembers.Where(g => g.GroupId == id).Select(u => u.UserId).ToList();
        
        var currAdmin = _context.GroupMembers.Where(g => g.GroupId == id && g.Admin == true).Select(u => u.UserId).ToList();

        if (group == null)
        {
            ViewBag.error = "Group not found.";
            return View(model);
        }

        //if the member emails are changed
        var newEmails = new List<string>();

        //if the admin emails are changed
        var newAdminEmails = new List<string>();

        //change the Group Name
        if (!string.IsNullOrWhiteSpace(model.NewGroupName) && model.NewGroupName.Length != 0)
        {  group.Name =  model.NewGroupName;
            await _context.SaveChangesAsync();
        }

         if (!string.IsNullOrWhiteSpace(model.NewAdminEmails) && !string.IsNullOrEmpty(model.NewMemberEmails))
            newAdminEmails = newAdminEmails.Concat(
                model.NewAdminEmails.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
            ).ToList();
            newAdminEmails = newAdminEmails.Concat(new List<string>{curr.Email}).ToList();

        if (!string.IsNullOrWhiteSpace(model.NewMemberEmails) && !string.IsNullOrEmpty(model.NewMemberEmails)){
            newEmails = newEmails.Concat(
                model.NewMemberEmails.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
            ).ToList();
            newEmails = newEmails.Concat(new List<string>{curr.Email}).ToList();
            //remove users
            var removedUser = _context.GroupMembers.Where(u => u.GroupId == id && !newEmails.Contains(u.User.Email)).Select(u => u.UserId).ToList();
            var fileAccessedGroup = _context.GroupFileAccesses.Where(u => u.GroupId == id).Select(u => u.FileId).ToList();
            await _context.UserFileAccesses.Where(u => removedUser.Contains(u.UserId) && fileAccessedGroup.Contains(u.FileId)).ExecuteDeleteAsync();
            await _context.GroupMembers.Where(u => removedUser.Contains(u.UserId) && u.GroupId == id).ExecuteDeleteAsync();
            await _context.SaveChangesAsync();

            //add on users
            var newUsers = _context.Users.Where(u => newEmails.Contains(u.Email) && !currUsers.Contains(u.Id)).ToList();
            newUsers = newUsers.Concat(new List<User>{curr}).ToList();
            var foundEmails = newUsers.Select(u => u.Email).ToHashSet();
            var missingEmails = newEmails.Where(e => !foundEmails.Contains(e)).ToList();
            foreach(var user in newUsers){
                foreach(var file in fileAccessedGroup){
                    _context.UserFileAccesses.Add(new UserFileAccess{
                        FileId = file,
                        UserId = user.Id
                    });
                }    
                _context.GroupMembers.Add( new GroupMember{
                    UserId = user.Id,
                    GroupId = id,
                    Admin = newAdminEmails.Contains(user.Email)
                });
        
            }
            await _context.SaveChangesAsync();
        }
        

        var curMembers = _context.GroupMembers.Include(m => m.User).Where(g => g.GroupId == id).ToList();
        foreach(var member in curMembers){
            if (newAdminEmails.Contains(member.User.Email)){
                member.Admin = true;
            }
            else{
                member.Admin = false;
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("DisplayGroup");
    }

    
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var filesIdAccessed = await _context.GroupFileAccesses
            .Where(u => u.GroupId == id)
            .Select(u => u.FileId)
            .ToListAsync();

        var usersIdAccessed = await _context.GroupMembers
            .Where(u => u.GroupId == id)
            .Select(u => u.UserId)
            .ToListAsync();

        // Remove file access entries for this group
        var groupFileAccesses = await _context.GroupFileAccesses
            .Where(g => g.GroupId == id)
            .ToListAsync();
        _context.GroupFileAccesses.RemoveRange(groupFileAccesses);

        // Remove users from the group
        var userGroup = await _context.GroupMembers
            .Where(g => g.GroupId == id)
            .ToListAsync();
        _context.GroupMembers.RemoveRange(userGroup);

        // Remove user access to files that were accessed via this group
        var fileUserAccess = await _context.UserFileAccesses
            .Where(u => usersIdAccessed.Contains(u.UserId) && filesIdAccessed.Contains(u.FileId))
            .ToListAsync();
        _context.UserFileAccesses.RemoveRange(fileUserAccess);

        // Remove the group itself
        var group = await _context.Groups.FirstOrDefaultAsync(u => u.Id == id);
        if (group != null)
        {
            _context.Groups.Remove(group);
        }

        await _context.SaveChangesAsync();

        TempData["Message"] = "✅ Group and related access have been deleted.";
        return RedirectToAction("DisplayGroup");
    }

}
}
