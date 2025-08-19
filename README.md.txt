# QuizzApplication

## Project Structure
- **backend/** : .NET Core Web API (C#)
- **frontend/** : ReactJS app
- **db/** : SQL Server scripts (schema, stored procedures, seed data)

## Setup Instructions

### Backend (API)
1. Open terminal
2. `cd backend`
3. `dotnet restore`
4. `dotnet run`

### Frontend (React)
1. Open terminal
2. `cd frontend`
3. `npm install`
4. `npm start`

### Database (SQL)
1. Open SQL Server Management Studio (SSMS)
2. Run scripts from `db/schema` → `db/procs` → `db/seed` (in order).
