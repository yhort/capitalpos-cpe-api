using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeStorageService
{
    string ObtenerCarpetaXml(string? ruc = null);
    string GuardarXml(string nombreArchivo, string contenidoXml, string? ruc = null);
    List<string> ListarXml(string? ruc = null);
    string? LeerXml(string nombreArchivo, string? ruc = null);
    string? ObtenerRutaXml(string nombreArchivo, string? ruc = null);

    string ObtenerCarpetaCdr(string? ruc = null);
    string GuardarCdr(string nombreArchivo, string contenido, string? ruc = null);
    List<string> ListarCdr(string? ruc = null);
    string? ObtenerRutaCdr(string nombreArchivo, string? ruc = null);
    string ObtenerCarpetaZip(string? ruc = null);
    string GuardarZipDesdeXml(string nombreXml, string? ruc = null);
    List<string> ListarZip(string? ruc = null);
    string? ObtenerRutaZip(string nombreArchivo, string? ruc = null);

    List<CpeArchivoInfoDto> ListarXmlInfo(string? ruc = null);
    List<CpeArchivoInfoDto> ListarZipInfo(string? ruc = null);
    List<CpeArchivoInfoDto> ListarCdrInfo(string? ruc = null);

    CpeArchivosResumenDto ObtenerResumenArchivos(string? ruc = null);

    CpeLimpiezaArchivosResponse LimpiarArchivosAntiguos(int diasAntiguedad, bool ejecutar, string? ruc = null);

    string GuardarCdrBytes(string nombreArchivo, byte[] contenido, string? ruc = null);
}
