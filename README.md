# Kupucha (demo)

Blueprint for a mobile-first stock trading marketplace.

Stack: React (Vite), ASP.NET Core 7 API, RabbitMQ, SQL Server (Docker Compose)

Quick run (requires Docker):

```bash
docker-compose up --build
```

Frontend dev (requires Node.js):

```bash
cd frontend
npm install
npm run dev
```

Backend dev (requires .NET 7 SDK):

```bash
cd backend/Kupucha.Api
dotnet run
```

GitHub push (first time)

```bash
git init
git add .
git commit -m "Initial scaffold: backend, frontend, worker, compose"
git remote add origin https://github.com/shivigupta31121988/kupucha-da-annai.git
git branch -M main
git push -u origin main
```
