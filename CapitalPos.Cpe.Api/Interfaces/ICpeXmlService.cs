using CapitalPos.Cpe.Api.Domain.Cpe;
using CapitalPos.Cpe.Api.Dtos;
namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeXmlService
{
    string GenerarXml(CpeDocumento documento);
}