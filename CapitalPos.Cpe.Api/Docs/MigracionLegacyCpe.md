# Migración Legacy CPE a CapitalPos.Cpe.Api

## Objetivo

Convertir la facturación electrónica actual del sistema WinForms en servicios limpios para una API REST consumible por:

- WinForms
- Web
- App móvil
- Otros sistemas

La API debe recibir JSON, procesar el comprobante, generar XML, firmar, enviar a SUNAT, recibir CDR y responder JSON.

---

## Flujo actual de la API

Cliente externo
→ POST /api/cpe/emitir
→ CpeController
→ CpeEmisionService
→ Validación
→ Generar XML
→ Firmar XML
→ Crear ZIP
→ Enviar SUNAT
→ Guardar CDR
→ Guardar historial
→ Responder JSON

---

## Código legacy de referencia

Ruta:

Legacy/WinFormsCpe

Archivos/carpetas:

- CPEConfig.cs
- CrearXML/
- Firma/
- Sunat/
- Entidades/

Este código NO debe compilar dentro de la API todavía. Solo se usa como referencia.

---

## Mapeo que debemos identificar

| Parte | Código legacy | Nuevo servicio API |
|---|---|---|
| Armar objeto comprobante | businessEntities / BE.CPE | CpeDocumento |
| Generar XML UBL | CrearXML | CpeXmlUblGenerator / CpeXmlRealService |
| Firmar XML | Signature | CpeFirmaService |
| Crear ZIP | CPEConfig / método auxiliar | CpeStorageService |
| Enviar SUNAT | CPEEnvio | CpeSunatService |
| Leer CDR | CPEEnvio / CPEConfig | CpeCdrService |
| Responder resultado | CPEConfig | CpeEmisionResponse |

---

## Reglas de migración

1. No copiar formularios WinForms.
2. No meter MessageBox, controles visuales ni rutas fijas.
3. No usar clases acopladas a pantalla.
4. Convertir todo a servicios.
5. Todo debe entrar por DTO JSON.
6. Todo debe responder con ApiResponse.
7. Primero probar en modo BETA.
8. Recién después pasar a producción.

---

## Servicios finales esperados

- ICpeXmlService
- ICpeFirmaService
- ICpeSunatService
- ICpeStorageService
- ICpeHistorialService
- ICpeCdrService

---

## Estado actual

✅ API base creada  
✅ Seguridad con API Key  
✅ Manejo global de errores  
✅ CorrelationId  
✅ Historial JSON  
✅ XML simulado  
✅ ZIP simulado  
✅ CDR simulado  
✅ Carpeta Legacy creada  
✅ Legacy excluido de compilación  
✅ Proyecto compila en net7.0

---

## Siguiente objetivo técnico

Identificar en CPEConfig.cs cuál es el método principal que actualmente hace:

1. Generar XML
2. Firmar
3. Comprimir ZIP
4. Enviar a SUNAT
5. Leer respuesta/CDR

Ese método será usado como guía para implementar los servicios reales de la API.

## Mapa real detectado desde Legacy

### Flujo principal para factura y boleta

Método principal legacy:

```csharp
Enviar_FacturaBoleta_aSunat(BE.CPE CPE)
```

Ubicación:

```text
Legacy/WinFormsCpe/CPEConfig.cs
```

Este método realiza el flujo completo de factura/boleta:

1. Arma el nombre del comprobante.
2. Define ruta de XML.
3. Define URL SUNAT.
4. Genera XML usando `objXML.CPE(CPE, nomARCHIVO, ruta)`.
5. Prepara datos de firma en `FirmadoRequest`.
6. Firma XML usando `objSignature.FirmaXMl(objPregunta)`.
7. Obtiene `DigestValue`.
8. Envía a SUNAT usando `objENV.Envio(...)`.
9. Devuelve un `Dictionary<string, string>` con la respuesta.

---

### Generación XML

Clase legacy:

```text
CrearXML/CrearXML.vb
```

Método principal:

```vb
Public Function CPE(comprobante As BE.CPE, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
```

Nuevo destino en API:

```text
CpeXmlUblGenerator / CpeXmlRealService
```

---

### Firma digital

Clase legacy usada desde `CPEConfig.cs`:

```csharp
objSignature.FirmaXMl(objPregunta)
```

Objetos relacionados:

```csharp
SG.FirmadoRequest
SG.FirmadoResponse
```

Nuevo destino en API:

```text
CpeFirmaService
```

---

### Envío SUNAT factura/boleta

Clase legacy:

```text
Sunat/ServiceSunat.vb
```

Método principal:

```vb
Public Function Envio(ruc As String, usu_sol As String, contra_sol As String, nombre_archivo As String, rutaArchivo As String, url As String, hash_cpe As String) As Dictionary(Of String, String)
```

Nuevo destino en API:

```text
CpeSunatService
```

---

### ZIP

Método legacy:

```csharp
public static void Comprimir(string nomArchivo, string ruta)
```

Ubicación:

```text
CPEConfig.cs
```

También existe lógica similar en:

```text
Sunat/Variables_Globales.vb
```

Nuevo destino en API:

```text
CpeStorageService
```

---

### Hash

Método legacy:

```csharp
private static string ObtenerHashSHA256(string filePath)
```

Nuevo destino en API:

```text
CpeHashService
```

---

### CDR / Ticket / Consulta

Métodos legacy detectados:

```csharp
EnvioTicketAsync(...)
DescomprimirConSharpZipLib(...)
```

Y en `ServiceSunat.vb`:

```vb
ConsultaTicket(...)
getStatusCDR(...)
getStatusFactura(...)
```

Nuevo destino en API:

```text
CpeCdrService
```

---

## Decisión de migración

Primero migraremos solo:

```text
Factura / Boleta
```

No migraremos todavía:

```text
Guía de remisión
Resumen de boletas
Baja de comprobantes
Nota de crédito
Consulta ticket
```

Eso se hará después de tener funcionando factura/boleta real en la API.

---

## Primer objetivo técnico real

Convertir este flujo legacy:

```text
Enviar_FacturaBoleta_aSunat
```

a este flujo limpio de API:

```text
POST /api/cpe/emitir
→ CpeEmisionService
→ CpeXmlRealService
→ CpeFirmaService
→ CpeStorageService
→ CpeSunatService
→ CpeCdrService
→ CpeEmisionResponse
```
## Avance UBL probado en API

Se probó la generación XML UBL desde la API usando:

```json
"SimularGeneracionXml": false:

Endpoint probado:

POST /api/cpe/pruebas/emitir-demo

Resultado:

El XML se genera correctamente.
El XML puede listarse desde /api/cpe/archivos/xml/info.
El XML puede visualizarse desde /api/cpe/archivos/xml/{nombreArchivo}.
Se confirmaron bloques SUNAT en el XML generado.

Bloques confirmados:

<cbc:ProfileID schemeName="Tipo de Operacion" schemeAgencyName="PE:SUNAT">
<cbc:CustomizationID schemeAgencyName="PE:SUNAT">2.0</cbc:CustomizationID>
<cbc:PriceTypeCode listName="Tipo de Precio" listAgencyName="PE:SUNAT">
<cbc:TaxExemptionReasonCode listAgencyName="PE:SUNAT" listName="Afectacion del IGV">
<cbc:ID schemeName="Codigo de tributos" schemeAgencyName="PE:SUNAT">
Estado de XML UBL

El XML UBL de la API ya tiene avances basados en el legacy:

Cabecera SUNAT
Emisor con atributos SUNAT
Cliente con atributos SUNAT
Items con catálogo 16
Afectación IGV con catálogo 07
Tributos con catálogo 05

Pendiente antes de SUNAT real:

Validar estructura completa UBL
Completar datos obligatorios faltantes
Firma digital real
ZIP real firmado
Envío real SUNAT
Lectura real de CDR

Guarda el archivo.