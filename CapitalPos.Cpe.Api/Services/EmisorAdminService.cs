using System.Security.Cryptography.X509Certificates;
using CapitalPos.Cpe.Api.Data;
using CapitalPos.Cpe.Api.Data.Entities;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapitalPos.Cpe.Api.Services;

public class EmisorAdminService : IEmisorAdminService
{
    private readonly CpeDbContext _db;
    private readonly ISecretProtector _protector;

    public EmisorAdminService(CpeDbContext db, ISecretProtector protector)
    {
        _db = db;
        _protector = protector;
    }

    public async Task<EmisorDtoResponse> RegistrarEmisorAsync(
        RegistrarEmisorRequest request,
        CancellationToken ct = default)
    {
        ValidarRuc(request.Ruc);
        var ruc = request.Ruc.Trim();
        var ambiente = NormalizarAmbiente(request.AmbienteDefault);

        if (await _db.Emisores.AnyAsync(e => e.Ruc == ruc, ct))
            throw new InvalidOperationException($"Ya existe un emisor con RUC {ruc}.");

        var ahora = DateTime.UtcNow;
        var emisor = new Emisor
        {
            Id = Guid.NewGuid(),
            Ruc = ruc,
            RazonSocial = request.RazonSocial.Trim(),
            NombreComercial = request.NombreComercial?.Trim(),
            Ubigeo = request.Ubigeo?.Trim(),
            Direccion = request.Direccion?.Trim(),
            Departamento = request.Departamento?.Trim(),
            Provincia = request.Provincia?.Trim(),
            Distrito = request.Distrito?.Trim(),
            AmbienteDefault = ambiente,
            ProveedorEnvio = CpeProveedoresEnvio.SunatDirecta,
            UrlOverrideBeta = request.UrlOverrideBeta?.Trim(),
            UrlOverrideProduccion = request.UrlOverrideProduccion?.Trim(),
            Activo = request.Activo,
            CreadoEn = ahora,
            ActualizadoEn = ahora
        };

        _db.Emisores.Add(emisor);
        await _db.SaveChangesAsync(ct);
        return MapEmisor(emisor);
    }

    public async Task<EmisorDtoResponse?> ActualizarEmisorAsync(
        string ruc,
        ActualizarEmisorRequest request,
        CancellationToken ct = default)
    {
        ValidarRuc(ruc);
        var emisor = await _db.Emisores.FirstOrDefaultAsync(e => e.Ruc == ruc.Trim(), ct);
        if (emisor == null)
            return null;

        emisor.RazonSocial = request.RazonSocial.Trim();
        emisor.NombreComercial = request.NombreComercial?.Trim();
        emisor.Ubigeo = request.Ubigeo?.Trim();
        emisor.Direccion = request.Direccion?.Trim();
        emisor.Departamento = request.Departamento?.Trim();
        emisor.Provincia = request.Provincia?.Trim();
        emisor.Distrito = request.Distrito?.Trim();
        emisor.AmbienteDefault = NormalizarAmbiente(request.AmbienteDefault);
        emisor.UrlOverrideBeta = request.UrlOverrideBeta?.Trim();
        emisor.UrlOverrideProduccion = request.UrlOverrideProduccion?.Trim();
        emisor.Activo = request.Activo;
        emisor.ActualizadoEn = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapEmisor(emisor);
    }

    public async Task<EmisorDtoResponse?> ObtenerEmisorAsync(string ruc, CancellationToken ct = default)
    {
        ValidarRuc(ruc);
        var emisor = await _db.Emisores.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Ruc == ruc.Trim(), ct);
        return emisor == null ? null : MapEmisor(emisor);
    }

    public async Task<List<EmisorDtoResponse>> ListarEmisoresAsync(CancellationToken ct = default)
    {
        var emisores = await _db.Emisores.AsNoTracking()
            .OrderBy(e => e.Ruc)
            .ToListAsync(ct);
        return emisores.Select(MapEmisor).ToList();
    }

    public async Task<EmisorOnboardingStatusDto?> ObtenerOnboardingAsync(
        string ruc,
        CancellationToken ct = default)
    {
        ValidarRuc(ruc);
        ruc = ruc.Trim();

        var emisor = await _db.Emisores.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Ruc == ruc, ct);
        if (emisor == null)
            return null;

        var cert = await _db.EmisorCertificados.AsNoTracking()
            .Where(c => c.EmisorId == emisor.Id && c.Activo)
            .OrderByDescending(c => c.CreadoEn)
            .FirstOrDefaultAsync(ct);

        var sols = await _db.EmisorCredencialesSol.AsNoTracking()
            .Where(c => c.EmisorId == emisor.Id && c.Activo)
            .ToListAsync(ct);

        var tieneBeta = sols.Any(s => s.Ambiente == CpeAmbientes.Beta);
        var tieneProd = sols.Any(s => s.Ambiente == CpeAmbientes.Produccion);
        var certVencido = cert?.VigenteHasta.HasValue == true
                          && cert.VigenteHasta!.Value.Date < DateTime.UtcNow.Date;

        var pendientes = new List<string>();
        if (!emisor.Activo)
            pendientes.Add("El emisor está inactivo.");
        if (cert == null)
            pendientes.Add("Falta certificado digital activo.");
        else if (certVencido)
            pendientes.Add("El certificado digital está vencido.");

        var ambiente = CpeAmbientes.Normalizar(emisor.AmbienteDefault);
        if (ambiente == CpeAmbientes.Beta && !tieneBeta)
            pendientes.Add("Faltan credenciales SOL para BETA.");
        if (ambiente == CpeAmbientes.Produccion && !tieneProd)
            pendientes.Add("Faltan credenciales SOL para PRODUCCION.");

        var listo = pendientes.Count == 0;

        return new EmisorOnboardingStatusDto
        {
            EmisorId = emisor.Id,
            Ruc = emisor.Ruc,
            RazonSocial = emisor.RazonSocial,
            Activo = emisor.Activo,
            AmbienteDefault = emisor.AmbienteDefault,
            TieneCertificadoActivo = cert != null && !certVencido,
            CertificadoThumbprint = cert?.HuellaThumbprint,
            CertificadoVigenteHasta = cert?.VigenteHasta,
            CertificadoVencido = certVencido,
            TieneCredencialSolBeta = tieneBeta,
            TieneCredencialSolProduccion = tieneProd,
            ListoParaEmitir = listo,
            Pendientes = pendientes
        };
    }

    public async Task<EmisorCertificadoInfoDto> RegistrarCertificadoAsync(
        string ruc,
        byte[] pfxBytes,
        string password,
        CancellationToken ct = default)
    {
        ValidarRuc(ruc);
        if (pfxBytes.Length == 0)
            throw new InvalidOperationException("El archivo PFX está vacío.");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("La contraseña del certificado es obligatoria.");

        var emisor = await _db.Emisores.FirstOrDefaultAsync(e => e.Ruc == ruc.Trim(), ct)
                     ?? throw new InvalidOperationException($"No existe emisor con RUC {ruc}.");

        using var certificado = new X509Certificate2(
            pfxBytes,
            password,
            X509KeyStorageFlags.Exportable);

        if (!certificado.HasPrivateKey)
            throw new InvalidOperationException("El certificado PFX no contiene clave privada.");

        var activos = await _db.EmisorCertificados
            .Where(c => c.EmisorId == emisor.Id && c.Activo)
            .ToListAsync(ct);
        foreach (var previo in activos)
            previo.Activo = false;

        var entity = new EmisorCertificado
        {
            Id = Guid.NewGuid(),
            EmisorId = emisor.Id,
            PfxCiphertext = _protector.ProtectBytes(pfxBytes, SecretProtectorPurposes.CertificadoPfx),
            PasswordCiphertext = _protector.ProtectString(password, SecretProtectorPurposes.CertificadoPassword),
            HuellaThumbprint = certificado.Thumbprint,
            VigenteDesde = certificado.NotBefore.ToUniversalTime(),
            VigenteHasta = certificado.NotAfter.ToUniversalTime(),
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _db.EmisorCertificados.Add(entity);
        emisor.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return MapCert(entity);
    }

    public async Task<EmisorCertificadoInfoDto?> ObtenerCertificadoActivoAsync(
        string ruc,
        CancellationToken ct = default)
    {
        ValidarRuc(ruc);
        var emisor = await _db.Emisores.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Ruc == ruc.Trim(), ct);
        if (emisor == null)
            return null;

        var cert = await _db.EmisorCertificados.AsNoTracking()
            .Where(c => c.EmisorId == emisor.Id && c.Activo)
            .OrderByDescending(c => c.CreadoEn)
            .FirstOrDefaultAsync(ct);

        return cert == null ? null : MapCert(cert);
    }

    public async Task RegistrarCredencialSolAsync(
        string ruc,
        RegistrarCredencialSolRequest request,
        CancellationToken ct = default)
    {
        ValidarRuc(ruc);
        ruc = ruc.Trim();
        var ambiente = NormalizarAmbiente(request.Ambiente);

        if (string.IsNullOrWhiteSpace(request.UsuarioSol) || string.IsNullOrWhiteSpace(request.ClaveSol))
            throw new InvalidOperationException("Usuario SOL y Clave SOL son obligatorios.");

        if (!request.UsuarioSol.Trim().StartsWith(ruc, StringComparison.Ordinal))
            throw new InvalidOperationException($"El Usuario SOL debe iniciar con el RUC {ruc}.");

        var emisor = await _db.Emisores.FirstOrDefaultAsync(e => e.Ruc == ruc, ct)
                     ?? throw new InvalidOperationException($"No existe emisor con RUC {ruc}.");

        var previos = await _db.EmisorCredencialesSol
            .Where(c => c.EmisorId == emisor.Id && c.Ambiente == ambiente && c.Activo)
            .ToListAsync(ct);
        foreach (var previo in previos)
            previo.Activo = false;

        _db.EmisorCredencialesSol.Add(new EmisorCredencialSol
        {
            Id = Guid.NewGuid(),
            EmisorId = emisor.Id,
            Ambiente = ambiente,
            UsuarioSolCiphertext = _protector.ProtectString(
                request.UsuarioSol.Trim(),
                SecretProtectorPurposes.UsuarioSol),
            ClaveSolCiphertext = _protector.ProtectString(
                request.ClaveSol,
                SecretProtectorPurposes.ClaveSol),
            Activo = true,
            CreadoEn = DateTime.UtcNow
        });

        emisor.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidarRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Trim().Length != 11 || !ruc.Trim().All(char.IsDigit))
            throw new InvalidOperationException("El RUC debe tener 11 dígitos numéricos.");
    }

    private static string NormalizarAmbiente(string ambiente)
    {
        if (!CpeAmbientes.EsValido(ambiente))
            throw new InvalidOperationException("El ambiente debe ser BETA o PRODUCCION.");
        return CpeAmbientes.Normalizar(ambiente);
    }

    private static EmisorDtoResponse MapEmisor(Emisor e) => new()
    {
        Id = e.Id,
        Ruc = e.Ruc,
        RazonSocial = e.RazonSocial,
        NombreComercial = e.NombreComercial,
        Activo = e.Activo,
        AmbienteDefault = e.AmbienteDefault,
        ProveedorEnvio = e.ProveedorEnvio,
        CreadoEn = e.CreadoEn,
        ActualizadoEn = e.ActualizadoEn
    };

    private static EmisorCertificadoInfoDto MapCert(EmisorCertificado c) => new()
    {
        Id = c.Id,
        HuellaThumbprint = c.HuellaThumbprint,
        VigenteDesde = c.VigenteDesde,
        VigenteHasta = c.VigenteHasta,
        Activo = c.Activo,
        CreadoEn = c.CreadoEn
    };
}
