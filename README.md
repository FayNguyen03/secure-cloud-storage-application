# Secure Cloud Storage 

## Features:

[X] Users can upload and download (if they gain access) files securely

[X] Uploaded files are encrypted using ...

[] User Group Management

[X] Public-Key Certificate-Based Key Sharing

[ ] Integration with Cloud Storage APIs (Dropbox, Google Drive, Box, Office365)

[ ] Add Users Securely


## Project Structure (Pending)

```
SecureCloudStorage.sln
│
├── SecureCloudStorage.Web/              # MVC Web App
│   ├── Controllers/
|   |   ├── RegisterController.cs   #Register new user and add the new user to the database
|   |   ├── SigninController.cs     #Sign in to an existing user
|   |   ├── HomeController.cs       #Control the home page
|   |   └── FileController.cs       #Manage file-processing tasks (uploading, downloading, encrypting, decrypting)
│   ├── Models/
|   |   ├── ErrorViewModel.cs
|   |   ├── AdminViewModel.cs
|   |   └── EncryptedFileViewModel.cs
│   ├── Views/
│   │   ├── Home/
│   │   ├── Files/
|   |   |   ├── Upload.cshtml
|   |   |   └── 
│   │   └── Shared/
│   ├── wwwroot/                        
│   ├── appsettings.json
│   └── Program.cs
│
├── SecureCloudStorage.Application/     
|   ├── IEncryptionService.cs       # Interface for the Encryption Service
|   ├── CertificateGenerationService.cs # Interface for the Encryption Service
│   └── EncryptionService.cs           #Encryption and decryption uploaded and downloaded files
├── SecureCloudStorage.Infrastructure/   # Secured Storage
├── SecureCloudStorage.Domain/           
|   ├── Entities/  #Binding with the MySQL Database
|   |   ├── User.cs     #Binding with the User table (storing users' information)
|   |   ├── Encrypted.cs #Binding with the User table (storing users' information)
│   ├── FileMetadata.cs
│   └── UserCertificate.cs
└── SecureCloudStorage.Shared/           

```

## Cryptographic Design

1. Hybrid Encryption Approach

[ ] Symmetric encryption (e.g., AES-256) for encrypting files (fast and secure).

[ ] Asymmetric encryption (e.g., RSA or ECC) for encrypting the AES key per user using their public certificate.

2. Key Management System

Each registered user has:

[ ] A public/private key pair (e.g., using RSA)

[ ] A self-signed digital certificate

[ ] File encrypted with a random AES key 

AES key encrypted with each group member’s public key

[ ] Store metadata alongside encrypted file (encrypted AES keys for each group member, initialization vector)

## System Architecture


[X] Login System 

Group Management UI: Add/remove users and import/export public certificates

[ ] File Manager:

    [ ] Select files

Encrypt/upload to cloud

Download/decrypt from cloud

Key Manager:

Load/import public/private keys (PFX, PEM)

Show group members and cert fingerprints

☁️ Cloud Integration
Use official SDKs:

Dropbox: Dropbox.Api

Google Drive: Google.Apis.Drive.v3

OneDrive/Office365: Microsoft.Graph

Box: Box.V2


Unlike wwwroot, files in Infrastructure/Storage/ are not accessible via HTTP — good! That’s exactly what we want for certs and sensitive encrypted files.





