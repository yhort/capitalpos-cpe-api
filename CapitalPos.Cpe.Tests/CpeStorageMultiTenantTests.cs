using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Infrastructure.Storage;
using CapitalPos.Cpe.Api.Infrastructure.Tenancy;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class CpeStorageMultiTenantTests
{
    [Fact]
    public void GuardarXml_DosRucs_AislaCarpetas()
    {
        var root = Path.Combine(Path.GetTempPath(), "cpe-mt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var accessor = new EmisorContextAccessor();
            var sut = new CpeStorageService(
                Options.Create(new CpeSettings
                {
                    Modo = "BETA",
                    RutaArchivos = root
                }),
                accessor);

            var rucA = "20100066603";
            var rucB = "20600000001";

            sut.GuardarXml("doc.xml", "<a/>", rucA);
            sut.GuardarXml("doc.xml", "<b/>", rucB);

            var rutaA = sut.ObtenerRutaXml("doc.xml", rucA)!;
            var rutaB = sut.ObtenerRutaXml("doc.xml", rucB)!;

            Assert.Contains(Path.Combine("BETA", rucA, "XML"), rutaA);
            Assert.Contains(Path.Combine("BETA", rucB, "XML"), rutaB);
            Assert.Equal("<a/>", File.ReadAllText(rutaA));
            Assert.Equal("<b/>", File.ReadAllText(rutaB));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ObtenerCarpeta_UsaAmbienteDelContexto()
    {
        var root = Path.Combine(Path.GetTempPath(), "cpe-mt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var accessor = new EmisorContextAccessor
            {
                Current = new EmisorRuntimeContext
                {
                    Ruc = "20100066603",
                    Ambiente = "PRODUCCION"
                }
            };

            var sut = new CpeStorageService(
                Options.Create(new CpeSettings
                {
                    Modo = "BETA",
                    RutaArchivos = root
                }),
                accessor);

            var carpeta = sut.ObtenerCarpetaXml();
            Assert.Contains(Path.Combine("PRODUCCION", "20100066603", "XML"), carpeta);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
