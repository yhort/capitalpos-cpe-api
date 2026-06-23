namespace CapitalPos.Cpe.Api.Dtos;

public class CpeHistorialResumenResponse
{
    public int TotalRegistros { get; set; }

    public int TotalOk { get; set; }
    public int TotalErrores { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime? PrimeraEmision { get; set; }
    public DateTime? UltimaEmision { get; set; }

    public Dictionary<string, int> PorEstado { get; set; } = new();
    public Dictionary<string, int> PorTipoComprobante { get; set; } = new();
    public Dictionary<string, decimal> MontoPorMoneda { get; set; } = new();
}