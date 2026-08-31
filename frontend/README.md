# JurisApp — Frontend

Interfaz de JurisApp (React + TypeScript + Vite). Consume la API existente; **no modifica el backend**.

## Requisitos

- Node.js 20+
- API local en `http://localhost:5248`

## Desarrollo

En una terminal, API:

```bash
cd backend/Presentation
dotnet run
```

En otra, frontend:

```bash
cd frontend
npm install
npm run dev
```

La app queda en `http://localhost:5173`. Vite proxea `/api` y `/uploads` hacia el backend.

Admin de desarrollo (seed): `admin@jurisapp.local` / `Admin123!`

Los códigos de verificación y enlaces de reset se imprimen en la consola del backend (email mock).

Los planes pagos, en Development, se activan con `POST /api/billing/simulate-purchase` (Stripe mock).

## Build

```bash
npm run build
npm run preview
```

Variable opcional de producción: `VITE_API_URL` (origen de la API, sin barra final).
