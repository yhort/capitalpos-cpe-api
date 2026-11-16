using CapitalPos.Cpe.Api.Data;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Domain.Tenancy;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Helpers;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Infrastructure.Tenancy;

public class EmisorRuntimeContextResolver : IEmisorRuntimeContextResolver
{
    public const string ErrorEmisorNoRegistrado = "ERROR_EMISOR_NO_REGISTRADO";
    public const string ErrorEmisorInactivo = "ERROR_EMISOR_INACTIVO";
    public const string ErrorCertificado = "ERROR_CERTIFICADO_EMISOR";
    public const string ErrorCredencialSol = "ERROR_CREDENCIAL_SOL";
    public const string ErrorRucInvalido = "ERROR_RUC_INVALIDO";

    private readonly CpeDbContext _db;
    private readonly ISecretProtector _protector;
    private readonly CpeSettings _settings;
    private readonly ILogger<EmisorRuntimeContextResolver> _logger;

    public EmisorRuntimeContextResolver(
        CpeDbContext db,
        ISecretProtector protector,
        IOptions<CpeSettings> options,
        ILogger<EmisorRuntimeContextResolver> logger)
    {
        _db = db;
        _protector = protector;
        _settings = options.Value;
        _logger = logger;
    }

    public Task<EmisorRuntimeResolveResult> ResolveFromRequestAsync(
        EmitirCpeRequest request,
        CancellationToken cancellationToken = default)
    {
        var ruc = CpeNombreHelper.ObtenerRucEmisor(request);
        return ResolveAsync(ruc, ambienteOverride: null, cancellationToken);
    }

    public async Task<EmisorRuntimeResolveResult> ResolveAsync(
        string ruc,
        string? ambienteOverride = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Trim().Length != 11 || !ruc.Trim().All(char.IsDigit))
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorRucInvalido,
                "El RUC del emisor es inválido.",
                "El RUC debe tener 11 dígitos numéricos.");
        }

        ruc = ruc.Trim();

        var emisor = await _db.Emisores
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Ruc == ruc, cancellationToken);

        if (emisor == null)
        {
            if (_settings.PermitirCredencialesLegacy)
            {
                var legacy = IntentarContextoLegacy(ruc);
                if (legacy.Ok)
                {
                    _logger.LogWarning(
                        "Usando credenciales legacy de CpeSettings para RUC {Ruc}. Migre el emisor a BD.",
                        ruc);
                    return legacy;
                }
            }

            return EmisorRuntimeResolveResult.Fail(
                ErrorEmisorNoRegistrado,
                "El emisor no está registrado en cpe-api.",
                $"No existe un emisor activo para el RUC {ruc}. Regístrelo antes de emitir.");
        }

        if (!emisor.Activo)
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorEmisorInactivo,
                "El emisor está inactivo.",
                $"El emisor con RUC {ruc} está inactivo.");
        }

        var ambiente = CpeAmbientes.Normalizar(
            string.IsNullOrWhiteSpace(ambienteOverride)
                ? emisor.AmbienteDefault
                : ambienteOverride);

        if (!CpeAmbientes.EsValido(ambiente))
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorEmisorNoRegistrado,
                "El ambiente del emisor es inválido.",
                "El ambiente debe ser BETA o PRODUCCION.");
        }

        var certificado = await _db.EmisorCertificados
            .AsNoTracking()
            .Where(c => c.EmisorId == emisor.Id && c.Activo)
            .OrderByDescending(c => c.CreadoEn)
            .FirstOrDefaultAsync(cancellationToken);

        if (certificado == null)
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorCertificado,
                "El emisor no tiene certificado digital activo.",
                $"Registre un certificado PFX activo para el RUC {ruc}.");
        }

        if (certificado.VigenteHasta.HasValue && certificado.VigenteHasta.Value.Date < DateTime.UtcNow.Date)
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorCertificado,
                "El certificado digital del emisor está vencido.",
                $"El certificado del RUC {ruc} venció el {certificado.VigenteHasta:yyyy-MM-dd}.");
        }

        var credencial = await _db.EmisorCredencialesSol
            .AsNoTracking()
            .Where(c => c.EmisorId == emisor.Id && c.Activo && c.Ambiente == ambiente)
            .OrderByDescending(c => c.CreadoEn)
            .FirstOrDefaultAsync(cancellationToken);

        if (credencial == null)
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorCredencialSol,
                "El emisor no tiene credenciales SOL activas para el ambiente.",
                $"Registre Usuario SOL / Clave SOL para el RUC {ruc} en ambiente {ambiente}.");
        }

        try
        {
            var pfx = _protector.UnprotectBytes(certificado.PfxCiphertext, SecretProtectorPurposes.CertificadoPfx);
            var password = _protector.UnprotectString(
                certificado.PasswordCiphertext,
                SecretProtectorPurposes.CertificadoPassword);
            var usuarioSol = _protector.UnprotectString(
                credencial.UsuarioSolCiphertext,
                SecretProtectorPurposes.UsuarioSol);
            var claveSol = _protector.UnprotectString(
                credencial.ClaveSolCiphertext,
                SecretProtectorPurposes.ClaveSol);

            if (!usuarioSol.Trim().StartsWith(ruc, StringComparison.Ordinal))
            {
                return EmisorRuntimeResolveResult.Fail(
                    ErrorCredencialSol,
                    "Las credenciales SOL no corresponden al RUC del emisor.",
                    $"El Usuario SOL debe iniciar con el RUC {ruc}.");
            }

            var urlSunat = ObtenerUrlSunat(emisor, ambiente);

            return EmisorRuntimeResolveResult.Success(new EmisorRuntimeContext
            {
                EmisorId = emisor.Id,
                Ruc = ruc,
                Ambiente = ambiente,
                ProveedorEnvio = emisor.ProveedorEnvio,
                CertificadoPfx = pfx,
                PasswordCertificado = password,
                UsuarioSol = usuarioSol,
                ClaveSol = claveSol,
                UrlSunat = urlSunat,
                EsLegacy = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudieron descifrar secretos del emisor {Ruc}", ruc);
            return EmisorRuntimeResolveResult.Fail(
                ErrorCertificado,
                "No se pudieron descifrar las credenciales del emisor.",
                "Verifique el key ring de Data Protection y vuelva a cargar certificado/SOL.");
        }
    }

    private EmisorRuntimeResolveResult IntentarContextoLegacy(string ruc)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(_settings.RutaCertificado) || !File.Exists(_settings.RutaCertificado))
            errores.Add("Legacy: no hay certificado configurado en CpeSettings.");

        if (string.IsNullOrWhiteSpace(_settings.PasswordCertificado))
            errores.Add("Legacy: falta PasswordCertificado en CpeSettings.");

        if (string.IsNullOrWhiteSpace(_settings.UsuarioSol))
            errores.Add("Legacy: falta UsuarioSol en CpeSettings.");
        else if (!_settings.UsuarioSol.Trim().StartsWith(ruc, StringComparison.Ordinal))
            errores.Add($"Legacy: UsuarioSol no corresponde al RUC {ruc}.");

        if (string.IsNullOrWhiteSpace(_settings.ClaveSol))
            errores.Add("Legacy: falta ClaveSol en CpeSettings.");

        var url = _settings.ObtenerUrlSunat();
        if (string.IsNullOrWhiteSpace(url))
            errores.Add("Legacy: falta URL SUNAT en CpeSettings.");

        if (errores.Count > 0)
        {
            return EmisorRuntimeResolveResult.Fail(
                ErrorEmisorNoRegistrado,
                "El emisor no está registrado y las credenciales legacy no aplican.",
                errores.ToArray());
        }

        var pfx = File.ReadAllBytes(_settings.RutaCertificado);
        var ambiente = CpeAmbientes.Normalizar(
            string.IsNullOrWhiteSpace(_settings.Modo) ? CpeAmbientes.Beta : _settings.Modo);

        return EmisorRuntimeResolveResult.Success(new EmisorRuntimeContext
        {
            EmisorId = Guid.Empty,
            Ruc = ruc,
            Ambiente = ambiente,
            ProveedorEnvio = CpeProveedoresEnvio.SunatDirecta,
            CertificadoPfx = pfx,
            PasswordCertificado = _settings.PasswordCertificado,
            UsuarioSol = _settings.UsuarioSol,
            ClaveSol = _settings.ClaveSol,
            UrlSunat = url,
            EsLegacy = true
        });
    }

    private string ObtenerUrlSunat(Data.Entities.Emisor emisor, string ambiente)
    {
        if (ambiente == CpeAmbientes.Produccion)
        {
            return !string.IsNullOrWhiteSpace(emisor.UrlOverrideProduccion)
                ? emisor.UrlOverrideProduccion!
                : _settings.UrlSunatProduccion;
        }

        return !string.IsNullOrWhiteSpace(emisor.UrlOverrideBeta)
            ? emisor.UrlOverrideBeta!
            : _settings.UrlSunatBeta;
    }
}
