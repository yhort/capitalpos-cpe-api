Imports ICSharpCode.SharpZipLib.Zip
Imports ICSharpCode.SharpZipLib.Checksums
Imports System.IO
Imports System.Xml
Module Variables_Globales
    Public Sub Comprimir(NOM_ARHIVO As String, RUTA As String)
        ' comprimir los ficheros del array en el zip indicado
        ' si crearAuto = True, zipfile será el directorio en el que se guardará
        ' y se generará automáticamente el nombre con la fecha y hora actual
        Dim objCrc32 As New Crc32()
        Dim strmZipOutputStream As ZipOutputStream
        '
        'If zipFic = "" Then
        '    zipFic = "."
        '    crearAuto = True
        'End If
        'If crearAuto Then
        '    ' si hay que crear el nombre del fichero
        '    ' éste será el path indicado y la fecha actual
        '    zipFic &= "\ZIP" & DateTime.Now.ToString("yyMMddHHmmss") & ".zip"
        'End If
        strmZipOutputStream = New ZipOutputStream(File.Create(RUTA & NOM_ARHIVO & ".ZIP"))
        ' Compression Level: 0-9
        ' 0: no(Compression)
        ' 9: maximum compression
        strmZipOutputStream.SetLevel(6)
        '

        Dim strmFile As FileStream = File.OpenRead(RUTA & NOM_ARHIVO & ".XML")
        Dim abyBuffer(Convert.ToInt32(strmFile.Length - 1)) As Byte
        '
        strmFile.Read(abyBuffer, 0, abyBuffer.Length)
        '
        '------------------------------------------------------------------
        ' para guardar sin el primer path
        'Dim sFile As String = strFile
        'Dim i As Integer = sFile.IndexOf("\")
        'If i > -1 Then
        '    sFile = sFile.Substring(i + 1).TrimStart
        'End If
        '------------------------------------------------------------------
        '
        '------------------------------------------------------------------
        ' para guardar sólo el nombre del fichero
        ' esto sólo se debe hacer si no se procesan directorios
        ' que puedan contener nombres repetidos
        'Dim sFile As String = Path.GetFileName(strFile)
        'Dim theEntry As ZipEntry = New ZipEntry(sFile)
        '------------------------------------------------------------------
        '
        ' se guarda con el path completo
        Dim theEntry As ZipEntry = New ZipEntry(NOM_ARHIVO & ".XML")
        '
        ' guardar la fecha y hora de la última modificación
        Dim fi As New FileInfo(NOM_ARHIVO & ".XML")
        theEntry.DateTime = fi.LastWriteTime
        'theEntry.DateTime = DateTime.Now
        '
        theEntry.Size = strmFile.Length
        strmFile.Close()
        objCrc32.Reset()
        objCrc32.Update(abyBuffer)
        theEntry.Crc = objCrc32.Value
        strmZipOutputStream.PutNextEntry(theEntry)
        strmZipOutputStream.Write(abyBuffer, 0, abyBuffer.Length)

        strmZipOutputStream.Finish()
        strmZipOutputStream.Close()
    End Sub

    Public Sub Descomprimir(ByVal RUTA As String,
                       Optional ByVal NOM_ARHIVO As String = "")
        ' descomprimir el contenido de zipFic en el directorio indicado.
        ' si zipFic no tiene la extensión .zip, se entenderá que es un directorio y
        ' se procesará el primer fichero .zip de ese directorio.
        ' si eliminar es True se eliminará ese fichero zip después de descomprimirlo.
        ' si renombrar es True se añadirá al final .descomprimido
        'If Not NOM_ARHIVO.ToLower.EndsWith(".ZIP") Then
        '    NOM_ARHIVO = Directory.GetFiles(NOM_ARHIVO, "*.ZIP")(0)
        'End If
        ' si no se ha indicado el directorio, usar el actual
        '
        Dim z As New ZipInputStream(File.OpenRead(RUTA & NOM_ARHIVO & ".ZIP"))  ''Baja de Factura
        Dim theEntry As ZipEntry
        Try
            Do
                theEntry = z.GetNextEntry()
                If Not theEntry Is Nothing Then
                    Dim fileName As String = theEntry.Name 'Path.GetFileName(theEntry.Name)
                    '
                    ' dará error si no existe el path
                    Dim streamWriter As FileStream
                    Try
                        If fileName <> "dummy/" Then
                            streamWriter = File.Create(RUTA & fileName)
                            Dim size As Integer
                            Dim data(2048) As Byte
                            Do
                                size = z.Read(data, 0, data.Length)
                                If (size > 0) Then
                                    streamWriter.Write(data, 0, size)
                                Else
                                    Exit Do
                                End If
                            Loop
                            streamWriter.Close()
                        End If
                    Catch ex As DirectoryNotFoundException
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName))
                        streamWriter = File.Create(fileName)
                    End Try
                Else
                    Exit Do
                End If
            Loop
            z.Close()
            File.Delete(RUTA & NOM_ARHIVO & ".ZIP")  ''Elimina la Baja Zip
        Catch ex As Exception
        End Try
    End Sub

    Public Function PrettyXML(XMLString As String) As String
        Dim sw As New StringWriter()
        Dim xw As New XmlTextWriter(sw)
        xw.Formatting = Formatting.Indented
        xw.Indentation = 4
        Dim doc As New XmlDocument
        doc.LoadXml(XMLString)
        doc.Save(xw)
        Return sw.ToString()
    End Function

End Module
