using SecureCloudStorage.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureCloudStorage.Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace SecureCloudStorage.Infrastructure;
public class AppDbContext: DbContext{
    public AppDbContext(DbContextOptions<AppDbContext>options): 
        base(options){}
//DbSet map a c# class to a database table
    public DbSet<User> Users {get; set;}

    public DbSet<EncryptedFile> EncryptedFiles {get; set;}

    public DbSet<UserFileAccess> UserFileAccesses {get; set;}

//define how the models map to the db schema
    protected override void OnModelCreating(ModelBuilder builder)
    {
        //use this line when we inherit from IdentityDbContext
        //tell EF how to build the user/role/claim tables
        base.OnModelCreating(builder);
        //EF Core that `UserFileAccess` table has a composite primary key, made of both `UserId` and `FileId` 
        builder.Entity<UserFileAccess>()
            .HasKey(x => new { x.UserId, x.FileId });
        //one-to-many relationship between EncryptedFile and UserFileAccess
        builder.Entity<UserFileAccess>()
            .HasOne(x => x.File)
            .WithMany(f => f.AccessList)
            .HasForeignKey(x => x.FileId);
        //one-to-many relationship between UserFileAccess and User

        builder.Entity<UserFileAccess>()
            .HasOne(x => x.User)
            .WithMany(u => u.FileAccesses)
            .HasForeignKey(x => x.UserId);      
        //	Maps foreign key and navigation for file access
    }
}
