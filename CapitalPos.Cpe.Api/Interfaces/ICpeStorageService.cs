using CapitalPos.Cpe.Api.Dtos;
namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeStorageService
{
    string ObtenerCarpetaXml();
    string GuardarXml(string nombreArchivo, string contenidoXml);
    List<string> ListarXml();
    string? LeerXml(string nombreArchivo);
    string? ObtenerRutaXml(string nombreArchivo);
    
    string ObtenerCarpetaCdr();
    string GuardarCdr(string nombreArchivo, string contenido);
    List<string> ListarCdr();
    string? ObtenerRutaCdr(string nombreArchivo);
    string ObtenerCarpetaZip();
    string GuardarZipDesdeXml(string nombreXml);
    List<string> ListarZip();
    string? ObtenerRutaZip(string nombreArchivo);
    
    List<CpeArchivoInfoDto> ListarXmlInfo();
    List<CpeArchivoInfoDto> ListarZipInfo();
    List<CpeArchivoInfoDto> ListarCdrInfo();
    
    CpeArchivosResumenDto ObtenerResumenArchivos();
    
    CpeLimpiezaArchivosResponse LimpiarArchivosAntiguos(int diasAntiguedad, bool ejecutar);
    
    
}