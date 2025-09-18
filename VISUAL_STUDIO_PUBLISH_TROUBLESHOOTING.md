# Visual Studio Publish Troubleshooting Guide

## ❌ Current Issues & Solutions

### 1. **Build Failed in Visual Studio**
```
System.AggregateException: One or more errors occurred. ---> Microsoft.WebTools.Shared.Exceptions.WebToolsException: Build failed. Check the Output window for more details.
```

### 2. **Entity Framework SQL Script Generation Failed**
```
error : Entity Framework SQL Script generation failed
```

### 3. **Azure Authentication Failed**
```
MSDEPLOY : error : The specified credentials cannot be used with the authentication scheme 'Basic'.
```

## ✅ **SOLUTIONS:**

### **Solution 1: Clean Visual Studio Cache**

#### **Step 1: Close Visual Studio completely**

#### **Step 2: Clean build artifacts**
```bash
# In project directory
dotnet clean
dotnet restore
dotnet build -c Release
```

#### **Step 3: Clear Visual Studio cache**
Delete these folders:
```
%LOCALAPPDATA%\Microsoft\VisualStudio\17.0_[instance]\ComponentModelCache
%LOCALAPPDATA%\Microsoft\VisualStudio\17.0_[instance]\MEFCacheBackup
%APPDATA%\Microsoft\VisualStudio\17.0\ActivityLog.xml
```

#### **Step 4: Restart Visual Studio**

---

### **Solution 2: Use Command Line Publish (RECOMMENDED)**

#### **Basic Publish (Works):**
```bash
dotnet publish -c Release --no-restore
```

#### **Deploy to Azure manually:**
1. **Zip publish folder:**
   ```bash
   # Navigate to bin/Release/net8.0/publish/
   # Create zip file of all contents
   ```

2. **Upload via Azure Portal:**
   - Go to Azure Portal > App Service > Deployment Center
   - Choose "Local Git" or "ZIP Deploy"
   - Upload the zip file

---

### **Solution 3: Fix Publish Profile Issues**

#### **Create new simplified publish profile:**

**File: `Properties/PublishProfiles/Azure-Manual.pubxml`**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <WebPublishMethod>Package</WebPublishMethod>
    <LastUsedBuildConfiguration>Release</LastUsedBuildConfiguration>
    <LastUsedPlatform>Any CPU</LastUsedPlatform>
    <SiteUrlToLaunchAfterPublish>https://mounamour-gzh3axfcfweubhdg.eastasia-01.azurewebsites.net</SiteUrlToLaunchAfterPublish>
    <LaunchSiteAfterPublish>true</LaunchSiteAfterPublish>
    <ExcludeApp_Data>false</ExcludeApp_Data>
    <TargetFramework>net8.0</TargetFramework>
    <SelfContained>false</SelfContained>
    
    <!-- Disable problematic features -->
    <PublishDatabaseSettings>False</PublishDatabaseSettings>
    <IncludeSetACLProviderOnDestination>False</IncludeSetACLProviderOnDestination>
  </PropertyGroup>
</Project>
```

---

### **Solution 4: Azure Authentication Issues**

#### **Re-authenticate Visual Studio:**
1. **Visual Studio > Tools > Options > Azure Service Authentication**
2. **Sign out and sign back in**
3. **Ensure correct subscription is selected**

#### **Alternative: Use Azure CLI**
```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "your-subscription-id"

# Deploy using az webapp
az webapp deployment source config-zip --resource-group MonAumor --name mounamour --src publish.zip
```

---

## 🚀 **RECOMMENDED DEPLOYMENT WORKFLOW:**

### **Method 1: Manual ZIP Deploy (Easiest)**
```bash
# 1. Build and publish
dotnet clean
dotnet publish -c Release -o ./publish

# 2. Create ZIP file from publish folder contents
# (Use Windows Explorer or 7-zip)

# 3. Upload via Azure Portal
# Portal > App Service > Deployment Center > ZIP Deploy
```

### **Method 2: GitHub Actions (Best for CI/CD)**
```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure
on:
  push:
    branches: [ Final_Update ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build -c Release --no-restore
    - name: Publish
      run: dotnet publish -c Release --no-build -o ./publish
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'mounamour'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

---

## 🔍 **DEBUGGING STEPS:**

### **Check Build Success:**
```bash
dotnet build -c Release --verbosity normal
# Should show: Build succeeded
```

### **Check Publish Success:**
```bash
dotnet publish -c Release --no-restore -o ./publish
# Should create files in ./publish folder
```

### **Test Published App Locally:**
```bash
cd publish
dotnet MonAmour.dll
# Should start without errors
```

### **Verify Published Files:**
Check that these files exist in publish folder:
- ✅ `MonAmour.dll`
- ✅ `MonAmour.deps.json`
- ✅ `MonAmour.runtimeconfig.json`
- ✅ `appsettings.json`
- ✅ `wwwroot/` folder with static files

---

## 📋 **CURRENT STATUS:**

- ✅ **Local build**: SUCCESS
- ✅ **Local publish**: SUCCESS  
- ❌ **Visual Studio publish**: FAILED (Entity Framework SQL Script generation)
- ❌ **Azure authentication**: FAILED (Credentials issue)
- ✅ **Manual deployment**: READY

## 🎯 **NEXT STEPS:**

1. **Use manual ZIP deploy** (fastest solution)
2. **Fix Visual Studio authentication** (for future deploys)
3. **Set up GitHub Actions** (for automated deployment)
4. **Test application** after deployment

**The application is ready for deployment - Visual Studio publish issues don't prevent manual deployment!**
