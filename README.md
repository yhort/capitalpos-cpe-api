# 🚀 CapitalPOS CPE API

> Backend desarrollado en **ASP.NET Core** para la emisión, validación y gestión de comprobantes electrónicos SUNAT.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-7-blue)
![C#](https://img.shields.io/badge/C%23-.NET-success)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Npgsql-336791)
![REST API](https://img.shields.io/badge/REST-API-orange)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-green)

---

# 📖 Descripción

CapitalPOS CPE API forma parte de la evolución tecnológica de **CapitalPOS**, un sistema comercial desarrollado durante más de **7 años**, utilizado para la gestión de ventas, inventarios y facturación electrónica.

El objetivo del proyecto es desacoplar completamente la lógica de Facturación Electrónica del sistema Windows Forms para convertirla en una API REST reutilizable por aplicaciones Desktop, Web y Mobile.

Actualmente este backend forma parte de la migración tecnológica del producto hacia una arquitectura moderna basada en **ASP.NET Core**.

---

# 💡 ¿Por qué este proyecto?

CapitalPOS nació como una aplicación Windows Forms.

Conforme fue creciendo el producto surgió la necesidad de desacoplar toda la lógica de Facturación Electrónica para que pudiera ser utilizada por distintos clientes:

- Windows Forms
- Aplicaciones Web
- Aplicaciones móviles
- Integraciones con terceros

Por ello se desarrolló esta API REST utilizando ASP.NET Core.

---

# 🛠 Tecnologías

- ASP.NET Core 7
- C#
- PostgreSQL (Npgsql / EF Core)
- REST API
- Swagger / OpenAPI
- Dependency Injection
- Middleware
- Arquitectura por Capas
- Git
- GitHub

---

# 🗄 Base de datos

El motor oficial del microservicio es **PostgreSQL**, con el proveedor **Npgsql.EntityFrameworkCore.PostgreSQL**.

Cadena de conexión de ejemplo (`ConnectionStrings:CpeDb` en `appsettings.json`):

```text
Host=localhost;Port=5432;Database=CapitalPosCpeDb;Username=postgres;Password=postgres
```

Ejemplo de conexión con `psql`:

```bash
psql "Host=localhost;Port=5432;Database=CapitalPosCpeDb;Username=postgres;Password=postgres"
```

Al arrancar la API (excepto el entorno `Testing`), EF Core aplica las migraciones automáticamente con `Database.Migrate()`.

Para regenerar la migración inicial en desarrollo:

```bash
dotnet ef migrations add InitialMultiTenant \
  --project CapitalPos.Cpe.Api/CapitalPos.Cpe.Api.csproj \
  --startup-project CapitalPos.Cpe.Api/CapitalPos.Cpe.Api.csproj \
  --context CpeDbContext \
  --output-dir Data/Migrations
```

---

# 🏗 Arquitectura General

```mermaid
flowchart LR

WF["CapitalPOS<br/>Windows Forms"]

WEB["Aplicación Web<br/>Angular"]

APP["Aplicación Móvil<br/>Flutter"]

API["CapitalPOS CPE API<br/>ASP.NET Core"]

PG["PostgreSQL"]

XML["Generación XML"]

FIRMA["Firma Digital"]

SUNAT["SUNAT"]

CDR["Recepción CDR"]

WF --> API

WEB --> API

APP --> API

API --> PG

API --> XML

API --> FIRMA

API --> SUNAT

SUNAT --> CDR
```

---

# 🏛 Arquitectura Interna

```mermaid
flowchart TB

Controllers

Services

Interfaces

Infrastructure

Domain

DTOs

Settings

Middlewares

Helpers

Controllers --> Services

Services --> Interfaces

Interfaces --> Infrastructure

Infrastructure --> Domain

Services --> DTOs

Services --> Helpers

Program --> Middlewares

Middlewares --> Controllers
```

---

# 📸 Capturas

## Swagger

Visualización de todos los endpoints disponibles.

![Swagger](screenshots/01-swagger-endpoints.png)

---

## Health Check

Estado de la API.

![Health](screenshots/02-health-check.png)

---

## Validación de Comprobantes

Validación de datos antes de generar el XML.

![Validación](screenshots/03-validacion-cpe.png)

---

## Generación de XML

Proceso de generación del comprobante electrónico.

![XML](screenshots/04-generar-xml.png)

---

## Arquitectura del Proyecto

Estructura del proyecto en Rider.

![Arquitectura](screenshots/05-arquitectura-proyecto.png)

---

# 📂 Organización del Proyecto

```
CapitalPos.Cpe.Api
│
├── Controllers
├── Docs
├── Domain
├── DTOs
├── Helpers
├── Infrastructure
│   ├── Firma
│   ├── Storage
│   ├── Sunat
│   └── Xml
├── Interfaces
├── Legacy
├── Mappers
├── Middlewares
├── Services
├── Settings
└── Program.cs
```

---

# ✅ Funcionalidades

## Gestión de Comprobantes

- Validación de comprobantes electrónicos
- Generación XML UBL
- Firma Digital (Modo Simulación)
- Envío SUNAT (Modo Simulación)
- Gestión de archivos XML
- Historial de operaciones

---

## Seguridad

- API Key Authentication
- Middleware personalizado
- Correlation ID
- Global Exception Middleware

---

## Diagnóstico

- Health Check
- Diagnóstico
- Historial
- Configuración

---

# 🌐 Endpoints

| Método | Endpoint | Descripción |
|---------|----------|-------------|
| GET | `/api/health` | Estado de la API |
| POST | `/api/cpe/validar` | Validación |
| POST | `/api/cpe/generar-xml` | Generación XML |
| POST | `/api/cpe/demo/emitir` | Emisión Demo |
| GET | `/api/cpe/config` | Configuración |
| GET | `/api/cpe/diagnostico` | Diagnóstico |

---

# 🚧 Estado del Proyecto

Proyecto en desarrollo activo.

Actualmente forma parte de la migración tecnológica de **CapitalPOS**, evolucionando desde Windows Forms hacia una arquitectura basada en APIs REST utilizando ASP.NET Core.

---

# 🗺 Roadmap

- ✅ Arquitectura Base
- ✅ Controllers
- ✅ Services
- ✅ Dependency Injection
- ✅ Health Check
- ✅ Swagger
- ✅ API Key Authentication
- ✅ Validación
- ✅ Generación XML
- ✅ Historial
- 🔄 Firma Digital Producción
- 🔄 Integración SUNAT Producción
- 🔄 Recepción CDR
- 🔄 OAuth / JWT
- 🔄 Frontend Angular
- 🔄 Aplicación Flutter

---

# 👨‍💻 Autor

## Yhort Anthony Cruz Arbañil

Full Stack Developer

Especialista en:

- ASP.NET Core
- C#
- PostgreSQL
- Windows Forms
- Angular
- REST APIs
- Facturación Electrónica SUNAT

---

## ⭐ Evolución del Proyecto

```text
CapitalPOS Windows Forms
            │
            ▼
 Migración de Facturación Electrónica
            │
            ▼
     CapitalPOS CPE API
            │
            ▼
      Angular + Flutter
```
