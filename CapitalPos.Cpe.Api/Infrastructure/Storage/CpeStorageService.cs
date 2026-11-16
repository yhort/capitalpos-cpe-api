using System.IO.Compression;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Infrastructure.Storage;

public class CpeStorageService : ICpeStorageService
{
    private readonly CpeSettings _settings;
    private readonly IEmisorContextAccessor _emisorContextAccessor;

    public CpeStorageService(
        IOptions<CpeSettings> options,
        IEmisorContextAccessor emisorContextAccessor)
    {
        _settings = options.Value;
        _emisorContextAccessor = emisorContextAccessor;
    }

    public string ObtenerCarpetaXml(string? ruc = null)
        => ObtenerCarpeta("XML", ruc);

    public string GuardarXml(string nombreArchivo, string contenidoXml, string? ruc = null)
    {
        var carpetaXml = ObtenerCarpetaXml(ruc);
        var rutaArchivo = Path.Combine(carpetaXml, nombreArchivo);
        File.WriteAllText(rutaArchivo, contenidoXml);
        return rutaArchivo;
    }

    public List<string> ListarXml(string? ruc = null)
        => ListarNombres(ObtenerCarpetaXml(ruc), "*.xml");

    public string? LeerXml(string nombreArchivo, string? ruc = null)
    {
        if (!NombreArchivoValido(nombreArchivo, ".xml"))
            return null;

        var rutaArchivo = Path.Combine(ObtenerCarpetaXml(ruc), nombreArchivo);
        return File.Exists(rutaArchivo) ? File.ReadAllText(rutaArchivo) : null;
    }

    public string? ObtenerRutaXml(string nombreArchivo, string? ruc = null)
    {
        if (!NombreArchivoValido(nombreArchivo, ".xml"))
            return null;

        var rutaArchivo = Path.Combine(ObtenerCarpetaXml(ruc), nombreArchivo);
        return File.Exists(rutaArchivo) ? rutaArchivo : null;
    }

    public string ObtenerCarpetaCdr(string? ruc = null)
        => ObtenerCarpeta("CDR", ruc);

    public string GuardarCdr(string nombreArchivo, string contenido, string? ruc = null)
    {
        var rutaArchivo = Path.Combine(ObtenerCarpetaCdr(ruc), nombreArchivo);
        File.WriteAllText(rutaArchivo, contenido);
        return rutaArchivo;
    }

    public string GuardarCdrBytes(string nombreArchivo, byte[] contenido, string? ruc = null)
    {
        if (!NombreArchivoValido(nombreArchivo, ".zip"))
            throw new ArgumentException("Nombre de CDR inválido.", nameof(nombreArchivo));

        var rutaArchivo = Path.Combine(ObtenerCarpetaCdr(ruc), nombreArchivo);
        File.WriteAllBytes(rutaArchivo, contenido);
        return rutaArchivo;
    }

    public List<string> ListarCdr(string? ruc = null)
        => ListarNombres(ObtenerCarpetaCdr(ruc), "*.zip");

    public string? ObtenerRutaCdr(string nombreArchivo, string? ruc = null)
    {
        if (!NombreArchivoValido(nombreArchivo, ".zip"))
            return null;

        var rutaArchivo = Path.Combine(ObtenerCarpetaCdr(ruc), nombreArchivo);
        return File.Exists(rutaArchivo) ? rutaArchivo : null;
    }

    public string ObtenerCarpetaZip(string? ruc = null)
        => ObtenerCarpeta("ZIP", ruc);

    public string GuardarZipDesdeXml(string nombreXml, string? ruc = null)
    {
        if (!NombreArchivoValido(nombreXml, ".xml"))
            throw new ArgumentException("Nombre de XML inválido.", nameof(nombreXml));

        var rutaXml = ObtenerRutaXml(nombreXml, ruc)
                      ?? throw new FileNotFoundException("No se encontró el XML para comprimir.", nombreXml);

        var carpetaZip = ObtenerCarpetaZip(ruc);
        var nombreZip = nombreXml.Replace(".xml", ".zip", StringComparison.OrdinalIgnoreCase);
        var rutaZip = Path.Combine(carpetaZip, nombreZip);

        if (File.Exists(rutaZip))
            File.Delete(rutaZip);

        using var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create);
        zip.CreateEntryFromFile(rutaXml, nombreXml);

        return rutaZip;
    }

    public List<string> ListarZip(string? ruc = null)
        => ListarNombres(ObtenerCarpetaZip(ruc), "*.zip");

    public string? ObtenerRutaZip(string nombreArchivo, string? ruc = null)
    {
        if (!NombreArchivoValido(nombreArchivo, ".zip"))
            return null;

        var rutaArchivo = Path.Combine(ObtenerCarpetaZip(ruc), nombreArchivo);
        return File.Exists(rutaArchivo) ? rutaArchivo : null;
    }

    public List<CpeArchivoInfoDto> ListarXmlInfo(string? ruc = null)
        => ListarArchivosInfo(ObtenerCarpetaXml(ruc), "*.xml");

    public List<CpeArchivoInfoDto> ListarZipInfo(string? ruc = null)
        => ListarArchivosInfo(ObtenerCarpetaZip(ruc), "*.zip");

    public List<CpeArchivoInfoDto> ListarCdrInfo(string? ruc = null)
        => ListarArchivosInfo(ObtenerCarpetaCdr(ruc), "*.zip");

    public CpeArchivosResumenDto ObtenerResumenArchivos(string? ruc = null)
    {
        var xml = ListarXmlInfo(ruc);
        var zip = ListarZipInfo(ruc);
        var cdr = ListarCdrInfo(ruc);

        return new CpeArchivosResumenDto
        {
            Xml = CrearResumenGrupo(xml),
            Zip = CrearResumenGrupo(zip),
            Cdr = CrearResumenGrupo(cdr),
            FechaConsulta = DateTime.Now
        };
    }

    public CpeLimpiezaArchivosResponse LimpiarArchivosAntiguos(
        int diasAntiguedad,
        bool ejecutar,
        string? ruc = null)
    {
        if (diasAntiguedad <= 0)
            diasAntiguedad = 7;

        var fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);
        var archivos = new List<CpeArchivoLimpiezaDto>();

        archivos.AddRange(ObtenerArchivosParaLimpieza("XML", ObtenerCarpetaXml(ruc), "*.xml", fechaLimite));
        archivos.AddRange(ObtenerArchivosParaLimpieza("ZIP", ObtenerCarpetaZip(ruc), "*.zip", fechaLimite));
        archivos.AddRange(ObtenerArchivosParaLimpieza("CDR", ObtenerCarpetaCdr(ruc), "*.zip", fechaLimite));

        if (ejecutar)
        {
            foreach (var archivo in archivos)
            {
                try
                {
                    if (File.Exists(archivo.Ruta))
                    {
                        File.Delete(archivo.Ruta);
                        archivo.Eliminado = true;
                        archivo.Mensaje = "Archivo eliminado correctamente.";
                    }
                    else
                    {
                        archivo.Eliminado = false;
                        archivo.Mensaje = "El archivo ya no existe.";
                    }
                }
                catch (Exception ex)
                {
                    archivo.Eliminado = false;
                    archivo.Mensaje = $"No se pudo eliminar. Error: {ex.Message}";
                }
            }
        }
        else
        {
            foreach (var archivo in archivos)
            {
                archivo.Eliminado = false;
                archivo.Mensaje = "Simulación: este archivo sería eliminado.";
            }
        }

        return new CpeLimpiezaArchivosResponse
        {
            Ejecutado = ejecutar,
            DiasAntiguedad = diasAntiguedad,
            FechaLimite = fechaLimite,
            TotalArchivosEncontrados = archivos.Count,
            TotalArchivosEliminados = archivos.Count(a => a.Eliminado),
            Archivos = archivos
        };
    }

    private string ObtenerCarpeta(string tipo, string? ruc)
    {
        var rucEfectivo = ResolverRuc(ruc);
        var modo = ResolverModo();

        string carpeta;
        if (!string.IsNullOrWhiteSpace(rucEfectivo))
        {
            carpeta = Path.Combine(_settings.RutaArchivos, modo, rucEfectivo, tipo);
        }
        else
        {
            // Compatibilidad legacy: sin RUC usa carpeta plana del modo.
            carpeta = Path.Combine(_settings.RutaArchivos, modo, tipo);
        }

        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        return carpeta;
    }

    private string ResolverModo()
    {
        var ambiente = _emisorContextAccessor.Current?.Ambiente;
        if (!string.IsNullOrWhiteSpace(ambiente))
            return ambiente.Trim().ToUpperInvariant();

        return string.IsNullOrWhiteSpace(_settings.Modo)
            ? "BETA"
            : _settings.Modo.Trim().ToUpperInvariant();
    }

    private string? ResolverRuc(string? ruc)
    {
        if (!string.IsNullOrWhiteSpace(ruc))
            return ruc.Trim();

        return _emisorContextAccessor.Current?.Ruc;
    }

    private static List<string> ListarNombres(string carpeta, string patron)
    {
        if (!Directory.Exists(carpeta))
            return new List<string>();

        return Directory
            .GetFiles(carpeta, patron)
            .Select(Path.GetFileName)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => nombre!)
            .ToList();
    }

    private static bool NombreArchivoValido(string nombreArchivo, string extension)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            return false;

        if (nombreArchivo.Contains(".."))
            return false;

        if (nombreArchivo.Contains('/') || nombreArchivo.Contains('\\'))
            return false;

        return nombreArchivo.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    private static List<CpeArchivoInfoDto> ListarArchivosInfo(string carpeta, string patron)
    {
        if (!Directory.Exists(carpeta))
            return new List<CpeArchivoInfoDto>();

        return Directory.GetFiles(carpeta, patron)
            .Select(ruta =>
            {
                var info = new FileInfo(ruta);
                return new CpeArchivoInfoDto
                {
                    Nombre = info.Name,
                    Extension = info.Extension,
                    TamanoBytes = info.Length,
                    TamanoKb = Math.Round(info.Length / 1024m, 2),
                    FechaCreacion = info.CreationTime,
                    FechaModificacion = info.LastWriteTime
                };
            })
            .OrderByDescending(a => a.FechaModificacion)
            .ToList();
    }

    private static CpeArchivosGrupoResumenDto CrearResumenGrupo(List<CpeArchivoInfoDto> archivos)
    {
        var ultimoArchivo = archivos
            .OrderByDescending(a => a.FechaModificacion)
            .FirstOrDefault();

        var tamanoTotalBytes = archivos.Sum(a => a.TamanoBytes);

        return new CpeArchivosGrupoResumenDto
        {
            Cantidad = archivos.Count,
            TamanoTotalBytes = tamanoTotalBytes,
            TamanoTotalKb = Math.Round(tamanoTotalBytes / 1024m, 2),
            UltimoArchivo = ultimoArchivo?.Nombre,
            FechaUltimoArchivo = ultimoArchivo?.FechaModificacion
        };
    }

    private static List<CpeArchivoLimpiezaDto> ObtenerArchivosParaLimpieza(
        string tipo,
        string carpeta,
        string patron,
        DateTime fechaLimite)
    {
        if (!Directory.Exists(carpeta))
            return new List<CpeArchivoLimpiezaDto>();

        return Directory.GetFiles(carpeta, patron)
            .Select(ruta => new FileInfo(ruta))
            .Where(info => info.LastWriteTime < fechaLimite)
            .Select(info => new CpeArchivoLimpiezaDto
            {
                Tipo = tipo,
                Nombre = info.Name,
                Ruta = info.FullName,
                FechaModificacion = info.LastWriteTime,
                Eliminado = false,
                Mensaje = string.Empty
            })
            .OrderBy(a => a.FechaModificacion)
            .ToList();
    }
}
