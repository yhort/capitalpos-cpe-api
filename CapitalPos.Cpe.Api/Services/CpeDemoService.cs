using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Services;

public class CpeDemoService
{
    public EmitirCpeRequest CrearBoletaDemo()
    {
        var correlativoDemo = int.Parse(DateTime.Now.ToString("HHmmss"));

        return new EmitirCpeRequest
        {
            RucEmisor = "20606264004",

            Emisor = new EmisorDto
            {
                Ruc = "20606264004",
                RazonSocial = "C.G CAPITAL SYSTEM S.A.C.",
                NombreComercial = "CAPITALPOS",
                Ubigeo = "150101",
                Direccion = "AV. PRINCIPAL 123",
                Departamento = "LIMA",
                Provincia = "LIMA",
                Distrito = "LIMA"
            },

            TipoComprobante = CatalogosSunat.TipoComprobante.Boleta,
            Serie = "B001",
            Correlativo = correlativoDemo,
            FechaEmision = DateTime.Now,
            Moneda = CatalogosSunat.Moneda.Soles,
            TipoOperacion = CatalogosSunat.TipoOperacion.VentaInterna,
            Observacion = "Comprobante demo generado desde API CPE.",

            FormaPago = CatalogosSunat.FormaPago.Contado,
            MontoPendientePago = 0,
            Cuotas = new List<CuotaPagoDto>(),

            Cliente = new ClienteDto
            {
                TipoDocumento = CatalogosSunat.TipoDocumentoIdentidad.Dni,
                NumeroDocumento = "12345678",
                RazonSocial = "CLIENTE DEMO"
            },

            Items = new List<ItemCpeDto>
            {
                new()
                {
                    Codigo = "DEMO001",
                    Descripcion = "Producto demo gravado",
                    UnidadMedida = "NIU",
                    Cantidad = 2,
                    ValorUnitario = 8.47m,
                    PrecioUnitario = 10.00m,
                    Subtotal = 16.94m,
                    Igv = 3.06m,
                    Total = 20.00m,
                    CodigoAfectacionIgv = CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa
                }
            },

            TotalGravada = 16.94m,
            TotalExonerada = 0,
            TotalInafecta = 0,
            TotalIgv = 3.06m,
            Total = 20.00m,
            MontoEnLetras = string.Empty
        };
    }
}