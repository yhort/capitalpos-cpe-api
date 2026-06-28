namespace CapitalPos.Cpe.Api.Infrastructure.Storage;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using CapitalPos.Cpe.Api.Interfaces;
using System.IO.Compression;
using CapitalPos.Cpe.Api.Dtos;

public class CpeStorageService : ICpeStorageService
{
    private readonly CpeSettings _settings;

    public CpeStorageService(IOptions<CpeSettings> options)
    {
        _settings = options.Value;
    }

    public string ObtenerCarpetaXml()
    {
        var carpeta = Path.Combine(_settings.RutaArchivos, _settings.Modo, "XML");

        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        return carpeta;
    }

    public string GuardarXml(string nombreArchivo, string contenidoXml)
    {
        var carpetaXml = ObtenerCarpetaXml();
        var rutaArchivo = Path.Combine(carpetaXml, nombreArchivo);

        File.WriteAllText(rutaArchivo, contenidoXml);

        return rutaArchivo;
    }
    
    public List<string> ListarXml()
    {
        var carpetaXml = ObtenerCarpetaXml();

        return Directory
            .GetFiles(carpetaXml, "*.xml")
            .Select(Path.GetFileName)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => nombre!)
            .ToList();
    }
    
    public string? LeerXml(string nombreArchivo)
    {
        if (!NombreArchivoValido(nombreArchivo))
            return null;

        var carpetaXml = ObtenerCarpetaXml();
        var rutaArchivo = Path.Combine(carpetaXml, nombreArchivo);

        if (!File.Exists(rutaArchivo))
            return null;

        return File.ReadAllText(rutaArchivo);
    }
    
    private static bool NombreArchivoValido(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            return false;

        if (nombreArchivo.Contains(".."))
            return false;

        if (nombreArchivo.Contains("/") || nombreArchivo.Contains("\\"))
            return false;

        if (!nombreArchivo.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
    
    public string? ObtenerRutaXml(string nombreArchivo)
    {
        if (!NombreArchivoValido(nombreArchivo))
            return null;

        var carpetaXml = ObtenerCarpetaXml();
        var rutaArchivo = Path.Combine(carpetaXml, nombreArchivo);

        if (!File.Exists(rutaArchivo))
            return null;

        return rutaArchivo;
    }
    
    private static bool NombreArchivoZipValido(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            return false;

        if (nombreArchivo.Contains(".."))
            return false;

        if (nombreArchivo.Contains("/") || nombreArchivo.Contains("\\"))
            return false;

        if (!nombreArchivo.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
    
    public string ObtenerCarpetaCdr()
    {
        var carpeta = Path.Combine(_settings.RutaArchivos, _settings.Modo, "CDR");

        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        return carpeta;
    }

    public string GuardarCdr(string nombreArchivo, string contenido)
    {
        var carpetaCdr = ObtenerCarpetaCdr();
        var rutaArchivo = Path.Combine(carpetaCdr, nombreArchivo);

        File.WriteAllText(rutaArchivo, contenido);

        return rutaArchivo;
    }
    
    public string GuardarCdrBytes(string nombreArchivo, byte[] contenido)
    {
        if (!NombreArchivoZipValido(nombreArchivo))
            throw new ArgumentException("Nombre de CDR inválido.", nameof(nombreArchivo));

        var carpetaCdr = ObtenerCarpetaCdr();
        var rutaArchivo = Path.Combine(carpetaCdr, nombreArchivo);

        File.WriteAllBytes(rutaArchivo, contenido);

        return rutaArchivo;
    }

    public List<string> ListarCdr()
    {
        var carpetaCdr = ObtenerCarpetaCdr();

        return Directory
            .GetFiles(carpetaCdr, "*.zip")
            .Select(Path.GetFileName)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => nombre!)
            .ToList();
    }

    public string? ObtenerRutaCdr(string nombreArchivo)
    {
        if (!NombreArchivoZipValido(nombreArchivo))
            return null;

        var carpetaCdr = ObtenerCarpetaCdr();
        var rutaArchivo = Path.Combine(carpetaCdr, nombreArchivo);

        if (!File.Exists(rutaArchivo))
            return null;

        return rutaArchivo;
    }
    
    public string ObtenerCarpetaZip()
    {
        var carpeta = Path.Combine(_settings.RutaArchivos, _settings.Modo, "ZIP");

        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        return carpeta;
    }

    public string GuardarZipDesdeXml(string nombreXml)
    {
        if (!NombreArchivoValido(nombreXml))
            throw new ArgumentException("Nombre de XML inválido.", nameof(nombreXml));

        var rutaXml = ObtenerRutaXml(nombreXml);

        if (rutaXml == null)
            throw new FileNotFoundException("No se encontró el XML para comprimir.", nombreXml);

        var carpetaZip = ObtenerCarpetaZip();
        var nombreZip = nombreXml.Replace(".xml", ".zip", StringComparison.OrdinalIgnoreCase);
        var rutaZip = Path.Combine(carpetaZip, nombreZip);

        if (File.Exists(rutaZip))
            File.Delete(rutaZip);

        using var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create);
        zip.CreateEntryFromFile(rutaXml, nombreXml);

        return rutaZip;
    }

    public List<string> ListarZip()
    {
        var carpetaZip = ObtenerCarpetaZip();

        return Directory
            .GetFiles(carpetaZip, "*.zip")
            .Select(Path.GetFileName)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Select(nombre => nombre!)
            .ToList();
    }

    public string? ObtenerRutaZip(string nombreArchivo)
    {
        if (!NombreArchivoZipValido(nombreArchivo))
            return null;

        var carpetaZip = ObtenerCarpetaZip();
        var rutaArchivo = Path.Combine(carpetaZip, nombreArchivo);

        if (!File.Exists(rutaArchivo))
            return null;

        return rutaArchivo;
    }
    
    public List<CpeArchivoInfoDto> ListarXmlInfo()
    {
        return ListarArchivosInfo(ObtenerCarpetaXml(), "*.xml");
    }

    public List<CpeArchivoInfoDto> ListarZipInfo()
    {
        return ListarArchivosInfo(ObtenerCarpetaZip(), "*.zip");
    }

    public List<CpeArchivoInfoDto> ListarCdrInfo()
    {
        return ListarArchivosInfo(ObtenerCarpetaCdr(), "*.zip");
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
    
    public CpeArchivosResumenDto ObtenerResumenArchivos()
    {
        var xml = ListarXmlInfo();
        var zip = ListarZipInfo();
        var cdr = ListarCdrInfo();

        return new CpeArchivosResumenDto
        {
            Xml = CrearResumenGrupo(xml),
            Zip = CrearResumenGrupo(zip),
            Cdr = CrearResumenGrupo(cdr),
            FechaConsulta = DateTime.Now
        };
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
    
    public CpeLimpiezaArchivosResponse LimpiarArchivosAntiguos(int diasAntiguedad, bool ejecutar)
    {
        if (diasAntiguedad <= 0)
            diasAntiguedad = 7;

        var fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);

        var archivos = new List<CpeArchivoLimpiezaDto>();

        archivos.AddRange(ObtenerArchivosParaLimpieza("XML", ObtenerCarpetaXml(), "*.xml", fechaLimite));
        archivos.AddRange(ObtenerArchivosParaLimpieza("ZIP", ObtenerCarpetaZip(), "*.zip", fechaLimite));
        archivos.AddRange(ObtenerArchivosParaLimpieza("CDR", ObtenerCarpetaCdr(), "*.zip", fechaLimite));

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