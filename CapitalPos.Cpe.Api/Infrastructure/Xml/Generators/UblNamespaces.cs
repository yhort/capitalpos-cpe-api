using System.Xml.Linq;

namespace CapitalPos.Cpe.Api.Infrastructure.Xml.Generators;

public static class UblNamespaces
{
    public static readonly XNamespace Invoice =
        "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

    public static readonly XNamespace Cac =
        "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

    public static readonly XNamespace Cbc =
        "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

    public static readonly XNamespace Ext =
        "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

    public static readonly XNamespace Sac =
        "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";

    public static readonly XNamespace Ds =
        "http://www.w3.org/2000/09/xmldsig#";
}