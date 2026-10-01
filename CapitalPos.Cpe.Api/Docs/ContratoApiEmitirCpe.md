# Contrato interno CPE - Emitir comprobante

Este documento define el contrato interno actual de `capitalpos-cpe-api` para
`POST /api/cpe/emitir`. El consumidor directo esperado es `capitalpos-api`,
que actua como fachada publica hacia Angular y normaliza la respuesta antes de
exponerla al frontend.

Este contrato describe el comportamiento real observado en:

- `Controllers/CpeController.cs`
- `Dtos/ApiResponse.cs`
- `Dtos/CpeEmisionResponse.cs`
- `Dtos/EmitirCpeRequest.cs`
- `Domain/CpeEstados.cs`
- `Services/CpeEmisionService.cs`

## Endpoint

```http
POST /api/cpe/emitir
Content-Type: application/json
X-API-KEY: <api-key-interna>
X-Correlation-ID: <opcional>
```

`X-Correlation-ID` es opcional. Si no llega, el middleware genera uno y lo
devuelve en la respuesta como `X-Correlation-ID`.

## Headers requeridos

- `X-API-KEY`: requerido cuando `CpeSecuritySettings.ApiKeyEnabled` es `true`.
  El nombre del header viene de `CpeSecuritySettings.HeaderName`; por defecto
  es `X-API-KEY`.
- `Content-Type: application/json`: requerido para serializar el request.

Rutas publicas como `/api/health` y `/swagger` pueden quedar fuera de API key
segun `CpeSecuritySettings.RutasPublicas`; `/api/cpe/emitir` no debe estar en
esa lista.

## Seguridad del endpoint

Este servicio debe permanecer interno. `capitalpos-web` no debe llamar a
`capitalpos-cpe-api` ni conocer la API key.

Si falta la API key o es invalida, el middleware responde antes de llegar al
controlador:

```json
{
  "ok": false,
  "mensaje": "No autorizado.",
  "data": null,
  "errores": [
    "Debe enviar el header X-API-KEY."
  ]
}
```

## Request principal

El cuerpo es `EmitirCpeRequest`.

Campos principales:

- `rucEmisor`: RUC del emisor. Tambien puede derivarse de `emisor.ruc`.
- `emisor`: datos fiscales y direccion del emisor.
- `tipoComprobante`: codigo SUNAT del comprobante: `01` factura, `03`
  boleta o `07` nota de credito.
- `serie`: serie del comprobante, por ejemplo `F001`, `B001` o `BC01`.
- `correlativo`: numero correlativo.
- `fechaEmision`: fecha de emision.
- `moneda`: moneda, por defecto `PEN`.
- `tipoOperacion`: tipo de operacion SUNAT, por defecto `0101`.
- `observacion`: texto opcional.
- `formaPago`: por defecto `CONTADO`.
- `montoPendientePago`: monto pendiente si aplica.
- `cuotas`: cuotas de pago si aplica.
- `cliente`: receptor del comprobante.
- `items`: detalle de productos o servicios.
- `totalGravada`, `totalExonerada`, `totalInafecta`, `totalIgv`, `total`:
  totales monetarios.
- `montoEnLetras`: total expresado en letras.
- `codigoMotivo`: obligatorio si `tipoComprobante` es `07`. Codigo del
  Catalogo 09 SUNAT (ej. `01` Anulacion de la operacion).
- `descripcionMotivo`: obligatorio si `tipoComprobante` es `07`. Sustento
  textual del motivo.
- `documentoReferencia`: obligatorio si `tipoComprobante` es `07`.
  - `tipoComprobante`: documento afectado (`01` o `03`).
  - `serieCorrelativo`: identificador `SERIE-NUMERO` (ej. `B001-41458`).

Reglas de serie para nota de credito (`07`):

- Si el documento afectado es boleta (`03`), la serie de la NC debe iniciar
  con `B` (ej. `BC01`).
- Si el documento afectado es factura (`01`), la serie de la NC debe iniciar
  con `F` (ej. `FC01`).

Ejemplo minimo funcional:

```json
{
  "rucEmisor": "20123456789",
  "emisor": {
    "ruc": "20123456789",
    "razonSocial": "CAPITALPOS DEMO SAC",
    "nombreComercial": "CAPITALPOS",
    "ubigeo": "150101",
    "direccion": "AV. DEMO 123",
    "departamento": "LIMA",
    "provincia": "LIMA",
    "distrito": "LIMA"
  },
  "tipoComprobante": "01",
  "serie": "F001",
  "correlativo": 1,
  "fechaEmision": "2026-07-10T00:00:00",
  "moneda": "PEN",
  "tipoOperacion": "0101",
  "formaPago": "CONTADO",
  "cliente": {
    "tipoDocumento": "6",
    "numeroDocumento": "20600000001",
    "razonSocial": "CLIENTE DEMO SAC"
  },
  "items": [
    {
      "codigo": "P001",
      "descripcion": "Producto demo",
      "unidadMedida": "NIU",
      "cantidad": 1,
      "valorUnitario": 100,
      "precioUnitario": 118,
      "subtotal": 100,
      "igv": 18,
      "total": 118,
      "codigoAfectacionIgv": "10"
    }
  ],
  "totalGravada": 100,
  "totalExonerada": 0,
  "totalInafecta": 0,
  "totalIgv": 18,
  "total": 118,
  "montoEnLetras": "CIENTO DIECIOCHO Y 00/100 SOLES"
}
```

Ejemplo minimo de nota de credito (`07`) que anula una boleta:

```json
{
  "rucEmisor": "20123456789",
  "emisor": {
    "ruc": "20123456789",
    "razonSocial": "CAPITALPOS DEMO SAC",
    "nombreComercial": "CAPITALPOS",
    "ubigeo": "150101",
    "direccion": "AV. DEMO 123",
    "departamento": "LIMA",
    "provincia": "LIMA",
    "distrito": "LIMA"
  },
  "tipoComprobante": "07",
  "serie": "BC01",
  "correlativo": 1,
  "fechaEmision": "2026-08-06T18:00:00",
  "moneda": "PEN",
  "tipoOperacion": "0101",
  "formaPago": "CONTADO",
  "codigoMotivo": "01",
  "descripcionMotivo": "Anulacion de la operacion",
  "documentoReferencia": {
    "tipoComprobante": "03",
    "serieCorrelativo": "B001-41458"
  },
  "cliente": {
    "tipoDocumento": "1",
    "numeroDocumento": "12345678",
    "razonSocial": "CLIENTE BOLETA"
  },
  "items": [
    {
      "codigo": "P001",
      "descripcion": "Producto demo",
      "unidadMedida": "NIU",
      "cantidad": 1,
      "valorUnitario": 100,
      "precioUnitario": 118,
      "subtotal": 100,
      "igv": 18,
      "total": 118,
      "codigoAfectacionIgv": "10"
    }
  ],
  "totalGravada": 100,
  "totalExonerada": 0,
  "totalInafecta": 0,
  "totalIgv": 18,
  "total": 118
}
```

## Response principal

El endpoint responde `ApiResponse<CpeEmisionResponse>`.

Forma general:

```json
{
  "ok": true,
  "mensaje": "Comprobante emitido correctamente.",
  "data": {
    "ok": true,
    "estado": "SIMULADO",
    "mensaje": "Comprobante aceptado en modo simulacion.",
    "comprobante": "F001-1",
    "hash": "hash-sha256-del-xml",
    "nombreXml": "20123456789-01-F001-1.xml",
    "nombreZip": "20123456789-01-F001-1.zip",
    "xmlFirmado": true,
    "nombreCdr": "R-20123456789-01-F001-1.zip",
    "fechaProceso": "2026-07-10T12:00:00",
    "etapas": [],
    "errores": []
  },
  "errores": []
}
```

La serializacion HTTP usa camelCase por la configuracion por defecto de ASP.NET
Core.

## Campos de ApiResponse

- `ok`: resultado del wrapper HTTP.
- `mensaje`: mensaje general del endpoint.
- `data`: `CpeEmisionResponse` cuando la emision llega a
  `CpeEmisionService`, tanto si el resultado funcional es exitoso como si es
  un error controlado. En errores de middleware o excepciones globales puede
  ser `null`.
- `errores`: lista simple de errores. En respuestas de error contiene
  mensajes planos.

## Campos de CpeEmisionResponse

- `ok`: resultado funcional de la emision CPE.
- `estado`: estado CPE interno.
- `mensaje`: mensaje funcional del proceso.
- `comprobante`: serie y correlativo, por ejemplo `F001-1`.
- `hash`: hash SHA-256 calculado desde el XML generado.
- `nombreXml`: nombre del XML generado o firmado, segun etapa alcanzada.
- `nombreZip`: nombre del ZIP enviado o preparado para SUNAT.
- `xmlFirmado`: indica si el XML fue firmado.
- `nombreCdr`: nombre del CDR si existe.
- `fechaProceso`: fecha y hora de procesamiento.
- `etapas`: lista de etapas ejecutadas.
- `errores`: lista simple de errores internos/controlados.

Campos de cada item de `etapas`:

- `etapa`: `INICIO`, `VALIDACION`, `XML`, `FIRMA`, `ZIP`, `SUNAT`, `CDR` o
  `ERROR_INTERNO`.
- `ok`: resultado de la etapa.
- `mensaje`: detalle de la etapa.
- `fecha`: fecha y hora de la etapa.

## Estados posibles

Definidos en `CpeEstados`:

- `SIMULADO`: emision aceptada en modo simulacion SUNAT.
- `ACEPTADO`: emision aceptada segun CDR real o proveedor SUNAT.
- `RECHAZADO`: rechazo funcional del comprobante.
- `OBSERVADO`: comprobante observado.
- `PENDIENTE`: comprobante pendiente.
- `ERROR_VALIDACION`: request o comprobante con datos invalidos.
- `ERROR_XML`: error al generar o validar XML.
- `ERROR_FIRMA`: error al firmar XML.
- `ERROR_SUNAT`: error funcional/controlado al enviar o interpretar SUNAT.
- `ERROR_CDR`: error al guardar, leer o interpretar CDR.
- `ERROR_INTERNO`: excepcion tecnica inesperada dentro de CPE API.

`capitalpos-api` actualmente publica un subconjunto canonico y puede conservar
estados adicionales solo si son seguros.

## Politica HTTP actual de capitalpos-cpe-api

- `200 OK`: `CpeEmisionResponse.Ok == true`. Incluye `SIMULADO` y, si aplica,
  `ACEPTADO`.
- `400 Bad Request`: `CpeEmisionResponse.Ok == false` para errores funcionales
  o controlados distintos de `ERROR_INTERNO`, por ejemplo
  `ERROR_VALIDACION`, `ERROR_XML`, `ERROR_FIRMA`, `ERROR_SUNAT`,
  `ERROR_CDR` o `RECHAZADO`.
- `500 Internal Server Error`: cuando el servicio devuelve
  `Estado == ERROR_INTERNO`, o cuando el middleware de API key detecta
  configuracion invalida.
- `401 Unauthorized`: falta `X-API-KEY` o la API key es invalida.

Para `400` y `500` generados por `CpeController.Emitir`, el body conserva el
`CpeEmisionResponse` en `data`. Esto permite que `capitalpos-api` lea
`data.estado`, `data.mensaje`, `data.etapas` y `data.errores` sin inferir el
estado desde mensajes planos.

## Ejemplos JSON

Los ejemplos reflejan el contrato HTTP actual. En los casos de error,
`data.ok` es `false` y `data.estado` conserva el estado interno de CPE.

### SIMULADO

HTTP `200 OK`.

```json
{
  "ok": true,
  "mensaje": "Comprobante emitido correctamente.",
  "data": {
    "ok": true,
    "estado": "SIMULADO",
    "mensaje": "Comprobante aceptado en modo simulacion. Modo: BETA. URL SUNAT: https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService",
    "comprobante": "F001-1",
    "hash": "abc123",
    "nombreXml": "20123456789-01-F001-1.xml",
    "nombreZip": "20123456789-01-F001-1.zip",
    "xmlFirmado": true,
    "nombreCdr": "R-20123456789-01-F001-1.zip",
    "fechaProceso": "2026-07-10T12:00:00",
    "etapas": [
      {
        "etapa": "INICIO",
        "ok": true,
        "mensaje": "Inicio de emision del comprobante F001-1.",
        "fecha": "2026-07-10T12:00:00"
      },
      {
        "etapa": "SUNAT",
        "ok": true,
        "mensaje": "Comprobante aceptado en modo simulacion.",
        "fecha": "2026-07-10T12:00:01"
      }
    ],
    "errores": []
  },
  "errores": []
}
```

### ACEPTADO

HTTP `200 OK`. Aplica cuando `SimularEnvioSunat` es `false` y el CDR real se
interpreta como aceptado.

```json
{
  "ok": true,
  "mensaje": "Comprobante emitido correctamente.",
  "data": {
    "ok": true,
    "estado": "ACEPTADO",
    "mensaje": "Aceptado por SUNAT.",
    "comprobante": "F001-2",
    "hash": "def456",
    "nombreXml": "20123456789-01-F001-2.xml",
    "nombreZip": "20123456789-01-F001-2.zip",
    "xmlFirmado": true,
    "nombreCdr": "R-20123456789-01-F001-2.zip",
    "fechaProceso": "2026-07-10T12:05:00",
    "etapas": [
      {
        "etapa": "SUNAT",
        "ok": true,
        "mensaje": "Aceptado por SUNAT.",
        "fecha": "2026-07-10T12:05:01"
      }
    ],
    "errores": []
  },
  "errores": []
}
```

### RECHAZADO

HTTP `400 Bad Request`. Estado interno esperado: `RECHAZADO`.

```json
{
  "ok": false,
  "mensaje": "El comprobante fue rechazado.",
  "data": {
    "ok": false,
    "estado": "RECHAZADO",
    "mensaje": "El comprobante fue rechazado.",
    "comprobante": "F001-3",
    "hash": null,
    "nombreXml": null,
    "nombreZip": null,
    "xmlFirmado": false,
    "nombreCdr": null,
    "fechaProceso": "2026-07-10T12:10:00",
    "etapas": [
      {
        "etapa": "SUNAT",
        "ok": false,
        "mensaje": "El comprobante fue rechazado.",
        "fecha": "2026-07-10T12:10:01"
      }
    ],
    "errores": [
      "El RUC del receptor no existe."
    ]
  },
  "errores": [
    "El RUC del receptor no existe."
  ]
}
```

### ERROR_VALIDACION

HTTP `400 Bad Request`. Estado interno generado por el servicio:
`ERROR_VALIDACION`.

```json
{
  "ok": false,
  "mensaje": "No se puede emitir el comprobante porque tiene errores de validacion.",
  "data": {
    "ok": false,
    "estado": "ERROR_VALIDACION",
    "mensaje": "No se puede emitir el comprobante porque tiene errores de validacion.",
    "comprobante": null,
    "hash": null,
    "nombreXml": null,
    "nombreZip": null,
    "xmlFirmado": false,
    "nombreCdr": null,
    "fechaProceso": "2026-07-10T12:15:00",
    "etapas": [
      {
        "etapa": "INICIO",
        "ok": true,
        "mensaje": "Inicio de emision del comprobante F001-4.",
        "fecha": "2026-07-10T12:15:00"
      },
      {
        "etapa": "VALIDACION",
        "ok": false,
        "mensaje": "El comprobante tiene errores de validacion.",
        "fecha": "2026-07-10T12:15:00"
      }
    ],
    "errores": [
      "Debe indicar la serie del comprobante.",
      "Debe indicar los datos del cliente."
    ]
  },
  "errores": [
    "Debe indicar la serie del comprobante.",
    "Debe indicar los datos del cliente."
  ]
}
```

### ERROR_SUNAT

HTTP `400 Bad Request`. Estado interno generado por el servicio:
`ERROR_SUNAT`.

```json
{
  "ok": false,
  "mensaje": "No se pudo enviar el comprobante a SUNAT.",
  "data": {
    "ok": false,
    "estado": "ERROR_SUNAT",
    "mensaje": "No se pudo enviar el comprobante a SUNAT.",
    "comprobante": "F001-5",
    "hash": "ghi789",
    "nombreXml": "20123456789-01-F001-5.xml",
    "nombreZip": "20123456789-01-F001-5.zip",
    "xmlFirmado": true,
    "nombreCdr": null,
    "fechaProceso": "2026-07-10T12:20:00",
    "etapas": [
      {
        "etapa": "SUNAT",
        "ok": false,
        "mensaje": "No se pudo enviar el comprobante a SUNAT.",
        "fecha": "2026-07-10T12:20:01"
      }
    ],
    "errores": [
      "SUNAT no respondio dentro del tiempo esperado."
    ]
  },
  "errores": [
    "SUNAT no respondio dentro del tiempo esperado."
  ]
}
```

### ERROR_INTERNO

HTTP `500 Internal Server Error`. Estado interno generado por el servicio:
`ERROR_INTERNO`.

```json
{
  "ok": false,
  "mensaje": "Ocurrio un error interno al emitir el comprobante.",
  "data": {
    "ok": false,
    "estado": "ERROR_INTERNO",
    "mensaje": "Ocurrio un error al emitir el comprobante.",
    "comprobante": null,
    "hash": null,
    "nombreXml": null,
    "nombreZip": null,
    "xmlFirmado": false,
    "nombreCdr": null,
    "fechaProceso": "2026-07-10T12:25:00",
    "etapas": [
      {
        "etapa": "ERROR_INTERNO",
        "ok": false,
        "mensaje": "Detalle tecnico no seguro para frontend",
        "fecha": "2026-07-10T12:25:00"
      }
    ],
    "errores": [
      "Detalle tecnico no seguro para frontend"
    ]
  },
  "errores": [
    "Detalle tecnico no seguro para frontend"
  ]
}
```

## Diferencias con el contrato publico de capitalpos-api

`capitalpos-api/Docs/ContratoCpeEmision.md` define un contrato publico mas
estable para Angular. Las diferencias actuales son:

- El contrato publico agrega `data.codigo`; `capitalpos-cpe-api` no tiene ese
  campo en `CpeEmisionResponse`.
- El contrato publico espera `data.errores` estructurados con `codigo`,
  `campo` y `mensaje`; `capitalpos-cpe-api` devuelve `List<string>`.
- En errores funcionales, el contrato publico conserva `data.estado`; el
  endpoint interno de CPE tambien conserva `data.estado`, pero mantiene
  errores como lista plana.
- El contrato publico no expone `xmlFirmado`, `fechaProceso` ni `etapas`;
  esos son detalles internos del servicio CPE.
- El contrato publico define estados tecnicos de gateway como `ERROR_CPE` y
  `RESPUESTA_CPE_INVALIDA`; esos no pertenecen a `capitalpos-cpe-api`, sino a
  la normalizacion de `capitalpos-api`.
- `capitalpos-cpe-api` puede emitir estados adicionales como `OBSERVADO` y
  `PENDIENTE`; `capitalpos-api` debe decidir si los conserva o los normaliza.
- Los mensajes internos pueden incluir detalles tecnicos de excepciones; el
  contrato publico debe reemplazarlos por mensajes seguros.

## Campos internos que capitalpos-api debe normalizar u ocultar

`capitalpos-api` debe tratar como internos o no publicos:

- `xmlFirmado`: util para diagnostico interno, no necesario para Angular.
- `fechaProceso`: puede exponerse solo si se decide como dato publico estable.
- `etapas`: contiene flujo tecnico interno y mensajes de excepcion; no debe
  mostrarse completo al frontend sin sanitizacion.
- `errores` planos: deben convertirse a errores publicos estructurados cuando
  sea posible.
- `hash`, `nombreXml`, `nombreZip`, `nombreCdr`: pueden exponerse como
  referencias de comprobante, pero nunca como rutas absolutas.
- Mensajes que incluyan URL SUNAT, ruta local, excepciones, SOAP crudo,
  nombres de archivos sensibles o configuracion interna.

## Datos que nunca deben exponerse

Ni `capitalpos-cpe-api` ni `capitalpos-api` deben exponer:

- API keys o el valor de `X-API-KEY`.
- Certificados PFX, rutas privadas de certificados o passwords.
- Usuario SOL, clave SOL o credenciales de SUNAT.
- Tokens JWT recibidos por `capitalpos-api`.
- Rutas absolutas del servidor o estructura interna sensible.
- XML crudo, ZIP base64, SOAP envelope, SOAP response completo o CDR crudo,
  salvo en endpoints internos de diagnostico protegidos y con una decision
  explicita.
- Cuerpos completos de request/response en logs cuando incluyan datos
  fiscales, credenciales, certificados o comprobantes completos.

## Compatibilidad actual con capitalpos-api

Compatible para el flujo de emision end-to-end.

`capitalpos-api` puede consumir exitosamente respuestas `200 OK` de
`capitalpos-cpe-api` porque `data.estado`, `data.mensaje`, nombres de archivos,
hash y errores existen en el `CpeEmisionResponse` exitoso.

`capitalpos-api` tambien puede consumir respuestas `400` y `500` generadas por
`CpeController.Emitir`, porque el body conserva `data.estado`, `data.mensaje`,
`data.etapas` y `data.errores`. El API principal sigue siendo responsable de
normalizar errores planos a su contrato publico, ocultar campos internos y
mapear codigos publicos.

## Brechas posteriores a CPE-002

- Evaluar agregar `codigo` interno o dejar que `capitalpos-api` lo derive de
  `estado`.
- Evaluar reemplazar `List<string> errores` por errores estructurados, o
  documentar oficialmente que la estructura publica se arma solo en
  `capitalpos-api`.
- Sanitizar errores provenientes de excepciones antes de devolverlos por HTTP.
- Evitar mensajes publicos con URL SUNAT, rutas internas o detalles SOAP.
- Confirmar el tratamiento publico de `OBSERVADO` y `PENDIENTE`.
- Mantener Swagger solo en desarrollo/red privada y revisar rutas publicas.
