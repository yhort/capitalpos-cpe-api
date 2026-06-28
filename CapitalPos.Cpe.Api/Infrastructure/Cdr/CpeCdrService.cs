using System.IO.Compression;
using System.Xml.Linq;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;

namespace CapitalPos.Cpe.Api.Infrastructure.Cdr;

public class CpeCdrService : ICpeCdrService
{
    private readonly ICpeStorageService _storageService;

    public CpeCdrService(ICpeStorageService storageService)
    {
        _storageService = storageService;
    }

    public CpeCdrResultadoDto LeerCdr(string nombreCdr)
    {
        if (string.IsNullOrWhiteSpace(nombreCdr))
        {
            return new CpeCdrResultadoDto
            {
                Ok = false,
                Estado = CpeEstados.ErrorCdr,
                NombreCdr = nombreCdr,
                Errores = new List<string>
                {
                    "Debe indicar el nombre del archivo CDR."
                }
            };
        }

        var rutaCdr = _storageService.ObtenerRutaCdr(nombreCdr);

        if (string.IsNullOrWhiteSpace(rutaCdr))
        {
            return new CpeCdrResultadoDto
            {
                Ok = false,
                Estado = CpeEstados.ErrorCdr,
                NombreCdr = nombreCdr,
                Errores = new List<string>
                {
                    $"No existe el archivo CDR: {nombreCdr}"
                }
            };
        }

        try
        {
            using var zip = ZipFile.OpenRead(rutaCdr);

            var entradaXml = zip.Entries
                .FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if (entradaXml == null)
            {
                return new CpeCdrResultadoDto
                {
                    Ok = false,
                    Estado = CpeEstados.ErrorCdr,
                    NombreCdr = nombreCdr,
                    Errores = new List<string>
                    {
                        "El ZIP del CDR no contiene un archivo XML."
                    }
                };
            }

            using var stream = entradaXml.Open();
            var xml = XDocument.Load(stream);

            var codigoSunat = ObtenerValor(xml, "ResponseCode");
            var mensajeSunat = ObtenerValor(xml, "Description");

            var observaciones = xml
                .Descendants()
                .Where(e => e.Name.LocalName == "Note")
                .Select(e => e.Value.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();

            var estado = codigoSunat == "0"
                ? CpeEstados.Aceptado
                : CpeEstados.Rechazado;

            return new CpeCdrResultadoDto
            {
                Ok = codigoSunat == "0",
                Estado = estado,
                CodigoSunat = codigoSunat,
                MensajeSunat = mensajeSunat,
                NombreCdr = nombreCdr,
                Observaciones = observaciones,
                Errores = new List<string>()
            };
        }
        catch (InvalidDataException)
        {
            return new CpeCdrResultadoDto
            {
                Ok = false,
                Estado = CpeEstados.ErrorCdr,
                NombreCdr = nombreCdr,
                Errores = new List<string>
                {
                    "El archivo CDR no es un ZIP válido. Probablemente es un CDR simulado guardado como texto."
                }
            };
        }
        catch (Exception ex)
        {
            return new CpeCdrResultadoDto
            {
                Ok = false,
                Estado = CpeEstados.ErrorCdr,
                NombreCdr = nombreCdr,
                Errores = new List<string>
                {
                    ex.Message
                }
            };
        }
    }

    private static string ObtenerValor(XDocument xml, string nombreNodo)
    {
        return xml
            .Descendants()
            .FirstOrDefault(e => e.Name.LocalName == nombreNodo)
            ?.Value
            .Trim() ?? string.Empty;
    }
}