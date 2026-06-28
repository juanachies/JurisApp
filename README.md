# JurisApp

**Integrantes:** Yair Uriel Pandolfi, Juana Chies Doumecq

Plataforma SaaS para abogados en Argentina.

## Backend (.NET Clean Architecture)

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Herramienta EF Core (una sola vez):

```bash
dotnet tool install --global dotnet-ef
```

### Cómo correr el backend

```bash
cd backend/Presentation
dotnet restore
dotnet build
dotnet run
```

En **Development** el backend usa **SQLite** (`jurisapp.dev.db` en la carpeta `Presentation`). Al iniciar, aplica migraciones y ejecuta seed de datos automáticamente.

URLs por defecto:

- API HTTP: `http://localhost:5248`
- API HTTPS: `https://localhost:7212`
- Swagger: `http://localhost:5248/swagger`
- Interfaz de prueba: `http://localhost:5248/test.html`

### Crear / actualizar la base de datos manualmente

```bash
cd backend/Presentation
dotnet ef database update --project ../Infrastructure --startup-project .
```

Para crear una nueva migración:

```bash
dotnet ef migrations add NombreMigracion --project ../Infrastructure --startup-project .
```

### Configuración

| Entorno | Base de datos | IA |
|---------|---------------|-----|
| Development | SQLite (`appsettings.Development.json`) | Mock (`AI:UseMock: true`) |
| Production | PostgreSQL (`ConnectionStrings:DefaultConnection`) | Claude (`AI:UseMock: false` + `AI:Claude:ApiKey`) |

**Development** incluye en `appsettings.Development.json`:

- JWT secret de desarrollo (no usar en producción)
- Connection string SQLite
- Mock de IA activado

**Nota:** Los user-secrets tienen prioridad sobre `appsettings.Development.json`. Si `Jwt:Secret` en user-secrets tiene menos de 32 caracteres, JWT fallará. Eliminá o actualizá esa clave si ocurre.

**Producción** requiere configurar (variables de entorno o user-secrets):

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `AI__Claude__ApiKey` y `AI__Claude__Model` (si `AI__UseMock=false`)

### Deploy API a Azure App Service con GitHub Actions

El workflow está en `.github/workflows/api-azure-app-service.yml`. En cada push a `main`, o manualmente con `workflow_dispatch`, ejecuta:

1. `dotnet restore`
2. `dotnet build --configuration Release`
3. `dotnet publish --configuration Release`
4. Deploy a Azure App Service con `azure/webapps-deploy`

Secrets necesarios en GitHub (`Settings` -> `Secrets and variables` -> `Actions`):

- `AZURE_WEBAPP_NAME`: nombre del Azure App Service donde se despliega la API.
- `AZURE_WEBAPP_PUBLISH_PROFILE`: contenido completo del publish profile descargado desde Azure App Service.

Configuración runtime necesaria en Azure App Service (`Configuration` -> `Application settings`):

- `ASPNETCORE_ENVIRONMENT`: `Production`
- `ConnectionStrings__DefaultConnection`: connection string PostgreSQL de producción.
- `Jwt__Secret`: secreto JWT fuerte, mínimo 32 caracteres.
- `Jwt__Issuer`: por ejemplo `JurisApp`.
- `Jwt__Audience`: por ejemplo `JurisApp`.
- `AI__UseMock`: `false` para usar IA real, o `true` para respuestas simuladas.
- `AI__Claude__Enabled`: `true` si se usa Claude real.
- `AI__Claude__ApiKey`: API key de Claude, solo si `AI__UseMock=false`.
- `AI__Claude__Model`: modelo de Claude a usar.
- `Stripe__UseMock`: `false` para Stripe real.
- `Stripe__SecretKey`: secret key de Stripe, si se habilita Stripe real.
- `Stripe__WebhookSecret`: webhook secret de Stripe, si se habilita Stripe real.
- `Stripe__SuccessUrl`: URL de éxito del checkout.
- `Stripe__CancelUrl`: URL de cancelación del checkout.

No subir secretos reales al repositorio. GitHub Actions usa solo secrets y Azure usa variables de entorno.

### Seed de desarrollo

Al levantar en Development se crean automáticamente:

- Planes: **Free**, **Pro**, **Max**
- Usuario admin: `admin@jurisapp.local` / `Admin123!`

### Flujo básico de prueba

1. Abrir `http://localhost:5248/test.html` o Swagger
2. **Register** con email y contraseña
3. **Login** y copiar el token (se guarda en localStorage en test.html)
4. **Get me** para verificar autenticación
5. **Crear perfil abogado** (necesario para carpetas y custom skills)
6. **Crear chat**
7. **Subir documento** (necesitás el Chat ID)
8. **Analizar documento** (devuelve análisis simulado)

En Swagger: clic en **Authorize**, pegar `Bearer {tu-token}`.

### Billing / Stripe (modo test)

Suscripciones **Pro** y **Max** se compran vía Stripe Checkout. El plan **Free** sigue activándose con `POST /api/plans/{planId}/subscribe`.

#### Modo simulado (sin Stripe)

En Development, `appsettings.Development.json` trae `"Stripe": { "UseMock": true }`. Con eso podés activar Pro/Max **sin keys ni Stripe CLI**:

1. Login en `test.html`
2. **Listar planes** → se autocompleta el Id de Pro o Max
3. **Simular compra (sin Stripe)** → activa la suscripción al instante
4. **Ver suscripción activa** para confirmar

Endpoint: `POST /api/billing/simulate-purchase` (solo Development + `Stripe:UseMock=true`).

Para activar Stripe real más adelante: `"Stripe:UseMock": false` y configurar secretos abajo.

#### Stripe real (cuando lo actives)

**Configurar secretos locales** (desde `backend/Presentation`):

```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
```

**Escuchar webhooks en local** (requiere [Stripe CLI](https://stripe.com/docs/stripe-cli)):

```bash
stripe login
stripe listen --forward-to http://localhost:5248/api/billing/webhook
```

Copiá el `whsec_...` que muestra el CLI al user-secret `Stripe:WebhookSecret`.

**Cargar Price IDs en la base de datos** (después de crear productos/precios en el [Dashboard de Stripe (test)](https://dashboard.stripe.com/test/products)):

```sql
UPDATE Plans SET StripePriceId = 'price_XXXXX', StripeProductId = 'prod_XXXXX' WHERE Type = 'Pro';
UPDATE Plans SET StripePriceId = 'price_YYYYY', StripeProductId = 'prod_YYYYY' WHERE Type = 'Max';
```

**Probar desde test.html** (`http://localhost:5248/test.html`):

1. Login con un usuario
2. **Listar planes** → copiar Id de Pro o Max
3. **Crear checkout Stripe** → abrir la URL en el navegador
4. Pagar con tarjeta de prueba: `4242 4242 4242 4242`, fecha futura, CVC cualquiera
5. **Ver suscripción activa** para confirmar

**Endpoints de billing:**

| Método | Ruta | Auth |
|--------|------|------|
| POST | `/api/billing/create-checkout-session` | JWT |
| POST | `/api/billing/simulate-purchase` | JWT (solo dev + UseMock) |
| POST | `/api/billing/webhook` | Stripe signature |

### Estructura del backend

```
backend/
  Domain/          # Entidades, enums (sin dependencias externas)
  Application/     # Services, DTOs, interfaces, Result
  Infrastructure/  # EF Core, repositorios, JWT, IA, archivos
  Presentation/    # API, Swagger, test.html
```

### Cambios realizados (reparación backend)

- SQLite para desarrollo local; PostgreSQL para producción
- `MockAIService` para probar IA sin API externa
- Migración EF `InitialCreate` + seed de planes y admin
- Swagger con autenticación JWT Bearer
- `wwwroot/test.html` para pruebas manuales
- Endpoints agregados: activar/desactivar skills, cancelar tarea IA, GET documento/tarea por id, PUT perfil abogado/carpeta, reject verificación
- Correcciones: HTTP 401 en errores Unauthorized, registro devuelve 201 correctamente

### Pendientes

- Integración real con Claude (`AI:UseMock=false`)
- Entidad `Audit` configurada en EF pero sin uso en Application
- `PagedResult<T>` definido pero no usado
- PostgreSQL en producción requiere connection string y migraciones aplicadas al servidor
