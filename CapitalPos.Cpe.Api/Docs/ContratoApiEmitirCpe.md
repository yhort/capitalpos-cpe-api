# Contrato API - Emitir CPE

Endpoint principal para emitir comprobantes electrónicos desde sistemas externos como WinForms, Web, Angular o aplicaciones móviles.

## Endpoint

POST /api/cpe/emitir

Headers requeridos:

X-API-KEY: <TU_API_KEY>
Content-Type: application/json

## Flujo interno

1. Validar comprobante
2. Generar XML UBL
3. Firmar XML
4. Crear ZIP
5. Enviar a SUNAT o simular envío según configuración
6. Guardar CDR
7. Devolver resultado al sistema consumidor

## Request principal

El endpoint recibe un objeto EmitirCpeRequest con estos bloques principales:

- rucEmisor
- emisor
- tipoComprobante
- serie
- correlativo
- fechaEmision
- moneda
- tipoOperacion
- formaPago
- cliente
- items
- totales

## Response principal

El endpoint responde con:

ApiResponse<CpeEmisionResponse>

Campos principales:

- ok
- mensaje
- data.ok
- data.estado
- data.mensaje
- data.comprobante
- data.hash
- data.nombreXml
- data.nombreZip
- data.xmlFirmado
- data.nombreCdr
- data.etapas
- data.errores

## Estados principales

- ACEPTADO
- RECHAZADO
- SIMULADO
- ERROR_VALIDACION
- ERROR_XML
- ERROR_FIRMA
- ERROR_SUNAT
- ERROR_CDR
- ERROR_INTERNO

## Notas de seguridad

- No subir API keys al repositorio.
- No subir certificados PFX al repositorio.
- No subir contraseñas ni credenciales SOL al repositorio.
- En desarrollo se puede usar firma real con SUNAT simulado.
- En producción se requiere certificado PFX real, password, usuario SOL y clave SOL configurados de forma segura.
