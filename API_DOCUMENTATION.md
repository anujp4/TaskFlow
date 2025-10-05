# TaskFlow API Documentation

## Base URL
https://localhost:7094/swagger/index.html
## Authentication
All protected endpoints require JWT Bearer token in header:
Authorization: Bearer {token}
## Endpoints

### Authentication

#### Register User
- **POST** `/auth/register`
- **Body**: RegisterDto
- **Response**: AuthResponseDto with JWT token

#### Login
- **POST** `/auth/login`
- **Body**: LoginDto
- **Response**: AuthResponseDto with JWT token

#### Get Current User
- **GET** `/auth/me`
- **Auth**: Required
- **Response**: UserDto

### Tasks

#### Get All Tasks
- **GET** `/tasks`
- **Auth**: Required

#### Get Task by ID
- **GET** `/tasks/{id}`
- **Auth**: Required

#### Get My Tasks
- **GET** `/tasks/my-tasks`
- **Auth**: Required

#### Get Tasks by User
- **GET** `/tasks/user/{userId}`
- **Auth**: Required

#### Get Tasks by Status
- **GET** `/tasks/status/{status}`
- **Auth**: Required

#### Get Overdue Tasks
- **GET** `/tasks/overdue`
- **Auth**: Required

#### Create Task
- **POST** `/tasks`
- **Auth**: Required
- **Body**: CreateTaskDto

#### Update Task
- **PUT** `/tasks/{id}`
- **Auth**: Required
- **Body**: UpdateTaskDto

#### Delete Task
- **DELETE** `/tasks/{id}`
- **Auth**: Required

## Enums

### TaskPriority
- 1: Low
- 2: Medium
- 3: High
- 4: Urgent

### TaskStatus
- 1: ToDo
- 2: InProgress
- 3: InReview
- 4: Completed
- 5: Cancelled