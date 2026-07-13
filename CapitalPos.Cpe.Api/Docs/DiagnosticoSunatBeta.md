# Diagnostico SUNAT beta

Este documento describe como interpretar el diagnostico seguro de preparacion
para envio real a SUNAT beta en `capitalpos-cpe-api`.

## Endpoint

```http
GET /api/cpe/diagnostico
X-API-KEY: <api-key-interna>
```

El endpoint no envia comprobantes a SUNAT. Solo revisa configuracion local,
archivos esperados y banderas de simulacion.

## Interpretacion

- `ok=true`: la configuracion minima esta lista para intentar envio real segun
  los checks actuales.
- `ok=false`: falta configuracion o hay simulaciones activas que impiden
  intentar SUNAT beta real.
- `SimularEnvioSunat=true`: el servicio no enviara a SUNAT; el resultado sera
  simulado.
- `SimularFirma=true`: el XML no tendra firma digital real.
- `Generacion XML` debe indicar XML real activado antes de una prueba beta
  real.
- `API key CPE` debe estar configurada si `CpeSecuritySettings.ApiKeyEnabled`
  esta activo.

## Checks principales

El diagnostico valida sin revelar secretos:

- modo `BETA` o `PRODUCCION`;
- ruta de archivos configurada y preparable;
- carpetas `XML`, `ZIP` y `CDR`;
- `SimularFirma`;
- `SimularGeneracionXml`;
- `SimularEnvioSunat`;
- ruta de certificado configurada;
- existencia del archivo de certificado;
- password de certificado configurado;
- lectura basica del certificado, cuando sea posible;
- usuario SOL configurado, con valor enmascarado;
- clave SOL configurada, sin mostrar valor;
- URL SUNAT configurada;
- `GuardarCdrSimulado`;
- API key interna configurada.

## Respuesta segura de ejemplo

```json
{
  "ok": false,
  "mensaje": "El diagnostico CPE encontro observaciones.",
  "data": {
    "ok": false,
    "modo": "BETA",
    "fechaRevision": "2026-07-11T10:00:00",
    "checks": [
      {
        "nombre": "Modo CPE",
        "ok": true,
        "mensaje": "Modo actual: BETA."
      },
      {
        "nombre": "SimularEnvioSunat",
        "ok": false,
        "mensaje": "Advertencia: el envio SUNAT esta simulado; no se enviara a SUNAT."
      },
      {
        "nombre": "Usuario SOL",
        "ok": true,
        "mensaje": "Usuario SOL configurado (20***01)."
      },
      {
        "nombre": "Clave SOL",
        "ok": true,
        "mensaje": "Clave SOL configurada."
      }
    ]
  },
  "errores": [
    "SimularEnvioSunat: Advertencia: el envio SUNAT esta simulado; no se enviara a SUNAT."
  ]
}
```

La respuesta no debe incluir API keys, passwords, clave SOL, contenido de
certificados, XML crudo, SOAP crudo ni rutas absolutas sensibles.

## Antes de SUNAT beta real

Para pasar de simulacion a beta real:

1. Configurar certificado PFX real y password fuera del repositorio.
2. Configurar usuario SOL y clave SOL fuera del repositorio.
3. Confirmar `Modo=BETA`.
4. Configurar `SimularFirma=false`.
5. Configurar `SimularEnvioSunat=false`.
6. Confirmar que la URL beta de SUNAT este configurada.
7. Ejecutar `GET /api/cpe/diagnostico` y revisar que no queden observaciones
   bloqueantes.
