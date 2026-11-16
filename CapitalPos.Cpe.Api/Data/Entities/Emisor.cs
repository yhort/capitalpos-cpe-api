namespace CapitalPos.Cpe.Api.Data.Entities;

public class Emisor
{
    public Guid Id { get; set; }

    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }

    public string? Ubigeo { get; set; }
    public string? Direccion { get; set; }
    public string? Departamento { get; set; }
    public string? Provincia { get; set; }
    public string? Distrito { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>BETA o PRODUCCION.</summary>
    public string AmbienteDefault { get; set; } = "BETA";

    /// <summary>SunatDirecta por defecto; preparado para OSE.</summary>
    public string ProveedorEnvio { get; set; } = "SunatDirecta";

    public string? UrlOverrideBeta { get; set; }
    public string? UrlOverrideProduccion { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<EmisorCertificado> Certificados { get; set; } = new List<EmisorCertificado>();
    public ICollection<EmisorCredencialSol> CredencialesSol { get; set; } = new List<EmisorCredencialSol>();
    public ICollection<ComprobanteEmitido> Comprobantes { get; set; } = new List<ComprobanteEmitido>();
}
