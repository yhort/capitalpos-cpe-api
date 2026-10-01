using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeValidacionNotaCreditoTests
{
    private readonly CpeValidacionService _sut = new();

    [Fact]
    public void Validar_NotaCreditoValidaSobreBoleta_SinErrores()
    {
        var errores = _sut.Validar(CrearRequestNotaCredito(
            serie: "BC01",
            tipoAfectado: CatalogosSunat.TipoComprobante.Boleta,
            serieCorrelativo: "B001-3002"));

        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_NotaCreditoSobreBoletaConSerieF_Error()
    {
        var errores = _sut.Validar(CrearRequestNotaCredito(
            serie: "FC01",
            tipoAfectado: CatalogosSunat.TipoComprobante.Boleta,
            serieCorrelativo: "B001-3002"));

        Assert.Contains(
            errores,
            e => e.Contains("serie debe iniciar con B", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_NotaCreditoSobreFacturaConSerieB_Error()
    {
        var request = CrearRequestNotaCredito(
            serie: "BC01",
            tipoAfectado: CatalogosSunat.TipoComprobante.Factura,
            serieCorrelativo: "F001-10");
        request.Cliente = new ClienteDto
        {
            TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Ruc,
            NumeroDocumento = "20600000001",
            RazonSocial = "CLIENTE FACTURA SAC"
        };

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("serie debe iniciar con F", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_NotaCreditoSinMotivoNiReferencia_Errores()
    {
        var request = CrearRequestNotaCredito(
            serie: "BC01",
            tipoAfectado: CatalogosSunat.TipoComprobante.Boleta,
            serieCorrelativo: "B001-3002");
        request.CodigoMotivo = null;
        request.DescripcionMotivo = null;
        request.DocumentoReferencia = null;

        var errores = _sut.Validar(request);

        Assert.Contains(errores, e => e.Contains("código de motivo", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errores, e => e.Contains("descripción del motivo", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errores, e => e.Contains("documento de referencia", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_NotaCreditoConMotivoInvalido_Error()
    {
        var request = CrearRequestNotaCredito(
            serie: "BC01",
            tipoAfectado: CatalogosSunat.TipoComprobante.Boleta,
            serieCorrelativo: "B001-3002");
        request.CodigoMotivo = "99";

        var errores = _sut.Validar(request);

        Assert.Contains(errores, e => e.Contains("Catálogo 09", StringComparison.OrdinalIgnoreCase));
    }

    private static EmitirCpeRequest CrearRequestNotaCredito(
        string serie,
        string tipoAfectado,
        string serieCorrelativo)
    {
        return new EmitirCpeRequest
        {
            RucEmisor = "20100066603",
            Emisor = new EmisorDto
            {
                Ruc = "20100066603",
                RazonSocial = "EMPRESA BETA SUNAT SAC",
                NombreComercial = "EMPRESA BETA",
                Ubigeo = "150101",
                Direccion = "AV. BETA 123",
                Departamento = "LIMA",
                Provincia = "LIMA",
                Distrito = "LIMA"
            },
            TipoComprobante = CatalogosSunat.TipoComprobante.NotaCredito,
            Serie = serie,
            Correlativo = 1,
            FechaEmision = DateTime.Today,
            Moneda = CatalogosSunat.Moneda.Soles,
            TipoOperacion = CatalogosSunat.TipoOperacion.VentaInterna,
            FormaPago = CatalogosSunat.FormaPago.Contado,
            CodigoMotivo = CatalogosSunat.MotivoNotaCredito.AnulacionDeLaOperacion,
            DescripcionMotivo = "Anulacion de la operacion",
            DocumentoReferencia = new DocumentoReferenciaDto
            {
                TipoComprobante = tipoAfectado,
                SerieCorrelativo = serieCorrelativo
            },
            Cliente = new ClienteDto
            {
                TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Dni,
                NumeroDocumento = "12345678",
                RazonSocial = "CLIENTE BOLETA BETA"
            },
            Items = new List<ItemCpeDto>
            {
                new ItemCpeDto
                {
                    Codigo = "P001",
                    Descripcion = "Producto gravado",
                    UnidadMedida = "NIU",
                    Cantidad = 1,
                    ValorUnitario = 100.00m,
                    PrecioUnitario = 118.00m,
                    Subtotal = 100.00m,
                    Igv = 18.00m,
                    Total = 118.00m,
                    CodigoAfectacionIgv = CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa
                }
            },
            TotalGravada = 100.00m,
            TotalExonerada = 0m,
            TotalInafecta = 0m,
            TotalIgv = 18.00m,
            Total = 118.00m
        };
    }
}
