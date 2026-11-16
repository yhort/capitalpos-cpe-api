using System.Text;
using CapitalPos.Cpe.Api.Data;
using CapitalPos.Cpe.Api.Data.Entities;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Infrastructure.Security;
using CapitalPos.Cpe.Api.Infrastructure.Tenancy;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CapitalPos.Cpe.Tests;

public class EmisorRuntimeContextResolverTests
{
    [Fact]
    public async Task ResolveAsync_EmisorNoRegistrado_SinLegacy_Falla()
    {
        await using var db = CrearDb();
        var sut = CrearSut(db, permitirLegacy: false);

        var result = await sut.ResolveAsync("20100066603");

        Assert.False(result.Ok);
        Assert.Equal(EmisorRuntimeContextResolver.ErrorEmisorNoRegistrado, result.CodigoError);
    }

    [Fact]
    public async Task ResolveAsync_DosEmisores_UsaCertificadoYSolDelRucCorrecto()
    {
        await using var db = CrearDb();
        var protector = CrearProtector();

        var rucA = "20100066603";
        var rucB = "20600000001";

        var emisorA = await CrearEmisorCompletoAsync(db, protector, rucA, "USUARIOA", "claveA");
        var emisorB = await CrearEmisorCompletoAsync(db, protector, rucB, "USUARIOB", "claveB");

        var sut = new EmisorRuntimeContextResolver(
            db,
            protector,
            Options.Create(new CpeSettings
            {
                PermitirCredencialesLegacy = false,
                UrlSunatBeta = "https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService",
                UrlSunatProduccion = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService"
            }),
            NullLogger<EmisorRuntimeContextResolver>.Instance);

        var ctxA = await sut.ResolveAsync(rucA);
        var ctxB = await sut.ResolveAsync(rucB);

        Assert.True(ctxA.Ok);
        Assert.True(ctxB.Ok);
        Assert.Equal(emisorA.Id, ctxA.Context!.EmisorId);
        Assert.Equal(emisorB.Id, ctxB.Context!.EmisorId);
        Assert.Equal($"{rucA}USUARIOA", ctxA.Context.UsuarioSol);
        Assert.Equal($"{rucB}USUARIOB", ctxB.Context.UsuarioSol);
        Assert.Equal("claveA", ctxA.Context.ClaveSol);
        Assert.Equal("claveB", ctxB.Context.ClaveSol);
        Assert.True(ctxA.Context.CertificadoPfx.Length > 0);
        Assert.True(ctxB.Context.CertificadoPfx.Length > 0);
    }

    [Fact]
    public async Task ResolveAsync_SinCertificado_Falla()
    {
        await using var db = CrearDb();
        db.Emisores.Add(new Emisor
        {
            Id = Guid.NewGuid(),
            Ruc = "20100066603",
            RazonSocial = "DEMO",
            AmbienteDefault = CpeAmbientes.Beta,
            ProveedorEnvio = CpeProveedoresEnvio.SunatDirecta,
            Activo = true,
            CreadoEn = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var sut = CrearSut(db, permitirLegacy: false);
        var result = await sut.ResolveAsync("20100066603");

        Assert.False(result.Ok);
        Assert.Equal(EmisorRuntimeContextResolver.ErrorCertificado, result.CodigoError);
    }

    private static CpeDbContext CrearDb()
    {
        var options = new DbContextOptionsBuilder<CpeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CpeDbContext(options);
    }

    private static ISecretProtector CrearProtector()
    {
        var provider = DataProtectionProvider.Create(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        return new DataProtectionSecretProtector(provider);
    }

    private static EmisorRuntimeContextResolver CrearSut(CpeDbContext db, bool permitirLegacy)
    {
        return new EmisorRuntimeContextResolver(
            db,
            CrearProtector(),
            Options.Create(new CpeSettings { PermitirCredencialesLegacy = permitirLegacy }),
            NullLogger<EmisorRuntimeContextResolver>.Instance);
    }

    private static async Task<Emisor> CrearEmisorCompletoAsync(
        CpeDbContext db,
        ISecretProtector protector,
        string ruc,
        string sufijoUsuario,
        string claveSol)
    {
        var emisor = new Emisor
        {
            Id = Guid.NewGuid(),
            Ruc = ruc,
            RazonSocial = $"Empresa {ruc}",
            AmbienteDefault = CpeAmbientes.Beta,
            ProveedorEnvio = CpeProveedoresEnvio.SunatDirecta,
            Activo = true,
            CreadoEn = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow
        };
        db.Emisores.Add(emisor);

        // Bytes opacos: el resolver solo descifra; la validación X509 ocurre al firmar.
        var pfx = Encoding.UTF8.GetBytes($"fake-pfx-{ruc}");
        const string password = "TestPassword123!";
        db.EmisorCertificados.Add(new EmisorCertificado
        {
            Id = Guid.NewGuid(),
            EmisorId = emisor.Id,
            PfxCiphertext = protector.ProtectBytes(pfx, SecretProtectorPurposes.CertificadoPfx),
            PasswordCiphertext = protector.ProtectString(password, SecretProtectorPurposes.CertificadoPassword),
            HuellaThumbprint = "TEST",
            VigenteDesde = DateTime.UtcNow.AddYears(-1),
            VigenteHasta = DateTime.UtcNow.AddYears(1),
            Activo = true,
            CreadoEn = DateTime.UtcNow
        });

        db.EmisorCredencialesSol.Add(new EmisorCredencialSol
        {
            Id = Guid.NewGuid(),
            EmisorId = emisor.Id,
            Ambiente = CpeAmbientes.Beta,
            UsuarioSolCiphertext = protector.ProtectString($"{ruc}{sufijoUsuario}", SecretProtectorPurposes.UsuarioSol),
            ClaveSolCiphertext = protector.ProtectString(claveSol, SecretProtectorPurposes.ClaveSol),
            Activo = true,
            CreadoEn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return emisor;
    }

}
