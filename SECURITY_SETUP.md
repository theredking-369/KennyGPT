# ?? Security Setup Guide - Protecting User Passwords

## ?? **IMPORTANT: Passwords Are Now Secure!**

The default user passwords (Rory & Leonique) are **NO LONGER** hardcoded in the source code. They are now stored securely using environment variables.

---

## ?? **How It Works**

### **1. Development (Your Local Machine)**

Passwords are stored in `appsettings.Development.json`, which is **excluded from Git** by `.gitignore`.

**File: `appsettings.Development.json`** (NOT committed to GitHub)
```json
{
  "DefaultUsers": {
    "Rory": {
      "Password": "503921"
    },
    "Leonique": {
      "Password": "12345"
    }
  }
}
```

### **2. Production (Azure App Service)**

Passwords are set as **Environment Variables** in Azure Portal.

---

## ?? **Setup Instructions**

### **For Local Development:**

1. **Create `appsettings.Development.json`** (if it doesn't exist):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DefaultUsers": {
    "Rory": {
      "Password": "YOUR_SECURE_PASSWORD_HERE"
    },
    "Leonique": {
      "Password": "YOUR_SECURE_PASSWORD_HERE"
    }
  }
}
```

2. **Verify `.gitignore` includes:**
```
appsettings.Development.json
appsettings.*.json
!appsettings.json
```

3. **Run your app locally:**
```bash
cd KennyGPT
dotnet run
```

? Passwords will be read from `appsettings.Development.json`

---

### **For Azure Production:**

#### **Option A: Azure Portal (Easy)**

1. Go to **Azure Portal** ? Your App Service (`kennygpt`)
2. Navigate to **Settings** ? **Environment Variables**
3. Add the following:

| Name | Value |
|------|-------|
| `DefaultUsers__Rory__Password` | `YOUR_SECURE_PASSWORD` |
| `DefaultUsers__Leonique__Password` | `YOUR_SECURE_PASSWORD` |

4. Click **Save**
5. **Restart** your app service

#### **Option B: Azure CLI**

```bash
az webapp config appsettings set --resource-group kennygpt-rg --name kennygpt --settings \
  "DefaultUsers__Rory__Password=YOUR_SECURE_PASSWORD" \
  "DefaultUsers__Leonique__Password=YOUR_SECURE_PASSWORD"
```

#### **Option C: Azure Key Vault (Most Secure)**

```bash
# Create Key Vault
az keyvault create --name kennygpt-vault --resource-group kennygpt-rg --location eastus

# Store secrets
az keyvault secret set --vault-name kennygpt-vault --name "RoryPassword" --value "YOUR_SECURE_PASSWORD"
az keyvault secret set --vault-name kennygpt-vault --name "LeoniquePassword" --value "YOUR_SECURE_PASSWORD"

# Grant App Service access
az webapp identity assign --name kennygpt --resource-group kennygpt-rg
az keyvault set-policy --name kennygpt-vault --object-id <APP_OBJECT_ID> --secret-permissions get list
```

Then update `appsettings.json`:
```json
{
  "DefaultUsers": {
    "Rory": {
      "Password": "@Microsoft.KeyVault(SecretUri=https://kennygpt-vault.vault.azure.net/secrets/RoryPassword/)"
    },
    "Leonique": {
      "Password": "@Microsoft.KeyVault(SecretUri=https://kennygpt-vault.vault.azure.net/secrets/LeoniquePassword/)"
    }
  }
}
```

---

## ?? **Verify Security**

### **Check Local Files:**

```bash
# This file should NOT exist in Git
git ls-files | grep appsettings.Development.json
# Should return nothing ?

# This file SHOULD be tracked
git ls-files | grep appsettings.json
# Should return: KennyGPT/appsettings.json ?
```

### **Check GitHub:**

Visit: https://github.com/theredking-369/KennyGPT/blob/main/KennyGPT/appsettings.json

**You should see:**
```json
"DefaultUsers": {
  "Rory": {
    "Password": "SET_THIS_IN_USER_SECRETS_OR_ENV_VARS"
  },
  "Leonique": {
    "Password": "SET_THIS_IN_USER_SECRETS_OR_ENV_VARS"
  }
}
```

? **No real passwords visible!**

---

## ?? **How Authentication Works**

1. **User visits `/login.html`**
2. **Enters username & password**
3. **AuthService compares:**
   - User's password ? SHA256 hash
   - Hash compared with stored `PasswordHash` in database
4. **If match:** JWT token issued ?
5. **If no match:** Login fails ?

**Real passwords are NEVER stored in the database—only SHA256 hashes!** ??

---

## ??? **Security Best Practices**

? **DO:**
- Use `appsettings.Development.json` for local development
- Use Azure Environment Variables for production
- Use Azure Key Vault for enterprise security
- Keep `.gitignore` up to date
- Rotate passwords regularly

? **DON'T:**
- Commit `appsettings.Development.json` to Git
- Hardcode passwords in source code
- Share passwords in Slack/Email
- Use weak passwords like "12345"

---

## ?? **Changing Passwords**

### **Local Development:**

Edit `appsettings.Development.json`:
```json
{
  "DefaultUsers": {
    "Rory": {
      "Password": "NEW_PASSWORD_HERE"
    }
  }
}
```

Then **delete the database** to re-seed:
```bash
# Delete database to force re-seeding
rm KennyGPT.db
# Or for SQL Server:
# Delete database in SQL Server Management Studio
```

### **Azure Production:**

Update environment variable in Azure Portal, then **restart** the app service.

---

## ?? **Troubleshooting**

### **"Cannot login with correct password"**

**Cause:** Database has old password hash

**Solution:**
```bash
# Delete database to re-seed
dotnet ef database drop --project KennyGPT
dotnet run
```

### **"Password configuration not found"**

**Cause:** `appsettings.Development.json` missing or malformed

**Solution:** Verify JSON syntax and file location

### **"401 Unauthorized on Azure"**

**Cause:** Environment variables not set in Azure

**Solution:** Add environment variables in Azure Portal ? Configuration ? Application Settings

---

## ?? **You're All Set!**

Your passwords are now secure and won't be exposed in your public GitHub repository! ??

**Questions?** Check the code in:
- `KennyGPT/Services/AuthService.cs` (password seeding logic)
- `KennyGPT/appsettings.json` (public configuration)
- `KennyGPT/appsettings.Development.json` (local secrets - not in Git)
