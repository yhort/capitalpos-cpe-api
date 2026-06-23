Imports System.IO
Imports System.Net.Http
Imports System.Net
Imports System.Xml
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.IO.Compression
Imports ICSharpCode.SharpZipLib.Zip
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports System.Text
Imports System.Net.Http.Headers
Imports System.Security.Cryptography

Public Class ServiceSunat

    Public Function Envio(ruc As String, usu_sol As String, contra_sol As String, nombre_archivo As String, rutaArchivo As String, url As String, hash_cpe As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim doc As New XmlDocument()
            Dim strCDR As String
            Dim strSOAP As String
            Dim rutaCompleta As String = rutaArchivo & nombre_archivo
            Comprimir(nombre_archivo, rutaArchivo)
            Dim rutaCdr As String = rutaArchivo & "R-" & nombre_archivo & ".ZIP"
            Dim NomFichierZIP As String = System.IO.Path.GetFileName(rutaCompleta & ".ZIP")
            Dim data As Byte() = System.IO.File.ReadAllBytes(rutaCompleta & ".ZIP")

            strSOAP = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' " &
                    "xmlns:ser='http://service.sunat.gob.pe' " &
                    "xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'> " &
                    "<soapenv:Header> " &
                    "<wsse:Security> " &
                    "<wsse:UsernameToken> " &
                    "<wsse:Username>" & ruc & usu_sol & "</wsse:Username> " &
                    "<wsse:Password>" & contra_sol & "</wsse:Password> " &
                    "</wsse:UsernameToken> " &
                    "</wsse:Security> " &
                    "</soapenv:Header> " &
                    "<soapenv:Body> " &
                    "<ser:sendBill> " &
                    "<fileName>" & NomFichierZIP & "</fileName> " &
                    "<contentFile>" & Convert.ToBase64String(data) & "</contentFile> " &
                    "</ser:sendBill> " &
                    "</soapenv:Body> " &
                    "</soapenv:Envelope>"


            Dim returned_value As String
            Dim strPostData As String
            Dim objRequest As Object



            strPostData = strSOAP
            'objRequest = CreateObject("MSXML2.XMLHTTP.3.0")
            objRequest = CreateObject("MSXML2.ServerXMLHTTP")
            With objRequest
                .Open("POST", url, False)
                .setRequestHeader("Content-Type", "application/xml")
                .send(strPostData)
                returned_value = .responseText
            End With
            doc.LoadXml(returned_value)

            '=======================validando respuesta========================
            Dim Lst As XmlNodeList = doc.SelectNodes("//faultcode")
            If Lst.Count() > 0 Then
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "0")
                dictionary.Add("mensaje", "ERROR AL ENVIAR A LA SUNAT")
                dictionary.Add("cod_sunat", doc.SelectSingleNode("//faultcode").InnerText.Replace("soap-env:Client.", ""))
                dictionary.Add("msj_sunat", doc.SelectSingleNode("//faultstring").InnerText)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", hash_cpe)
            Else
                strCDR = doc.SelectSingleNode("//applicationResponse").InnerText
                Dim byteCDR As Byte() = Convert.FromBase64String(strCDR)
                Dim s As IO.FileStream
                s = IO.File.Open(rutaCdr, IO.FileMode.Append)
                s.Write(byteCDR, 0, byteCDR.Length)
                s.Close()

                '===============descomprimo el xml=============
                Descomprimir(rutaArchivo, "R-" & nombre_archivo)
                '================================================================
                Dim xmlCDR As New XmlDocument()
                Dim rutaxmlCDR = rutaArchivo & "R-" & nombre_archivo & ".XML"
                xmlCDR.Load(rutaxmlCDR)

                '=======================nombre de espacios para obtener los valores del xml======================
                Dim nsmgr As New XmlNamespaceManager(doc.NameTable)
                nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")
                Dim nsmgrSing As New XmlNamespaceManager(doc.NameTable)
                nsmgrSing.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#")

                '========================asignamos valores de retorno======================
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "1")
                dictionary.Add("mensaje", "COMPROBANTE ENVIADO CORRECTAMENTE")
                dictionary.Add("cod_sunat", xmlCDR.SelectSingleNode("//cbc:ResponseCode", nsmgr).InnerText)
                dictionary.Add("msj_sunat", xmlCDR.SelectSingleNode("//cbc:Description", nsmgr).InnerText.ToUpper)
                dictionary.Add("hash_cdr", xmlCDR.SelectSingleNode("//ds:DigestValue", nsmgrSing).InnerText)
                dictionary.Add("hash_cpe", hash_cpe)
                '=============eliminas el archivo comprimido que enviamos===============
                File.Delete(rutaCompleta & ".ZIP")
            End If

            'https://stackoverflow.com/questions/16889895/c-sharp-xmldocument-selectnodes-is-not-working
        Catch ex As Exception
            'MsgBox("Error al Enviar Doc a Sunat: " & vbCrLf & ex.Message, MsgBoxStyle.Exclamation, "Advertencia de Seguridad")
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CONECTARSE A LA SUNAT: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", hash_cpe)
        End Try
        Return dictionary
    End Function


    Public Function EnvioGuiaRemision(ruc As String, usu_sol As String, contra_sol As String, nombre_archivo As String, rutaArchivo As String, url As String, hash_cpe As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim doc As New XmlDocument()
            Dim strCDR As String
            Dim strSOAP As String
            Dim rutaCompleta As String = rutaArchivo & nombre_archivo
            Comprimir(nombre_archivo, rutaArchivo)
            Dim rutaCdr As String = rutaArchivo & "R-" & nombre_archivo & ".ZIP"
            Dim NomFichierZIP As String = System.IO.Path.GetFileName(rutaCompleta & ".ZIP")
            Dim data As Byte() = System.IO.File.ReadAllBytes(rutaCompleta & ".ZIP")


            Dim returned_value As String
            Dim strPostData As String
            Dim objRequest As Object

            'strPostData = strSOAP'
            'objRequest = CreateObject("MSXML2.XMLHTTP.3.0")

            'ACA PUEDE IR EL TOKEN'

            ' URL pública de la API de SUNAT
            url = $"https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/{nombre_archivo}"



            'objRequest = CreateObject("MSXML2.ServerXMLHTTP")
            'With objRequest
            '    .Open("POST", url, False)
            '    .setRequestHeader("Content-Type", "application/xml")
            '    .send(strPostData)
            '    returned_value = .responseText
            'End With


            doc.LoadXml(returned_value)

            '=======================validando respuesta========================
            Dim Lst As XmlNodeList = doc.SelectNodes("//faultcode")
            If Lst.Count() > 0 Then
                Dim xmsmsunt As String = ""
                Dim xcod As String = ""
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "0")
                dictionary.Add("cod_sunat", doc.SelectSingleNode("//faultstring").InnerText.Replace("soap-env:Client.", ""))
                xcod = doc.SelectSingleNode("//faultstring").InnerText.Replace("soap-env:Client.", "")
                dictionary.Add("msj_sunat", doc.SelectSingleNode("//message").InnerText)
                xmsmsunt = doc.SelectSingleNode("//message").InnerText
                MsgBox("Error al Enviar a la Sunat" & vbCrLf & "Codigo: " & xcod & vbCrLf & xmsmsunt, MsgBoxStyle.Exclamation, "Advertencia de Seguridad")
                dictionary.Add("mensaje", "ERROR AL ENVIAR A LA SUNAT" & vbCrLf & "Codigo: " & xcod & vbCrLf & xmsmsunt)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", hash_cpe)

                ''Nuevo metodo de Captura:


            Else
                strCDR = doc.SelectSingleNode("//applicationResponse").InnerText
                Dim byteCDR As Byte() = Convert.FromBase64String(strCDR)
                Dim s As IO.FileStream
                s = IO.File.Open(rutaCdr, IO.FileMode.Append)
                s.Write(byteCDR, 0, byteCDR.Length)
                s.Close()

                '===============descomprimo el xml=============
                Descomprimir(rutaArchivo, "R-" & nombre_archivo)
                '================================================================
                Dim xmlCDR As New XmlDocument()
                Dim rutaxmlCDR = rutaArchivo & "R-" & nombre_archivo & ".XML"
                xmlCDR.Load(rutaxmlCDR)

                '=======================nombre de espacios para obtener los valores del xml======================
                Dim nsmgr As New XmlNamespaceManager(doc.NameTable)
                nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")
                Dim nsmgrSing As New XmlNamespaceManager(doc.NameTable)
                nsmgrSing.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#")

                '========================asignamos valores de retorno======================
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "1")
                dictionary.Add("mensaje", "COMPROBANTE ENVIADO CORRECTAMENTE")
                'MsgBox("El Documento fue Enviado y Aceptado por la Sunat", MsgBoxStyle.Information, "Envio a Sunat")
                dictionary.Add("cod_sunat", xmlCDR.SelectSingleNode("//cbc:ResponseCode", nsmgr).InnerText)
                dictionary.Add("msj_sunat", xmlCDR.SelectSingleNode("//cbc:Description", nsmgr).InnerText.ToUpper)
                dictionary.Add("hash_cdr", xmlCDR.SelectSingleNode("//ds:DigestValue", nsmgrSing).InnerText)
                dictionary.Add("hash_cpe", hash_cpe)
                '=============eliminas el archivo comprimido que enviamos===============
                File.Delete(rutaCompleta & ".ZIP")
            End If

            'https://stackoverflow.com/questions/16889895/c-sharp-xmldocument-selectnodes-is-not-working


        Catch ex As Exception
            MsgBox("Error al Enviar Guia a Sunat: " & vbCrLf & ex.Message, MsgBoxStyle.Exclamation, "Advertencia de Seguridad")
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CONECTARSE A LA SUNAT: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", hash_cpe)
        End Try
        Return dictionary
    End Function


    'Public Function EnvioXmls(ByVal path As String, ByVal nombreFile As String, ByVal tokenAccess As String) As Dictionary(Of String, String)
    '    ' Definir la URL de la API de Nubefact
    '    Dim url As String = $"https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/{nombreFile}"
    '    Dim responseContent As String = String.Empty

    '    Try

    '        Dim rutaCompleta As String = path & nombreFile
    '        'Comprimir(nombre_archivo, rutaArchivo)
    '        'Dim rutaCdr As String = rutaArchivo & "R-" & nombre_archivo & ".ZIP"
    '        'Dim NomFichierZIP As String = System.IO.Path.GetFileName(rutaCompleta & ".ZIP")
    '        Dim datax As Byte() = System.IO.File.ReadAllBytes(rutaCompleta & ".ZIP")


    '        ' Llamar a la función Comprimir para comprimir el archivo XML en un archivo ZIP
    '        Comprimir(nombreFile, path)

    '        ' Leer el archivo comprimido ZIP como bytes
    '        'Dim zipFilePath As String = System.IO.Path.Combine(path, nombreFile & ".ZIP")
    '        'Dim zipFileBytes As Byte() = File.ReadAllBytes(zipFilePath)


    '        ' Codificar el archivo ZIP en Base64
    '        Dim zipBase64 As String = Convert.ToBase64String(datax)

    '        ' Calcular el hash SHA256 del archivo ZIP
    '        'Dim hashZip As String = ComputeSha256Hash(zipFilePath)
    '        Dim hashZip As String = ComputeSha256Hash(rutaCompleta)

    '        ' Crear el diccionario con los datos del archivo
    '        Dim data As New Dictionary(Of String, String) From {
    '            {"nomArchivo", nombreFile & ".zip"},  ' Nombre del archivo .zip
    '            {"arcGreZip", zipBase64},  ' Contenido del archivo en Base64
    '            {"hashZip", hashZip}  ' Hash SHA256 del archivo
    '        }

    '        ' Serializar los datos a JSON
    '        Dim jsonData As String = JsonConvert.SerializeObject(New With {.archivo = data})
    '        Dim content As New StringContent(jsonData, Encoding.UTF8, "application/json")
    '        content.Headers.ContentType = New MediaTypeHeaderValue("application/json")

    '        ' Agregar el encabezado de autenticación con el token de acceso
    '        Using client As New HttpClient()
    '            client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", tokenAccess)

    '            ' Realizar la solicitud POST sincrónica
    '            Dim response As HttpResponseMessage = client.PostAsync(url, content).Result

    '            ' Asegurar que la respuesta sea exitosa
    '            response.EnsureSuccessStatusCode()

    '            ' Leer la respuesta como una cadena sincrónica
    '            Dim result As String = response.Content.ReadAsStringAsync().Result

    '            ' Deserializar la respuesta JSON y retornarlo como un diccionario
    '            Return JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(result)
    '        End Using
    '    Catch ex As HttpRequestException
    '        responseContent = ex.Message
    '        ' Manejo de excepciones en caso de que falle la solicitud
    '        Console.WriteLine($"Error al enviar el XML: {ex.Message}")
    '    Catch ex As Exception
    '        ' Manejo de excepciones generales
    '        responseContent = ex.Message
    '        Console.WriteLine($"Error: {ex.Message}")
    '    End Try

    '    ' En caso de error, retornar el diccionario con el mensaje de error
    '    Return New Dictionary(Of String, String) From {{"success", "false"}, {"message", responseContent}}
    'End Function



    ' Función para enviar el archivo XML en formato ZIP
    'Public Function EnviarXML(path As String, nombreArchivo As String, tokenAcceso As String) As JObject
    '    'Dim url As String = "https://api-cpe.sunat.gob.pe/v1/contribuyente/gem/comprobantes/" & nombreArchivo 
    '    Dim url As String = "https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/" & nombreArchivo
    '    Dim archivoZip As String = path & nombreArchivo & ".zip"

    '    ' Leer archivo y convertirlo a Base64
    '    Dim archivoBytes As Byte() = File.ReadAllBytes(archivoZip)
    '    Dim archivoBase64 As String = Convert.ToBase64String(archivoBytes)

    '    ' Crear objeto JSON para enviar en la solicitud
    '    Dim data As New With {
    '            Key .nomArchivo = nombreArchivo & ".zip",
    '            Key .arcGreZip = archivoBase64,
    '            Key .hashZip = ObtenerHashSHA256(archivoZip)
    '    }

    '    ' Serializar a JSON
    '    Dim jsonData As String = Newtonsoft.Json.JsonConvert.SerializeObject(New With {Key .archivo = data})

    '    ' Realizar la solicitud HTTP POST
    '    Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
    '    request.Method = "POST"
    '    request.Headers.Add("Authorization", "Bearer " & tokenAcceso)
    '    request.ContentType = "application/json"

    '    ' Escribir el JSON en el cuerpo de la solicitud
    '    Using streamWriter As New StreamWriter(request.GetRequestStream())
    '        streamWriter.Write(jsonData)
    '    End Using

    '    ' Obtener la respuesta
    '    Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
    '    Using reader As New StreamReader(response.GetResponseStream())
    '        Dim jsonResponse As String = reader.ReadToEnd()
    '        Return JObject.Parse(jsonResponse) ' Parsear respuesta como JSON
    '    End Using
    'End Function

    Public Function EnviarXML(ruta As String, nombreArchivo As String, tokenAcceso As String) As Dictionary(Of String, String)
        ' Configurar la URL y la solicitud HTTP
        Dim url As String = "https://gre-test.nubefact.com/v1/contribuyente/gem/comprobantes/" & nombreArchivo
        Dim archivoZip As String = Path.Combine(ruta, nombreArchivo & ".zip")

        ' Leer el archivo y convertirlo a Base64
        Dim archivoBytes As Byte() = File.ReadAllBytes(archivoZip)
        Dim archivoBase64 As String = Convert.ToBase64String(archivoBytes)

        ' Crear objeto JSON con los datos
        Dim data = New With {
        Key .nomArchivo = nombreArchivo & ".zip",
        Key .arcGreZip = archivoBase64,
        Key .hashZip = ObtenerHashSHA256(archivoZip)
        }

        ' Serializar el objeto a JSON
        Dim jsonData As String = JsonConvert.SerializeObject(data)

        ' Crear la solicitud HTTP POST
        Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
        request.Method = "POST"
        request.Headers.Add("Authorization", "Bearer " & tokenAcceso)
        request.ContentType = "application/json"

        ' Escribir los datos JSON en el cuerpo de la solicitud (sincrónicamente)
        Using streamWriter As New StreamWriter(request.GetRequestStream())
            streamWriter.Write(jsonData)
        End Using

        Try
            ' Obtener la respuesta del servidor
            Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                Using reader As New StreamReader(response.GetResponseStream())
                    Dim jsonResponse As String = reader.ReadToEnd()
                    Dim jsonObject As JObject = JObject.Parse(jsonResponse)

                    ' Convertir la respuesta en un Dictionary(Of String, String)
                    Dim dictionary As Dictionary(Of String, String) = jsonObject.ToObject(Of Dictionary(Of String, String))()
                    Return dictionary
                End Using
            End Using
        Catch ex As WebException
            ' Manejar errores relacionados con la solicitud
            Console.WriteLine("Error de solicitud: " & ex.Message)
            Throw
        Catch ex As Exception
            ' Manejar otros tipos de errores
            Console.WriteLine("Error: " & ex.Message)
            Throw
        End Try
    End Function


    ' Función para obtener el hash SHA256 de un archivo
    Public Function ObtenerHashSHA256(archivo As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Using stream As FileStream = File.OpenRead(archivo)
                Dim hash As Byte() = sha256.ComputeHash(stream)
                Return BitConverter.ToString(hash).Replace("-", "").ToLower()
            End Using
        End Using
    End Function
    Public Function EnvioResumen(ruc As String, usu_sol As String, contra_sol As String, nombre_archivo As String, rutaArchivo As String, url As String, hash_cpe As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim doc As New XmlDocument()
            Dim ticket As String
            Dim strSOAP As String
            Dim rutaCompleta As String = rutaArchivo & nombre_archivo
            Comprimir(nombre_archivo, rutaArchivo)
            Dim rutaCdr As String = rutaArchivo & "R-" & nombre_archivo & ".ZIP"
            Dim NomFichierZIP As String = System.IO.Path.GetFileName(rutaCompleta & ".ZIP")
            Dim data As Byte() = System.IO.File.ReadAllBytes(rutaCompleta & ".ZIP")

            strSOAP = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' " &
                    "xmlns:ser='http://service.sunat.gob.pe' " &
                    "xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'> " &
                    "<soapenv:Header> " &
                    "<wsse:Security> " &
                    "<wsse:UsernameToken> " &
                    "<wsse:Username>" & ruc & usu_sol & "</wsse:Username> " &
                    "<wsse:Password>" & contra_sol & "</wsse:Password> " &
                    "</wsse:UsernameToken> " &
                    "</wsse:Security> " &
                    "</soapenv:Header> " &
                    "<soapenv:Body> " &
                    "<ser:sendSummary> " &
                    "<fileName>" & NomFichierZIP & "</fileName> " &
                    "<contentFile>" & Convert.ToBase64String(data) & "</contentFile> " &
                    "</ser:sendSummary> " &
                    "</soapenv:Body> " &
                    "</soapenv:Envelope>"


            Dim returned_value As String
            Dim strPostData As String
            Dim objRequest As Object

            strPostData = strSOAP
            'objRequest = CreateObject("MSXML2.XMLHTTP.3.0")
            objRequest = CreateObject("MSXML2.ServerXMLHTTP")
            With objRequest
                .Open("POST", url, False)
                .setRequestHeader("Content-Type", "application/xml")
                .send(strPostData)
                returned_value = .responseText
            End With
            doc.LoadXml(returned_value)

            '=======================validando respuesta========================
            Dim Lst As XmlNodeList = doc.SelectNodes("//faultcode")
            If Lst.Count() > 0 Then
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "0")
                dictionary.Add("mensaje", "ERROR AL ENVIAR A LA SUNAT")
                MsgBox("Error al Enviar el Xml a la Sunat", MsgBoxStyle.Exclamation, "Service Sunat")
                dictionary.Add("cod_sunat", doc.SelectSingleNode("//faultcode").InnerText.Replace("soap-env:Client.", ""))
                dictionary.Add("msj_sunat", doc.SelectSingleNode("//faultstring").InnerText)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", hash_cpe)
            Else
                ticket = doc.SelectSingleNode("//ticket").InnerText
                '========================asignamos valores de retorno======================
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "1")
                dictionary.Add("mensaje", "COMPROBANTE ENVIADO CORRECTAMENTE")
                'MsgBox("COMPROBANTE ENVIADO CORRECTAMENTE a la Sunat", MsgBoxStyle.Exclamation, "Service Sunat")
                'MsgBox("Error al Enviar el Resumen a la Sunat:" & vbCrLf & ex.Message, MsgBoxStyle.Exclamation, "Enviando Resumen a la Sunat ..")

                dictionary.Add("cod_sunat", "")
                dictionary.Add("msj_sunat", ticket)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", hash_cpe)
                '=============eliminas el archivo comprimido que enviamos===============
                File.Delete(rutaCompleta & ".ZIP")
            End If

            'https://stackoverflow.com/questions/16889895/c-sharp-xmldocument-selectnodes-is-not-working
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CONECTARSE A LA SUNAT: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", hash_cpe)
        End Try
        Return dictionary
    End Function

    Public Function ConsultaTicket(ruc As String, usu_sol As String, contra_sol As String, nombre_archivo As String, rutaArchivo As String, url As String, hash_cdr As String, ticket As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim doc As New XmlDocument()
            Dim strCDR As String
            Dim strSOAP As String
            'Dim rutaCompleta As String = rutaArchivo & nombre_archivo
            'Comprimir(nombre_archivo, rutaArchivo)
            Dim rutaCdr As String = rutaArchivo & "R-" & nombre_archivo & ".ZIP"
            'Dim NomFichierZIP As String = System.IO.Path.GetFileName(rutaCompleta & ".ZIP")
            'Dim data As Byte() = System.IO.File.ReadAllBytes(rutaCompleta & ".ZIP")

            strSOAP = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' " &
                    "xmlns:ser='http://service.sunat.gob.pe' " &
                    "xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'> " &
                    "<soapenv:Header> " &
                    "<wsse:Security> " &
                    "<wsse:UsernameToken> " &
                    "<wsse:Username>" & ruc & usu_sol & "</wsse:Username> " &
                    "<wsse:Password>" & contra_sol & "</wsse:Password> " &
                    "</wsse:UsernameToken> " &
                    "</wsse:Security> " &
                    "</soapenv:Header> " &
                    "<soapenv:Body> " &
                    "<ser:getStatus> " &
                    "<ticket>" & ticket & "</ticket> " &
                    "</ser:getStatus>" &
                    "</soapenv:Body> " &
                    "</soapenv:Envelope>"


            Dim returned_value As String
            Dim strPostData As String
            Dim objRequest As Object

            strPostData = strSOAP
            'objRequest = CreateObject("MSXML2.XMLHTTP.3.0")
            objRequest = CreateObject("MSXML2.ServerXMLHTTP")
            With objRequest
                .Open("POST", url, False)
                .setRequestHeader("Content-Type", "application/xml")
                .send(strPostData)
                returned_value = .responseText
            End With
            doc.LoadXml(returned_value)

            '=======================validando respuesta========================
            Dim Lst As XmlNodeList = doc.SelectNodes("//faultcode")
            If Lst.Count() > 0 Then
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "0")
                dictionary.Add("mensaje", "ERROR AL ENVIAR A LA SUNAT")
                dictionary.Add("cod_sunat", doc.SelectSingleNode("//faultcode").InnerText.Replace("soap-env:Client.", ""))
                dictionary.Add("msj_sunat", doc.SelectSingleNode("//faultstring").InnerText)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", "")
            Else
                Dim statuCode As String

                statuCode = doc.SelectSingleNode("//statusCode").InnerText
                If statuCode = "0" Or statuCode = "99" Then
                    strCDR = doc.SelectSingleNode("//content").InnerText
                    Dim byteCDR As Byte() = Convert.FromBase64String(strCDR)
                    Dim s As IO.FileStream
                    s = IO.File.Open(rutaCdr, IO.FileMode.Append)
                    s.Write(byteCDR, 0, byteCDR.Length)
                    s.Close()

                    '===============descomprimo el xml=============
                    Descomprimir(rutaArchivo, "R-" & nombre_archivo) ''una baja de factura
                    '================================================================
                    Dim xmlCDR As New XmlDocument()
                    Dim rutaxmlCDR = rutaArchivo & "R-" & nombre_archivo & ".XML"
                    xmlCDR.Load(rutaxmlCDR)

                    '=======================nombre de espacios para obtener los valores del xml======================
                    Dim nsmgr As New XmlNamespaceManager(doc.NameTable)
                    nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")
                    Dim nsmgrSing As New XmlNamespaceManager(doc.NameTable)
                    nsmgrSing.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#")

                    '========================asignamos valores de retorno======================
                    dictionary = New Dictionary(Of String, String)
                    dictionary.Add("flg_rta", "1")
                    dictionary.Add("mensaje", "COMPROBANTE ENVIADO CORRECTAMENTE")
                    dictionary.Add("cod_sunat", xmlCDR.SelectSingleNode("//cbc:ResponseCode", nsmgr).InnerText)
                    dictionary.Add("msj_sunat", xmlCDR.SelectSingleNode("//cbc:Description", nsmgr).InnerText.ToUpper)
                    dictionary.Add("hash_cdr", xmlCDR.SelectSingleNode("//ds:DigestValue", nsmgrSing).InnerText)
                    dictionary.Add("hash_cpe", "")
                    'File.Delete(rutaCompleta & ".ZIP")
                Else
                    dictionary = New Dictionary(Of String, String)
                    dictionary.Add("flg_rta", "0")
                    dictionary.Add("mensaje", "ERROR AL CONSULTAR TICKET")
                    dictionary.Add("cod_sunat", doc.SelectSingleNode("//statusCode").InnerText)
                    dictionary.Add("msj_sunat", doc.SelectSingleNode("//content").InnerText.ToUpper)
                    dictionary.Add("hash_cdr", "")
                    dictionary.Add("hash_cpe", "")
                End If
                '=============eliminas el archivo comprimido que enviamos===============
            End If

            'https://stackoverflow.com/questions/16889895/c-sharp-xmldocument-selectnodes-is-not-working
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CONECTARSE A LA SUNAT: " & ex.Message)
            MsgBox("Error al Consultar Ticket:" & vbCrLf & ex.Message, MsgBoxStyle.Exclamation, "Respuesta de la Sunat")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function

    Public Function getStatusCDR(ruc As String,
                                 usu_sol As String,
                                 contra_sol As String,
                                 nombre_archivo As String,
                                 rutaArchivo As String,
                                 url As String,
                                 ruc_emisor As String,
                                 tipo_comprobante As String,
                                 serie As String,
                                 numero As String
                                 ) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim doc As New XmlDocument()
            Dim strCDR As String
            Dim strSOAP As String
            'Dim rutaCompleta As String = rutaArchivo & nombre_archivo
            'Comprimir(nombre_archivo, rutaArchivo)
            Dim rutaCdr As String = rutaArchivo & "R-" & nombre_archivo & ".ZIP"
            'Dim NomFichierZIP As String = System.IO.Path.GetFileName(rutaCompleta & ".ZIP")
            'Dim data As Byte() = System.IO.File.ReadAllBytes(rutaCompleta & ".ZIP")

            'strSOAP = "<SOAP-ENV:Envelope" & Chr(13)
            'strSOAP = strSOAP & "xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'" &
            '            "xmlns:SOAP-ENC='http://schemas.xmlsoap.org/soap/encoding/'" &
            '            "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'" &
            '            "xmlns:xsd='http://www.w3.org/2001/XMLSchema'" &
            '            "xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>" &
            '            "<SOAP-ENV:Header" &
            '                "xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope'>" &
            '                "<wsse:Security>" &
            '                    "<wsse:UsernameToken>" &
            '                        "<wsse:Username>" & ruc & usu_sol & "</wsse:Username>" &
            '                        "<wsse:Password>" & contra_sol & "</wsse:Password>" &
            '                    "</wsse:UsernameToken>" &
            '                "</wsse:Security>" &
            '            "</SOAP-ENV:Header>" &
            '            "<SOAP-ENV:Body>" &
            '                "<m:getStatusCdr" &
            '                    "xmlns:m='http://service.sunat.gob.pe'>" &
            '                    "<rucComprobante>" & ruc_emisor & "</rucComprobante>" &
            '                    "<tipoComprobante>" & tipo_comprobante & "</tipoComprobante>" &
            '                    "<serieComprobante>" & serie & "</serieComprobante>" &
            '                    "<numeroComprobante>" & numero & "</numeroComprobante>" &
            '                "</m:getStatusCdr>" &
            '            "</SOAP-ENV:Body>" &
            '        "</SOAP-ENV:Envelope>"

            strSOAP = "<SOAP-ENV:Envelope
                        xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'
                        xmlns:SOAP-ENC='http://schemas.xmlsoap.org/soap/encoding/'
                        xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
                        xmlns:xsd='http://www.w3.org/2001/XMLSchema'
                        xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
                        <SOAP-ENV:Header
                            xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope'>
                            <wsse:Security>
                                <wsse:UsernameToken>
                                    <wsse:Username>" & ruc & usu_sol & "</wsse:Username>
                                    <wsse:Password>" & contra_sol & "</wsse:Password>
                                </wsse:UsernameToken>
                            </wsse:Security>
                        </SOAP-ENV:Header>
                        <SOAP-ENV:Body>
                            <m:getStatusCdr
                                xmlns:m='http://service.sunat.gob.pe'>
                                <rucComprobante>" & ruc_emisor & "</rucComprobante>
                                <tipoComprobante>" & tipo_comprobante & "</tipoComprobante>
                                <serieComprobante>" & serie & "</serieComprobante>
                                <numeroComprobante>" & numero & "</numeroComprobante>
                            </m:getStatusCdr>
                        </SOAP-ENV:Body>
                    </SOAP-ENV:Envelope>"



            Dim returned_value As String
            Dim strPostData As String
            Dim objRequest As Object

            strPostData = PrettyXML(strSOAP)
            'objRequest = CreateObject("MSXML2.XMLHTTP.3.0")
            objRequest = CreateObject("MSXML2.ServerXMLHTTP")
            With objRequest
                .Open("POST", url, False)
                .setRequestHeader("Content-Type", "text/xml")
                .send(strPostData)
                returned_value = .responseText
            End With
            doc.LoadXml(returned_value)

            '=======================validando respuesta========================
            Dim Lst As XmlNodeList = doc.SelectNodes("//faultcode")
            If Lst.Count() > 0 Then
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "0")
                dictionary.Add("mensaje", "Error AL ENVIAR A LA SUNAT")
                dictionary.Add("cod_sunat", doc.SelectSingleNode("//faultcode").InnerText.Replace("ns0:", ""))
                dictionary.Add("msj_sunat", doc.SelectSingleNode("//faultstring").InnerText)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", "")
            Else
                Dim statuCode As String

                statuCode = doc.SelectSingleNode("//statusCode").InnerText
                If statuCode = "0004" Then
                    strCDR = doc.SelectSingleNode("//content").InnerText
                    Dim byteCDR As Byte() = Convert.FromBase64String(strCDR)
                    Dim s As IO.FileStream
                    s = IO.File.Open(rutaCdr, IO.FileMode.Append)
                    s.Write(byteCDR, 0, byteCDR.Length)
                    s.Close()

                    '===============descomprimo el xml=============
                    Descomprimir(rutaArchivo, "R-" & nombre_archivo)
                    '================================================================
                    Dim xmlCDR As New XmlDocument()
                    Dim rutaxmlCDR = rutaArchivo & "R-" & nombre_archivo & ".XML"
                    xmlCDR.Load(rutaxmlCDR)

                    '=======================nombre de espacios para obtener los valores del xml======================
                    Dim nsmgr As New XmlNamespaceManager(doc.NameTable)
                    nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")
                    Dim nsmgrSing As New XmlNamespaceManager(doc.NameTable)
                    nsmgrSing.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#")

                    '========================asignamos valores de retorno======================
                    dictionary = New Dictionary(Of String, String)
                    dictionary.Add("flg_rta", "1")
                    dictionary.Add("mensaje", "COMPROBANTE ENVIADO CORRECTAMENTE")
                    dictionary.Add("cod_sunat", xmlCDR.SelectSingleNode("//cbc:ResponseCode", nsmgr).InnerText)
                    dictionary.Add("msj_sunat", xmlCDR.SelectSingleNode("//cbc:Description", nsmgr).InnerText.ToUpper)
                    dictionary.Add("hash_cdr", xmlCDR.SelectSingleNode("//ds:DigestValue", nsmgrSing).InnerText)
                    dictionary.Add("hash_cpe", "")
                    'File.Delete(rutaCompleta & ".ZIP")
                Else
                    dictionary = New Dictionary(Of String, String)
                    dictionary.Add("flg_rta", "0")
                    dictionary.Add("mensaje", "ERROR AL CONSULTAR STATUS CDR")
                    dictionary.Add("cod_sunat", doc.SelectSingleNode("//statusCode").InnerText)
                    dictionary.Add("msj_sunat", doc.SelectSingleNode("//statusMessage").InnerText.ToUpper)
                    dictionary.Add("hash_cdr", "")
                    dictionary.Add("hash_cpe", "")
                End If
            End If

            'https://stackoverflow.com/questions/16889895/c-sharp-xmldocument-selectnodes-is-not-working
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CONECTARSE A LA SUNAT: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function

    Public Function getStatusFactura(ruc As String,
                                     usu_sol As String,
                                     contra_sol As String,
                                     url As String,
                                     ruc_emisor As String,
                                     tipo_comprobante As String,
                                     serie As String,
                                     numero As String
                                 ) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim doc As New XmlDocument()
            Dim strSOAP As String

            strSOAP = "<soapenv:Envelope xmlns:ser='http://service.sunat.gob.pe' " &
                        "xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' " &
                        "xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'> " &
                        "<soapenv:Header> " &
                        "<wsse:Security> " &
                        "<wsse:UsernameToken> " &
                        "<wsse:Username>" & ruc & usu_sol & "</wsse:Username> " &
                        "<wsse:Password>" & contra_sol & "</wsse:Password> " &
                        "</wsse:UsernameToken> " &
                        "</wsse:Security> " &
                        "</soapenv:Header> " &
                        "<soapenv:Body> " &
                        "<ser:getStatus> " &
                        "<rucComprobante>" & ruc_emisor & "</rucComprobante> " &
                        "<tipoComprobante>" & tipo_comprobante & "</tipoComprobante> " &
                        "<serieComprobante>" & serie & "</serieComprobante> " &
                        "<numeroComprobante>" & numero & "</numeroComprobante> " &
                        "</ser:getStatus> " &
                        "</soapenv:Body> " &
                        "</soapenv:Envelope>"

            'strSOAP = "<soapenv:Envelope xmlns:ser='http://service.sunat.gob.pe'
            '        xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'
            '        xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            '            <soapenv:Header>
            '                <wsse:Security>
            '                    <wsse:UsernameToken>
            '                        <wsse:Username>20100066603MODDATOS</wsse:Username>
            '                        <wsse:Password>moddatos</wsse:Password>
            '                    </wsse:UsernameToken>
            '                </wsse:Security>
            '            </soapenv:Header>
            '           <soapenv:Body>
            '              <ser:getStatus>
            '                  <rucComprobante>1028308796</rucComprobante>
            '                  <tipoComprobante>01</tipoComprobante>
            '                  <serieComprobante>F213</serieComprobante>
            '                  <numeroComprobante>12345</numeroComprobante>
            '              </ser:getStatus>
            '           </soapenv:Body>
            '        </soapenv:Envelope>"



            Dim returned_value As String
            Dim strPostData As String
            Dim objRequest As Object

            strPostData = strSOAP
            'objRequest = CreateObject("MSXML2.XMLHTTP.3.0")
            objRequest = CreateObject("MSXML2.ServerXMLHTTP")
            With objRequest
                .Open("POST", url, False)
                .setRequestHeader("Content-Type", "text/xml")
                .send(strPostData)
                returned_value = .responseText
            End With
            doc.LoadXml(returned_value)

            '=======================validando respuesta========================
            Dim Lst As XmlNodeList = doc.SelectNodes("//faultcode")
            If Lst.Count() > 0 Then
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "0")
                dictionary.Add("mensaje", "ERROR AL CONSULTAR EN LA SUNAT")
                dictionary.Add("cod_sunat", doc.SelectSingleNode("//faultcode").InnerText.Replace("ns0:", ""))
                dictionary.Add("msj_sunat", doc.SelectSingleNode("//faultstring").InnerText)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", "")
            Else
                Dim statuCode As String
                Dim statuMensaje As String

                statuCode = doc.SelectSingleNode("//statusCode").InnerText
                statuMensaje = doc.SelectSingleNode("//statusMessage").InnerText

                '========================asignamos valores de retorno======================
                dictionary = New Dictionary(Of String, String)
                dictionary.Add("flg_rta", "1")
                dictionary.Add("mensaje", "COMPROBANTE CONSULTADO CORRECTAMENTE")
                dictionary.Add("cod_sunat", statuCode)
                dictionary.Add("msj_sunat", statuMensaje)
                dictionary.Add("hash_cdr", "")
                dictionary.Add("hash_cpe", "")
            End If

            'https://stackoverflow.com/questions/16889895/c-sharp-xmldocument-selectnodes-is-not-working
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CONECTARSE A LA SUNAT: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function


End Class
