# Arquitectura Frontend Angular - CapitalPOS Web

## 1. Objetivo del frontend

El frontend de CapitalPOS Web será una aplicación web moderna, profesional y escalable desarrollada con Angular.

La aplicación Angular será el cliente principal del sistema y consumirá la API ASP.NET Core como backend único.

WinForms queda postergado. La prioridad pasa a ser la aplicación web, sin cambiar la arquitectura general del sistema.

## 2. Versión de Angular

No se fija una versión específica de Angular en este documento.

La versión se definirá al momento de crear el proyecto usando la versión estable más reciente indicada por la documentación oficial de Angular en angular.dev.

Criterio:

* Usar la versión estable más reciente de Angular.
* Usar Angular CLI actualizado.
* Evitar versiones beta, RC o experimentales.
* Confirmar compatibilidad con Node.js, TypeScript, RxJS y PrimeNG antes de iniciar implementación.
* Documentar la versión exacta usada al crear el proyecto frontend.

Comando base esperado:

npm install -g @angular/cli
ng version
ng new capitalpos-web

## 3. Arquitectura general

La aplicación será organizada bajo una arquitectura Feature-Based.

La estructura separará claramente:

* Núcleo global de la aplicación.
* Layout principal.
* Componentes compartidos.
* Features funcionales.
* Servicios de comunicación con API.
* Guards.
* Interceptors.
* Modelos.
* Utilidades.

El objetivo es evitar una aplicación desordenada y preparar el frontend para crecer con módulos como:

* Dashboard.
* CPE / Facturación electrónica.
* Ventas.
* Productos.
* Inventario.
* Compras.
* Caja.
* Reportes.
* Sucursales.
* Usuarios.
* Configuración.

## 4. Principios técnicos

La aplicación utilizará:

* Standalone Components.
* Lazy Loading por feature.
* Signals cuando corresponda.
* RxJS para flujos asíncronos y comunicación HTTP.
* JWT para autenticación futura.
* Http Interceptors.
* Guards.
* SCSS.
* Arquitectura Feature-Based.
* Formularios reactivos.
* Buenas prácticas actuales de Angular.
* Separación entre UI, estado y acceso a datos.

## 5. Estructura de carpetas propuesta

src/
app/
core/
auth/
config/
guards/
interceptors/
models/
services/
utils/

```
layout/
  shell/
  sidebar/
  topbar/
  breadcrumb/
  footer/

shared/
  components/
  directives/
  pipes/
  validators/
  ui/

features/
  auth/
  dashboard/
  cpe/
  ventas/
  productos/
  inventario/
  compras/
  caja/
  reportes/
  configuracion/

app.config.ts
app.routes.ts
```

assets/
environments/
styles/

## 6. Responsabilidad de cada carpeta

core/

Contendrá servicios y configuraciones globales de la aplicación.

Ejemplos:

* Configuración de API.
* Servicio de autenticación.
* Interceptors.
* Guards.
* Manejo global de errores.
* Servicios singleton.
* Modelos globales.

layout/

Contendrá la estructura visual principal de la aplicación.

Ejemplos:

* Shell principal.
* Sidebar.
* Topbar.
* Breadcrumb.
* Footer.
* Menú responsive.

shared/

Contendrá elementos reutilizables sin lógica de negocio fuerte.

Ejemplos:

* Componentes visuales genéricos.
* Pipes.
* Directivas.
* Validadores.
* Componentes UI reutilizables.

features/

Contendrá los módulos funcionales del sistema.

Cada feature tendrá sus propias páginas, componentes, modelos, servicios y rutas.

## 7. Organización interna por feature

Cada feature importante seguirá esta estructura:

features/nombre-feature/
pages/
components/
data-access/
models/
routes/

Ejemplo para CPE:

features/cpe/
pages/
emitir-cpe-page/
historial-cpe-page/
detalle-cpe-page/

components/
cpe-form/
cpe-items-table/
cpe-result-card/

data-access/
cpe-api.service.ts
cpe.facade.ts

models/
emitir-cpe.request.ts
cpe-emision.response.ts
api-response.model.ts

cpe.routes.ts

## 8. Layout principal

La aplicación tendrá un diseño tipo ERP/POS comercial.

Estructura visual:

* Sidebar lateral.
* Topbar superior.
* Área principal de contenido.
* Breadcrumb.
* Panel de usuario.
* Menú de notificaciones.
* Área de acciones rápidas.

Comportamiento responsive:

Desktop:

* Sidebar visible.
* Topbar fija.
* Contenido amplio.

Tablet:

* Sidebar colapsable.
* Tablas con scroll horizontal.

Mobile:

* Menú tipo drawer.
* Acciones principales visibles.
* Formularios adaptados a una columna.

## 9. Sistema de rutas

Rutas base propuestas:

/auth/login
/app/dashboard
/app/cpe/emitir
/app/cpe/historial
/app/cpe/detalle/:id
/app/ventas
/app/productos
/app/inventario
/app/compras
/app/caja
/app/reportes
/app/configuracion

Estrategia:

* /auth será público.
* /app será protegido.
* Cada feature se cargará con lazy loading.
* Las rutas internas se dividirán por responsabilidad.
* Las rutas administrativas requerirán permisos o roles.

## 10. Autenticación y autorización

La API actual usa X-API-KEY para proteger endpoints.

Para el frontend comercial se preparará una arquitectura compatible con JWT.

Fase inicial:

* Interceptor para agregar X-API-KEY en desarrollo.
* Guard básico para rutas protegidas.
* Configuración desde environment.

Fase futura:

* Login real.
* Access Token JWT.
* Refresh Token.
* Auth Guard.
* Role Guard.
* Permission Guard.
* Manejo de sesión expirada.
* Cierre de sesión.
* Persistencia segura del estado de sesión.

Estructura prevista:

core/auth/
auth.service.ts
auth.store.ts
auth.guard.ts
role.guard.ts
permission.guard.ts
token-storage.service.ts

## 11. Comunicación con la API

La comunicación con la API se realizará desde servicios data-access.

El frontend no llamará directamente HttpClient desde componentes de pantalla.

Flujo correcto:

Page Component
-> Facade
-> Api Service
-> HttpClient
-> API ASP.NET Core

Ejemplo para CPE:

features/cpe/data-access/cpe-api.service.ts
features/cpe/data-access/cpe.facade.ts

Endpoints principales iniciales:

POST /api/cpe/emitir
POST /api/cpe/validar
POST /api/cpe/generar-xml
POST /api/cpe/firmar-xml
POST /api/cpe/enviar-sunat
GET /api/cpe/cdr/{nombreCdr}
GET /api/cpe/diagnostico
GET /api/cpe/archivos/zip/info
GET /api/cpe/archivos/cdr/info

## 12. Contrato de respuesta API

La API responde usando el wrapper:

ApiResponse<T>

Estructura general:

* ok
* mensaje
* data
* errores

Para emisión CPE:

ApiResponse<CpeEmisionResponse>

Campos importantes:

* data.ok
* data.estado
* data.mensaje
* data.comprobante
* data.hash
* data.nombreXml
* data.nombreZip
* data.xmlFirmado
* data.nombreCdr
* data.etapas
* data.errores

El frontend debe respetar el contrato ya documentado en:

Docs/ContratoApiEmitirCpe.md

## 13. Manejo de estado

La estrategia de estado será progresiva.

Se usará:

* Signals para estado local de UI.
* Computed signals para valores derivados.
* RxJS para llamadas HTTP y flujos asíncronos.
* Facades para coordinar UI, estado y API.

No se usará inicialmente una librería global pesada de estado.

Criterio:

* Estado pequeño y local: signal.
* Estado derivado: computed.
* Procesos HTTP: Observable.
* Comunicación compleja entre partes: facade.
* Estado global futuro: evaluar solo si el proyecto lo requiere.

## 14. Manejo de errores

Se implementará un manejo de errores consistente.

Elementos previstos:

* Http Error Interceptor.
* Mapeador de errores.
* Mensajes de error amigables.
* Toast global.
* Registro en consola solo en desarrollo.
* Manejo de sesión expirada.
* Manejo de error 401.
* Manejo de error 403.
* Manejo de error 500.
* Manejo de errores de validación.

Tipos de error:

* Error de validación.
* Error de conexión.
* Error de autenticación.
* Error de autorización.
* Error interno de API.
* Error SUNAT.
* Error de firma.
* Error de CDR.

## 15. Notificaciones y carga

La aplicación tendrá:

* Toast para mensajes rápidos.
* Confirm dialogs para acciones críticas.
* Loading global para peticiones HTTP.
* Loading local para botones y formularios.
* Mensajes inline en formularios.
* Estados visuales para éxito, advertencia y error.

Ejemplos:

* Comprobante emitido correctamente.
* XML firmado correctamente.
* SUNAT rechazó el comprobante.
* No se pudo leer el CDR.
* Sesión expirada.
* Error de conexión con API.

## 16. Librería UI

La librería UI propuesta es PrimeNG.

Justificación:

CapitalPOS Web será un sistema ERP/POS con pantallas densas de datos. Se requieren componentes como:

* Tablas avanzadas.
* Paginación.
* Ordenamiento.
* Filtros.
* Diálogos.
* Menús.
* Cards.
* Toast.
* Confirmaciones.
* Dropdowns.
* Calendarios.
* Formularios.
* Tabs.
* Paneles.
* Scroll en tablas.
* Componentes para dashboards.

PrimeNG encaja bien para sistemas administrativos, comerciales y ERP/POS.

Se usará:

* PrimeNG.
* PrimeIcons.
* Tema compatible con la versión actual.
* SCSS personalizado para identidad visual CapitalPOS.

Antes de instalar PrimeNG se verificará compatibilidad con la versión estable actual de Angular.

## 17. Estrategia de estilos

Se usará SCSS.

Estructura propuesta:

styles/
_variables.scss
_layout.scss
_forms.scss
_tables.scss
_buttons.scss
_responsive.scss
styles.scss

Criterios visuales:

* Diseño moderno.
* Interfaz limpia.
* Uso profesional de espacios.
* Colores sobrios.
* Buen contraste.
* Responsive.
* Preparado para pantallas de trabajo diario.
* Evitar apariencia de landing page.
* Priorizar productividad del usuario.

## 18. Tema visual inicial

Estilo esperado:

* Fondo general gris claro.
* Cards blancas.
* Sidebar oscuro o azul profesional.
* Color principal azul/indigo.
* Estados:

  * Verde para éxito.
  * Rojo para error.
  * Ámbar para advertencia.
  * Azul para información.

La UI debe sentirse como un sistema comercial serio, similar a un ERP/POS SaaS.

## 19. Convenciones de nombres

Componentes de página:

emitir-cpe-page.component.ts
historial-cpe-page.component.ts
dashboard-page.component.ts

Componentes internos:

cpe-form.component.ts
cpe-items-table.component.ts
cpe-result-card.component.ts

Servicios:

cpe-api.service.ts
auth.service.ts
notification.service.ts

Facades:

cpe.facade.ts
auth.facade.ts

Modelos:

emitir-cpe.request.ts
cpe-emision.response.ts
api-response.model.ts

Guards:

auth.guard.ts
role.guard.ts
permission.guard.ts

Interceptors:

auth.interceptor.ts
error.interceptor.ts
loading.interceptor.ts

## 20. Reglas de organización del código

Reglas:

* No colocar lógica de negocio compleja en componentes.
* No llamar HttpClient directamente desde páginas.
* No duplicar modelos.
* No mezclar features.
* No crear componentes sin una responsabilidad clara.
* No crear servicios globales innecesarios.
* No subir claves, tokens ni URLs privadas al repositorio.
* Documentar decisiones importantes.
* Probar cada fase antes de continuar.
* Hacer commits pequeños y claros.

## 21. Compatibilidad con backend actual

El frontend será compatible con la API ASP.NET Core existente.

Backend actual:

* API ASP.NET Core.
* Endpoint principal POST /api/cpe/emitir.
* Seguridad por X-API-KEY en desarrollo.
* Firma real con PFX.
* SUNAT simulado o real según configuración.
* Respuestas usando ApiResponse<T>.
* CDR, XML y ZIP generados por backend.

El frontend no generará XML, no firmará comprobantes y no enviará directamente a SUNAT.

Todas esas responsabilidades seguirán en la API.

## 22. Plan de desarrollo por fases

### Fase 0 - Arquitectura frontend

Objetivo:

Definir y documentar arquitectura, estructura, rutas, seguridad, UI, comunicación API y fases.

Resultado esperado:

* Documento ArquitecturaFrontendAngular.md aprobado.
* Decisión técnica validada.
* Sin código todavía.

### Fase 1 - Crear proyecto Angular base

Objetivo:

Crear el proyecto Angular usando la versión estable más reciente.

Incluye:

* Angular CLI actualizado.
* Proyecto con SCSS.
* Standalone.
* Configuración inicial.
* Primer commit.
* Repositorio GitHub.

### Fase 2 - Layout principal

Objetivo:

Crear shell visual del sistema.

Incluye:

* Sidebar.
* Topbar.
* Área de contenido.
* Breadcrumb.
* Responsive base.

### Fase 3 - Core HTTP

Objetivo:

Preparar comunicación base con API.

Incluye:

* Environment.
* Api config.
* HttpClient.
* Auth interceptor.
* Error interceptor.
* Loading interceptor.
* ApiResponse model.

### Fase 4 - Auth base

Objetivo:

Preparar autenticación inicial.

Incluye:

* Login visual.
* Auth service.
* Auth store.
* Guard.
* Manejo temporal de API key.
* Preparación para JWT.

### Fase 5 - Feature CPE

Objetivo:

Crear primera feature funcional conectada al backend.

Incluye:

* Pantalla Emitir CPE.
* Formulario base.
* Tabla de items.
* Consumo POST /api/cpe/emitir.
* Visualización de resultado.
* Visualización de etapas.

### Fase 6 - Historial y CDR

Objetivo:

Consultar archivos y resultados generados.

Incluye:

* Listado de XML.
* Listado de ZIP.
* Listado de CDR.
* Lectura de CDR.
* Estados ACEPTADO, RECHAZADO, SIMULADO.

### Fase 7 - Dashboard

Objetivo:

Crear panel de resumen.

Incluye:

* Tarjetas de estado.
* Últimos comprobantes.
* Accesos rápidos.
* Estado de API.
* Diagnóstico CPE.

### Fase 8 - Productos e inventario

Objetivo:

Iniciar módulos comerciales.

Incluye:

* Productos.
* Categorías.
* Marcas.
* Almacenes.
* Stock.
* Movimientos.

### Fase 9 - Ventas POS web

Objetivo:

Crear flujo de venta web.

Incluye:

* Cliente.
* Items.
* Totales.
* Emisión CPE.
* Resultado SUNAT.
* CDR.

### Fase 10 - Compras, caja y reportes

Objetivo:

Agregar módulos operativos del ERP/POS.

Incluye:

* Compras.
* Caja.
* Reportes.
* Exportaciones.

### Fase 11 - Seguridad avanzada

Objetivo:

Implementar autenticación real.

Incluye:

* JWT.
* Roles.
* Permisos.
* Refresh token.
* Auditoría.
* Sesión expirada.

### Fase 12 - Deploy

Objetivo:

Publicar frontend.

Incluye:

* Build producción.
* Variables de entorno.
* Hosting.
* Conexión con API publicada.
* Pruebas finales.

## 23. Estrategia Git

Reglas:

* Un commit por avance estable.
* No commitear cambios rotos.
* Ejecutar build antes de commit.
* Documentar cambios importantes.
* Mantener main estable.
* Subir a GitHub al cerrar cada fase.

Mensajes sugeridos:

* Documentar arquitectura frontend Angular.
* Crear proyecto Angular base.
* Configurar layout principal.
* Configurar core HTTP.
* Implementar feature CPE inicial.

## 24. Criterio de aprobación antes de implementar

Antes de escribir código del frontend debe quedar aprobado:

* Arquitectura general.
* Estructura de carpetas.
* Versión Angular a usar.
* Librería UI.
* Estrategia de rutas.
* Estrategia de seguridad.
* Estrategia de comunicación API.
* Plan por fases.

Una vez aprobado este documento, recién se inicia la Fase 1.
