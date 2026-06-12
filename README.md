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
