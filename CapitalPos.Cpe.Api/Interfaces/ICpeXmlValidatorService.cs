using CapitalPos.Cpe.Api.Dtos;

namespace CapitalPos.Cpe.Api.Interfaces;

public interface ICpeXmlValidatorService
{
    CpeXmlValidacionResponse ValidarXml(string xml);
}