# JurisApp

**Integrantes:** Yair Uriel Pandolfi, Juana Chies Doumecq

JurisApp es una plataforma SaaS orientada a profesionales del derecho en Argentina. Permite gestionar documentos, conversaciones asistidas por inteligencia artificial, casos jurídicos y suscripciones, ofreciendo herramientas para el análisis y organización de información legal.

## Tecnologías

### Backend

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* SQLite (desarrollo)
* PostgreSQL (producción)
* JWT Authentication

### Integraciones

* OpenAI (ChatGPT)
* Stripe

## Arquitectura

El backend sigue los principios de **Clean Architecture**.

```text
backend/
├── Domain/          # Entidades y reglas de negocio
├── Application/     # Casos de uso, DTOs e interfaces
├── Infrastructure/  # Persistencia e integraciones externas
└── Presentation/    # API REST, Swagger y configuración
```

## Requisitos

* .NET 10 SDK
* EF Core CLI

Instalar EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

## Ejecución local

```bash
cd backend/Presentation
dotnet restore
dotnet build
dotnet run
```

La aplicación iniciará utilizando SQLite en entorno de desarrollo.

### URLs por defecto

| Servicio  | URL                           |
| --------- | ----------------------------- |
| API HTTP  | http://localhost:5248         |
| API HTTPS | https://localhost:7212        |
| Swagger   | http://localhost:5248/swagger |

## Base de datos

Aplicar migraciones:

```bash
dotnet ef database update --project ../Infrastructure --startup-project .
```

Crear una nueva migración:

```bash
dotnet ef migrations add NombreMigracion --project ../Infrastructure --startup-project .
```

## Configuración

### Desarrollo

* Base de datos SQLite
* Servicios de IA simulados (Mock)
* Configuración en `appsettings.Development.json`

### Producción

Variables requeridas:

```text
ConnectionStrings__DefaultConnection
Jwt__Secret
AI__OpenAI__ApiKey
AI__OpenAI__Model
```

## Autenticación

La API utiliza autenticación JWT Bearer.

Para probar endpoints protegidos desde Swagger:

1. Iniciar sesión.
2. Obtener el token JWT.
3. Presionar **Authorize**.
4. Ingresar:

```text
Bearer {token}
```

## Funcionalidades principales

* Registro e inicio de sesión.
* Gestión de usuarios y perfiles.
* Gestión de planes y suscripciones.
* Creación y administración de chats.
* Carga y almacenamiento de documentos.
* Análisis de documentos mediante IA.
* Gestión de carpetas y casos jurídicos.
* Solicitud y validación de abogados.
* Creación de skills personalizadas.
* Ejecución de tareas automatizadas asistidas por IA.

## Despliegue

El proyecto incluye un workflow de GitHub Actions para despliegue automático en Azure App Service.

Archivo:

```text
.github/workflows/api-azure-app-service.yml
```

Variables requeridas en GitHub:

```text
AZURE_WEBAPP_NAME
AZURE_WEBAPP_PUBLISH_PROFILE
```

Variables requeridas en Azure:

```text
ASPNETCORE_ENVIRONMENT
ConnectionStrings__DefaultConnection
Jwt__Secret
AI__OpenAI__ApiKey
AI__OpenAI__Model
```

## Seguridad

* No almacenar secretos en el repositorio.
* Utilizar variables de entorno o User Secrets durante el desarrollo.
* Configurar claves JWT seguras en producción.
