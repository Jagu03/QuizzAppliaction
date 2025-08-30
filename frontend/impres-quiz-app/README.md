# Impres Quiz – React Frontend (Web + Mobile Responsive)

This is a Vite + React + TypeScript + Material UI app for your Quiz backend.

## Features
- **Landing** with Host / Player paths
- **Host**: dev-login by UserId → choose published quiz → create session → lobby (players) → start/end/cancel
- **Player**: join by PIN → play (per-question timer, client-side) → submit answer → view leaderboard
- **Admin (read)**: list quizzes, view questions & choices (extendable to full CRUD)

> Auth is a stub (enter host UserId). Replace with JWT later.

## Run
```bash
npm install
cp .env.sample .env      # set VITE_API_BASE_URL to your API
npm run dev
```
Open http://localhost:5173

## Build
```bash
npm run build
npm run preview
```

## Structure
```
src/
  api/         axios client + endpoints
  components/  shared UI (NavBar)
  pages/       route pages (host/, player/)
  state/       (reserved for context/store if needed)
```

## Mobile
The UI is responsive. To package as a mobile app, you can add Capacitor:
```bash
npm i @capacitor/core @capacitor/cli
npx cap init impres-quiz com.impres.quiz
npm run build
npx cap add android
npx cap open android
```