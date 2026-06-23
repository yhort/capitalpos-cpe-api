using CapitalPos.Cpe.Api.Domain.Cpe;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;

namespace CapitalPos.Cpe.Api.Mappers;

public static class CpeDocumentoMapper
{
    public static CpeDocumento FromRequest(EmitirCpeRequest request)
    {
        return new CpeDocumento
        {
            NumeroComprobante = CpeNombreHelper.ObtenerNumeroComprobante(request),

            Emisor = new CpeEmisor
            {
                Ruc = request.Emisor.Ruc,
                RazonSocial = request.Emisor.RazonSocial,
                NombreComercial = request.Emisor.NombreComercial,
                Ubigeo = request.Emisor.Ubigeo,
                Direccion = request.Emisor.Direccion,
                Departamento = request.Emisor.Departamento,
                Provincia = request.Emisor.Provincia,
                Distrito = request.Emisor.Distrito
            },

            Cliente = new CpeCliente
            {
                TipoDocumento = request.Cliente.TipoDocumento,
                NumeroDocumento = request.Cliente.NumeroDocumento,
                RazonSocial = request.Cliente.RazonSocial
            },

            TipoComprobante = request.TipoComprobante,
            Serie = request.Serie,
            Correlativo = request.Correlativo,

            FechaEmision = request.FechaEmision,
            Moneda = request.Moneda,
            TipoOperacion = request.TipoOperacion,
            Observacion = request.Observacion,

            FormaPago = request.FormaPago,
            MontoPendientePago = request.MontoPendientePago,

            Cuotas = request.Cuotas.Select(c => new CpeCuota
            {
                Numero = c.Numero,
                FechaVencimiento = c.FechaVencimiento,
                Monto = c.Monto
            }).ToList(),

            Items = request.Items.Select(i => new CpeItem
            {
                Codigo = i.Codigo,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                Cantidad = i.Cantidad,
                ValorUnitario = i.ValorUnitario,
                PrecioUnitario = i.PrecioUnitario,
                Subtotal = i.Subtotal,
                Igv = i.Igv,
                Total = i.Total,
                CodigoAfectacionIgv = i.CodigoAfectacionIgv
            }).ToList(),

            TotalGravada = request.TotalGravada,
            TotalExonerada = request.TotalExonerada,
            TotalInafecta = request.TotalInafecta,
            TotalIgv = request.TotalIgv,
            Total = request.Total,
            
            MontoEnLetras = string.IsNullOrWhiteSpace(request.MontoEnLetras)
                ? NumeroALetrasHelper.ConvertirMonto(request.Total, request.Moneda)
                : request.MontoEnLetras
        };
    }
}