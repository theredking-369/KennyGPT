# 🤖 KennyGPT - Full-Stack AI Chat Application

<div align="center">

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-Cross--Platform-512BD4)](https://dotnet.microsoft.com/apps/maui)
[![Azure](https://img.shields.io/badge/Azure-Cloud%20Hosted-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o-412991?logo=openai)](https://openai.com/)


**A production-ready AI chatbot featuring ASP.NET Core backend, Azure Static Web App frontend, and cross-platform .NET MAUI mobile app**

[Features](#-features) • [Quick Start](#-quick-start) • [Architecture](#-architecture) • [Documentation](#-documentation) • [Deployment](#-deployment)

<!-- Option 1: Animated typing banner (works immediately) -->
![KennyGPT Banner](https://readme-typing-svg.herokuapp.com?font=Fira+Code&weight=600&size=30&duration=3000&pause=1000&color=667EEA&center=true&vCenter=true&width=800&height=100&lines=🤖+KennyGPT+-+AI+Chat+Application;Built+with+.NET+9+%26+Azure;Cross-Platform+•+Mobile+•+Web+•+API)

<!-- Option 2: Static gradient banner (uncomment when you create custom banner) -->
<!-- ![KennyGPT Banner](assets/banner.png) -->

</div>

---

## 🌟 Overview

**KennyGPT** is a complete full-stack AI chat application showcasing modern .NET development practices:

- 🚀 **Backend API**: ASP.NET Core 9.0 with Entity Framework Core
- 🌐 **Web Frontend**: Vanilla JavaScript SPA on Azure Static Web Apps  
- 📱 **Mobile App**: .NET MAUI for Android, iOS, Windows, and macOS
- 🤖 **AI Integration**: Azure OpenAI (GPT-4o) with web search via SerpAPI
- 💾 **Database**: Azure SQL Database for conversation persistence
- 🔐 **Security**: API key authentication and secure credential storage

Perfect for learning full-stack .NET development, AI integration, or as a foundation for your own AI-powered applications!

---

## ✨ Features

### 🎯 Core Features
- ✅ **Real-time AI Chat** - Powered by Azure OpenAI (GPT-4o)
- ✅ **Web Search Integration** - Current information via SerpAPI
- ✅ **Conversation Management** - Persistent cloud storage
- ✅ **Multi-Platform Support** - Web, Android, iOS, Windows, macOS
- ✅ **API Key Authentication** - Secure access control
- ✅ **Custom System Prompts** - Personalize AI behavior

### 📱 Mobile App Features  
- ⏰ **Message Timestamps** - Track conversation timeline
- 🗑️ **Delete Conversations** - With confirmation dialogs
- 🌓 **Dark Mode** - System-wide theme with persistence
- 💬 **ChatGPT-style UI** - Modern message bubbles
- 🔐 **Secure Storage** - Encrypted API key storage
- 📂 **Conversation History** - Cloud sync
- ✨ **Smooth Animations** - Polished UX

### 🌐 Web App Features
- 🎨 **Modern Gradient UI** - Responsive design
- 📝 **System Instructions** - Customize AI responses
- 🔄 **Real-time Updates** - Instant conversation switching
- 💾 **Local Storage** - Remember preferences

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│                  Client Layer                       │
│  ┌──────────────────┐      ┌──────────────────┐   │
│  │   Web App (SPA)  │      │  Mobile App MAUI │   │
│  │  Azure Static    │      │  Android/iOS/    │   │
│  │   Web Apps       │      │  Windows/macOS   │   │
│  └────────┬─────────┘      └────────┬─────────┘   │
└───────────┼────────────────────────┼───────────────┘
            │                        │
            │   HTTPS + API Key Auth │
            └────────┬───────────────┘
                     ↓
         ┌───────────────────────┐
         │   ASP.NET Core API    │
         │   Azure App Service   │
         │  ┌─────────────────┐  │
         │  │ ApiKey          │  │
         │  │ Middleware      │  │
         │  └─────────────────┘  │
         └───────────┬───────────┘
                     │
      ┏━━━━━━━━━━━━━┻━━━━━━━━━━━━━┓
      ↓              ↓              ↓
┌───────────┐  ┌──────────┐  ┌──────────┐
│  Azure    │  │  Azure   │  │ SerpAPI  │
│  OpenAI   │  │   SQL    │  │  Search  │
│  GPT-4o   │  │ Database │  │  Engine  │
└───────────┘  └──────────┘  └──────────┘
```

### Data Flow
1. **Client** sends chat request with API key
2. **API Middleware** validates authentication
3. **Controller** processes request and manages conversation
4. **Azure OpenAI** generates AI responses
5. **SerpAPI** (optional) fetches real-time web data
6. **Database** persists conversation history
7. **Response** returns to client with AI message

---

## 📁 Projects

### 1️⃣ **KennyGPT** - Backend API
**📍 Directory**: `KennyGPT/`  
**Tech**: ASP.NET Core 9.0, EF Core, Azure OpenAI SDK

```
KennyGPT/
├── Controllers/
│   └── ChatController.cs         # REST API endpoints
├── Services/
│   ├── AzureService.cs           # OpenAI integration
│   └── SerpAPIService.cs         # Web search
├── Middleware/
│   └── ApiKeyMiddleware.cs       # Authentication
├── Data/
│   └── ChatDbContext.cs          # EF Core DbContext
├── Models/
│   ├── MConversation.cs
│   ├── MChatMessage.cs
│   ├── MChatRequest.cs (DTO)
│   └── MChatResponse.cs (DTO)
└── Migrations/                   # Database migrations
```

**📡 API Endpoints:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/chat/send` | Send message, get AI response |
| `GET` | `/api/chat/conversations` | List all conversations |
| `GET` | `/api/chat/conversations/{id}/messages` | Get conversation messages |
| `DELETE` | `/api/chat/conversations/{id}` | Delete conversation |
| `GET` | `/api/chat/test` | Health check |

### 2️⃣ **MauiGPT** - Mobile App  
**📍 Directory**: `MauiGPT/`  
**Tech**: .NET MAUI 9.0, MVVM Pattern

```
MauiGPT/
├── Views/
│   ├── LoginPage.xaml            # Login UI
│   └── ChatPage.xaml             # Chat interface
├── ViewModels/
│   ├── LoginViewModel.cs         # Login logic
│   ├── ChatViewModel.cs          # Chat logic
│   └── ViewModelBase.cs          # Base MVVM
├── Services/
│   ├── AzureService.cs           # API client
│   └── ThemeService.cs           # Dark mode
├── Models/                       # Shared DTOs
├── Converters/                   # XAML value converters
├── Helpers/
│   ├── ConnectivityHelper.cs
│   └── AppLogger.cs
└── Resources/                    # Assets, fonts, styles
```

**🎯 Supported Platforms:**
- ✅ **Android** (API 21+)
- ✅ **iOS** (15.0+)
- ✅ **Windows** (10.0.17763+)
- ✅ **macOS** (Catalyst 15.0+)

### 3️⃣ **Web Frontend**  
**📍 Hosting**: Azure Static Web Apps  
**Tech**: Vanilla JavaScript, HTML5, CSS3

🌐 **Live Demo**: [gray-ocean-0040c6203.2.azurestaticapps.net](https://gray-ocean-0040c6203.2.azurestaticapps.net)

---

## 🚀 Quick Start

### Prerequisites
- ✅ [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+)  
  *With MAUI workload for mobile development*
- ✅ [Azure Account](https://azure.microsoft.com/free/) (free tier available)
- ✅ Azure OpenAI Service access
- ✅ (Optional) [SerpAPI Key](https://serpapi.com/)

### 1. Clone Repository
```bash
git clone https://github.com/theredking-369/KennyGPT.git
cd KennyGPT
```

### 2. Configure Backend Secrets

The project uses **User Secrets** to keep credentials secure and out of source control.

```bash
cd KennyGPT

# Initialize user secrets
dotnet user-secrets init

# Set Azure OpenAI credentials
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_AZURE_OPENAI_API_KEY"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "YOUR_DEPLOYMENT_NAME"

# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_SQL_CONNECTION_STRING"

# (Optional) Set SerpAPI key for web search
dotnet user-secrets set "SerpApi:ApiKey" "YOUR_SERPAPI_KEY"
```

### 3. Setup Database
```bash
# Apply EF Core migrations
dotnet ef database update

# Start backend API
dotnet run
# API will run at: https://localhost:7066
```

### 4. Run Mobile App

#### On Android:
1. Open `KennyGPT.sln` in Visual Studio 2022
2. Set `MauiGPT (net9.0-android)` as startup project
3. Select your Android device/emulator
4. Press **F5** to run

#### On Windows:
1. Set `MauiGPT (net9.0-windows)` as startup project
2. Press **F5** to run

#### On iOS/macOS:
1. Requires macOS with Xcode installed
2. Set appropriate platform as startup project
3. Press **F5** to run

### 5. First-Time Login
1. Launch the mobile app
2. You'll see the **login screen**
3. Enter your **API key** (create one if needed)
4. Tap **"Unlock Access"**
5. Start chatting! 🎉

---

## ⚙️ Configuration

### Backend Configuration

**📄 `appsettings.json`** (empty placeholders - safe to commit):
```json
{
  "AzureOpenAI": {
    "Endpoint": "",
    "ApiKey": "",
    "DeploymentName": ""
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "SerpApi": {
    "ApiKey": ""
  }
}
```

**🔐 User Secrets** (stored locally, NOT in repo):
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://bkennygpt.openai.azure.com/",
    "ApiKey": "your-actual-key-here",
    "DeploymentName": "kenny-4o"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;..."
  }
}
```

### API Authentication

All API requests require the `X-API-Key` header:

```bash
curl -X POST "https://kennygpt.azurewebsites.net/api/chat/send" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: YOUR_API_KEY" \
  -d '{
    "message": "Hello, AI!",
    "conversationId": null,
    "systemPrompt": "You are a helpful assistant"
  }'
```

### Mobile App Configuration

1. **First Launch**: App prompts for API key
2. **Storage**: Key encrypted in MAUI `SecureStorage`
3. **Backend URL**: Edit `MauiGPT/Services/AzureService.cs`:
   ```csharp
   _apiBaseUrl = "https://kennygpt.azurewebsites.net/api";
   ```

---

## 🌐 Deployment

### Deploy Backend to Azure App Service

#### Option 1: Visual Studio
1. Right-click `KennyGPT` project → **Publish**
2. Choose **Azure → Azure App Service (Windows)**
3. Sign in to Azure
4. Create/select App Service
5. Click **Publish**

#### Option 2: Azure CLI
```bash
# Login to Azure
az login

# Create resource group
az group create --name KennyGPT-RG --location eastus

# Create App Service plan
az appservice plan create --name KennyGPT-Plan --resource-group KennyGPT-RG --sku B1

# Create web app
az webapp create --name kennygpt --resource-group KennyGPT-RG --plan KennyGPT-Plan

# Deploy code
dotnet publish -c Release
az webapp deployment source config-zip --resource-group KennyGPT-RG --name kennygpt --src publish.zip
```

#### Configure App Service Settings
Add secrets as **Application Settings** in Azure Portal:
- `AzureOpenAI__Endpoint`
- `AzureOpenAI__ApiKey`
- `AzureOpenAI__DeploymentName`
- `ConnectionStrings__DefaultConnection`

### Deploy Mobile App

#### Android (Google Play Store)
```bash
# Create release build
dotnet publish MauiGPT/MauiGPT.csproj -c Release -f net9.0-android

# Sign APK (requires keystore)
jarsigner -keystore your-keystore.keystore \
  -storepass YOUR_PASSWORD \
  MauiGPT/bin/Release/net9.0-android/com.companyname.mauigpt-Signed.apk \
  your_alias

# Upload to Google Play Console
```

#### iOS (App Store)
1. Open in **Visual Studio for Mac** or **VS Code**
2. Select **Release** configuration
3. **Archive** the app
4. Upload to **App Store Connect**

#### Windows (Microsoft Store)
```bash
# Create MSIX package
dotnet publish MauiGPT/MauiGPT.csproj -c Release -f net9.0-windows10.0.19041.0 -p:RuntimeIdentifierOverride=win10-x64

# Submit to Microsoft Partner Center
```

---

## 🛠️ Technologies

<table>
<tr>
<td valign="top" width="33%">

### Backend
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- Azure OpenAI SDK 2.1.0
- Azure SQL Database
- SerpAPI Integration
- Swashbuckle (OpenAPI)

</td>
<td valign="top" width="33%">

### Mobile
- .NET MAUI 9.0
- MVVM Pattern
- XAML
- SecureStorage API
- Shell Navigation
- Community Toolkit

</td>
<td valign="top" width="33%">

### Cloud & DevOps
- Azure App Service
- Azure Static Web Apps
- Azure SQL Database
- Azure OpenAI Service
- GitHub (Version Control)
- GitHub Actions (CI/CD)

</td>
</tr>
</table>

---

## 🔐 Security

### ✅ Security Best Practices

1. **User Secrets** - Development credentials stored locally, never committed
2. **API Key Auth** - Middleware validates all requests
3. **HTTPS Only** - All communication encrypted
4. **CORS** - Configured for specific origins only
5. **SecureStorage** - Mobile credentials encrypted on device
6. **Input Validation** - Sanitization of user inputs

### 🔒 What's Safe to Commit
- ✅ Source code
- ✅ `appsettings.json` (with empty values)
- ✅ Database migrations
- ✅ Documentation
- ✅ `.csproj` files

### ⚠️ NEVER Commit
- ❌ API keys or secrets
- ❌ Connection strings
- ❌ `secrets.json` files
- ❌ `appsettings.Development.json`
- ❌ `.user` files

### 🛡️ Before Making Repository Public

**Already protected!** Your `.gitignore` has been updated to exclude:
- User secrets directories
- Development settings
- Database files
- Build artifacts

**Verify no secrets in Git history:**
```bash
git log -p | grep -i "apikey"
git log -p | grep -i "connectionstring"
```

---

## 📚 Documentation

- 📖 **[Deployment Guide](DEPLOYMENT_GUIDE.md)** - Complete deployment walkthrough
- 🆕 **[New Features](MauiGPT/NEW_FEATURES.md)** - Timestamps, delete, dark mode
- 🔧 **[Delete Troubleshooting](MauiGPT/DELETE_TROUBLESHOOTING.md)** - Debug delete issues
- 🚀 **[Quick Guide](MauiGPT/QUICK_GUIDE.md)** - Quick reference
- 🧭 **[Navigation Fix](MauiGPT/NAVIGATION_FIX.md)** - Shell navigation guide
- 🎨 **[Flyout Fix](MauiGPT/FLYOUTPAGE_FIX.md)** - Sidebar implementation

---

## 🐛 Troubleshooting

### Common Issues

<details>
<summary><b>Backend won't start</b></summary>

```bash
# Check migrations applied
dotnet ef database update

# Verify user secrets
dotnet user-secrets list

# Check connection to Azure SQL
# Test connection string in Azure Data Studio
```
</details>

<details>
<summary><b>Mobile app can't connect to API</b></summary>

1. **Check internet** on device
2. **Verify API URL** in `AzureService.cs`
3. **Test endpoint** in browser: `https://kennygpt.azurewebsites.net/api/chat/test`
4. **Check API key** is correct
5. **View logs** in Visual Studio → Output → Debug

</details>

<details>
<summary><b>API Key authentication fails</b></summary>

- Ensure `X-API-Key` header is set
- Verify ApiKeyMiddleware is registered in `Program.cs`
- Check CORS is before middleware
- Test with Postman/curl first

</details>

<details>
<summary><b>Dark mode not working</b></summary>

- Theme toggle in sidebar top-right
- Preference saved in `Preferences` API
- Restart app to verify persistence
- Check `ThemeService.cs` initialization

</details>

See detailed troubleshooting: [`DEPLOYMENT_GUIDE.md`](DEPLOYMENT_GUIDE.md)

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork** the repository
2. **Create** a feature branch
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. **Commit** your changes
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
4. **Push** to the branch
   ```bash
   git push origin feature/AmazingFeature
   ```
5. **Open** a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Add XML documentation to public APIs
- Write unit tests for new features
- Update README if adding features

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2024 Rory (theredking-369)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files...
```

---

## 👤 Author

**Rory** (theredking-369)

- GitHub: [@theredking-369](https://github.com/theredking-369)
- Repository: [KennyGPT](https://github.com/theredking-369/KennyGPT)

---

## 🙏 Acknowledgments

- **Azure OpenAI** for GPT-4o capabilities
- **Microsoft** for .NET MAUI and ASP.NET Core frameworks
- **SerpAPI** for real-time web search integration
- **Community** for feedback and support

---

## 📊 Project Statistics

- **Lines of Code**: ~6,000+
- **Languages**: C#, JavaScript, XAML
- **Platforms**: 5 (Web, Android, iOS, Windows, macOS)
- **Azure Services**: 4 (App Service, SQL, Static Web Apps, OpenAI)
- **API Endpoints**: 5 RESTful endpoints
- **Features**: 20+ implemented features

---

## 🗺️ Roadmap

### ✅ Completed
- [x] Core chat functionality
- [x] Multi-platform mobile app
- [x] Dark mode support
- [x] Message timestamps
- [x] Delete conversations
- [x] Web search integration

### 🚧 In Progress
- [ ] Voice input (speech-to-text)
- [ ] Image attachments
- [ ] Export conversations (PDF/JSON)

### 📝 Planned
- [ ] Push notifications
- [ ] Offline mode with sync
- [ ] Multi-language support
- [ ] Analytics dashboard
- [ ] Docker containerization
- [ ] Kubernetes deployment
- [ ] Multiple AI model support

---

## 📞 Support

Need help? Here's how:

1. **📖 Check Documentation** - Most answers in the docs
2. **🔍 Search Issues** - Check existing GitHub issues
3. **🐛 Report Bug** - [Open a new issue](https://github.com/theredking-369/KennyGPT/issues)
4. **💡 Request Feature** - [Start a discussion](https://github.com/theredking-369/KennyGPT/discussions)

---

<div align="center">

## ⭐ Star History

If you find this project helpful, please consider giving it a ⭐!

[![Star History Chart](https://api.star-history.com/svg?repos=theredking-369/KennyGPT&type=Date)](https://star-history.com/#theredking-369/KennyGPT&Date)

---

**Built with ❤️ using .NET 9 and Azure**

[⬆ Back to Top](#-kennygpt---full-stack-ai-chat-application)

</div>
