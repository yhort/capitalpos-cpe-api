namespace CapitalPos.Cpe.Api.Dtos;

public class RegistrarEmisorRequest
{
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Ubigeo { get; set; }
    public string? Direccion { get; set; }
    public string? Departamento { get; set; }
    public string? Provincia { get; set; }
    public string? Distrito { get; set; }
    public string AmbienteDefault { get; set; } = "BETA";
    public string? UrlOverrideBeta { get; set; }
    public string? UrlOverrideProduccion { get; set; }
    public bool Activo { get; set; } = true;
}

public class ActualizarEmisorRequest
{
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Ubigeo { get; set; }
    public string? Direccion { get; set; }
    public string? Departamento { get; set; }
    public string? Provincia { get; set; }
    public string? Distrito { get; set; }
    public string AmbienteDefault { get; set; } = "BETA";
    public string? UrlOverrideBeta { get; set; }
    public string? UrlOverrideProduccion { get; set; }
    public bool Activo { get; set; } = true;
}

public class RegistrarCredencialSolRequest
{
    public string Ambiente { get; set; } = "BETA";
    public string UsuarioSol { get; set; } = string.Empty;
    public string ClaveSol { get; set; } = string.Empty;
}

public class EmisorDtoResponse
{
    public Guid Id { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public bool Activo { get; set; }
    public string AmbienteDefault { get; set; } = string.Empty;
    public string ProveedorEnvio { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
}

public class EmisorOnboardingStatusDto
{
    public Guid EmisorId { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string AmbienteDefault { get; set; } = string.Empty;
    public bool TieneCertificadoActivo { get; set; }
    public string? CertificadoThumbprint { get; set; }
    public DateTime? CertificadoVigenteHasta { get; set; }
    public bool CertificadoVencido { get; set; }
    public bool TieneCredencialSolBeta { get; set; }
    public bool TieneCredencialSolProduccion { get; set; }
    public bool ListoParaEmitir { get; set; }
    public List<string> Pendientes { get; set; } = new();
}

public class EmisorCertificadoInfoDto
{
    public Guid Id { get; set; }
    public string? HuellaThumbprint { get; set; }
    public DateTime? VigenteDesde { get; set; }
    public DateTime? VigenteHasta { get; set; }
    public bool Activo { get; set; }
    public DateTime CreadoEn { get; set; }
}
