using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Services;

public class CatalogosService
{
    public List<CatalogoItemDto> ObtenerTiposComprobante()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.TipoComprobante.Factura,
                Descripcion = "Factura"
            },
            new()
            {
                Codigo = CatalogosSunat.TipoComprobante.Boleta,
                Descripcion = "Boleta"
            },
            new()
            {
                Codigo = CatalogosSunat.TipoComprobante.NotaCredito,
                Descripcion = "Nota de crédito"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerMotivosNotaCredito()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.AnulacionDeLaOperacion,
                Descripcion = "Anulación de la operación"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.AnulacionPorErrorEnElRuc,
                Descripcion = "Anulación por error en el RUC"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.CorreccionPorErrorEnLaDescripcion,
                Descripcion = "Corrección por error en la descripción"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.DescuentoGlobal,
                Descripcion = "Descuento global"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.DescuentoPorItem,
                Descripcion = "Descuento por ítem"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.DevolucionTotal,
                Descripcion = "Devolución total"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.DevolucionPorItem,
                Descripcion = "Devolución por ítem"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.Bonificacion,
                Descripcion = "Bonificación"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.DisminucionEnElValor,
                Descripcion = "Disminución en el valor"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.OtrosConceptos,
                Descripcion = "Otros conceptos"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.AjustesDeOperacionesDeExportacion,
                Descripcion = "Ajustes de operaciones de exportación"
            },
            new()
            {
                Codigo = CatalogosSunat.MotivoNotaCredito.AjustesAfectosAlIvap,
                Descripcion = "Ajustes afectos al IVAP"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerTiposDocumentoIdentidad()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.TipoDocumentoIdentidad.Dni,
                Descripcion = "DNI"
            },
            new()
            {
                Codigo = CatalogosSunat.TipoDocumentoIdentidad.Ruc,
                Descripcion = "RUC"
            },
            new()
            {
                Codigo = CatalogosSunat.TipoDocumentoIdentidad.SinIdentificacion,
                Descripcion = "Sin documento (consumidor final / boleta ≤ S/ 700)"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerMonedas()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.Moneda.Soles,
                Descripcion = "Soles"
            },
            new()
            {
                Codigo = CatalogosSunat.Moneda.Dolares,
                Descripcion = "Dólares"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerTiposOperacion()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.TipoOperacion.VentaInterna,
                Descripcion = "Venta interna"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerAfectacionesIgv()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa,
                Descripcion = "Gravado - Operación onerosa"
            },
            new()
            {
                Codigo = CatalogosSunat.AfectacionIgv.ExoneradoOperacionOnerosa,
                Descripcion = "Exonerado - Operación onerosa"
            },
            new()
            {
                Codigo = CatalogosSunat.AfectacionIgv.InafectoOperacionOnerosa,
                Descripcion = "Inafecto - Operación onerosa"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerFormasPago()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CatalogosSunat.FormaPago.Contado,
                Descripcion = "Contado"
            },
            new()
            {
                Codigo = CatalogosSunat.FormaPago.Credito,
                Descripcion = "Crédito"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerEstadosCpe()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CpeEstados.Simulado,
                Descripcion = "Simulado"
            },
            new()
            {
                Codigo = CpeEstados.Aceptado,
                Descripcion = "Aceptado"
            },
            new()
            {
                Codigo = CpeEstados.Rechazado,
                Descripcion = "Rechazado"
            },
            new()
            {
                Codigo = CpeEstados.Observado,
                Descripcion = "Observado"
            },
            new()
            {
                Codigo = CpeEstados.Pendiente,
                Descripcion = "Pendiente"
            },
            new()
            {
                Codigo = CpeEstados.ErrorValidacion,
                Descripcion = "Error de validación"
            },
            new()
            {
                Codigo = CpeEstados.ErrorXml,
                Descripcion = "Error al generar XML"
            },
            new()
            {
                Codigo = CpeEstados.ErrorFirma,
                Descripcion = "Error al firmar XML"
            },
            new()
            {
                Codigo = CpeEstados.ErrorSunat,
                Descripcion = "Error al enviar a SUNAT"
            },
            new()
            {
                Codigo = CpeEstados.ErrorCdr,
                Descripcion = "Error al procesar CDR"
            },
            new()
            {
                Codigo = CpeEstados.ErrorInterno,
                Descripcion = "Error interno"
            }
        };
    }

    public List<CatalogoItemDto> ObtenerEtapasCpe()
    {
        return new List<CatalogoItemDto>
        {
            new()
            {
                Codigo = CpeEtapas.Inicio,
                Descripcion = "Inicio del proceso"
            },
            new()
            {
                Codigo = CpeEtapas.Validacion,
                Descripcion = "Validación del comprobante"
            },
            new()
            {
                Codigo = CpeEtapas.Xml,
                Descripcion = "Generación del XML"
            },
            new()
            {
                Codigo = CpeEtapas.Firma,
                Descripcion = "Firma digital del XML"
            },
            new()
            {
                Codigo = CpeEtapas.Zip,
                Descripcion = "Generación del ZIP"
            },
            new()
            {
                Codigo = CpeEtapas.Sunat,
                Descripcion = "Envío a SUNAT"
            },
            new()
            {
                Codigo = CpeEtapas.Cdr,
                Descripcion = "Recepción o guardado del CDR"
            },
            new()
            {
                Codigo = CpeEtapas.ErrorInterno,
                Descripcion = "Error interno del proceso"
            }
        };
    }
}