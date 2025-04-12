# Secure Cloud Storage 

## Features:

### Security Features:

- [X] AES Encryption for Files: Files are encrypted using AES before being stored.

- [X] RSA Encryption for Keys: AES keys are encrypted per user using each user’s public key (X.509 certificate).

- [X] Metadata Management: Each file has a metadata file storing the IV and encrypted AES keys per user.

- [X] Master Key Encryption: AES keys are additionally encrypted and stored using a master key for backup and adding new members purpose.

### User Access Management Features:

- [X] User Accounts: Users can sign in and have unique identity through their email.

- [X] User-Based File Access: File can be shared with an individual so this individual will have an encrypted AES key for future decryption.

### Group Access Management Features:

- [X] Group Creation: Admins can create named groups with selected members.

- [X] Group Admin Role: Admins have edit and delete privileges within their group.

- [X] Add/Remove Group Members: Edit group members dynamically.

- [X] Group-Based File Access: Files can be shared with entire groups, not just individuals.

- [X] Dynamic Key Distribution: When a new user is added to a group, AES keys for all previously shared files are encrypted for the new user automatically.

### Group Access Management Features:

- [X] File Upload: Upload files through a form.

- [X] Multi-Recipient Access: Assign access to individual users or entire groups.

- [X] File Encryption and Storage: Files are stored encrypted on the server-side storage.

- [X] Download Access Control: Only authorized users can decrypt and download files.

- [X] Unauthorized Access Protection: Unauthorized users can only see the files or download the encrypted version.


## Project Structure 

```
SecureCloudStorage.sln
│
├── SecureCloudStorage.Web/             
│   ├── Controllers/
|   |   ├── RegisterController.cs   #Control the registration for new user as well as the generation of the user certificates
|   |   ├── SigninController.cs     #Control the sign in actions
|   |   ├── HomeController.cs       #Control the home page and the navigation bar
|   |   ├── GroupController.cs      #Control the group-scoped activities (creating new groups, adding or removing members, providing file access to a group, ...)
|   |   └── FileController.cs       #Manage file-processing tasks (uploading, downloading, encrypting, decrypting, ...)
│   ├── Models/
|   |   ├── ErrorViewModel.cs
|   |   ├── AdminViewModel.cs
|   |   └── EncryptedFileViewModel.cs
│   ├── Views/      #User Interface
│   │   ├── Home/
|   |   |   ├── Index.cshtml
|   |   |   └── Privacy.cshtml
│   │   ├── Files/
|   |   |   ├── DisplayFiles.cshtml
|   |   |   ├── Upload.cshtml
|   |   |   └── UploadSuccessfully.cshtml
|   |   ├── Group/
|   |   |   ├── AddGroup.cshtml
|   |   |   ├── DeleteGroup.cshtml
|   |   |   ├── DisplayGroup.cshtml
|   |   |   └── EditGroup.cshtml
|   |   ├── Register/
|   |   |   ├── Register.cshtml
|   |   |   └── RegisterSuccessfully.cshtml
|   |   ├── Signin/
|   |   |   └── Signin.cshtml
│   │   └── Shared/
|   |   |   ├── _Layout.cshtml
|   |   |   ├── _Layout.cshtml.css
|   |   |   ├── Error.cshtml.css
|   |   |   └── _ValidationScriptsPartial.cshtml
│   ├── wwwroot/                        
│   ├── appsettings.json
│   ├── appsettings.Develoipemnt.json
│   └── Program.cs
|
├── SecureCloudStorage.Application/     
|   ├── IEncryptionService.cs      
|   ├── EncryptionService.cs  
|   ├── AesKeyService.cs
│   └── CertificateGenerationService.cs   
|
├── SecureCloudStorage.Infrastructure/   #Secured Storage Accessed Only from Server Side
|   ├── Data/
|   |   └── AppDbContext.cs        
|   ├── Storage/ 
|   |   ├── uploads/    #Store encrypted version of the uploaded file
|   |   ├── certs/      #Store users' certificates + public keys + metadata
|   |   ├── certs-private/ #Store users' password-secured private keys
|   |   ├── metada/ #Store users' password-secured private keys
│   └── CertificateGenerationService.cs   
| 
└──  SecureCloudStorage.Domain/           
    ├── Entities/  #Binding with the MySQL Database
    |   ├── User.cs     #Binding with the User table (storing users' information)
    |   ├── EncryptedFile.cs #Binding with the EncryptedFile table (storing encrypted uploaded files' information - file name, uploader, time)
    |   ├── UserFileAccess.cs
    |   ├── Group.cs    #Binding with the GroupMember table (storing groups' information)
    |   ├── GroupMember.cs  #Binding with the UserMember table (linking users with their groups and indicating whether a user has admin right or not)
    |   ├── GroupFileAccess.cs #Binding with the GroupFileAccess table (linking groups with the files that all members can access)
    ├── FileMetadata.cs
    └── UserCertificate.cs
```

## Cryptographic Design

1. Hybrid Encryption Approach

- [X] Symmetric encryption (AES-256) for encrypting files (fast and secure).

- [X] Asymmetric encryption (RSA) for encrypting the AES key per user using their public certificate.

2. Key Management System

Each registered user has:

- [X] A public/private key pair (e.g., using RSA)

- [X] A self-signed digital certificate (securely stored at the server-side)

- [X] File encrypted with a random but consistent AES key 

- [X] Store metadata alongside encrypted file (encrypted AES keys for each group member, initialization vector)



## Note
Check password that generates .pfx: `openssl pkcs12 -in yourfile.pfx -info -nokeys`
