# 🌱 Garden Group Incident System

An IT service-desk / incident-ticketing web app for the fictional *Garden Group*, built with **ASP.NET Core MVC (.NET 8)** and **MongoDB**. Employees report incidents, the service desk triages and hands over tickets, and supervisors track everything from a dashboard with statistics and archiving.

> Originally a four-person team project for an Inholland University NoSQL course. This version has been reworked so anyone can clone it and run it locally — the original MongoDB Atlas cloud database has been replaced with a dockerised MongoDB plus seed data, and leaked credentials were removed.

## Screenshots

<!-- Add 2–3 screenshots here (login, ticket list, dashboard/statistics).
     Put images in docs/ and reference them, e.g.:
     ![Tickets](docs/tickets.png) -->

## Features

- Role-based login (supervisor, service desk, regular employee) with session auth
- Create, view, edit and track incident **tickets** (type, priority, deadline, status)
- Ticket **hand-over** workflow between service-desk staff
- **Filtering** by keyword and **sorting** by priority
- Employee management (CRUD) and a supervisor **dashboard with statistics**
- **Archiving** of tickets older than two years into a separate collection
- **Password reset** via secure email link

## Tech stack

- ASP.NET Core MVC, .NET 8, C#
- **MongoDB** (`MongoDB.Driver`), document model with embedded sub-documents
- Layered architecture: **Controllers → Services → Repositories → MongoDB**
- MailKit for password-reset email
- Docker Compose for the database

## Run it locally

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download) and [Docker](https://www.docker.com/products/docker-desktop/).

```bash
# 1. Start MongoDB and load seed data (one command)
docker compose up -d        # seeds the GardenGrup database on first start

# 2. Run the app
cd GardenGroupIncidentSystem
dotnet run
```

Then open the URL printed in the console (e.g. `https://localhost:5001`).

A web GUI for the database is available at <http://localhost:8081> (mongo-express, login `admin` / `admin`).

### Demo accounts

Login is by **first name + password**:

| Role          | First name | Password   | Lands on   |
|---------------|------------|------------|------------|
| Supervisor    | Alice      | admin123   | Home       |
| Service desk  | Bob        | desk123    | Home       |
| Regular staff | Carol      | user123    | Dashboard  |

### Reset the database

```bash
docker compose down -v   # removes the data volume
docker compose up -d     # recreates and re-seeds
```

### Email (optional)

Password-reset emails are disabled by default (no SMTP credentials). The app runs fine without them. To enable real emails, put your own SMTP username/password in `.env` (see `.env.example`) — never commit real credentials.

## My contribution

This was a four-person team project. I (**YuChang Huang**) built the **forgot-password / password-reset feature** end to end:

- Request-a-reset flow that generates a secure, time-limited token (`PasswordResetTokenService`)
- Email delivery of the reset link (`EmailService`, MailKit)
- The reset-password page and token validation (`PasswordResetService`, `PasswordResetController`)

I also did the work to make this repository runnable locally (dockerised MongoDB, seed data, removing leaked credentials, documentation).

## Team & acknowledgements

Garden Group Incident System was built by a four-person student team for an Inholland University course. Original feature work:

- **Gabriele Amorosi** — ticket sorting by priority
- **Puok Kamler** — ticket filtering by keyword
- **YuChang Huang** — password reset workflow
- **Abdullah Abdullah** — archiving of old tickets

This repository is my maintained, locally-runnable version. Original group repository: <https://github.com/MrGablo/Nosql-GardenGroup-Group3>.

## License

Released under the MIT License — see [LICENSE](LICENSE).
