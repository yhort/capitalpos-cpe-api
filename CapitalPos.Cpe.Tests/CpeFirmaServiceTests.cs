using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Infrastructure.Firma;
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
            RutaCertificado = rutaCertificado,
            PasswordCertificado = passwordCertificado
        });

        return new CpeFirmaService(settings, storage ?? new FakeStorageService(rutaXml: null));
    }

    private sealed class FakeStorageService : ICpeStorageService
    {
        private readonly string? _rutaXml;

        public FakeStorageService(string? rutaXml)
        {
            _rutaXml = rutaXml;
        }

        public string? ObtenerRutaXml(string nombreArchivo) => _rutaXml;

        public string ObtenerCarpetaXml() => string.Empty;
        public string GuardarXml(string nombreArchivo, string contenidoXml) => string.Empty;
        public List<string> ListarXml() => new();
        public string? LeerXml(string nombreArchivo) => null;
        public string ObtenerCarpetaCdr() => string.Empty;
        public string GuardarCdr(string nombreArchivo, string contenido) => string.Empty;
        public List<string> ListarCdr() => new();
        public string? ObtenerRutaCdr(string nombreArchivo) => null;
        public string ObtenerCarpetaZip() => string.Empty;
        public string GuardarZipDesdeXml(string nombreXml) => string.Empty;
        public List<string> ListarZip() => new();
        public string? ObtenerRutaZip(string nombreArchivo) => null;
        public List<CpeArchivoInfoDto> ListarXmlInfo() => new();
        public List<CpeArchivoInfoDto> ListarZipInfo() => new();
        public List<CpeArchivoInfoDto> ListarCdrInfo() => new();
        public CpeArchivosResumenDto ObtenerResumenArchivos() => new();
        public CpeLimpiezaArchivosResponse LimpiarArchivosAntiguos(int diasAntiguedad, bool ejecutar) =>
            new();
        public string GuardarCdrBytes(string nombreArchivo, byte[] contenido) => string.Empty;
    }
}
