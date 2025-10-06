# TaskFlow - Enterprise Task Management System

## Overview
TaskFlow is a cloud-based task management platform built with .NET Core 9, featuring microservices architecture, JWT authentication, and AI-powered capabilities.
## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code
- Update connection string in TaskFlow.API/appsettings.json
- Run migrations
### Installation
1. Clone the repository
git clone https://github.com/anujp4/TaskFlow.git
cd TaskFlow
### Features Implemented (Phase 1)

### Day 1
- ✅ Clean Architecture (Core, Application, Infrastructure, API layers)
- ✅ Domain entities with base entity pattern
- ✅ Repository pattern with Unit of Work
- ✅ Entity Framework Core with SQL Server
- ✅ Database migrations
- ✅ SOLID principles throughout

### Day 2
- ✅ JWT Authentication (Register, Login)
- ✅ RESTful API with full CRUD operations
- ✅ AutoMapper for object mapping
- ✅ DTOs for request/response
- ✅ Identity management with ASP.NET Core Identity
- ✅ Swagger documentation with JWT support

### Day 3
- ✅ FluentValidation for input validation
- ✅ Global error handling middleware
- ✅ Structured logging with Serilog
- ✅ Pagination, filtering, and sorting
- ✅ Unit tests with xUnit, Moq, FluentAssertions
- ✅ API versioning
- ✅ Request/Response logging

### Technology Stack

- **.NET 8.0** - Framework
- **ASP.NET Core Web API** - REST API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT** - Authentication
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Logging
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Test assertions
- **Swagger/OpenAPI** - API documentation

## Architecture
TaskFlow/
├── TaskFlow.API/              # Presentation layer
│   ├── Controllers/
│   │   └── V1/               # Version 1 controllers
│   ├── Middleware/           # Custom middleware
│   └── Program.cs
├── TaskFlow.Application/      # Business logic layer
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Mappings/
│   ├── Services/
│   └── Validators/
├── TaskFlow.Core/            # Domain layer
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
├── TaskFlow.Infrastructure/   # Data access layer
│   ├── Data/
│   └── Repositories/
### API Endpoints
Authentication (v1)

- POST /api/v1/auth/register - Register new user
- POST /api/v1/auth/login - Login
- GET /api/v1/auth/me - Get current user

Tasks (v1)

- GET /api/v1/tasks - Get all tasks
- GET /api/v1/tasks/paged - Get paginated tasks with filters
- GET /api/v1/tasks/{id} - Get task by ID
-GET /api/v1/tasks/my-tasks - Get current user's tasks
- GET /api/v1/tasks/user/{userId} - Get tasks by user
- GET /api/v1/tasks/status/{status} - Get tasks by status
- GET /api/v1/tasks/overdue - Get overdue tasks
- POST /api/v1/tasks - Create task
- PUT /api/v1/tasks/{id} - Update task
- DELETE /api/v1/tasks/{id} - Delete task

Query Parameters for Pagination

- PageNumber (default: 1)
- PageSize (default: 10, max: 100)
- SearchTerm - Search in title/description
- Status - Filter by status (1-5)
-Priority - Filter by priority (1-4)
- AssignedToId - Filter by assigned user
- IsOverdue - Filter overdue tasks
- SortBy - Sort field (title, priority, status, duedate, createdat)
- SortOrder - Sort direction (asc, desc)

### Logging
Logs are written to:
- Console (Development)
Files in Logs/ directory (Production)
- Rolling daily log files with 30-day retention

Log levels:

Information - General flow
Warning - Unexpected behavior
Error - Failures
Fatal - Application crashes

### Testing
- Test coverage includes:

### Unit tests for services
- Authentication tests
- Task CRUD operation tests
- Validation tests


### Contributing

- Fork the repository
- Create feature branch (git checkout -b feature/AmazingFeature)
- Commit changes (git commit -m 'Add some AmazingFeature')
- Push to branch (git push origin feature/AmazingFeature)
- Open Pull Request
### Design Patterns Used

1. **Repository Pattern** - Data access abstraction
2. **Unit of Work Pattern** - Transaction management
3. **Dependency Injection** - Loose coupling
### Contact
Your Name - anujpardeshi9@example.com
Project Link: https://github.com/anujp4/TaskFlow
