using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Services;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeValidacionFacturaBoletaTests
{
    private readonly CpeValidacionService _sut = new();

    [Fact]
    public void Validar_FacturaValidaConRuc_SinErrores()
    {
        var errores = _sut.Validar(CrearRequestFactura());

        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_FacturaConSerieB_Error()
    {
        var request = CrearRequestFactura();
        request.Serie = "B001";

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("serie debe iniciar con F", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_FacturaConClienteDni_Error()
    {
        var request = CrearRequestFactura();
        request.Cliente = new ClienteDto
        {
            TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Dni,
            NumeroDocumento = "12345678",
            RazonSocial = "CLIENTE SIN RUC"
        };

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("cliente debe tener RUC", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_BoletaValidaConDni_SinErrores()
    {
        var errores = _sut.Validar(CrearRequestBoleta());

        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_BoletaMenorOIgual700_SinDocumento_SinErroresYNormalizaConsumidorFinal()
    {
        var request = CrearRequestBoleta();
        request.Cliente = new ClienteDto
        {
            TipoDocumento = "",
            NumeroDocumento = "",
            RazonSocial = "MARIA LOPEZ"
        };

        var errores = _sut.Validar(request);

        Assert.Empty(errores);
        Assert.Equal(CatalogosSunat.TipoDocumentoIdentidad.SinIdentificacion, request.Cliente.TipoDocumento);
        Assert.Equal(CatalogosSunat.TipoDocumentoIdentidad.NumeroSinIdentificacion, request.Cliente.NumeroDocumento);
    }

    [Fact]
    public void Validar_BoletaDe700_SoloNombre_SinErrores()
    {
        var request = CrearRequestBoletaConTotal(700.00m);
        request.Cliente = new ClienteDto
        {
            TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Dni,
            NumeroDocumento = null!,
            RazonSocial = "JUAN PEREZ"
        };

        var errores = _sut.Validar(request);

        Assert.Empty(errores);
        Assert.Equal("-", request.Cliente.TipoDocumento);
        Assert.Equal("-", request.Cliente.NumeroDocumento);
    }

    [Fact]
    public void Validar_BoletaMayorA700_SinDocumento_Error()
    {
        var request = CrearRequestBoletaConTotal(700.01m);
        request.Cliente = new ClienteDto
        {
            TipoDocumento = "",
            NumeroDocumento = "",
            RazonSocial = "JUAN PEREZ"
        };

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("mayor a S/ 700", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_BoletaConSerieF_Error()
    {
        var request = CrearRequestBoleta();
        request.Serie = "F001";

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("serie debe iniciar con B", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_BoletaSinItems_Error()
    {
        var request = CrearRequestBoleta();
        request.Items = new List<ItemCpeDto>();

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("al menos un ítem", StringComparison.OrdinalIgnoreCase)
                || e.Contains("al menos un item", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_BoletaConTotalInconsistente_Error()
    {
        var request = CrearRequestBoleta();
        request.Total = 999.99m;

        var errores = _sut.Validar(request);

        Assert.NotEmpty(errores);
        Assert.Contains(
            errores,
            e => e.Contains("total", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validar_SinRucEmisor_ErroresClaros()
    {
        var request = CrearRequestBoleta();
        request.RucEmisor = "";
        request.Emisor.Ruc = "";

        var errores = _sut.Validar(request);

        Assert.Contains(
            errores,
            e => e.Contains("RUC del emisor", StringComparison.OrdinalIgnoreCase));
    }

    private static EmitirCpeRequest CrearRequestFactura()
    {
        return new EmitirCpeRequest
        {
            RucEmisor = "20100066603",
            Emisor = CrearEmisor(),
            TipoComprobante = CatalogosSunat.TipoComprobante.Factura,
            Serie = "F001",
            Correlativo = 100,
            FechaEmision = DateTime.Today,
            Moneda = CatalogosSunat.Moneda.Soles,
            TipoOperacion = CatalogosSunat.TipoOperacion.VentaInterna,
            FormaPago = CatalogosSunat.FormaPago.Contado,
            Cliente = new ClienteDto
            {
                TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Ruc,
                NumeroDocumento = "20600000001",
                RazonSocial = "CLIENTE FACTURA SAC"
            },
            Items = CrearItemGravado(),
            TotalGravada = 100.00m,
            TotalExonerada = 0m,
            TotalInafecta = 0m,
            TotalIgv = 18.00m,
            Total = 118.00m
        };
    }

    private static EmitirCpeRequest CrearRequestBoleta()
    {
        return new EmitirCpeRequest
        {
            RucEmisor = "20100066603",
            Emisor = CrearEmisor(),
            TipoComprobante = CatalogosSunat.TipoComprobante.Boleta,
            Serie = "B001",
            Correlativo = 3002,
            FechaEmision = DateTime.Today,
            Moneda = CatalogosSunat.Moneda.Soles,
            TipoOperacion = CatalogosSunat.TipoOperacion.VentaInterna,
            FormaPago = CatalogosSunat.FormaPago.Contado,
            Cliente = new ClienteDto
            {
                TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Dni,
                NumeroDocumento = "12345678",
                RazonSocial = "CLIENTE BOLETA BETA"
            },
            Items = CrearItemGravado(),
            TotalGravada = 100.00m,
            TotalExonerada = 0m,
            TotalInafecta = 0m,
            TotalIgv = 18.00m,
            Total = 118.00m
        };
    }

    private static EmisorDto CrearEmisor()
    {
        return new EmisorDto
        {
            Ruc = "20100066603",
            RazonSocial = "EMPRESA BETA SUNAT SAC",
            NombreComercial = "EMPRESA BETA",
            Ubigeo = "150101",
            Direccion = "AV. BETA 123",
            Departamento = "LIMA",
            Provincia = "LIMA",
            Distrito = "LIMA"
        };
    }

    private static EmitirCpeRequest CrearRequestBoletaConTotal(decimal total)
    {
        var totalR = decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        var subtotal = decimal.Round(totalR / 1.18m, 2, MidpointRounding.AwayFromZero);
        var igv = decimal.Round(totalR - subtotal, 2, MidpointRounding.AwayFromZero);
        var request = CrearRequestBoleta();
        request.Items = new List<ItemCpeDto>
        {
            new ItemCpeDto
            {
                Codigo = "P001",
                Descripcion = "Producto gravado",
                UnidadMedida = "NIU",
                Cantidad = 1,
                ValorUnitario = subtotal,
                PrecioUnitario = totalR,
                Subtotal = subtotal,
                Igv = igv,
                Total = totalR,
                CodigoAfectacionIgv = CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa
            }
        };
        request.TotalGravada = subtotal;
        request.TotalExonerada = 0m;
        request.TotalInafecta = 0m;
        request.TotalIgv = igv;
        request.Total = totalR;
        return request;
    }

    private static List<ItemCpeDto> CrearItemGravado()
    {
        return new List<ItemCpeDto>
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
        };
    }
}
