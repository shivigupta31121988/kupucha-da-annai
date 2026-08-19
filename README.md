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

OAuth & Auth setup

- To enable Google sign-in, create credentials at https://console.developers.google.com, then set `GOOGLE_CLIENT_ID` and `GOOGLE_CLIENT_SECRET` as environment variables (or in `docker-compose.yml` under `backend.environment`).
- For Apple sign-in you must register the App ID and keys with Apple; set `APPLE_CLIENT_ID` and related key configuration in the environment. Apple integration requires additional setup; this scaffold includes a placeholder `OpenIdConnect` configuration.
- Set `JWT_KEY` to a secure secret to sign JWTs (change the default in `docker-compose.yml`).

Authentication flow (demo):
- Visit `http://localhost:3000` and click "Login with Google". The backend will redirect to Google and on success issue a JWT and redirect back to the frontend with `?token=...` in the URL.
- The frontend will store the token in `localStorage` for demo purposes.

