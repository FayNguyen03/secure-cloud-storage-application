# secure-cloud-storage-application

## Features Implemented

Features to Implement
File Encryption/Decryption

User Group Management

Public-Key Certificate-Based Key Sharing

Integration with Cloud Storage APIs (Dropbox, Google Drive, Box, Office365)

Add/Remove Users Securely



## Project Structure (Pending)

```
SecureCloudStorage.sln
│
├── SecureCloudStorage.Web/              # MVC Web App
│   ├── Controllers/
|   |   ├── HomeController.cs
|   |   └── FileController.cs
│   ├── Models/
|   |   ├── ErrorViewModel.cs
|   |   └── EncryptedFileViewModel.cs
│   ├── Views/
│   │   ├── Home/
│   │   ├── Files/
|   |   |   ├── Upload.cshtml
│   │   └── Shared/
│   ├── wwwroot/                         # JS, CSS, Bootstrap, Uploaded files
|   |   └── uploads/
│   ├── appsettings.json
│   └── Program.cs
│
├── SecureCloudStorage.Application/     # Business logic (services, DTOs) 
|   ├── IEncryptionService.cs
│   └── EncryptionService.cs
├── SecureCloudStorage.Infrastructure/   # Cloud SDKs, crypto, certs
├── SecureCloudStorage.Domain/           # Core entities (FileMetadata, UserCert)
│   ├── FileMetadata.cs
│   └── UserCertificate.cs
├── SecureCloudStorage.Tests/            # Unit tests
└── SecureCloudStorage.Shared/           # Common helpers, extensions

```

## Cryptographic Design
1. Hybrid Encryption Approach

- Symmetric encryption (e.g., AES-256) for encrypting files (fast and secure).

Asymmetric encryption (e.g., RSA or ECC) for encrypting the AES key per user using their public certificate.

2. Key Management System
Each user has:

A public/private key pair (e.g., using RSA)

A digital certificate (self-signed or CA-issued)

File encrypted with a random AES key → AES key encrypted with each group member’s public key

Store metadata alongside encrypted file: e.g., encrypted AES keys for each group member

## System Architecture
📂 Client App (C# Desktop App – WPF/.NET)

Login System (optional for local credentials)

Group Management UI: Add/remove users and import/export public certificates

File Manager:

Select files

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

