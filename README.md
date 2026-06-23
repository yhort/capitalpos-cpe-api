# CapitalPOS CPE API

API REST desarrollada en ASP.NET Core para la emisión, validación y gestión de comprobantes electrónicos SUNAT.

## Descripción

CapitalPOS CPE API forma parte de la evolución tecnológica de CapitalPOS, una solución comercial desarrollada para la gestión de ventas, inventarios y facturación electrónica.

El proyecto tiene como objetivo desacoplar la lógica de emisión electrónica del sistema de escritorio tradicional, permitiendo su consumo desde aplicaciones Web, Desktop y Mobile.

## Tecnologías

* ASP.NET Core
* C#
* REST API
* SQL Server
* Swagger / OpenAPI
* Dependency Injection
* Arquitectura por Capas

## Arquitectura

El proyecto está organizado en las siguientes capas:

* Controllers
* Services
* Interfaces
* Infrastructure
* Domain
* DTOs
* Settings
* Middlewares
* Helpers

## Funcionalidades Implementadas

### Gestión de Comprobantes

* Validación de comprobantes
* Generación de XML
* Firma digital (modo simulación)
* Envío a SUNAT (modo simulación)
* Gestión de archivos XML

### Seguridad

* API Key Authentication
* Middleware de validación
* Manejo global de excepciones
* Correlation ID

### Diagnóstico y Monitoreo

* Health Check
* Configuración de entorno
* Historial de operaciones
* Diagnóstico de emisión

## Endpoints Principales

### Salud del servicio

GET /api/health

### Validación de comprobantes

POST /api/cpe/validar

### Generación de XML

POST /api/cpe/generar/generar-xml

### Emisión de comprobantes

POST /api/cpe/demo/emitir

## Estado del Proyecto

Proyecto en desarrollo activo.

Actualmente forma parte de la migración de CapitalPOS desde una arquitectura Windows Forms hacia una arquitectura moderna basada en APIs REST utilizando ASP.NET Core.

## Próximas Funcionalidades

* Integración completa SUNAT
* Gestión de CDR
* OAuth Authentication
* Dashboard de monitoreo
* Frontend Angular
* Aplicaciones móviles

## Autor

Yhort Anthony Cruz Arbañil

Full Stack Developer

C# | ASP.NET Core | SQL Server | Angular | Facturación Electrónica SUNAT
