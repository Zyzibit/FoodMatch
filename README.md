# 🍽️ FoodMatch

> An intelligent meal planning system with AI integration, supporting recipe management, products, and shopping lists.

FoodMatch is a comprehensive web application that helps users efficiently plan meals, manage recipes, and automatically generate shopping lists. Leveraging OpenAI's artificial intelligence, the application offers personalized culinary suggestions tailored to user dietary preferences.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Latest-4169E1?logo=postgresql)](https://www.postgresql.org/)

---

<img width="1215" height="547" alt="image" src="https://github.com/user-attachments/assets/1401941a-9bd4-4c74-bf85-f5e1a56929fb" />



## ✨ Key Features

- 🤖 **OpenAI Integration** - Intelligent recipe and meal plan suggestions
- 📅 **Meal Planning** - Create and manage weekly meal plans
- 📖 **Recipe Database** - Complete recipe library with ability to add custom recipes
- 🛒 **Automatic Shopping Lists** - Generate shopping lists based on meal plans
- 🥗 **Product Management** - OpenFoodFacts integration for detailed product information
- 👤 **User Management** - Secure registration and JWT authentication
- 🎯 **Dietary Preferences** - Personalization according to diets and user preferences
- 📊 **Unit Tracking** - Support for various measurement units for ingredients
- 📧 **Email Notifications** - Notification system for users
- 🔒 **Security** - Role-based authorization with Identity Framework

---

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Database**: PostgreSQL 
- **ORM**: Entity Framework Core
- **Cache**: Redis
- **Authentication**: JWT Bearer + ASP.NET Identity
- **AI**: OpenAI API (GPT-4)
- **Email**: SMTP (Gmail)

### Frontend
- **Framework**: React 19
- **Language**: TypeScript 5.9
- **Build Tool**: Vite 7
- **UI Library**: Material-UI (MUI) v7
- **Routing**: React Router v7
- **Forms**: React Hook Form + Zod
- **Styling**: Emotion (CSS-in-JS)
- **Testing**: Vitest + Testing Library

### DevOps & Tools
- **Containerization**: Docker + Docker Compose
- **Admin Tools**: pgAdmin 4, Redis Commander
- **Testing**: xUnit, Vitest

---

## 📋 Requirements

Before starting, make sure you have installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or newer
- [Node.js](https://nodejs.org/) (LTS) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

---

## 🚀 Installation and Configuration

### 1. Clone the repository

```bash
git clone <repository-url>
cd foodmatch
```

### 2. Start Infrastructure (Docker)

Start PostgreSQL, Redis, and administrative tools:

```bash
cd backend/inzynierka
docker-compose up -d
```

**Available services:**
- PostgreSQL: `localhost:5433`
- Redis: `localhost:6379`
- pgAdmin: `http://localhost:8080` (admin@foodmatch.com / admin123)
- Redis Commander: `http://localhost:8081`

### 3. Backend Configuration

#### a) Environment Variables

Edit the file `backend/inzynierka/appsettings.json`:

```json
{
  "AI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  },
  "Email": {
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=foodmatch;Username=postgres;Password=postgres"
  }
}
```

**⚠️ WARNING**: This file contains sensitive data. In production, use **User Secrets** or **environment variables**.

#### b) Database Migrations

```bash
cd backend/inzynierka
dotnet ef database update
```

Or use the built-in migration on first application startup.

#### c) Start the API

```bash
cd backend/inzynierka
dotnet run
```

The API will be available at: `http://localhost:5127`  
Swagger UI: `http://localhost:5127/swagger`

### 4. Frontend Configuration

#### a) Install Dependencies

```bash
cd frontend
npm install
```

#### b) Configure API URL

Edit `frontend/src/config.ts` if the API runs on a different port:

```typescript
export const API_URL = 'http://localhost:5127';
```

#### c) Start the Application

```bash
npm run dev
```

The application will be available at: `http://localhost:5173`

---

## 📦 Building for Production

### Backend

```bash
cd backend/inzynierka
dotnet publish -c Release -o ./publish
```

### Frontend

```bash
cd frontend
npm run build
```

The built application will be located in the `frontend/dist/` folder.

---

## 🧪 Running Tests

### Backend Tests

```bash
# Unit tests
cd backend/inzynierka.Tests
dotnet test

# Integration tests
cd backend/inzynierka.IntegrationTests
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test
```

---

## 📁 Project Structure

```
foodmatch/
├── backend/
│   ├── inzynierka/                  # Main ASP.NET Core application
│   │   ├── AI/                      # OpenAI integration
│   │   ├── Auth/                    # Authentication and authorization
│   │   ├── Data/                    # DbContext and EF configuration
│   │   ├── MealPlans/               # Meal planning
│   │   ├── Products/                # Product management
│   │   ├── Recipes/                 # Culinary recipes
│   │   ├── ShoppingList/            # Shopping lists
│   │   ├── Users/                   # User management
│   │   ├── UserPreferences/         # User preferences
│   │   ├── Migrations/              # Entity Framework migrations
│   │   └── Program.cs               # Application entry point
│   ├── inzynierka.Tests/            # Unit tests
│   └── inzynierka.IntegrationTests/ # Integration tests
│
├── frontend/
│   ├── src/
│   │   ├── components/              # React components
│   │   ├── pages/                   # Application pages
│   │   ├── services/                # API services
│   │   ├── contexts/                # React Context
│   │   ├── types/                   # TypeScript types
│   │   ├── utils/                   # Utility functions
│   │   └── __tests__/               # Unit tests
│   └── public/                      # Static files
│
└── README.md
```

---

## 🔑 Key API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh-token` - Refresh token

### Recipes
- `GET /api/recipes` - Recipe list
- `POST /api/recipes` - Add recipe
- `GET /api/recipes/{id}` - Recipe details
- `PUT /api/recipes/{id}` - Update recipe
- `DELETE /api/recipes/{id}` - Delete recipe

### Meal Plans
- `GET /api/mealplans` - Plans list
- `POST /api/mealplans` - Create plan
- `GET /api/mealplans/{id}` - Plan details

### Products
- `GET /api/products` - Product list
- `GET /api/products/search` - Search products

### Shopping Lists
- `GET /api/shoppinglists` - User's shopping lists
- `POST /api/shoppinglists` - Create list

---

## 🔐 Security

- **JWT Tokens**: Access token (60 min) + Refresh token (7 days)
- **Password Hashing**: ASP.NET Identity with default password hasher
- **CORS**: Configured for `http://localhost:5173`
- **HTTPS**: Required in production
- **Role-based authorization**: Admin and User

**⚠️ Remember**:
- Change `JWT:secret` in `appsettings.json` to a secure key (min. 32 characters)
- Remove API keys from code before commit
- Use HTTPS in production
- Regularly update dependencies

---

## 🐛 Known Issues and Solutions

### EF Core migration issues
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Frontend doesn't connect to API
Check CORS configuration in `Program.cs` and URL in `config.ts`.

### Docker containers won't start
```bash
docker-compose down -v
docker-compose up -d
```

---

## 📝 API Documentation

After starting the backend, full Swagger documentation is available at:
`http://localhost:5127/swagger`

---

## 🤝 Contributing

### Code Style Conventions
- Backend: C# Coding Conventions (Microsoft)
- Frontend: ESLint + Prettier

### Workflow
1. Fork the project
2. Create a branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

MIT

---

## 👥 Authors

- **Szymon Zych** - frontend
- **Kamil Ziółkowski** - backend

---

## 📧 Contact

For questions or issues, open an Issue on GitHub.

---

## 🙏 Acknowledgments

- [OpenFoodFacts](https://world.openfoodfacts.org/) - Product database
  
---

**Made with ❤️ for food lovers**
