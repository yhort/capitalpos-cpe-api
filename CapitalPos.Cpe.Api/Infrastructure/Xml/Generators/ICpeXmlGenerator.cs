using CapitalPos.Cpe.Api.Domain.Cpe;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;

public interface ICpeXmlGenerator
{
    string Generar(CpeDocumento documento);
}