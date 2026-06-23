using System.Xml.Linq;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml;

public class CpeXmlValidatorService : ICpeXmlValidatorService
{
    public CpeXmlValidacionResponse ValidarXml(string xml)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(xml))
        {
            errores.Add("El XML está vacío.");
        }
        else
        {
            try
            {
                XDocument.Parse(xml);
            }
            catch (Exception ex)
            {
                errores.Add($"El XML no está bien formado: {ex.Message}");
            }
        }

        if (errores.Count > 0)
        {
            return new CpeXmlValidacionResponse
            {
                Ok = false,
                Mensaje = "El XML tiene errores.",
                Errores = errores
            };
        }

        return new CpeXmlValidacionResponse
        {
            Ok = true,
            Mensaje = "El XML está bien formado."
        };
    }
}