using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeXmlResumenService
{
    CpeXmlResumenResponse ObtenerResumen(string xml);
}