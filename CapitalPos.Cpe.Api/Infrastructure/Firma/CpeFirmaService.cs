using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using CapitalPos.Cpe.Api.Domain;
using CapitalPos.Cpe.Api.Dtos;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.Extensions.Options;

namespace CapitalPos.Cpe.Api.Infrastructure.Firma;

public class CpeFirmaService : ICpeFirmaService
{
    private readonly CpeSettings _settings;
    private readonly ICpeStorageService _storageService;

    public CpeFirmaService(
        IOptions<CpeSettings> options,
        ICpeStorageService storageService)
    {
        _settings = options.Value;
        _storageService = storageService;
    }

    public CpeFirmaResponse FirmarXml(string nombreXml)
    {
        if (string.IsNullOrWhiteSpace(nombreXml))
        {
            return new CpeFirmaResponse
            {
                Ok = false,
                Mensaje = "El nombre del XML es obligatorio.",
                NombreXml = nombreXml,
                XmlFirmado = false,
                Errores = new List<string>
                {
                    "Debe indicar el nombre del archivo XML a firmar."
                }
            };
        }

        if (_settings.SimularFirma)
        {
            return new CpeFirmaResponse
            {
                Ok = true,
                Mensaje = "XML firmado correctamente en modo simulación.",
                NombreXml = nombreXml,
                XmlFirmado = true,
                Errores = new List<string>()
            };
        }

        var erroresConfiguracion = ValidarConfiguracionFirma();

        if (erroresConfiguracion.Count > 0)
        {
            return new CpeFirmaResponse
            {
                Ok = false,
                Mensaje = "No se puede firmar el XML porque la configuración de firma no es válida.",
                NombreXml = nombreXml,
                XmlFirmado = false,
                Errores = erroresConfiguracion
            };
        }

        var rutaXml = _storageService.ObtenerRutaXml(nombreXml);

        if (string.IsNullOrWhiteSpace(rutaXml) || !File.Exists(rutaXml))
        {
            return new CpeFirmaResponse
            {
                Ok = false,
                Mensaje = "No se encontró el XML para firmar.",
                NombreXml = nombreXml,
                XmlFirmado = false,
                Errores = new List<string>
                {
                    $"No existe el archivo XML: {nombreXml}"
                }
            };
        }

        try
        {
            FirmarArchivoXml(rutaXml);

            return new CpeFirmaResponse
            {
                Ok = true,
                Mensaje = "XML firmado correctamente.",
                NombreXml = nombreXml,
                XmlFirmado = true,
                Errores = new List<string>()
            };
        }
        catch (Exception ex)
        {
            return new CpeFirmaResponse
            {
                Ok = false,
                Mensaje = "No se pudo firmar el XML.",
                NombreXml = nombreXml,
                XmlFirmado = false,
                Errores = new List<string>
                {
                    ex.Message
                }
            };
        }
    }

    private List<string> ValidarConfiguracionFirma()
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(_settings.RutaCertificado))
        {
            errores.Add("La ruta del certificado digital es obligatoria.");
        }
        else if (!File.Exists(_settings.RutaCertificado))
        {
            errores.Add($"No existe el certificado digital en la ruta: {_settings.RutaCertificado}");
        }

        if (string.IsNullOrWhiteSpace(_settings.PasswordCertificado))
        {
            errores.Add("La contraseña del certificado digital es obligatoria.");
        }

        return errores;
    }

    private void FirmarArchivoXml(string rutaXml)
    {
        var certificado = new X509Certificate2(
            _settings.RutaCertificado,
            _settings.PasswordCertificado,
            X509KeyStorageFlags.Exportable
        );

        if (!certificado.HasPrivateKey)
        {
            throw new InvalidOperationException(
                "El certificado digital no contiene clave privada."
            );
        }

        var privateKey = certificado.GetRSAPrivateKey();

        if (privateKey == null)
        {
            throw new InvalidOperationException(
                "No se pudo obtener la clave privada RSA del certificado."
            );
        }

        var xmlDoc = new XmlDocument
        {
            PreserveWhitespace = true
        };

        xmlDoc.Load(rutaXml);

        var extensionContent = ObtenerExtensionContent(xmlDoc);

        extensionContent.RemoveAll();

        var signedXml = new SignedXml(xmlDoc)
        {
            SigningKey = privateKey
        };

        signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
        signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

        var reference = new Reference
        {
            Uri = string.Empty,
            DigestMethod = SignedXml.XmlDsigSHA256Url
        };

        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigExcC14NTransform());

        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(certificado));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();

        var firmaXml = signedXml.GetXml();
        firmaXml.SetAttribute("Id", "SignatureSP");

        var nodoImportado = xmlDoc.ImportNode(firmaXml, true);
        extensionContent.AppendChild(nodoImportado);

        xmlDoc.Save(rutaXml);
    }

    private static XmlNode ObtenerExtensionContent(XmlDocument xmlDoc)
    {
        var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);

        namespaceManager.AddNamespace(
            "ext",
            "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"
        );

        var extensionContent = xmlDoc.SelectSingleNode(
            "//ext:UBLExtensions/ext:UBLExtension/ext:ExtensionContent",
            namespaceManager
        );

        if (extensionContent == null)
        {
            throw new InvalidOperationException(
                "No se encontró el nodo ext:ExtensionContent para insertar la firma digital."
            );
        }

        return extensionContent;
    }
}