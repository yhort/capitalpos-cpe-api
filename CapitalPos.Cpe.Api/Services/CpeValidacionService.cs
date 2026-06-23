using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Services;

public class CpeValidacionService
{
    public List<string> Validar(EmitirCpeRequest request)
    {
        var errores = new List<string>();

        // =========================
        // EMISOR PRINCIPAL
        // =========================
        if (string.IsNullOrWhiteSpace(request.RucEmisor))
        {
            errores.Add("El RUC del emisor es obligatorio.");
        }
        else
        {
            if (request.RucEmisor.Length != 11)
                errores.Add("El RUC del emisor debe tener 11 dígitos.");

            if (!EsNumerico(request.RucEmisor))
                errores.Add("El RUC del emisor debe contener solo números.");
        }

        // =========================
        // EMISOR DETALLADO
        // =========================
        if (request.Emisor == null)
        {
            errores.Add("Los datos del emisor son obligatorios.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Emisor.Ruc))
                errores.Add("El RUC del emisor en el objeto Emisor es obligatorio.");

            if (!string.IsNullOrWhiteSpace(request.Emisor.Ruc))
            {
                if (request.Emisor.Ruc.Length != 11)
                    errores.Add("El RUC del emisor en el objeto Emisor debe tener 11 dígitos.");

                if (!EsNumerico(request.Emisor.Ruc))
                    errores.Add("El RUC del emisor en el objeto Emisor debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(request.Emisor.RazonSocial))
                errores.Add("La razón social del emisor es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.Emisor.Ubigeo))
                errores.Add("El ubigeo del emisor es obligatorio.");

            if (!string.IsNullOrWhiteSpace(request.Emisor.Ubigeo))
            {
                if (request.Emisor.Ubigeo.Length != 6)
                    errores.Add("El ubigeo del emisor debe tener 6 dígitos.");

                if (!EsNumerico(request.Emisor.Ubigeo))
                    errores.Add("El ubigeo del emisor debe contener solo números.");
            }

            if (string.IsNullOrWhiteSpace(request.Emisor.Direccion))
                errores.Add("La dirección del emisor es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.Emisor.Departamento))
                errores.Add("El departamento del emisor es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Emisor.Provincia))
                errores.Add("La provincia del emisor es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.Emisor.Distrito))
                errores.Add("El distrito del emisor es obligatorio.");

            if (!string.IsNullOrWhiteSpace(request.RucEmisor) &&
                !string.IsNullOrWhiteSpace(request.Emisor.Ruc) &&
                request.RucEmisor != request.Emisor.Ruc)
            {
                errores.Add("El RUC emisor principal no coincide con el RUC del objeto Emisor.");
            }
        }

        // =========================
        // TIPO COMPROBANTE
        // =========================
        if (string.IsNullOrWhiteSpace(request.TipoComprobante))
        {
            errores.Add("El tipo de comprobante es obligatorio.");
        }
        else
        {
            var tiposPermitidos = new[]
            {
                CatalogosSunat.TipoComprobante.Factura,
                CatalogosSunat.TipoComprobante.Boleta
            };

            if (!tiposPermitidos.Contains(request.TipoComprobante))
                errores.Add("El tipo de comprobante debe ser 01 Factura o 03 Boleta.");
        }

        // =========================
        // SERIE
        // =========================
        if (string.IsNullOrWhiteSpace(request.Serie))
        {
            errores.Add("La serie es obligatoria.");
        }
        else
        {
            if (request.Serie.Length != 4)
                errores.Add("La serie debe tener 4 caracteres.");

            if (request.TipoComprobante == CatalogosSunat.TipoComprobante.Factura &&
                !request.Serie.StartsWith("F", StringComparison.OrdinalIgnoreCase))
            {
                errores.Add("Para factura, la serie debe iniciar con F.");
            }

            if (request.TipoComprobante == CatalogosSunat.TipoComprobante.Boleta &&
                !request.Serie.StartsWith("B", StringComparison.OrdinalIgnoreCase))
            {
                errores.Add("Para boleta, la serie debe iniciar con B.");
            }
        }

        if (request.Correlativo <= 0)
            errores.Add("El correlativo debe ser mayor a 0.");

        // =========================
        // FECHA / MONEDA / OPERACIÓN
        // =========================
        if (request.FechaEmision == default)
            errores.Add("La fecha de emisión es obligatoria.");

        if (request.FechaEmision.Date > DateTime.Now.Date)
            errores.Add("La fecha de emisión no puede ser futura.");

        if (string.IsNullOrWhiteSpace(request.Moneda))
        {
            errores.Add("La moneda es obligatoria.");
        }
        else
        {
            var monedasPermitidas = new[]
            {
                CatalogosSunat.Moneda.Soles,
                CatalogosSunat.Moneda.Dolares
            };

            if (!monedasPermitidas.Contains(request.Moneda.ToUpper()))
                errores.Add("La moneda debe ser PEN o USD.");
        }

        if (string.IsNullOrWhiteSpace(request.TipoOperacion))
        {
            errores.Add("El tipo de operación es obligatorio.");
        }
        else
        {
            if (request.TipoOperacion.Length != 4)
                errores.Add("El tipo de operación debe tener 4 caracteres.");
        }

        // =========================
        // CLIENTE
        // =========================
        if (request.Cliente == null)
        {
            errores.Add("El cliente es obligatorio.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Cliente.TipoDocumento))
                errores.Add("El tipo de documento del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Cliente.NumeroDocumento))
                errores.Add("El número de documento del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Cliente.RazonSocial))
                errores.Add("La razón social o nombre del cliente es obligatorio.");

            if (!string.IsNullOrWhiteSpace(request.Cliente.TipoDocumento) &&
                !string.IsNullOrWhiteSpace(request.Cliente.NumeroDocumento))
            {
                if (request.Cliente.TipoDocumento == CatalogosSunat.TipoDocumentoIdentidad.Dni)
                {
                    if (request.Cliente.NumeroDocumento.Length != 8)
                        errores.Add("El DNI del cliente debe tener 8 dígitos.");

                    if (!EsNumerico(request.Cliente.NumeroDocumento))
                        errores.Add("El DNI del cliente debe contener solo números.");
                }

                if (request.Cliente.TipoDocumento == CatalogosSunat.TipoDocumentoIdentidad.Ruc)
                {
                    if (request.Cliente.NumeroDocumento.Length != 11)
                        errores.Add("El RUC del cliente debe tener 11 dígitos.");

                    if (!EsNumerico(request.Cliente.NumeroDocumento))
                        errores.Add("El RUC del cliente debe contener solo números.");
                }
            }

            if (request.TipoComprobante == CatalogosSunat.TipoComprobante.Factura &&
                request.Cliente.TipoDocumento != CatalogosSunat.TipoDocumentoIdentidad.Ruc)
            {
                errores.Add("Para factura, el cliente debe tener RUC.");
            }
        }

        // =========================
        // FORMA DE PAGO
        // =========================
        if (string.IsNullOrWhiteSpace(request.FormaPago))
        {
            errores.Add("La forma de pago es obligatoria.");
        }
        else
        {
            var formasPagoPermitidas = new[]
            {
                CatalogosSunat.FormaPago.Contado,
                CatalogosSunat.FormaPago.Credito
            };

            var formaPago = request.FormaPago.ToUpper();

            if (!formasPagoPermitidas.Contains(formaPago))
                errores.Add("La forma de pago debe ser CONTADO o CREDITO.");

            if (formaPago == CatalogosSunat.FormaPago.Contado && request.MontoPendientePago > 0)
                errores.Add("Si la forma de pago es CONTADO, el monto pendiente debe ser 0.");

            if (formaPago == CatalogosSunat.FormaPago.Credito && request.MontoPendientePago <= 0)
                errores.Add("Si la forma de pago es CREDITO, el monto pendiente debe ser mayor a 0.");

            if (formaPago == CatalogosSunat.FormaPago.Credito &&
                !MontosCoinciden(request.MontoPendientePago, request.Total))
            {
                errores.Add(
                    "Si la forma de pago es CREDITO, el monto pendiente debe coincidir con el total del comprobante.");
            }
        }

        if (request.MontoPendientePago < 0)
            errores.Add("El monto pendiente de pago no puede ser negativo.");

        if (!TieneDecimalesValidos(request.MontoPendientePago))
            errores.Add("El monto pendiente de pago debe tener máximo 2 decimales.");

        // =========================
        // CUOTAS
        // =========================
        var formaPagoNormalizada = request.FormaPago?.ToUpper() ?? string.Empty;

        if (formaPagoNormalizada == CatalogosSunat.FormaPago.Contado)
        {
            if (request.Cuotas != null && request.Cuotas.Count > 0)
                errores.Add("Si la forma de pago es CONTADO, no debe enviar cuotas.");
        }

        if (formaPagoNormalizada == CatalogosSunat.FormaPago.Credito)
        {
            if (request.Cuotas == null || request.Cuotas.Count == 0)
            {
                errores.Add("Si la forma de pago es CREDITO, debe enviar al menos una cuota.");
            }
            else
            {
                var sumaCuotas = request.Cuotas.Sum(c => c.Monto);

                if (!MontosCoinciden(sumaCuotas, request.MontoPendientePago))
                    errores.Add(
                        $"La suma de cuotas ({sumaCuotas}) no coincide con el monto pendiente ({request.MontoPendientePago}).");

                foreach (var cuota in request.Cuotas)
                {
                    if (cuota.Numero <= 0)
                        errores.Add("El número de cuota debe ser mayor a 0.");

                    if (cuota.FechaVencimiento == default)
                        errores.Add($"La fecha de vencimiento de la cuota {cuota.Numero} es obligatoria.");

                    if (cuota.FechaVencimiento.Date < request.FechaEmision.Date)
                        errores.Add(
                            $"La fecha de vencimiento de la cuota {cuota.Numero} no puede ser menor a la fecha de emisión.");

                    if (cuota.Monto <= 0)
                        errores.Add($"El monto de la cuota {cuota.Numero} debe ser mayor a 0.");

                    if (!TieneDecimalesValidos(cuota.Monto))
                        errores.Add($"El monto de la cuota {cuota.Numero} debe tener máximo 2 decimales.");
                }

                var numerosDuplicados = request.Cuotas
                    .GroupBy(c => c.Numero)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (numerosDuplicados.Count > 0)
                    errores.Add($"Hay números de cuota duplicados: {string.Join(", ", numerosDuplicados)}.");
            }
        }

        // =========================
        // ITEMS
        // =========================
        if (request.Items == null || request.Items.Count == 0)
        {
            errores.Add("El comprobante debe tener al menos un item.");
        }
        else
        {
            foreach (var item in request.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Descripcion))
                    errores.Add("La descripción del item es obligatoria.");

                if (string.IsNullOrWhiteSpace(item.UnidadMedida))
                    errores.Add($"La unidad de medida del item {item.Descripcion} es obligatoria.");

                if (item.Cantidad <= 0)
                    errores.Add($"La cantidad del item {item.Descripcion} debe ser mayor a 0.");

                if (item.ValorUnitario < 0)
                    errores.Add($"El valor unitario del item {item.Descripcion} no puede ser negativo.");

                if (item.PrecioUnitario <= 0)
                    errores.Add($"El precio unitario del item {item.Descripcion} debe ser mayor a 0.");

                if (item.Subtotal < 0)
                    errores.Add($"El subtotal del item {item.Descripcion} no puede ser negativo.");

                if (item.Igv < 0)
                    errores.Add($"El IGV del item {item.Descripcion} no puede ser negativo.");

                if (item.Total <= 0)
                    errores.Add($"El total del item {item.Descripcion} debe ser mayor a 0.");

                if (!TieneDecimalesValidos(item.Cantidad))
                    errores.Add($"La cantidad del item {item.Descripcion} debe tener máximo 2 decimales.");

                if (!TieneDecimalesValidos(item.ValorUnitario))
                    errores.Add($"El valor unitario del item {item.Descripcion} debe tener máximo 2 decimales.");

                if (!TieneDecimalesValidos(item.PrecioUnitario))
                    errores.Add($"El precio unitario del item {item.Descripcion} debe tener máximo 2 decimales.");

                if (!TieneDecimalesValidos(item.Subtotal))
                    errores.Add($"El subtotal del item {item.Descripcion} debe tener máximo 2 decimales.");

                if (!TieneDecimalesValidos(item.Igv))
                    errores.Add($"El IGV del item {item.Descripcion} debe tener máximo 2 decimales.");

                if (!TieneDecimalesValidos(item.Total))
                    errores.Add($"El total del item {item.Descripcion} debe tener máximo 2 decimales.");

                if (string.IsNullOrWhiteSpace(item.CodigoAfectacionIgv))
                {
                    errores.Add($"El código de afectación IGV del item {item.Descripcion} es obligatorio.");
                }
                else
                {
                    var codigosPermitidos = new[]
                    {
                        CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa,
                        CatalogosSunat.AfectacionIgv.ExoneradoOperacionOnerosa,
                        CatalogosSunat.AfectacionIgv.InafectoOperacionOnerosa
                    };

                    if (!codigosPermitidos.Contains(item.CodigoAfectacionIgv))
                        errores.Add($"El código de afectación IGV del item {item.Descripcion} debe ser 10, 20 o 30.");

                    if (item.CodigoAfectacionIgv == CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa &&
                        item.Igv <= 0)
                    {
                        errores.Add($"El item {item.Descripcion} es gravado, por lo tanto debe tener IGV mayor a 0.");
                    }

                    if ((item.CodigoAfectacionIgv == CatalogosSunat.AfectacionIgv.ExoneradoOperacionOnerosa ||
                         item.CodigoAfectacionIgv == CatalogosSunat.AfectacionIgv.InafectoOperacionOnerosa) &&
                        item.Igv != 0)
                    {
                        errores.Add(
                            $"El item {item.Descripcion} es exonerado o inafecto, por lo tanto su IGV debe ser 0.");
                    }
                }

                var subtotalCalculado = item.Cantidad * item.ValorUnitario;

                if (!MontosCoinciden(subtotalCalculado, item.Subtotal))
                    errores.Add($"El subtotal del item {item.Descripcion} no coincide con cantidad x valor unitario.");

                var totalItemCalculadoConIgv = item.Subtotal + item.Igv;

                if (!MontosCoinciden(totalItemCalculadoConIgv, item.Total))
                    errores.Add($"El total del item {item.Descripcion} no coincide con subtotal + IGV.");
            }

            var sumaItems = request.Items.Sum(i => i.Total);

            if (!MontosCoinciden(sumaItems, request.Total))
                errores.Add(
                    $"La suma de los items ({sumaItems}) no coincide con el total del comprobante ({request.Total}).");
        }

        // =========================
        // TOTALES
        // =========================
        if (request.Total <= 0)
            errores.Add("El total debe ser mayor a 0.");

        if (!string.IsNullOrWhiteSpace(request.MontoEnLetras) &&
            request.MontoEnLetras.Length < 5)
        {
            errores.Add("El monto en letras no parece válido.");
        }

        if (request.TotalGravada < 0)
            errores.Add("El total gravado no puede ser negativo.");

        if (request.TotalExonerada < 0)
            errores.Add("El total exonerado no puede ser negativo.");

        if (request.TotalInafecta < 0)
            errores.Add("El total inafecto no puede ser negativo.");

        if (request.TotalIgv < 0)
            errores.Add("El IGV no puede ser negativo.");

        if (!TieneDecimalesValidos(request.TotalGravada))
            errores.Add("El total gravado debe tener máximo 2 decimales.");

        if (!TieneDecimalesValidos(request.TotalExonerada))
            errores.Add("El total exonerado debe tener máximo 2 decimales.");

        if (!TieneDecimalesValidos(request.TotalInafecta))
            errores.Add("El total inafecto debe tener máximo 2 decimales.");

        if (!TieneDecimalesValidos(request.TotalIgv))
            errores.Add("El IGV debe tener máximo 2 decimales.");

        if (!TieneDecimalesValidos(request.Total))
            errores.Add("El total debe tener máximo 2 decimales.");

        var totalCalculado = request.TotalGravada
                             + request.TotalExonerada
                             + request.TotalInafecta
                             + request.TotalIgv;

        if (!MontosCoinciden(totalCalculado, request.Total))
            errores.Add(
                $"La suma de gravada + exonerada + inafecta + IGV ({totalCalculado}) no coincide con el total ({request.Total}).");

        if (request.Items != null && request.Items.Count > 0)
        {
            var sumaGravada = request.Items
                .Where(i => i.CodigoAfectacionIgv == CatalogosSunat.AfectacionIgv.GravadoOperacionOnerosa)
                .Sum(i => i.Subtotal);

            var sumaExonerada = request.Items
                .Where(i => i.CodigoAfectacionIgv == CatalogosSunat.AfectacionIgv.ExoneradoOperacionOnerosa)
                .Sum(i => i.Subtotal);

            var sumaInafecta = request.Items
                .Where(i => i.CodigoAfectacionIgv == CatalogosSunat.AfectacionIgv.InafectoOperacionOnerosa)
                .Sum(i => i.Subtotal);

            var sumaIgv = request.Items.Sum(i => i.Igv);

            if (!MontosCoinciden(sumaGravada, request.TotalGravada))
                errores.Add(
                    $"La suma de items gravados ({sumaGravada}) no coincide con el total gravado ({request.TotalGravada}).");

            if (!MontosCoinciden(sumaExonerada, request.TotalExonerada))
                errores.Add(
                    $"La suma de items exonerados ({sumaExonerada}) no coincide con el total exonerado ({request.TotalExonerada}).");

            if (!MontosCoinciden(sumaInafecta, request.TotalInafecta))
                errores.Add(
                    $"La suma de items inafectos ({sumaInafecta}) no coincide con el total inafecto ({request.TotalInafecta}).");

            if (!MontosCoinciden(sumaIgv, request.TotalIgv))
                errores.Add(
                    $"La suma de IGV de los items ({sumaIgv}) no coincide con el total IGV ({request.TotalIgv}).");
        }

        return errores;
    }

    private static bool EsNumerico(string valor)
    {
        return !string.IsNullOrWhiteSpace(valor) && valor.All(char.IsDigit);
    }

    private static bool TieneDecimalesValidos(decimal valor)
    {
        return decimal.Round(valor, 2) == valor;
    }

    private static bool MontosCoinciden(decimal monto1, decimal monto2)
    {
        var diferencia = Math.Abs(monto1 - monto2);
        return diferencia <= 0.01m;
    }
}