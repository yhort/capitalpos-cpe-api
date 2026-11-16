using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Infrastructure.Firma;
using CapitalPos.Cpe.Api.Infrastructure.Tenancy;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeFirmaServiceTests
{
    [Fact]
    public void FirmarXml_ModoSimulado_RespuestaExitosa()
    {
        var sut = CrearSut(simularFirma: true);

        var resultado = sut.FirmarXml("20100066603-03-B001-3002.xml");

        Assert.True(resultado.Ok);
        Assert.True(resultado.XmlFirmado);
        Assert.Equal("20100066603-03-B001-3002.xml", resultado.NombreXml);
        Assert.Contains("simulación", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(resultado.Errores);
    }

    [Fact]
    public void FirmarXml_SinNombre_RespuestaFallida()
    {
        var sut = CrearSut(simularFirma: true);

        var resultado = sut.FirmarXml("  ");

        Assert.False(resultado.Ok);
        Assert.False(resultado.XmlFirmado);
        Assert.Contains("obligatorio", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            resultado.Errores,
            e => e.Contains("nombre del archivo XML", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FirmarXml_FirmaRealSinCertificado_RespuestaFallida()
    {
        var sut = CrearSut(
            simularFirma: false,
            rutaCertificado: "",
            passwordCertificado: "");

        var resultado = sut.FirmarXml("20100066603-03-B001-3002.xml");

        Assert.False(resultado.Ok);
        Assert.False(resultado.XmlFirmado);
        Assert.Contains("configuración de firma", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            resultado.Errores,
            e => e.Contains("certificado digital", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            resultado.Errores,
            e => e.Contains("contraseña del certificado", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FirmarXml_FirmaRealSinArchivoXml_RespuestaFallida()
    {
        var rutaCertificado = Path.GetTempFileName();
        try
        {
            // Debe existir y no estar vacío para superar la validación legacy de ruta.
            File.WriteAllBytes(rutaCertificado, new byte[] { 1, 2, 3, 4 });

            var sut = CrearSut(
                simularFirma: false,
                rutaCertificado: rutaCertificado,
                passwordCertificado: "demo-password",
                storage: new FakeStorageService(rutaXml: null));

            var resultado = sut.FirmarXml("archivo-inexistente.xml");

            Assert.False(resultado.Ok);
            Assert.False(resultado.XmlFirmado);
            Assert.Contains("No se encontró el XML", resultado.Mensaje, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(
                resultado.Errores,
                e => e.Contains("No existe el archivo XML", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            File.Delete(rutaCertificado);
        }
    }

    private static CpeFirmaService CrearSut(
        bool simularFirma,
        string rutaCertificado = "",
        string passwordCertificado = "",
        ICpeStorageService? storage = null)
    {
        var settings = Options.Create(new CpeSettings
        {
            SimularFirma = simularFirma,
            PermitirCredencialesLegacy = true,
            RutaCertificado = rutaCertificado,
            PasswordCertificado = passwordCertificado
        });

        return new CpeFirmaService(
            settings,
            storage ?? new FakeStorageService(rutaXml: null),
            new EmisorContextAccessor());
    }

    private sealed class FakeStorageService : ICpeStorageService
    {
        private readonly string? _rutaXml;

        public FakeStorageService(string? rutaXml)
        {
            _rutaXml = rutaXml;
        }

        public string? ObtenerRutaXml(string nombreArchivo, string? ruc = null) => _rutaXml;
        public string ObtenerCarpetaXml(string? ruc = null) => string.Empty;
        public string GuardarXml(string nombreArchivo, string contenidoXml, string? ruc = null) => string.Empty;
        public List<string> ListarXml(string? ruc = null) => new();
        public string? LeerXml(string nombreArchivo, string? ruc = null) => null;
        public string ObtenerCarpetaCdr(string? ruc = null) => string.Empty;
        public string GuardarCdr(string nombreArchivo, string contenido, string? ruc = null) => string.Empty;
        public List<string> ListarCdr(string? ruc = null) => new();
        public string? ObtenerRutaCdr(string nombreArchivo, string? ruc = null) => null;
        public string ObtenerCarpetaZip(string? ruc = null) => string.Empty;
        public string GuardarZipDesdeXml(string nombreXml, string? ruc = null) => string.Empty;
        public List<string> ListarZip(string? ruc = null) => new();
        public string? ObtenerRutaZip(string nombreArchivo, string? ruc = null) => null;
        public List<CpeArchivoInfoDto> ListarXmlInfo(string? ruc = null) => new();
        public List<CpeArchivoInfoDto> ListarZipInfo(string? ruc = null) => new();
        public List<CpeArchivoInfoDto> ListarCdrInfo(string? ruc = null) => new();
        public CpeArchivosResumenDto ObtenerResumenArchivos(string? ruc = null) => new();
        public CpeLimpiezaArchivosResponse LimpiarArchivosAntiguos(int diasAntiguedad, bool ejecutar, string? ruc = null) =>
            new();
        public string GuardarCdrBytes(string nombreArchivo, byte[] contenido, string? ruc = null) => string.Empty;
    }
}
