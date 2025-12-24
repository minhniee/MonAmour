# Mon Amour - Premium Dating Service Platform

Mon Amour is a platform providing personalized full-service dating planning, helping transform emotional moments into carefully designed and romantic experiences.

## 📋 Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database](#database)
- [API Endpoints](#api-endpoints)
- [Chatbot AI](#chatbot-ai)
- [Deployment](#deployment)

## 🎯 Introduction

Mon Amour specializes in:
- ✨ Organizing romantic, impressive, and meaningful dates
- 💝 Emotion-as-a-Service - transforming emotional moments into carefully designed experiences
- 🎨 Providing diverse and creative dating concepts
- 📍 Consulting and supporting customers in planning perfect dates

## ✨ Features

### Concept Management
- Manage dating concepts with images, descriptions, and pricing
- Categorize by category, color, and ambiance
- Manage locations and partners

### Product Management
- Manage products with multiple images
- Categorize products by category
- Manage inventory and pricing

### Order Management
- Process orders and payments
- Integration with VietQR and Casso for automatic payment
- Track order status

### Blog & Content
- Manage blog with categories and comments
- Real-time comment system with SignalR
- Manage banners for homepage and services

### Chatbot AI (MonMon AI)
- Automatic consultation with OpenAI GPT-3.5-turbo
- Generate concept images with DALL-E 3
- Consult on concepts, booking, and answer questions

### User Management
- Registration/login system
- Admin/User role-based authorization
- Profile and wishlist management

### Reports & Statistics
- Revenue reports
- Order statistics
- User data analysis

## 🛠 Technology Stack

### Backend
- .NET 8.0 - Main framework
- ASP.NET Core MVC - Web framework
- Entity Framework Core 9.0.8 - ORM
- SQL Server - Database
- SignalR - Real-time communication

### Frontend
- Tailwind CSS - Styling framework
- Alpine.js - Lightweight JavaScript framework
- SweetAlert2 - Beautiful alerts
- Chart.js - Data visualization

### Services & APIs
- Cloudinary - Image hosting and optimization
- OpenAI API - GPT-3.5-turbo for chatbot and DALL-E 3 for image generation
- VietQR API - Generate payment QR codes
- Casso API - Automatic payment confirmation
- SMTP (Gmail) - Email sending

### Tools & Libraries
- Newtonsoft.Json - JSON processing
- SixLabors.ImageSharp - Image processing
- Portable.BouncyCastle - Cryptography

## 📁 Project Structure

```
MonAmour_v9/
├── Controllers/          # MVC Controllers
│   ├── AdminController.cs
│   ├── ConceptController.cs
│   ├── HomeController.cs
│   ├── AuthController.cs
│   ├── CartController.cs
│   └── ...
├── Models/              # Entity models
│   ├── Concept.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── User.cs
│   └── ...
├── Services/            # Business logic
│   ├── Interfaces/
│   └── Implements/
├── ViewModels/          # View models
├── Views/               # Razor views
├── wwwroot/             # Static files
│   ├── js/
│   │   └── chatbot.js   # Chatbot AI logic
│   ├── css/
│   └── images/
├── Migrations/          # EF Core migrations
├── Middleware/          # Custom middleware
├── Filters/             # Action filters
├── Helpers/             # Helper classes
├── Util/                # Utilities
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── MonAmour.csproj     # Project file
```

## 🚀 Installation

### System Requirements
- .NET 8.0 SDK
- SQL Server 2019+ or Azure SQL Database
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone repository**
```bash
git clone <repository-url>
cd MonAmour_v9
```

2. **Install dependencies**
```bash
dotnet restore
```

3. **Configure database**
- Update connection string in `appsettings.json`
- Run migrations:
```bash
dotnet ef database update
```

4. **Configure API keys**
- Update API keys in `appsettings.json`:
  - OpenAI API key (for chatbot)
  - Cloudinary credentials
  - VietQR API credentials
  - Casso API credentials
  - Email SMTP settings

5. **Run application**
```bash
dotnet run
```

The application will run at:
- HTTP: `http://localhost:5012`
- HTTPS: `https://localhost:7239`

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MonAmourDb_final;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False"
  },
  "Chatbot": {
    "OpenAIApiKey": "sk-proj-YOUR_OPENAI_KEY"
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  },
  "VietQR": {
    "ApiBase": "https://api.vietqr.io",
    "BankCode": "ICB",
    "AccountNo": "YOUR_ACCOUNT",
    "ClientId": "YOUR_CLIENT_ID",
    "ApiKey": "YOUR_API_KEY"
  },
  "Casso": {
    "ApiBase": "https://oauth.casso.vn",
    "ApiKey": "YOUR_CASSO_KEY",
    "MerchantId": "YOUR_MERCHANT_ID"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "YOUR_EMAIL",
    "Password": "YOUR_PASSWORD",
    "From": "YOUR_EMAIL"
  }
}
```

## 🗄️ Database

### Migrations

The project uses Entity Framework Core Migrations:

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# View applied migrations
dotnet ef migrations list
```

### Apply migrations via API

You can apply migrations via endpoints:
```
POST /database/migrate
GET /database/migrations/status
```

### Main Tables

- **User** - Users
- **Role, User_Role** - Role-based authorization
- **Concept, Concept_img, Concept_Category** - Dating concepts
- **Product, Product_img, Product_Category** - Products
- **Order, OrderItem** - Orders
- **Payment, PaymentDetail, PaymentStatus** - Payments
- **Blog, Blog_Category, Blog_Comment** - Blog
- **Partner, Location** - Partners and locations
- **Review** - Reviews
- **Wish_list** - Wishlists
- **BannerHomepage, BannerService, BannerProduct** - Banners
- **CassoTransactions** - Casso transactions

## 🔌 API Endpoints

### Public APIs

- `GET /api/chatbot/config` - Get chatbot API config
- `GET /health` - Health check
- `GET /database/migrations/status` - Migration status
- `POST /database/migrate` - Apply migrations

### Authentication

- `GET /Auth/Login` - Login page
- `POST /Auth/Login` - Process login
- `GET /Auth/Signup` - Signup page
- `POST /Auth/Signup` - Process signup
- `POST /Auth/Logout` - Logout

### Concept

- `GET /Concept/ListConcept` - List concepts
- `GET /Concept/ConceptDetail/{id}` - Concept details
- `POST /Concept/SubmitContact` - Submit consultation request
- `POST /Concept/SubmitChatbotContact` - Submit request via chatbot

### Cart & Order

- `GET /Cart/Index` - Shopping cart
- `POST /Cart/AddToCart` - Add to cart
- `POST /Cart/Checkout` - Checkout
- `GET /Cart/PaymentCallback` - Payment callback

## 🤖 Chatbot AI

### MonMon AI

The chatbot is integrated with OpenAI to:
- Consult on concepts and services
- Generate concept images with DALL-E 3
- Assist with booking
- Answer questions

### Configuration

The chatbot uses:
- **GPT-3.5-turbo** for main chat
- **GPT-4o-mini** for concept description (fallback)
- **DALL-E 3** for image generation

### Training Data

The chatbot can load training data from:
```
wwwroot/data/chatbot_training_data.json
```

## 🚢 Deployment

### Docker

The project includes a Dockerfile:

```bash
docker build -t monamour .
docker run -p 8080:80 monamour
```

### Azure App Service

1. Publish project:
```bash
dotnet publish -c Release
```

2. Deploy to Azure App Service
3. Configure connection strings and app settings in Azure Portal
4. Enable Application Insights (optional)

### Environment Variables

You can override configuration using environment variables:
- `ConnectionStrings__DefaultConnection`
- `Chatbot__OpenAIApiKey`
- `Cloudinary__ApiKey`
- etc.

## 📝 Development

### Code Structure

- **Controllers**: Handle HTTP requests
- **Services**: Business logic layer
- **Models**: Entity models and DbContext
- **ViewModels**: Data transfer objects for views
- **Middleware**: Custom middleware (RememberMe, etc.)
- **Filters**: Action filters (Authorization, etc.)

### Best Practices

- Use async/await for database operations
- Dependency Injection for all services
- Repository pattern via Services layer
- ViewModels to separate data models and views
- Error handling with try-catch and logging

## 🔒 Security

- Session-based authentication
- Anti-forgery tokens for forms
- Role-based authorization (Admin/User)
- Password hashing with BCrypt
- SQL injection protection with EF Core parameterized queries
- XSS protection with Razor encoding

## 📧 Support

- Email: thieuducdung254@gmail.com
- Hotline: 0393312336
- Website: https://monamour.vn

