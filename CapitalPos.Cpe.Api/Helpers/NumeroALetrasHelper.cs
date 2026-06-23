namespace CapitalPos.Cpe.Api.Helpers;

public static class NumeroALetrasHelper
{
    public static string ConvertirMonto(decimal monto, string moneda)
    {
        var entero = (long)Math.Truncate(monto);

        var centimosDecimal = (monto - entero) * 100;
        var centimos = (int)Math.Round(centimosDecimal, 0, MidpointRounding.AwayFromZero);

        if (centimos == 100)
        {
            entero++;
            centimos = 0;
        }

        var monedaTexto = ObtenerNombreMoneda(moneda);
        var letras = ConvertirNumero(entero);

        return $"SON {letras} CON {centimos:00}/100 {monedaTexto}".ToUpper();
    }

    private static string ObtenerNombreMoneda(string moneda)
    {
        return moneda.ToUpper() switch
        {
            "USD" => "DOLARES",
            _ => "SOLES"
        };
    }

    private static string ConvertirNumero(long numero)
    {
        if (numero == 0)
            return "CERO";

        if (numero < 0)
            return "MENOS " + ConvertirNumero(Math.Abs(numero));

        if (numero <= 99)
            return ConvertirDecenas((int)numero);

        if (numero <= 999)
            return ConvertirCentenas((int)numero);

        if (numero <= 999999)
            return ConvertirMiles((int)numero);

        if (numero <= 999999999)
            return ConvertirMillones(numero);

        return numero.ToString();
    }

    private static string ConvertirDecenas(int numero)
    {
        string[] unidades =
        {
            "", "UNO", "DOS", "TRES", "CUATRO", "CINCO",
            "SEIS", "SIETE", "OCHO", "NUEVE"
        };

        string[] especiales =
        {
            "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE",
            "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE"
        };

        string[] decenas =
        {
            "", "", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA",
            "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
        };

        if (numero < 10)
            return unidades[numero];

        if (numero < 20)
            return especiales[numero - 10];

        if (numero == 20)
            return "VEINTE";

        if (numero < 30)
            return "VEINTI" + unidades[numero - 20];

        var decena = numero / 10;
        var unidad = numero % 10;

        if (unidad == 0)
            return decenas[decena];

        return $"{decenas[decena]} Y {unidades[unidad]}";
    }

    private static string ConvertirCentenas(int numero)
    {
        if (numero == 100)
            return "CIEN";

        string[] centenas =
        {
            "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS",
            "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
        };

        var centena = numero / 100;
        var resto = numero % 100;

        if (resto == 0)
            return centenas[centena];

        return $"{centenas[centena]} {ConvertirDecenas(resto)}";
    }

    private static string ConvertirMiles(int numero)
    {
        var miles = numero / 1000;
        var resto = numero % 1000;

        string textoMiles;

        if (miles == 1)
            textoMiles = "MIL";
        else
            textoMiles = $"{ConvertirNumero(miles)} MIL";

        if (resto == 0)
            return textoMiles;

        return $"{textoMiles} {ConvertirNumero(resto)}";
    }

    private static string ConvertirMillones(long numero)
    {
        var millones = numero / 1000000;
        var resto = numero % 1000000;

        string textoMillones;

        if (millones == 1)
            textoMillones = "UN MILLON";
        else
            textoMillones = $"{ConvertirNumero(millones)} MILLONES";

        if (resto == 0)
            return textoMillones;

        return $"{textoMillones} {ConvertirNumero(resto)}";
    }
}