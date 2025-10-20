# 📦 KennyGPT Deployment Guide

Complete guide for deploying the KennyGPT application to production environments.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Backend Deployment](#backend-deployment)
- [Mobile App Deployment](#mobile-app-deployment)
- [Web Frontend Deployment](#web-frontend-deployment)
- [Post-Deployment Checklist](#post-deployment-checklist)
- [Troubleshooting](#troubleshooting)

---

## 🌟 Overview

KennyGPT consists of three deployable components:
1. **Backend API** - ASP.NET Core hosted on Azure App Service
2. **Mobile App** - .NET MAUI apps for Android, iOS, Windows, macOS
3. **Web Frontend** - Static web app hosted on Azure Static Web Apps

---

## ✅ Prerequisites

### General Requirements
- Azure subscription ([Get free account](https://azure.microsoft.com/free/))
- GitHub account (for CI/CD)
- Visual Studio 2022 (17.8+)
- .NET 9 SDK installed

### For Mobile Deployment
- **Android**: Android SDK, signing keystore
- **iOS**: macOS with Xcode, Apple Developer account
- **Windows**: Windows 10 SDK, developer certificate
- **macOS**: Xcode, Apple Developer account

---

## 🚀 Backend Deployment

### Option 1: Deploy via Visual Studio (Recommended)

#### Step 1: Prepare Project
```bash
cd KennyGPT
dotnet restore
dotnet build -c Release
```

#### Step 2: Publish from Visual Studio
1. Open `KennyGPT.sln` in Visual Studio 2022
2. Right-click `KennyGPT` project → **Publish**
3. Click **Add a publish profile**
4. Select **Azure → Azure App Service (Windows)**
5. Sign in to your Azure account
6. Create new App Service or select existing:
   - **Name**: `kennygpt` (or your preferred name)
   - **Resource Group**: Create new or select existing
   - **Hosting Plan**: Choose appropriate tier (B1 or higher)
   - **Region**: Select closest to your users
7. Click **Create**
8. Once profile is created, click **Publish**

#### Step 3: Configure App Settings
After deployment, add application settings in Azure Portal:

1. Go to **Azure Portal → App Services → kennygpt**
2. Navigate to **Configuration → Application settings**
3. Add the following settings:

| Name | Value | Type |
|------|-------|------|
| `AzureOpenAI__Endpoint` | `https://your-resource.openai.azure.com/` | Application setting |
| `AzureOpenAI__ApiKey` | Your Azure OpenAI API key | Application setting |
| `AzureOpenAI__DeploymentName` | Your deployment name (e.g., `kenny-4o`) | Application setting |
| `ConnectionStrings__DefaultConnection` | Your Azure SQL connection string | Connection string |
| `SerpApi__ApiKey` | Your SerpAPI key (optional) | Application setting |

4. Click **Save** and **Continue**

### Option 2: Deploy via Azure CLI

```bash
# Login to Azure
az login

# Set variables
RESOURCE_GROUP="KennyGPT-RG"
APP_SERVICE_PLAN="KennyGPT-Plan"
WEB_APP_NAME="kennygpt"
LOCATION="eastus"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Create App Service plan
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux false

# Create web app
az webapp create \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNET|9.0"

# Configure app settings
az webapp config appsettings set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    AzureOpenAI__Endpoint="https://your-resource.openai.azure.com/" \
    AzureOpenAI__ApiKey="your-api-key" \
    AzureOpenAI__DeploymentName="kenny-4o"

# Deploy code
cd KennyGPT
dotnet publish -c Release -o ./publish
cd publish
zip -r ../publish.zip .
cd ..

az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $WEB_APP_NAME \
  --src publish.zip
```

### Option 3: Deploy via GitHub Actions

Create `.github/workflows/deploy-backend.yml`:

```yaml
name: Deploy Backend to Azure

on:
  push:
    branches: [ main ]
    paths:
      - 'KennyGPT/**'
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore KennyGPT/KennyGPT.csproj
    
    - name: Build
      run: dotnet build KennyGPT/KennyGPT.csproj -c Release --no-restore
    
    - name: Publish
      run: dotnet publish KennyGPT/KennyGPT.csproj -c Release -o ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'kennygpt'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

**Setup:**
1. In Azure Portal, download the publish profile for your App Service
2. In GitHub: Settings → Secrets and variables → Actions
3. Add new secret: `AZURE_WEBAPP_PUBLISH_PROFILE` with the downloaded profile content

### Database Migration

```bash
# Before first deployment, apply migrations
dotnet ef database update --project KennyGPT

# Or enable auto-migration (already configured in Program.cs)
# Migrations will run automatically on app startup
```

---

## 📱 Mobile App Deployment

### Android Deployment

#### Step 1: Create Signing Keystore
```bash
keytool -genkey -v -keystore kennygpt.keystore \
  -alias kennygpt \
  -keyalg RSA \
  -keysize 2048 \
  -validity 10000

# Save the keystore password securely!
```

#### Step 2: Build Release APK
```bash
cd MauiGPT

# Build release APK
dotnet publish MauiGPT.csproj \
  -c Release \
  -f net9.0-android \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=../kennygpt.keystore \
  -p:AndroidSigningKeyAlias=kennygpt \
  -p:AndroidSigningKeyPass="YOUR_KEYSTORE_PASSWORD" \
  -p:AndroidSigningStorePass="YOUR_KEYSTORE_PASSWORD"

# APK will be in: bin/Release/net9.0-android/publish/
```

#### Step 3: Upload to Google Play Console
1. Go to [Google Play Console](https://play.google.com/console)
2. Create new app or select existing
3. Navigate to **Production → Create new release**
4. Upload the signed APK
5. Complete store listing:
   - App name: **KennyGPT**
   - Short description: "AI-powered chat assistant"
   - Full description: (Use content from README)
   - Screenshots: Take from running app
   - Feature graphic: 1024x500px banner
6. Submit for review

### iOS Deployment

#### Prerequisites
- macOS with Xcode installed
- Apple Developer Program membership ($99/year)
- Valid iOS Distribution Certificate
- App Store Provisioning Profile

#### Step 1: Configure Signing
1. Open `MauiGPT.csproj` in Visual Studio for Mac
2. Right-click project → **Options**
3. Navigate to **Build → iOS Bundle Signing**
4. Select **Distribution** configuration
5. Choose your signing identity and provisioning profile

#### Step 2: Archive App
```bash
# In Visual Studio for Mac:
# Build → Archive for Publishing

# Or via command line:
dotnet publish MauiGPT.csproj \
  -c Release \
  -f net9.0-ios \
  -p:ArchiveOnBuild=true \
  -p:RuntimeIdentifier=ios-arm64
```

#### Step 3: Submit to App Store
1. Open **Xcode → Window → Organizer**
2. Select your archive
3. Click **Distribute App**
4. Choose **App Store Connect**
5. Upload archive
6. Complete App Store listing in [App Store Connect](https://appstoreconnect.apple.com/)

### Windows Deployment

#### Step 1: Create MSIX Package
```bash
cd MauiGPT

# Create MSIX package
dotnet publish MauiGPT.csproj \
  -c Release \
  -f net9.0-windows10.0.19041.0 \
  -p:RuntimeIdentifierOverride=win10-x64 \
  -p:Platform=x64 \
  -p:GenerateAppxPackageOnBuild=true
```

#### Step 2: Sign Package
```bash
# Create self-signed certificate (for testing)
New-SelfSignedCertificate -Type Custom \
  -Subject "CN=KennyGPT" \
  -KeyUsage DigitalSignature \
  -FriendlyName "KennyGPT Certificate" \
  -CertStoreLocation "Cert:\CurrentUser\My"

# Sign the MSIX
signtool sign /fd SHA256 /a /f YourCertificate.pfx \
  /p YourPassword \
  bin/Release/net9.0-windows10.0.19041.0/win10-x64/MauiGPT.msix
```

#### Step 3: Submit to Microsoft Store
1. Go to [Microsoft Partner Center](https://partner.microsoft.com/dashboard)
2. Create new app submission
3. Upload MSIX package
4. Complete store listing
5. Submit for certification

### macOS Deployment

Similar to iOS deployment, requires:
- Apple Developer Program membership
- macOS Distribution Certificate
- Mac App Store Provisioning Profile

```bash
dotnet publish MauiGPT.csproj \
  -c Release \
  -f net9.0-maccatalyst \
  -p:CreatePackage=true
```

---

## 🌐 Web Frontend Deployment

The web frontend is already deployed on Azure Static Web Apps. To update:

### Option 1: Via GitHub (Automatic)
1. Push changes to `main` branch
2. GitHub Actions will automatically deploy
3. Check deployment status in Actions tab

### Option 2: Manual Deploy via Azure CLI
```bash
# Install Azure Static Web Apps CLI
npm install -g @azure/static-web-apps-cli

# Deploy
cd web-frontend
swa deploy --app-name gray-ocean-0040c6203 \
  --resource-group YourResourceGroup \
  --subscription-id YourSubscriptionId
```

---

## ✅ Post-Deployment Checklist

### Backend Verification
- [ ] API health check: `https://kennygpt.azurewebsites.net/api/chat/test`
- [ ] Swagger docs: `https://kennygpt.azurewebsites.net` (if enabled)
- [ ] Test chat endpoint with curl/Postman
- [ ] Verify database connection
- [ ] Check application logs in Azure Portal

### Mobile App Verification
- [ ] App installs without errors
- [ ] Login screen appears correctly
- [ ] Can enter API key
- [ ] API key is validated
- [ ] Can send/receive messages
- [ ] Conversations load from backend
- [ ] Dark mode toggle works
- [ ] Delete conversations works
- [ ] Logout returns to login screen

### Web App Verification
- [ ] Site loads: `https://gray-ocean-0040c6203.2.azurestaticapps.net`
- [ ] Can connect to backend API
- [ ] Chat functionality works
- [ ] Conversation history loads
- [ ] Theme toggle works

### Security Checks
- [ ] HTTPS enforced on all endpoints
- [ ] API key authentication working
- [ ] CORS properly configured
- [ ] No secrets in source code
- [ ] Application settings secured in Azure

---

## 🐛 Troubleshooting

### Backend Issues

**Problem**: App Service shows "Application Error"
```bash
# Check logs
az webapp log tail --name kennygpt --resource-group KennyGPT-RG

# Or in Azure Portal: App Service → Monitoring → Log stream
```

**Problem**: Database connection fails
```bash
# Test connection string
dotnet ef database update --connection "YOUR_CONNECTION_STRING"

# Verify firewall rules in Azure SQL allow Azure services
```

**Problem**: OpenAI API calls fail
- Verify API key is correct in Application Settings
- Check Azure OpenAI service is not rate-limited
- Verify deployment name matches your Azure OpenAI deployment

### Mobile App Issues

**Problem**: Android APK won't install
- Check minimum SDK version (API 21+)
- Verify APK is properly signed
- Enable "Install from Unknown Sources" if sideloading

**Problem**: iOS app rejected by App Store
- Review rejection reason in App Store Connect
- Common issues: missing privacy policy, improper permissions
- Ensure app follows Apple's guidelines

**Problem**: App can't connect to backend
- Verify API URL in `AzureService.cs`
- Check device has internet connection
- Test backend URL in browser
- Verify API key is valid

### Common Deployment Errors

**Error**: "The specified framework 'Microsoft.NETCore.App', version '9.0.0' was not found"
```bash
# Ensure Azure App Service has .NET 9 runtime
# In Azure Portal: Configuration → General settings → Stack settings
# Set: .NET Version = .NET 9
```

**Error**: "An error occurred while applying migrations"
```bash
# Manually apply migrations
dotnet ef database update --project KennyGPT

# Or check auto-migration is enabled in Program.cs
```

**Error**: "CORS policy blocked"
```bash
# Verify CORS configuration in Program.cs includes your frontend URLs
# Add to appsettings.json:
{
  "AllowedOrigins": [
    "https://gray-ocean-0040c6203.2.azurestaticapps.net",
    "https://kennygpt.azurewebsites.net"
  ]
}
```

---

## 📞 Support

Need help with deployment?

1. **Check Logs**:
   - Backend: Azure Portal → App Service → Log stream
   - Mobile: Visual Studio → Output → Debug

2. **Common Resources**:
   - [Azure App Service Docs](https://learn.microsoft.com/azure/app-service/)
   - [.NET MAUI Deployment](https://learn.microsoft.com/dotnet/maui/deployment/)
   - [Azure Static Web Apps](https://learn.microsoft.com/azure/static-web-apps/)

3. **Get Help**:
   - [Open an issue](https://github.com/theredking-369/KennyGPT/issues)
   - Check troubleshooting guides in other docs

---

## 🎯 Production Readiness

Before going to production:

- [ ] Set up monitoring (Application Insights)
- [ ] Configure backup for Azure SQL
- [ ] Set up alerts for errors/downtime
- [ ] Implement rate limiting
- [ ] Add logging/telemetry
- [ ] Set up CI/CD pipeline
- [ ] Create staging environment
- [ ] Document rollback procedure
- [ ] Set up health checks
- [ ] Configure auto-scaling

---

**Last Updated**: January 2025  
**Version**: 1.0

For more information, see the main [README.md](../README.md)
