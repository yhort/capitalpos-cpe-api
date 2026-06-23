using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BE = businessEntities;
using EV = CPEEnvio;
using XM = CrearXML;
using SG = Signature;
using System.IO;

using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using Microsoft.ReportingServices.ReportProcessing.OnDemandReportObjectModel;
using System.Web.UI.WebControls;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.Dynamic;
using System.Xml;
using System.IO.Compression;
using Microsoft.SqlServer.Server;

namespace Microsell_Lite
{
    public class CPEConfig
    {

        XM.CrearXML objXML = new XM.CrearXML();
        SG.FirmadoRequest objPregunta = new SG.FirmadoRequest();
        SG.FirmadoResponse objRespuesta = new SG.FirmadoResponse();
        SG.Signature objSignature = new SG.Signature();
        EV.ServiceSunat objENV = new EV.ServiceSunat();
        //Public NoAbrirPdf As Boolean
        public string RutaCompletaxml = "";
        public Dictionary<string, string> Enviar_FacturaBoleta_aSunat(BE.CPE CPE)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string nomARCHIVO = "";
            string ruta = "";
            string rutaFirma = "";
            string url = "";

            //============================================

            nomARCHIVO = CPE.NRO_DOCUMENTO_EMPRESA + "-" + CPE.COD_TIPO_DOCUMENTO + "-" + CPE.NRO_COMPROBANTE;

            if ((CPE.TIPO_PROCESO == 3))
            { //'Pruebas
                ruta = @"D:\\CPE_2\\BETA\\";
                url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
            }
            else if ((CPE.TIPO_PROCESO == 1))
            { //Produccion
                ruta = @"D:\\CPE_2\\PRODUCCION\\";
                url = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService"; //'Produccion
            }

            //rutaFirma = "D:\\CPE\\FIRMA\\" & CPE.NRO_DOCUMENTO_EMPRESA & ".pfx"
            rutaFirma = @"D:\\CPE_2\\FIRMABETA\\FIRMABETA.pfx";

            //===================creamos xml(comprobante)===================
            if (CPE.COD_TIPO_DOCUMENTO == "01" | CPE.COD_TIPO_DOCUMENTO == "03")
            {
                dictionary = objXML.CPE(CPE, nomARCHIVO, ruta);
            }

            if (dictionary["flg_rta"] == "0")
            {
                return dictionary;
            }
            //=================datos para la firma====================
            objPregunta.ruta_Firma = rutaFirma;
            objPregunta.contra_Firma = CPE.CONTRA_FIRMA;
            objPregunta.ruta_xml = ruta + nomARCHIVO + ".XML";
            RutaCompletaxml = ruta + nomARCHIVO + ".XML";
            objPregunta.flg_firma = 0;
            objRespuesta = objSignature.FirmaXMl(objPregunta);
            //'====================creamos pdf====================
            //Dim RptPDF As New Frm_Print_Pdf_Factura_Vic
            //'Dim RptPDF As New Metodos
            //CPE.HASH_CPE = objRespuesta.DigestValue
            //CPE.RUTA_CODIGO_BARRA = "D:\\CPE\\CODIGOBARRA\\" & nomARCHIVO & ".BMP"
            //CPE.RUTA_PDF = "D:\\CPE\\BETA\\" & nomARCHIVO & ".PDF"
            //RptPDF.TraerReporteComprobante_dePrueba(CPE)  ''USAN EL MISMO metodo al de la Prueba ''Prueba en version de conocer el sistema
            //'Abrimos el Aviso de Enviando a la Sunat:

            //====================enviamos documento a la sunat=========================
            dictionary = objENV.Envio(CPE.NRO_DOCUMENTO_EMPRESA, CPE.USUARIO_SOL_EMPRESA, CPE.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url, objRespuesta.DigestValue);
            CPE.HASH_CPE = dictionary["hash_cpe"];
            return dictionary;

        }

        //public  Dictionary<string, string> Enviar_GuiaRemision_aSunat(BE.CPE_GUIA_REMISION CPE_GRX)
        //{
        //    Dictionary<string, string> dictionary = new Dictionary<string, string>();
        //    string nomARCHIVO = "";
        //    string ruta = "";
        //    string rutaFirma = "";
        //    string url = "";

        //    //============================================


        //    nomARCHIVO = CPE_GRX.NRO_DOCUMENTO_EMPRESA + "-" + CPE_GRX.COD_TIPO_DOCUMENTO + "-" + CPE_GRX.NRO_COMPROBANTE;

        //    //if ((CPE.TIPO_PROCESO == 3))
        //    //{ //'Pruebas
        //    //    ruta = @"D:\\CPE_2\\BETA\\";
        //    //    url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
        //    //}
        //    //else if ((CPE.TIPO_PROCESO == 1))
        //    //{ //Produccion
        //    //    ruta = @"D:\\CPE_2\\PRODUCCION\\";
        //    //     //'Produccion
        //    //}
        //    //url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/{nomARCHIVO}";

        //    //rutaFirma = "D:\\CPE\\FIRMA\\" & CPE.NRO_DOCUMENTO_EMPRESA & ".pfx"
        //    rutaFirma = @"D:\\CPE_2\\FIRMABETA\\FIRMABETA.pfx";

        //    //===================creamos xml(comprobante)===================
        //    if (CPE_GRX.COD_TIPO_DOCUMENTO == "09" | CPE_GRX.COD_TIPO_DOCUMENTO == "31")
        //    {
        //        dictionary = objXML.CPE_GUIA_REMISION(CPE_GRX, nomARCHIVO, ruta);
        //    }


        //    if (dictionary["flg_rta"] == "0")
        //    {
        //        return dictionary;
        //    }
        //    //=================datos para la firma====================
        //    objPregunta.ruta_Firma = rutaFirma;
        //    objPregunta.contra_Firma = CPE_GRX.CONTRA_FIRMA;
        //    objPregunta.ruta_xml = ruta + nomARCHIVO + ".XML";
        //    RutaCompletaxml = ruta + nomARCHIVO + ".XML";
        //    objPregunta.flg_firma = 0;
        //    objRespuesta = objSignature.FirmaXMl(objPregunta);
        //    //'====================creamos pdf====================
        //    //Dim RptPDF As New Frm_Print_Pdf_Factura_Vic
        //    //'Dim RptPDF As New Metodos
        //    //CPE.HASH_CPE = objRespuesta.DigestValue
        //    //CPE.RUTA_CODIGO_BARRA = "D:\\CPE\\CODIGOBARRA\\" & nomARCHIVO & ".BMP"
        //    //CPE.RUTA_PDF = "D:\\CPE\\BETA\\" & nomARCHIVO & ".PDF"
        //    //RptPDF.TraerReporteComprobante_dePrueba(CPE)  ''USAN EL MISMO metodo al de la Prueba ''Prueba en version de conocer el sistema
        //    //'Abrimos el Aviso de Enviando a la Sunat:



        //    //====================enviamos documento a la sunat=========================
        //    dictionary = objENV.Envio(CPE_GRX.NRO_DOCUMENTO_EMPRESA, CPE_GRX.USUARIO_SOL_EMPRESA, CPE_GRX.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url, objRespuesta.DigestValue);
        //    //dictionary = await EnvioXml(ruta, nomARCHIVO, CPE_GRX.TOKEN);
        //    CPE_GRX.HASH_CPE = dictionary["hash_cpe"];
        //    return dictionary;

        //}
        //1.-obtener el token:
        public async Task<Dictionary<string, string>> Enviar_GuiaRemision_aSunat(BE.CPE_GUIA_REMISION CPE_GRX)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string nomARCHIVO = "";
            string ruta = "";
            string rutaFirma = "";
            string url = "";
            //============================================
            nomARCHIVO = CPE_GRX.NRO_DOCUMENTO_EMPRESA + "-" + CPE_GRX.COD_TIPO_DOCUMENTO + "-" + CPE_GRX.NRO_COMPROBANTE;

            //if ((CPE.TIPO_PROCESO == 3))
            //{ //'Pruebas
            //    ruta = @"D:\\CPE_2\\BETA\\";
            //    url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
            //}
            //else if ((CPE.TIPO_PROCESO == 1))
            //{ //Produccion
            //    ruta = @"D:\\CPE_2\\PRODUCCION\\";
            //     //'Produccion
            //}
            //url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/{nomARCHIVO}";

            //rutaFirma = "D:\\CPE\\FIRMA\\" & CPE.NRO_DOCUMENTO_EMPRESA & ".pfx"
            ruta = @"D:\\CPE_2\\PRODUCCION\\";
            rutaFirma = @"D:\\CPE_2\\FIRMABETA\\FIRMABETA.pfx";

            //===================creamos xml(comprobante)===================
            if (CPE_GRX.COD_TIPO_DOCUMENTO == "09" )
            {
                dictionary = objXML.CPE_GUIA_REMISION(CPE_GRX, nomARCHIVO, ruta);
            }
            else if (CPE_GRX.COD_TIPO_DOCUMENTO == "31")
            {
                dictionary = objXML.CPE_TRANSPORTISTA(CPE_GRX, nomARCHIVO, ruta);
            }

            if (dictionary["flg_rta"] == "0")
            {
                return dictionary;
            }
            //=================datos para la firma====================
            objPregunta.ruta_Firma = rutaFirma;
            objPregunta.contra_Firma = CPE_GRX.CONTRA_FIRMA;
            objPregunta.ruta_xml = ruta + nomARCHIVO + ".XML";
            RutaCompletaxml = ruta + nomARCHIVO + ".XML";
            objPregunta.flg_firma = 0;
            objRespuesta = objSignature.FirmaXMl(objPregunta);

            ////se añade codigo 2901
            //try
            //{
            //    // Enviar el archivo XML comprimido a la SUNAT
            //    dictionary = await EnviarXmlAsync(ruta, nomARCHIVO, CPE_GRX.TOKEN);

            //    // Guardar el hash CPE para futuras validaciones
            //    CPE_GRX.HASH_CPE = dictionary["hash_cpe"];

            //    // Si es necesario, obtener el numTicket y fecRecepcion
            //    string numTicket = dictionary["numTicket"];
            //    string fecRecepcion = dictionary["fecRecepcion"];

            //    // Aquí puedes usar numTicket y fecRecepcion como se necesite
            //    Console.WriteLine($"Ticket: {numTicket}, Fecha Recepción: {fecRecepcion}");

            //    return dictionary; // Retornar la respuesta de SUNAT
            //}
            //catch (Exception ex)
            //{
            //    // Manejo de errores durante el envío
            //    Console.WriteLine("Error al enviar la guía a SUNAT: " + ex.Message);
            //    return new Dictionary<string, string> { { "error", ex.Message } };
            //}

            //'====================creamos pdf====================
            //Dim RptPDF As New Frm_Print_Pdf_Factura_Vic
            //'Dim RptPDF As New Metodos
            //CPE.HASH_CPE = objRespuesta.DigestValue
            //CPE.RUTA_CODIGO_BARRA = "D:\\CPE\\CODIGOBARRA\\" & nomARCHIVO & ".BMP"
            //CPE.RUTA_PDF = "D:\\CPE\\BETA\\" & nomARCHIVO & ".PDF"
            //RptPDF.TraerReporteComprobante_dePrueba(CPE)  ''USAN EL MISMO metodo al de la Prueba ''Prueba en version de conocer el sistema
            //'Abrimos el Aviso de Enviando a la Sunat:

            //====================enviamos documento a la sunat=========================
            //dictionary = objENV.EnvioGuiaRemision(CPE_GRX.NRO_DOCUMENTO_EMPRESA, CPE_GRX.USUARIO_SOL_EMPRESA, CPE_GRX.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url,CPE_GRX.TOKEN, objRespuesta.DigestValue);

            //dictionary =  await EnviarXML(ruta, nomARCHIVO, CPE_GRX.TOKEN);
            //CPE_GRX.HASH_CPE = dictionary["hash_cpe"];

            // Enviar el XML a la SUNAT
            // Llamamos a EnviarXML y esperamos la respuesta
            //dictionary =  EnviarXML(ruta, nomARCHIVO, CPE_GRX.TOKEN);

            //CPE_GRX.HASH_CPE = dictionary["hash_cpe"]; 

            //dictionary = await Task.Run(() =>  EnvioXml(ruta, nomARCHIVO, CPE_GRX.TOKEN));
            dictionary = await EnviarXmlAsync(ruta, nomARCHIVO, CPE_GRX.TOKEN);
            //CPE_GRX.HASH_CPE = dictionary["hash_cpe"];
            // Asignar el hashCpe después de recibir respuesta de SUNAT
            CPE_GRX.HASH_CPE = dictionary.ContainsKey("hash_cpe") ? dictionary["hash_cpe"] : "";

            //*
            // 1) Obtener ticket del envío
            string ticket;
            if (!dictionary.TryGetValue("numTicket", out ticket) || string.IsNullOrWhiteSpace(ticket))
            {
                dictionary["error"] = "SUNAT no devolvió numTicket. No se puede obtener el CDR.";
                return dictionary;
            }

            // 2) Polling del ticket (hasta que deje de estar en proceso "98")
            for (int i = 0; i < 12; i++) // 12 intentos x 5s = ~1 minuto
            {
                Dictionary<string, string> rptaCdr = null;
                try
                {
                    rptaCdr = await EnvioTicketAsync(
                        ruta /* rutaArchivoCdr (misma carpeta donde están tus XML/ZIP) */,
                        ticket,
                        CPE_GRX.TOKEN,
                        CPE_GRX.NRO_DOCUMENTO_EMPRESA,
                        nomARCHIVO
                    );
                }
                catch (Exception ex)
                {
                    // Si falla la consulta del ticket, corta y devuelve detalle
                    dictionary["error"] = "Error consultando ticket: " + ex.Message;
                    return dictionary;
                }

                // Leer estados de respuesta con TryGetValue (para .NET 4.8)
                string codTicket;
                if (!rptaCdr.TryGetValue("ticket_rpta", out codTicket))
                    codTicket = null; // esperado: "98", "99" o "0"

                // "98" => en proceso, espera y vuelve a consultar
                if (codTicket == "98")
                {
                    await Task.Delay(5000); // 5s
                    continue;
                }

                // Estados terminales: "0" (listo) o "99" (error)
                // Mergea todo lo que vino del CDR a la respuesta final:
                foreach (var kv in rptaCdr)
                    dictionary[kv.Key] = kv.Value;

                // Aquí, si codTicket == "0", ya se generó y guardó R-{nomARCHIVO}.ZIP y R-{nomARCHIVO}.xml en 'ruta'
                // Si codTicket == "99", revisa dictionary["cdr_msj_sunat"] y dictionary["numerror"] para el detalle
                return dictionary;
            }

            // 3) Si salió del bucle sin obtener "0" o "99"
            dictionary["error"] = "Tiempo de espera agotado esperando el CDR (codRespuesta 98 persistente).";
            //FIN

            return dictionary;
        }
        public  async Task<string> GetToken(string clientId, string clientSecret, string usuarioSecundario, string usuarioPassword)
        {
            string url = $"https://api-seguridad.sunat.gob.pe/v1/clientessol/{clientId}/oauth2/token/";
            //prueba nubefact:
            //string url = $"https://gre-test.nubefact.com/v1/clientessol/{clientId}/oauth2/token/";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (HttpClient client = new HttpClient())
            {
                var data = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "scope", "https://api-cpe.sunat.gob.pe" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "username", usuarioSecundario },
                    { "password", usuarioPassword }
                };

                var content = new FormUrlEncodedContent(data);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                //HttpResponseMessage response = await client.PostAsync(url, content);
                //response.EnsureSuccessStatusCode();

                //string result = await response.Content.ReadAsStringAsync();
                //JObject jsonResponse = JObject.Parse(result);

                //return jsonResponse["access_token"].ToString();
                //codigo nuevo para manejar excepciones:
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    // Verificar si la respuesta no es exitosa
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        LogError($"Error en la solicitud: {response.StatusCode}, Contenido de la respuesta: {errorResponse}");
                        return null;  // Retorna null o un valor por defecto si la respuesta es un error
                    }

                    string result = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(result);

                    // Verificar que el "access_token" esté presente en la respuesta
                    if (jsonResponse["access_token"] != null)
                    {
                        return jsonResponse["access_token"].ToString();
                    }
                    else
                    {
                        LogError("Error: No se encontró el token en la respuesta de la API.");
                        return null;
                    }
                }
                catch (Exception ex)
                {

                    // Registrar el error completo, incluyendo la pila de la excepción y la respuesta HTTP
                    LogError($"Error al obtener el token: {ex.ToString()}");
                    return null;  // Retorna null o un valor por defecto en caso de error
                }


            }
        }

        // Método para registrar los errores en un archivo de log (opcional)
        private void LogError(string message)
        {
            // Aquí puedes implementar tu propia lógica para registrar los errores, como un archivo de texto o una base de datos.
            // A continuación, te muestro un ejemplo sencillo que escribe en un archivo de log:
            try
            {
                string logFilePath = "error_log.txt";
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch
            {
                // Si no se puede escribir en el log, puedes manejarlo como desees.
            }
        }

        //public static async Task<dynamic> EnvioXmlx(string path, string nombreFile, string tokenAccess)
        //{
        //    string url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/{nombreFile}";



        //    using (HttpClient client = new HttpClient())
        //    {
        //        var data = new Dictionary<string, string>
        //        {
        //            { "nomArchivo", nombreFile + ".zip" },
        //            { "arcGreZip", Convert.ToBase64String(File.ReadAllBytes(Path.Combine(path, nombreFile + ".zip"))) },
        //            { "hashZip", ComputeSha256Hash(Path.Combine(path, nombreFile + ".zip")) }
        //        };

        //        var jsonData = JsonConvert.SerializeObject(new { archivo = data });
        //        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAccess);

        //        HttpResponseMessage response = await client.PostAsync(url, content);
        //        response.EnsureSuccessStatusCode();

        //        string result = await response.Content.ReadAsStringAsync();
        //        return JsonConvert.DeserializeObject(result);
        //    }
        //}

        private static readonly HttpClient client = new HttpClient(); // Instancia de HttpClient (idealmente se debe usar como singleton).

        // Método para enviar XML a SUNAT de manera asincrónica
        public async Task<Dictionary<string, string>> EnviarXmlAsync(string ruta, string nomARCHIVO, string tokenAcceso)
        {
            string archivoXml = Path.Combine(ruta, nomARCHIVO + ".XML");

            if (!File.Exists(archivoXml))
            {
                throw new FileNotFoundException("El archivo XML no existe en la ruta especificada.", archivoXml);
            }

            Comprimir(nomARCHIVO, ruta);

            string archivoZip = Path.Combine(ruta, nomARCHIVO + ".ZIP");
            if (!File.Exists(archivoZip))
            {
                throw new FileNotFoundException("El archivo ZIP no se ha creado correctamente.", archivoZip);
            }

            byte[] archivoBytes = File.ReadAllBytes(archivoZip);
            string archivoBase64 = Convert.ToBase64String(archivoBytes);

            string hashZip = ObtenerHashSHA256(archivoZip); // ✅ Este es tu hashCpe

            var data = new
            {
                archivo = new
                {
                    nomArchivo = nomARCHIVO + ".zip",
                    arcGreZip = archivoBase64,
                    hashZip = hashZip
                }
            };

            string jsonData = JsonConvert.SerializeObject(data);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = "https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/" + nomARCHIVO;
            //string url = "https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/" + nomARCHIVO;

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Headers = { { "Authorization", "Bearer " + tokenAcceso } },
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(jsonResponse);

                    string numTicket = jsonObject["numTicket"]?.ToString() ?? "";
                    string fecRecepcion = jsonObject["fecRecepcion"]?.ToString() ?? "";

                    // ✅ Armamos la respuesta final con el hash incluido
                    var resultado = new Dictionary<string, string>
                    {
                        { "numTicket", numTicket },
                        { "fecRecepcion", fecRecepcion },
                        { "hash_cpe", hashZip } // << este es el que antes no se devolvía
                    };

                    return resultado;
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error en la respuesta de la SUNAT: " + errorResponse);
                    throw new Exception("Error al enviar el XML a SUNAT.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el XML a SUNAT: " + ex.Message);
                throw;
            }
        }

        //public static void Comprimir(string nomArchivo, string ruta)
        //{
        //    try
        //    {
        //        // Verificar si el archivo XML existe
        //        string archivoXml = Path.Combine(ruta, nomArchivo + ".XML");
        //        if (!File.Exists(archivoXml))
        //        {
        //            throw new FileNotFoundException("El archivo XML no se encuentra en la ruta especificada.");
        //        }

        //        //Eliminar el BOM(Byte Order Mark) si existe
        //        //byte[] bytesWithoutBOM;
        //        //using (StreamReader sr = new StreamReader(archivoXml, Encoding.UTF8))
        //        //{
        //        //    bytesWithoutBOM = new UTF8Encoding(false).GetBytes(sr.ReadToEnd());
        //        //}


        //        // Crear un objeto Crc32 para calcular el CRC
        //        Crc32 objCrc32 = new Crc32();

        //        // Crear el archivo ZIP de salida
        //        string archivoZip = Path.Combine(ruta, nomArchivo + ".ZIP");
        //        using (ZipOutputStream strmZipOutputStream = new ZipOutputStream(File.Create(archivoZip)))
        //        {
        //            // Establecer el nivel de compresión
        //            strmZipOutputStream.SetLevel(6); // De 0 a 9 (0 sin compresión, 9 máxima compresión)

        //            // Abrir el archivo XML para leerlo
        //            using (FileStream strmFile = File.OpenRead(archivoXml))
        //            {
        //                byte[] abyBuffer = new byte[strmFile.Length];
        //                strmFile.Read(abyBuffer, 0, abyBuffer.Length);

        //                // Crear una entrada en el archivo ZIP para el archivo XML
        //                ZipEntry theEntry = new ZipEntry(nomArchivo + ".XML");

        //                // Establecer la fecha y hora del último cambio
        //                FileInfo fi = new FileInfo(archivoXml);
        //                theEntry.DateTime = fi.LastWriteTime;
        //                theEntry.Size = strmFile.Length;

        //                // Calcular el CRC del archivo XML
        //                objCrc32.Reset();
        //                objCrc32.Update(abyBuffer);
        //                theEntry.Crc = objCrc32.Value;

        //                // Agregar la entrada al archivo ZIP y escribir los datos
        //                strmZipOutputStream.PutNextEntry(theEntry);
        //                strmZipOutputStream.Write(abyBuffer, 0, abyBuffer.Length);
        //            }

        //            // Finalizar la escritura del archivo ZIP
        //            strmZipOutputStream.Finish();
        //        }

        //        Console.WriteLine("Archivo comprimido correctamente.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error al comprimir el archivo: " + ex.Message);
        //    }
        //}

        //COMPRIMIR SIN BOOM:
        public static void Comprimir(string nomArchivo, string ruta)
        {
            try
            {
                // Verificar si el archivo XML existe
                string archivoXml = Path.Combine(ruta, nomArchivo + ".XML");
                if (!File.Exists(archivoXml))
                {
                    throw new FileNotFoundException("El archivo XML no se encuentra en la ruta especificada.");
                }

                // Eliminar el BOM (Byte Order Mark) si existe
                byte[] bytesWithoutBOM;
                using (StreamReader sr = new StreamReader(archivoXml, Encoding.UTF8))
                {
                    bytesWithoutBOM = new UTF8Encoding(false).GetBytes(sr.ReadToEnd());
                }

                // Crear un objeto Crc32 para calcular el CRC
                Crc32 objCrc32 = new Crc32();

                // Crear el archivo ZIP de salida
                string archivoZip = Path.Combine(ruta, nomArchivo + ".ZIP");
                using (ZipOutputStream strmZipOutputStream = new ZipOutputStream(File.Create(archivoZip)))
                {
                    // Establecer el nivel de compresión
                    strmZipOutputStream.SetLevel(6); // De 0 a 9 (0 sin compresión, 9 máxima compresión)

                    // Crear una entrada en el archivo ZIP para el archivo XML
                    ZipEntry theEntry = new ZipEntry(nomArchivo + ".XML");

                    // Establecer la fecha y hora del último cambio
                    FileInfo fi = new FileInfo(archivoXml);
                    theEntry.DateTime = fi.LastWriteTime;
                    theEntry.Size = bytesWithoutBOM.Length;

                    // Calcular el CRC del archivo XML sin BOM
                    objCrc32.Reset();
                    objCrc32.Update(bytesWithoutBOM);
                    theEntry.Crc = objCrc32.Value;

                    // Agregar la entrada al archivo ZIP y escribir los datos
                    strmZipOutputStream.PutNextEntry(theEntry);
                    strmZipOutputStream.Write(bytesWithoutBOM, 0, bytesWithoutBOM.Length);

                    // Finalizar la escritura del archivo ZIP
                    strmZipOutputStream.Finish();
                }

                Console.WriteLine("Archivo comprimido correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al comprimir el archivo: " + ex.Message);
            }
        }

        private static string ObtenerHashSHA256(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public async Task<Dictionary<string, string>> EnvioTicketAsync(string rutaArchivoCdr, string ticket, string tokenAccess, string ruc, string nombreFile)
        {
            var mensaje = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(ticket))
            {
                mensaje["cdr_hash"] = string.Empty;
                mensaje["cdr_msj_sunat"] = "Ticket vacio";
                mensaje["cdr_ResponseCode"] = null;
                mensaje["numerror"] = null;
            }
            else
            {
                mensaje["ticket"] = ticket;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("numRucEnvia", ruc);
                    client.DefaultRequestHeaders.Add("numTicket", ticket);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAccess);

                    //var response = await client.GetAsync($"https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/envios/{ticket}");//prueba
                    var response = await client.GetAsync($"https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/envios/{ticket}"); //sunat

                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic response3 = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string codRespuesta = response3.codRespuesta;

                    mensaje["ticket_rpta"] = codRespuesta;

                    if (codRespuesta == "99")
                    {
                        var error = response3.error;
                        mensaje["cdr_hash"] = string.Empty;
                        mensaje["cdr_msj_sunat"] = error.desError;
                        mensaje["cdr_ResponseCode"] = "99";
                        mensaje["numerror"] = error.numError;
                    }
                    else if (codRespuesta == "98")
                    {
                        mensaje["cdr_hash"] = string.Empty;
                        mensaje["cdr_msj_sunat"] = "Envío en proceso";
                        mensaje["cdr_ResponseCode"] = "98";
                        mensaje["numerror"] = "98";
                    }
                    else if (codRespuesta == "0")
                    {

                        string arcCdr = (string)response3.arcCdr; // ← castea el JToken a string

                        mensaje["arcCdr"] = response3.arcCdr;
                        //mensaje["indCdrGenerado"] = response3.indCdrGenerado;
                        mensaje["indCdrGenerado"] = (string)response3.indCdrGenerado; //agregado
                        string zipFilePath = Path.Combine(rutaArchivoCdr, $"R-{nombreFile}.ZIP");
                        //Se limpia posibles saltos de lineas/espacios  --agreado
                        arcCdr = arcCdr?.Trim().Replace("\r", "").Replace("\n", "");

                        if (string.IsNullOrWhiteSpace(arcCdr))
                            throw new InvalidOperationException("La respuesta no contiene arcCdr."); //agregado

                        byte[] zipBytes = Convert.FromBase64String(arcCdr);
                        File.WriteAllBytes(zipFilePath, zipBytes); //agregado

                        //File.WriteAllBytes(zipFilePath, Convert.FromBase64String(response3.arcCdr.ToString()));

                        // Descomprimir el archivo ZIP utilizando SharpZipLib
                        string xmlPath = DescomprimirConSharpZipLib(zipFilePath, rutaArchivoCdr);

                        //DescomprimirConSharpZipLib(zipFilePath, rutaArchivoCdr);
                        // Hash CDR
                        var docCdr = new XmlDocument();
                        docCdr.Load(xmlPath);
                        //docCdr.Load(Path.Combine(rutaArchivoCdr, $"R-{nombreFile}.xml"));

                        mensaje["cdr_hash"] = docCdr.GetElementsByTagName("DigestValue")[0]?.InnerText;
                        mensaje["cdr_msj_sunat"] = docCdr.GetElementsByTagName("Description")[0]?.InnerText;
                        mensaje["cdr_ResponseCode"] = docCdr.GetElementsByTagName("ResponseCode")[0]?.InnerText;
                        mensaje["numerror"] = string.Empty;
                    }
                    else
                    {
                        mensaje["cdr_hash"] = string.Empty;
                        mensaje["cdr_msj_sunat"] = "SUNAT FUERA DE SERVICIO";
                        mensaje["cdr_ResponseCode"] = "88";
                        mensaje["numerror"] = "88";
                    }
                }
            }
            return mensaje;
        }
        private string  DescomprimirConSharpZipLib(string archivoZip, string rutaDestino )
        {

            if (!Directory.Exists(rutaDestino))
                Directory.CreateDirectory(rutaDestino);

            string xmlExtraido = null;

            using (var fs = File.OpenRead(archivoZip))
            using (var zipStream = new ZipInputStream(fs))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    var safeName = Path.GetFileName(entry.Name);
                    var outPath = Path.Combine(rutaDestino, safeName);

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(outPath);
                        continue;
                    }

                    var dir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    using (var outFile = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        // ZipInputStream hereda de Stream, CopyTo está disponible
                        zipStream.CopyTo(outFile);
                    }

                    if (safeName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        xmlExtraido = outPath;
                }
            }
            if (string.IsNullOrEmpty(xmlExtraido))
                throw new FileNotFoundException("El ZIP no contenía ningún XML CDR.", archivoZip);
            return xmlExtraido;
            /*
            using (FileStream fs = File.OpenRead(archivoZip))
            using (ZipInputStream zipStream = new ZipInputStream(fs))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    string filePath = Path.Combine(rutaDestino, entry.Name);

                    using (FileStream streamWriter = File.Create(filePath))
                    {
                        int size = 2048;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }*/
        }

        //public Dictionary<string, string> EnvioXml(string ruta, string nomARCHIVO, string tokenAcceso)
        //{
        //    //string url = "https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/" + nomARCHIVO;
        //    //string archivoZip = Path.Combine(ruta, nomARCHIVO + ".zip");
        //    // Ruta del archivo XML que se va a comprimir
        //    string archivoXml = Path.Combine(ruta, nomARCHIVO + ".XML");

        //    // Comprimir el archivo XML
        //    Comprimir(nomARCHIVO, ruta);

        //    // Ruta del archivo ZIP que se creó
        //    string archivoZip = Path.Combine(ruta, nomARCHIVO + ".ZIP");

        //    byte[] archivoBytes = File.ReadAllBytes(archivoZip);
        //    string archivoBase64 = Convert.ToBase64String(archivoBytes);

        //    var data = new
        //    {
        //        nomArchivo = nomARCHIVO + ".zip",
        //        arcGreZip = archivoBase64,
        //        hashZip = ObtenerHashSHA256(archivoZip)
        //    };

        //    string jsonData = JsonConvert.SerializeObject(data);

        //    // Establecer la versión de TLS que debe usar el cliente
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    string url = "https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/" + nomARCHIVO;
        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Method = "POST";
        //    request.Headers.Add("Authorization", "Bearer " + tokenAcceso);
        //    request.ContentType = "application/json";

        //    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //    {
        //        streamWriter.Write(jsonData);
        //    }

        //    try
        //    {
        //        var response = (HttpWebResponse)request.GetResponse();
        //        using (var reader = new StreamReader(response.GetResponseStream()))
        //        {
        //            string jsonResponse = reader.ReadToEnd();
        //            JObject jsonObject = JObject.Parse(jsonResponse);

        //            // Convertir el JObject en un Dictionary<string, string>
        //            return jsonObject.ToObject<Dictionary<string, string>>();
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        // Si hay un error en la respuesta del servidor, obtenemos el detalle
        //        using (var streamReader = new StreamReader(ex.Response.GetResponseStream()))
        //        {
        //            string errorResponse = streamReader.ReadToEnd();
        //            Console.WriteLine("Error detallado del servidor: " + errorResponse);
        //        }

        //        // También podrías capturar el mensaje de error general
        //        Console.WriteLine("Error general: " + ex.Message);
        //        throw;
        //    }
        //}

        public Dictionary<string, string> Enviar_Baja_de_FE(BE.CPE_BAJA CPEBaja)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            string nomARCHIVO = "";
            string ruta = "";
            string rutaFirma = "";
            string url = "";


            nomARCHIVO = CPEBaja.NRO_DOCUMENTO_EMPRESA + "-" + CPEBaja.CODIGO + "-" + CPEBaja.SERIE + "-" + CPEBaja.SECUENCIA;

            if ((CPEBaja.TIPO_PROCESO == 3))
            { //'Pruebas
                ruta = @"D:\\CPE_2\\BETA\\";
                url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
            }
            else if ((CPEBaja.TIPO_PROCESO == 1))
            { //Produccion
                ruta = @"D:\\CPE_2\\PRODUCCION\\";
                url = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService"; //'Produccion
            }


            //valores:            
            rutaFirma = @"D:\\CPE_2\\FIRMABETA\\FIRMABETA.pfx";
            //===================creamos xml(comprobante)===================                
            dictionary = objXML.ResumenBaja(CPEBaja, nomARCHIVO, ruta);

            if (dictionary["flg_rta"] == "0")
            {
                return dictionary;
            }

            //=================datos para la firma====================
            objPregunta.ruta_Firma = rutaFirma;
            objPregunta.contra_Firma = CPEBaja.CONTRA_FIRMA;
            objPregunta.ruta_xml = ruta + nomARCHIVO + ".XML";
            RutaCompletaxml = ruta + nomARCHIVO + ".XML";
            objPregunta.flg_firma = 0;
            objRespuesta = objSignature.FirmaXMl(objPregunta);
            //====================0 llamamos la metodo apra enviar :
            dictionary = objENV.EnvioResumen(CPEBaja.NRO_DOCUMENTO_EMPRESA, CPEBaja.USUARIO_SOL_EMPRESA, CPEBaja.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url, objRespuesta.DigestValue);
            CPEBaja.HASH_CPE = dictionary["hash_cpe"];
            return dictionary;



        }

        //consultas de Bajas:
        public Dictionary<string, string> Consulta_Ticket_de_Baja(BE.CONSULTA_TICKET CPETicket)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            string nomARCHIVO = "";
            string ruta = "";          
            string url = "";


            nomARCHIVO = CPETicket.NRO_DOCUMENTO_EMPRESA + "-" + CPETicket.TIPO_DOCUMENTO + "-" + CPETicket.NRO_DOCUMENTO;

            if (CPETicket.TIPO_PROCESO == 3)
            { //'Pruebas
                ruta = @"D:\\CPE_2\\BETA\\";
                url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
            }
            else if ((CPETicket.TIPO_PROCESO == 1))
            { //Produccion
                ruta = @"D:\\CPE_2\\PRODUCCION\\";
                url = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService"; //'Produccion
            }          
                     
            //====================0 llamamos la metodo apra enviar :
            dictionary = objENV.ConsultaTicket(CPETicket.NRO_DOCUMENTO_EMPRESA, CPETicket.USUARIO_SOL_EMPRESA, CPETicket.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url, objRespuesta.DigestValue, CPETicket.TICKET);
           
            return dictionary;

        }

        //pegar: para Crear el Resumen de Boletas:
        public Dictionary<string, string> Enviar_ResumenBoletas(BE.CPE_RESUMEN_BOLETA CPEResumen)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            string nomARCHIVO = "";
            string ruta = "";
            string rutaFirma = "";
            string url = "";


            nomARCHIVO = CPEResumen.NRO_DOCUMENTO_EMPRESA + "-" + CPEResumen.CODIGO + "-" + CPEResumen.SERIE + "-" + CPEResumen.SECUENCIA;

            if ((CPEResumen.TIPO_PROCESO == 3))
            { //'Pruebas
                ruta = @"D:\\CPE_2\\BETA\\";
                url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
            }
            else if ((CPEResumen.TIPO_PROCESO == 1))
            { //Produccion
                ruta = @"D:\\CPE_2\\PRODUCCION\\";
                url = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService"; //'Produccion
            }


            //valores:            
            rutaFirma = @"D:\\CPE_2\\FIRMABETA\\FIRMABETA.pfx";
            //===================creamos xml(comprobante)===================                
            dictionary = objXML.ResumenBoleta(CPEResumen, nomARCHIVO, ruta);

            if (dictionary["flg_rta"] == "0")
            {
                return dictionary;
            }

            //=================datos para la firma====================
            objPregunta.ruta_Firma = rutaFirma;
            objPregunta.contra_Firma = CPEResumen.CONTRA_FIRMA;
            objPregunta.ruta_xml = ruta + nomARCHIVO + ".XML";
            RutaCompletaxml = ruta + nomARCHIVO + ".XML";
            objPregunta.flg_firma = 0;
            objRespuesta = objSignature.FirmaXMl(objPregunta);
            //====================0 llamamos la metodo apra enviar :
            dictionary = objENV.EnvioResumen(CPEResumen.NRO_DOCUMENTO_EMPRESA, CPEResumen.USUARIO_SOL_EMPRESA, CPEResumen.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url, objRespuesta.DigestValue);
            
            return dictionary;



        }

        //metodo para enviar NC
        public Dictionary<string, string> Enviar_NotaCredito_aSunat(BE.CPE CPE)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string nomARCHIVO = "";
            string ruta = "";
            string rutaFirma = "";
            string url = "";

            //============================================


            nomARCHIVO = CPE.NRO_DOCUMENTO_EMPRESA + "-" + CPE.COD_TIPO_DOCUMENTO + "-" + CPE.NRO_COMPROBANTE;

            if ((CPE.TIPO_PROCESO == 3))
            { //'Pruebas
                ruta = @"D:\\CPE_2\\BETA\\";
                url = "https://e-beta.sunat.gob.pe:443/ol-ti-itcpfegem-beta/billService";
            }
            else if ((CPE.TIPO_PROCESO == 1))
            { //Produccion
                ruta = @"D:\\CPE_2\\PRODUCCION\\";
                url = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService"; //'Produccion
            }


            //rutaFirma = "D:\\CPE\\FIRMA\\" & CPE.NRO_DOCUMENTO_EMPRESA & ".pfx"
            rutaFirma = @"D:\\CPE_2\\FIRMABETA\\FIRMABETA.pfx";

            //===================creamos xml(comprobante)===================
            if (CPE.COD_TIPO_DOCUMENTO == "07")
            {
                dictionary = objXML.CPE_NC(CPE, nomARCHIVO, ruta);
            }

            if (dictionary["flg_rta"] == "0")
            {
                return dictionary;
            }
            //=================datos para la firma====================
            objPregunta.ruta_Firma = rutaFirma;
            objPregunta.contra_Firma = CPE.CONTRA_FIRMA;
            objPregunta.ruta_xml = ruta + nomARCHIVO + ".XML";
            RutaCompletaxml = ruta + nomARCHIVO + ".XML";
            objPregunta.flg_firma = 0;
            objRespuesta = objSignature.FirmaXMl(objPregunta);
              

            //====================enviamos documento a la sunat=========================
            dictionary = objENV.Envio(CPE.NRO_DOCUMENTO_EMPRESA, CPE.USUARIO_SOL_EMPRESA, CPE.PASS_SOL_EMPRESA, nomARCHIVO, ruta, url, objRespuesta.DigestValue);
            CPE.HASH_CPE = dictionary["hash_cpe"];
            return dictionary;

        }



    }
}
