# Fitness Tracker API

**Fitness Tracker API** is a modern backend service designed for tracking fitness activities, planning workouts, and monitoring user progress. Built using **ASP.NET Core**.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)
- [Testing](#testing)

## Features

### Core Functionalities

#### Workout Logging
- Log workouts with exercises, sets, reps, duration, and intensity.
- Filter, sort, and paginate workouts for easy retrieval.

#### Workout Plan Management
- Create, update, and delete predefined workout plans.
- Enforce unique workout plan names per user using normalization.

#### Progress Tracking
- Track user progress over time for specific exercises.
- Validate that users are linked to exercises before tracking progress.

#### Exercise Management
- Avoid exercise duplication through normalized naming conventions.
- Link exercises to users for consistency and integrity.

#### Fitness Insights
- Generate insights like daily progress, weekly/monthly summaries, and trends.
- Identify underutilized exercises and recommend improvements.
## Technologies Used

- **ASP.NET Core** for backend development.
- **Entity Framework Core** for database access.
- **SQL Server** as the relational database.
- **JWT Authentication** for user security.
- **AutoMapper** for seamless mapping between DTOs and models.
- **xUnit** for unit testing.

---

## Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Velovo123/fitness-tracker-api.git
   cd fitness-tracker-api
2. **Set up the Database:**
   - Install and run **SQL Server**.
   - Update the `appsettings.json` file with your database connection string.
   - Apply database migrations:
     ```bash
     dotnet ef database update
     ```
3. **Build and Run the Application:**
   - Build the project:
     ```bash
     dotnet build
     ```
   - Run the application:
     ```bash
     dotnet run
     ```
## API Endpoints

### Authentication
- **POST** `/api/user/register` - Register a new user.
- **POST** `/api/user/login` - Authenticate and obtain a JWT token.

### User Profile
- **GET** `/api/user/profile` - Retrieve the authenticated user's profile.
- **GET** `/api/user/by-email/{email}` *(Admin only)* - Get user profile by email.

### Workouts
- **GET** `/api/workouts` - Retrieve a list of workouts with filters.
- **POST** `/api/workouts` - Log a new workout.
- **GET** `/api/workouts/date/{date}` - Get workouts for a specific date.
- **DELETE** `/api/workouts/date/{date}` - Delete a workout by date.

### Workout Plans
- **GET** `/api/workout-plans` - Get workout plans with filters.
- **POST** `/api/workout-plans` - Save a workout plan.
- **GET** `/api/workout-plans/name/{name}` - Retrieve a workout plan by name.
- **DELETE** `/api/workout-plans/name/{name}` - Delete a workout plan by name.

### Progress Tracking
- **GET** `/api/progress-records` - Retrieve progress records with filters.
- **POST** `/api/progress-records` - Save a new progress record.
- **GET** `/api/progress-records/date/{date}/exercise/{exerciseName}` - Get a progress record by date and exercise name.
- **DELETE** `/api/progress-records/date/{date}/exercise/{exerciseName}` - Delete a progress record.

### Insights
- **GET** `/api/insights/average-workout-duration` - Calculate the average workout duration.
- **GET** `/api/insights/most-frequent-exercises` - Get the most frequently performed exercises.
- **GET** `/api/insights/exercise-progress-trend` - Analyze progress trends for an exercise.
- **GET** `/api/insights/summary` - Get weekly or monthly workout summaries.
- **GET** `/api/insights/weekly-monthly-comparison` - Compare workout data across weeks or months.
- **GET** `/api/insights/daily-progress` - Retrieve progress details for a specific day.
- **GET** `/api/insights/recommendations/underutilized-exercises` - Suggest underutilized exercises based on workout history.
## Project Structure

- **Controllers**: Handle HTTP requests and responses.
- **Services**: Contain business logic for workouts, plans, exercises, progress tracking, and insights.
- **Repositories**: Serve as the data access layer, interacting with the database.
- **Models**: Represent database entities.
- **DTOs**: Define structured data for API requests and responses.
## Testing

Comprehensive unit tests are implemented using **xUnit** to validate:
- Business logic.
- Data consistency.
- CRUD operations.
- Edge cases (e.g., invalid or missing data).

To run tests, use the following command:
```bash
dotnet test
