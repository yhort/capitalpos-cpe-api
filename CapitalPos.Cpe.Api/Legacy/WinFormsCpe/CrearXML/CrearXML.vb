Public Class CrearXML

    Public Function CPE(comprobante As BE.CPE, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim xml As String
            Dim doc As New XmlDocument()
            xml = "<?xml version='1.0' encoding='utf-8'?>
<Invoice xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' xmlns:ccts='urn:un:unece:uncefact:documentation:2' xmlns:ds='http://www.w3.org/2000/09/xmldsig#' xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2' xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2' xmlns:udt='urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2' xmlns='urn:oasis:names:specification:ubl:schema:xsd:Invoice-2'>
	<ext:UBLExtensions>
		<ext:UBLExtension>
			<ext:ExtensionContent>
			</ext:ExtensionContent>
		</ext:UBLExtension>
	</ext:UBLExtensions>
	<cbc:UBLVersionID>2.1</cbc:UBLVersionID>
	<cbc:CustomizationID schemeAgencyName='PE:SUNAT'>2.0</cbc:CustomizationID>
	<cbc:ProfileID schemeName='Tipo de Operacion' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo51'>" & comprobante.TIPO_OPERACION & "</cbc:ProfileID>
	<cbc:ID>" & comprobante.NRO_COMPROBANTE & "</cbc:ID>
	<cbc:IssueDate>" & comprobante.FECHA_DOCUMENTO & "</cbc:IssueDate>
	<cbc:IssueTime>00:00:00</cbc:IssueTime>
	<cbc:DueDate>" & comprobante.FECHA_VTO & "</cbc:DueDate>
	<cbc:InvoiceTypeCode listAgencyName='PE:SUNAT' listName='Tipo de Documento' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01' listID='" & comprobante.TIPO_OPERACION & "' name='Tipo de Operacion' listSchemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo51'>" & comprobante.COD_TIPO_DOCUMENTO & "</cbc:InvoiceTypeCode>"

            If (comprobante.TOTAL_LETRAS <> "") Then
                xml = xml & "<cbc:Note languageLocaleID='1000'>" & comprobante.TOTAL_LETRAS & "</cbc:Note>"
            End If
            'If (comprobante.TOTAL_DETRACCIONES > 0) Then
            '    xml = xml & "<cbc:Note languageLocaleID='2006'><![CDATA[Operacion sujeta a detraccion]]></cbc:Note>"
            'End If
            xml = xml & "<cbc:DocumentCurrencyCode listID='ISO 4217 Alpha' listName='Currency' listAgencyName='United Nations Economic Commission for Europe'>" & comprobante.COD_MONEDA & "</cbc:DocumentCurrencyCode>
            <cbc:LineCountNumeric>" & comprobante.detalle.Count & "</cbc:LineCountNumeric>"
            If (comprobante.NRO_OTR_COMPROBANTE <> "") Then
                xml = xml & "<cac:OrderReference>
                    <cbc:ID>" & comprobante.NRO_OTR_COMPROBANTE & "</cbc:ID>
            </cac:OrderReference>"
            End If

            If (comprobante.NRO_GUIA_REMISION <> "") Then
                xml = xml & "<cac:DespatchDocumentReference>
		    <cbc:ID>" & comprobante.NRO_GUIA_REMISION & "</cbc:ID>
		    <cbc:IssueDate>" & comprobante.FECHA_GUIA_REMISION & "</cbc:IssueDate>
		    <cbc:DocumentTypeCode listAgencyName='PE:SUNAT' listName='Tipo de Documento' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01'>" & comprobante.COD_GUIA_REMISION & "</cbc:DocumentTypeCode>
                </cac:DespatchDocumentReference>"
            End If


            xml = xml & "
            <cac:Signature>
		<cbc:ID>" & comprobante.NRO_COMPROBANTE & "</cbc:ID>
		<cac:SignatoryParty>
			<cac:PartyIdentification>
				<cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
			</cac:PartyIdentification>
			<cac:PartyName>
				<cbc:Name>" & comprobante.RAZON_SOCIAL_EMPRESA & "</cbc:Name>
			</cac:PartyName>
		</cac:SignatoryParty>
		<cac:DigitalSignatureAttachment>
			<cac:ExternalReference>
				<cbc:URI>#" & comprobante.NRO_COMPROBANTE & "</cbc:URI>
			</cac:ExternalReference>
		</cac:DigitalSignatureAttachment>
	</cac:Signature>

	<cac:AccountingSupplierParty>
		<cac:Party>
			<cac:PartyIdentification>
				<cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_EMPRESA & "' schemeName='Documento de Identidad' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
			</cac:PartyIdentification>
			<cac:PartyName>
				<cbc:Name><![CDATA[" & comprobante.NOMBRE_COMERCIAL_EMPRESA & "]]></cbc:Name>
			</cac:PartyName>

                        <cac:PartyLegalEntity>
                <cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]></cbc:RegistrationName>
                <cac:RegistrationAddress>
                    <!-- Elemento ID -->
                    <cbc:ID schemeName='Ubigeos' schemeAgencyName='PE:INEI'>" & comprobante.CODIGO_UBIGEO_EMPRESA & "</cbc:ID>

                    <!-- Código del tipo de dirección -->
                    <cbc:AddressTypeCode listAgencyName='PE:SUNAT' listName='Establecimientos anexos'>0000</cbc:AddressTypeCode>
        
                    <!-- Elementos de la dirección -->
                    <cbc:CityName><![CDATA[" & comprobante.DEPARTAMENTO_EMPRESA & "]]></cbc:CityName>
                    <cbc:CountrySubentity><![CDATA[" & comprobante.PROVINCIA_EMPRESA & "]]></cbc:CountrySubentity>
                    <cbc:District><![CDATA[" & comprobante.DISTRITO_EMPRESA & "]]></cbc:District>
        
                    <!-- Línea de dirección: Aquí es donde típicamente se coloca AddressLine -->
                    <cac:AddressLine>
                        <cbc:Line><![CDATA[" & comprobante.DIRECCION_EMPRESA & "]]></cbc:Line>
                    </cac:AddressLine>

                    <!-- Elemento Country -->
                    <cac:Country>
                        <cbc:IdentificationCode listID='ISO 3166-1' listAgencyName='United Nations Economic Commission for Europe' listName='Country'>" & comprobante.CODIGO_PAIS_EMPRESA & "</cbc:IdentificationCode>
                    </cac:Country>
                </cac:RegistrationAddress>
            </cac:PartyLegalEntity>  
		</cac:Party>
	</cac:AccountingSupplierParty>

	<cac:AccountingCustomerParty>
		<cac:Party>
			<cac:PartyIdentification>
				<cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_CLIENTE & "' schemeName='Documento de Identidad' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:ID>
			</cac:PartyIdentification>           
			<cac:PartyLegalEntity>
				<cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL_CLIENTE & "]]></cbc:RegistrationName>
				<cac:RegistrationAddress>					
					<cac:AddressLine>
						<cbc:Line><![CDATA[" & comprobante.DIRECCION_CLIENTE & "]]></cbc:Line>
					</cac:AddressLine>                                        					
				</cac:RegistrationAddress>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingCustomerParty>"

            'forma pago contado o credito en solo fact'


            If comprobante.TOTAL_DETRACCIONES > 0 Then

                xml = xml & "<cac:PaymentTerms>
                         <cbc:ID>Detraccion</cbc:ID>                         
                          <cbc:PaymentMeansID schemeName='Codigo de detraccion' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo54'>027</cbc:PaymentMeansID>
                        
                         <cbc:Amount currencyID= '" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_DETRACCIONES & "</cbc:Amount>
                         </cac:PaymentTerms>"

                'fin de froma de pago para fact'

            End If

            xml = xml & "<cac:PaymentTerms>
                    <cbc:ID>FormaPago</cbc:ID>
                    <cbc:PaymentMeansID>Contado</cbc:PaymentMeansID>
                    </cac:PaymentTerms>"



            If comprobante.TOTAL_DESCUENTOGLO > 0 Then
                xml = xml & "<cac:AllowanceCharge>
                       <cbc:ChargeIndicator>false</cbc:ChargeIndicator>
                       <cbc:AllowanceChargeReasonCode listName='Cargo/descuento' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo53'>00</cbc:AllowanceChargeReasonCode>
                       <cbc:Amount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_DESCUENTOGLO & "</cbc:Amount>
                       </cac:AllowanceCharge>"
            End If


            xml = xml & "<cac:TaxTotal>
		<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_IGV & "</cbc:TaxAmount>
		<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_GRAVADAS & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_IGV & "</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>1000</cbc:ID>
					<cbc:Name>IGV</cbc:Name>
					<cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"

            If (comprobante.TOTAL_ISC > 0) Then
                xml = xml & "<cac:TaxSubtotal>
            	<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_ISC & "</cbc:TaxableAmount>
            	<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_ISC & "</cbc:TaxAmount>
            	<cac:TaxCategory>
            		<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
            		<cac:TaxScheme>
            			<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>2000</cbc:ID>
            			<cbc:Name>ISC</cbc:Name>
            			<cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
            		</cac:TaxScheme>
            	</cac:TaxCategory>
            </cac:TaxSubtotal>"
            End If


            '//CAMPO NUEVO
            If (comprobante.TOTAL_EXPORTACION > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_EXPORTACION & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>G</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9995</cbc:ID>
					<cbc:Name>EXP</cbc:Name>
					<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_GRATUITAS > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_GRATUITAS & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>Z</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9996</cbc:ID>
					<cbc:Name>GRA</cbc:Name>
					<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_EXONERADAS > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_EXONERADAS & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>E</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9997</cbc:ID>
					<cbc:Name>EXO</cbc:Name>
					<cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_INAFECTA > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_INAFECTA & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>O</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9998</cbc:ID>
					<cbc:Name>INA</cbc:Name>
					<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_OTR_IMP > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_OTR_IMP & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_OTR_IMP & "</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9999</cbc:ID>
					<cbc:Name>OTR</cbc:Name>
					<cbc:TaxTypeCode>OTH</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            '//TOTAL=GRAVADA+IGV+EXONERADA
            '//NO ENTRA GRATUITA(INAFECTA) NI DESCUENTO
            '//SUB_TOTAL=PRECIO(SIN IGV) * CANTIDAD
            xml = xml & "</cac:TaxTotal>
            
	    <cac:LegalMonetaryTotal>
            
            <cbc:LineExtensionAmount  currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.SUB_TOTAL & "</cbc:LineExtensionAmount >
            <cbc:TaxInclusiveAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL & "</cbc:TaxInclusiveAmount>
		    <cbc:PayableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL & "</cbc:PayableAmount>
	    </cac:LegalMonetaryTotal>"


            For x As Integer = 0 To comprobante.detalle.Count - 1
                If comprobante.detalle(x).COD_TIPO_OPERACION = "10" Or comprobante.detalle(x).COD_TIPO_OPERACION = "40" Then
                    xml = xml & "<cac:InvoiceLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:InvoicedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:InvoicedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_CONIGV & "</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>" & comprobante.detalle(x).PRECIO_TIPO_CODIGO & "</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>


		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
					            <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='Afectacion del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>1000</cbc:ID>
						            <cbc:Name>IGV</cbc:Name>
						            <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"
                    'PRECIO

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_SIN_IMPUESTO & "</cbc:PriceAmount>
		            </cac:Price>
	            </cac:InvoiceLine>"
                ElseIf comprobante.detalle(x).COD_TIPO_OPERACION = "20" Then 'EXONERADAS
                    xml = xml & "<cac:InvoiceLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:InvoicedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:InvoicedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_SIN_IMPUESTO & "</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>01</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>
		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>E</cbc:ID>
					            <cbc:Percent>0.00</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigo de Tipo de Afectación del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>9997</cbc:ID>
						            <cbc:Name>EXO</cbc:Name>
						            <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_SIN_IMPUESTO & "</cbc:PriceAmount>
		            </cac:Price>
	            </cac:InvoiceLine>"
                ElseIf comprobante.detalle(x).COD_TIPO_OPERACION = "30" Then 'INAFECTO
                    xml = xml & "<cac:InvoiceLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:InvoicedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:InvoicedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO & "</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>02</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>
		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>O</cbc:ID>
					            <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigo de Tipo de Afectación del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>9998</cbc:ID>
						            <cbc:Name>INA</cbc:Name>
						            <cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:PriceAmount>
		            </cac:Price>
	            </cac:InvoiceLine>"
                ElseIf comprobante.detalle(x).COD_TIPO_OPERACION = "31" Then 'GRATUITAS
                    xml = xml & "<cac:InvoiceLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:InvoicedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:InvoicedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>02</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>
		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>Z</cbc:ID>
					            <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigo de Tipo de Afectación del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>9996</cbc:ID>
						            <cbc:Name>GRA</cbc:Name>
						            <cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:PriceAmount>
		            </cac:Price>
	            </cac:InvoiceLine>"
                End If
            Next
            xml = xml & "</Invoice>"

            doc.LoadXml(xml)
            doc.Save(ruta & nomArchivo & ".XML")


            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function

    Public Function CPE_NC(comprobante As BE.CPE, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim xml As String
            Dim doc As New XmlDocument()
            xml = "<?xml version='1.0' encoding='UTF-8'?>
<CreditNote xmlns='urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2' xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' xmlns:ccts='urn:un:unece:uncefact:documentation:2' xmlns:ds='http://www.w3.org/2000/09/xmldsig#' xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2' xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2' xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1' xmlns:udt='urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
    <ext:UBLExtensions>
        <ext:UBLExtension>
            <ext:ExtensionContent>
            </ext:ExtensionContent>
        </ext:UBLExtension>
    </ext:UBLExtensions>
    <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
    <cbc:CustomizationID>2.0</cbc:CustomizationID>
    <cbc:ID>" & comprobante.NRO_COMPROBANTE & "</cbc:ID>
    <cbc:IssueDate>" & comprobante.FECHA_DOCUMENTO & "</cbc:IssueDate>
    <cbc:IssueTime>00:00:00</cbc:IssueTime>
    <cbc:DocumentCurrencyCode>" & comprobante.COD_MONEDA & "</cbc:DocumentCurrencyCode>
    <cac:DiscrepancyResponse>
        <cbc:ReferenceID>" & comprobante.NRO_DOCUMENTO_MODIFICA & "</cbc:ReferenceID>
        <cbc:ResponseCode>" & comprobante.COD_TIPO_MOTIVO & "</cbc:ResponseCode>
        <cbc:Description><![CDATA[" & comprobante.DESCRIPCION_MOTIVO & "]]></cbc:Description>
    </cac:DiscrepancyResponse>
    <cac:BillingReference>
        <cac:InvoiceDocumentReference>
            <cbc:ID>" & comprobante.NRO_DOCUMENTO_MODIFICA & "</cbc:ID>
            <cbc:DocumentTypeCode>" & comprobante.TIPO_COMPROBANTE_MODIFICA & "</cbc:DocumentTypeCode>
        </cac:InvoiceDocumentReference>
    </cac:BillingReference>
    <cac:Signature>
        <cbc:ID>IDSignST</cbc:ID>
        <cac:SignatoryParty>
            <cac:PartyIdentification>
                <cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
                <cbc:Name><![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]></cbc:Name>
            </cac:PartyName>
        </cac:SignatoryParty>
        <cac:DigitalSignatureAttachment>
            <cac:ExternalReference>
                <cbc:URI>#SignatureSP</cbc:URI>
            </cac:ExternalReference>
        </cac:DigitalSignatureAttachment>
    </cac:Signature>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PartyIdentification>
                <cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_EMPRESA & "' schemeName='SUNAT:Identificador de Documento de Identidad' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
                <cbc:Name><![CDATA[" & comprobante.NOMBRE_COMERCIAL_EMPRESA & "]]></cbc:Name>
            </cac:PartyName>
            <cac:PartyLegalEntity>
<cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]></cbc:RegistrationName>
                <cac:RegistrationAddress>
                    <cbc:AddressTypeCode>0001</cbc:AddressTypeCode>
                </cac:RegistrationAddress>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:AccountingCustomerParty>
        <cac:Party>
            <cac:PartyIdentification>
                <cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_CLIENTE & "' schemeName='SUNAT:Identificador de Documento de Identidad' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL_CLIENTE & "]]></cbc:RegistrationName>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingCustomerParty>


            <cac:TaxTotal>
                <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_IGV & "</cbc:TaxAmount>
                <cac:TaxSubtotal>
        <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_GRAVADAS & "</cbc:TaxableAmount>
        <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_IGV & "</cbc:TaxAmount>
                    <cac:TaxCategory>
                        <cac:TaxScheme>
                            <cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>1000</cbc:ID>
                            <cbc:Name>IGV</cbc:Name>
                            <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
                        </cac:TaxScheme>
                    </cac:TaxCategory>
                </cac:TaxSubtotal>"

            If (comprobante.TOTAL_ISC > 0) Then
                xml = xml & "<cac:TaxSubtotal>
            	<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_ISC & "</cbc:TaxableAmount>
            	<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_ISC & "</cbc:TaxAmount>
            	<cac:TaxCategory>
            		<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
            		<cac:TaxScheme>
            			<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>2000</cbc:ID>
            			<cbc:Name>ISC</cbc:Name>
            			<cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
            		</cac:TaxScheme>
            	</cac:TaxCategory>
            </cac:TaxSubtotal>"
            End If
            '//CAMPO NUEVO
            If (comprobante.TOTAL_EXPORTACION > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_EXPORTACION & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>G</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9995</cbc:ID>
					<cbc:Name>EXP</cbc:Name>
					<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_GRATUITAS > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_GRATUITAS & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>Z</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9996</cbc:ID>
					<cbc:Name>GRA</cbc:Name>
					<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_EXONERADAS > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_EXONERADAS & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>E</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9997</cbc:ID>
					<cbc:Name>EXO</cbc:Name>
					<cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_INAFECTA > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_INAFECTA & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>O</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9998</cbc:ID>
					<cbc:Name>INA</cbc:Name>
					<cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If
            If (comprobante.TOTAL_OTR_IMP > 0) Then
                xml = xml & "<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_OTR_IMP & "</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_OTR_IMP & "</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
				<cac:TaxScheme>
					<cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>9999</cbc:ID>
					<cbc:Name>OTR</cbc:Name>
					<cbc:TaxTypeCode>OTH</cbc:TaxTypeCode>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>"
            End If

            xml = xml & "</cac:TaxTotal>
    <cac:LegalMonetaryTotal>
         <cbc:LineExtensionAmount  currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.SUB_TOTAL & "</cbc:LineExtensionAmount >
         <cbc:TaxInclusiveAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL & "</cbc:TaxInclusiveAmount>
        <cbc:PayableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL & "</cbc:PayableAmount>
    </cac:LegalMonetaryTotal>"

            For x As Integer = 0 To comprobante.detalle.Count - 1
                If comprobante.detalle(x).COD_TIPO_OPERACION = "10" Or comprobante.detalle(x).COD_TIPO_OPERACION = "40" Then
                    xml = xml & "<cac:CreditNoteLine>
        <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
<cbc:CreditedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "'  unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:CreditedQuantity>
<cbc:LineExtensionAmount currencyID ='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:LineExtensionAmount>
        <cac:PricingReference>
            <cac:AlternativeConditionPrice>
                <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_CONIGV & "</cbc:PriceAmount>
                <cbc:PriceTypeCode>" & comprobante.detalle(x).PRECIO_TIPO_CODIGO & "</cbc:PriceTypeCode>
            </cac:AlternativeConditionPrice>
        </cac:PricingReference>

        <cac:TaxTotal>
        <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
            <cac:TaxSubtotal>
        <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
        <cbc:TaxAmount currencyID ='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
                <cac:TaxCategory>
                    <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
                    <cbc:TaxExemptionReasonCode>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                    <cac:TaxScheme>
                        <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>1000</cbc:ID>
                        <cbc:Name>IGV</cbc:Name>
                        <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
                    </cac:TaxScheme>
                </cac:TaxCategory>
            </cac:TaxSubtotal>
        </cac:TaxTotal>

        <cac:Item>
        <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
                    <cac:SellersItemIdentification>
                        <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
                    </cac:SellersItemIdentification>
                </cac:Item>
                <cac:Price>
        <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_SIN_IMPUESTO & "</cbc:PriceAmount>
                </cac:Price>
            </cac:CreditNoteLine>"
                ElseIf comprobante.detalle(x).COD_TIPO_OPERACION = "20" Then 'EXONERADAS
                    xml = xml & "<cac:CreditNoteLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:CreditedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:CreditedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_SIN_IMPUESTO & "</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>01</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>
		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0.00</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>E</cbc:ID>
					            <cbc:Percent>0.00</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigo de Tipo de Afectación del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>9997</cbc:ID>
						            <cbc:Name>EXO</cbc:Name>
						            <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO_SIN_IMPUESTO & "</cbc:PriceAmount>
		            </cac:Price>
	            </cac:CreditNoteLine>"
                ElseIf comprobante.detalle(x).COD_TIPO_OPERACION = "30" Then 'INAFECTO
                    xml = xml & "<cac:CreditNoteLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:CreditedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:CreditedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO & "</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>02</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>
		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>E</cbc:ID>
					            <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigo de Tipo de Afectación del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>9998</cbc:ID>
						            <cbc:Name>INA</cbc:Name>
						            <cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:PriceAmount>
		            </cac:Price>
	            </cac:CreditNoteLine>"
                ElseIf comprobante.detalle(x).COD_TIPO_OPERACION = "31" Then 'GRATUITAS
                    xml = xml & "<cac:CreditNoteLine>
		            <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
		            <cbc:CreditedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>" & comprobante.detalle(x).CANTIDAD & "</cbc:CreditedQuantity>
		            <cbc:LineExtensionAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:LineExtensionAmount>
		            <cac:PricingReference>
			            <cac:AlternativeConditionPrice>
				            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:PriceAmount>
				            <cbc:PriceTypeCode listName='Tipo de Precio' listAgencyName='PE:SUNAT' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo16'>02</cbc:PriceTypeCode>
			            </cac:AlternativeConditionPrice>
		            </cac:PricingReference>
		            <cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
			            <cac:TaxSubtotal>
				            <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
				            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:TaxAmount>
				            <cac:TaxCategory>
					            <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>E</cbc:ID>
					            <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
					            <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigo de Tipo de Afectación del IGV' listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
					            <cac:TaxScheme>
						            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier' schemeAgencyName='United Nations Economic Commission for Europe'>9996</cbc:ID>
						            <cbc:Name>GRA</cbc:Name>
						            <cbc:TaxTypeCode>FRE</cbc:TaxTypeCode>
					            </cac:TaxScheme>
				            </cac:TaxCategory>
			            </cac:TaxSubtotal>
		            </cac:TaxTotal>"

                    'ver este video minuto 8 (para comprender isc)
                    'https://www.youtube.com/watch?v=GaVSoGkJ6fs
                    'isc=(base_imponible * %isc)/100
                    'base imponible igv=base_imponible+isc
                    'igv=(base_imponible_igv*18)/100
                    'H. Catálogo No. 08: Códigos de Tipos de Sistema de Cálculo del ISC
                    If comprobante.detalle(x).ISC > 0 Then
                        xml = xml & "<cac:TaxSubtotal>
                                    <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).BI_ISC & "</cbc:TaxableAmount>
                                    <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
                                    <cac:TaxCategory>
                                        <cbc:ID schemeID='UN/ECE 5305' schemeName='Tax Category Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>S</cbc:ID>
                                        <cbc:Percent>" & comprobante.detalle(x).POR_ISC & "</cbc:Percent>
                                        <cbc:TaxExemptionReasonCode listAgencyName='PE:SUNAT' listName='SUNAT:Codigode Tipo de Afectación del IGV'listURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07'>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                                        <cbc:TierRange>" & comprobante.detalle(x).TIPO_ISC & "</cbc:TierRange>
                                        <cac:TaxScheme>
                                            <cbc:ID schemeID='UN/ECE 5153' schemeName='Tax Scheme Identifier'schemeAgencyName='United Nations Economic Commission for Europe'>2000</cbc:ID>
                                            <cbc:Name>ISC</cbc:Name>
                                            <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                                        </cac:TaxScheme>
                                    </cac:TaxCategory>
                                </cac:TaxSubtotal>
                            </cac:TaxTotal>"
                    End If

                    xml = xml & "<cac:Item>
			            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
			            <cac:SellersItemIdentification>
				            <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
			            </cac:SellersItemIdentification>
		            </cac:Item>
		            <cac:Price>
			            <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>0</cbc:PriceAmount>
		            </cac:Price>
	            </cac:CreditNoteLine>"
                End If
            Next
            xml = xml & "</CreditNote>"

            doc.LoadXml(xml)
            doc.Save(ruta & nomArchivo & ".XML")

            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary

    End Function


    Public Function CPE_ND(comprobante As BE.CPE, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim xml As String
            Dim doc As New XmlDocument()
            xml = "<?xml version='1.0' encoding='UTF-8'?>
<DebitNote xmlns='urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2' xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' xmlns:ccts='urn:un:unece:uncefact:documentation:2' xmlns:ds='http://www.w3.org/2000/09/xmldsig#' xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2' xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2' xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1' xmlns:udt='urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
    <ext:UBLExtensions>
        <ext:UBLExtension>
            <ext:ExtensionContent>
            </ext:ExtensionContent>
        </ext:UBLExtension>
    </ext:UBLExtensions>
    <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
    <cbc:CustomizationID>2.0</cbc:CustomizationID>
    <cbc:ID>" & comprobante.NRO_COMPROBANTE & "</cbc:ID>
    <cbc:IssueDate>" & comprobante.FECHA_DOCUMENTO & "</cbc:IssueDate>
    <cbc:IssueTime>00:00:00</cbc:IssueTime>
    <cbc:DocumentCurrencyCode>" & comprobante.COD_MONEDA & "</cbc:DocumentCurrencyCode>
    <cac:DiscrepancyResponse>
        <cbc:ReferenceID>" & comprobante.NRO_DOCUMENTO_MODIFICA & "</cbc:ReferenceID>
        <cbc:ResponseCode>" & comprobante.COD_TIPO_MOTIVO & "</cbc:ResponseCode>
        <cbc:Description><![CDATA[" & comprobante.DESCRIPCION_MOTIVO & "]]></cbc:Description>
    </cac:DiscrepancyResponse>
    <cac:BillingReference>
        <cac:InvoiceDocumentReference>
            <cbc:ID>" & comprobante.NRO_DOCUMENTO_MODIFICA & "</cbc:ID>
            <cbc:DocumentTypeCode>" & comprobante.TIPO_COMPROBANTE_MODIFICA & "</cbc:DocumentTypeCode>
        </cac:InvoiceDocumentReference>
    </cac:BillingReference>
    <cac:Signature>
        <cbc:ID>IDSignST</cbc:ID>
        <cac:SignatoryParty>
            <cac:PartyIdentification>
                <cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
                <cbc:Name><![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]></cbc:Name>
            </cac:PartyName>
        </cac:SignatoryParty>
        <cac:DigitalSignatureAttachment>
            <cac:ExternalReference>
                <cbc:URI>#SignatureSP</cbc:URI>
            </cac:ExternalReference>
        </cac:DigitalSignatureAttachment>
    </cac:Signature>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PartyIdentification>
                <cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_EMPRESA & "' schemeName='SUNAT:Identificador de Documento de Identidad' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyName>
                <cbc:Name><![CDATA[" & comprobante.NOMBRE_COMERCIAL_EMPRESA & "']]></cbc:Name>
            </cac:PartyName>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]></cbc:RegistrationName>
                <cac:RegistrationAddress>
                    <cbc:AddressTypeCode>0001</cbc:AddressTypeCode>
                </cac:RegistrationAddress>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:AccountingCustomerParty>
        <cac:Party>
            <cac:PartyIdentification>
                <cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_CLIENTE & "' schemeName='SUNAT:Identificador de Documento de Identidad' schemeAgencyName='PE:SUNAT' schemeURI='urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06'>" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:ID>
            </cac:PartyIdentification>
            <cac:PartyLegalEntity>
<cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL_CLIENTE & "]]></cbc:RegistrationName>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingCustomerParty>
    <cac:TaxTotal>
        <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_IGV & "</cbc:TaxAmount>
        <cac:TaxSubtotal>
<cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_GRAVADAS & "</cbc:TaxableAmount>
            <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL_IGV & "</cbc:TaxAmount>
            <cac:TaxCategory>
                <cac:TaxScheme>
                    <cbc:ID schemeID='UN/ECE 5153' schemeAgencyID='6'>1000</cbc:ID>
                    <cbc:Name>IGV</cbc:Name>
                    <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
                </cac:TaxScheme>
            </cac:TaxCategory>
        </cac:TaxSubtotal>
    </cac:TaxTotal>
    <cac:RequestedMonetaryTotal>
<cbc:PayableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.TOTAL & "</cbc:PayableAmount>
    </cac:RequestedMonetaryTotal>"

            For x As Integer = 0 To comprobante.detalle.Count - 1

                xml = xml & "<cac:DebitNoteLine>
        <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
        <cbc:DebitedQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "'>" & comprobante.detalle(x).CANTIDAD & "</cbc:DebitedQuantity>
        <cbc:LineExtensionAmount currencyID ='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:LineExtensionAmount>
        <cac:PricingReference>
            <cac:AlternativeConditionPrice>
                <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO & "</cbc:PriceAmount>
                <cbc:PriceTypeCode>" & comprobante.detalle(x).PRECIO_TIPO_CODIGO & "</cbc:PriceTypeCode>
            </cac:AlternativeConditionPrice>
        </cac:PricingReference>
        <cac:TaxTotal>
        <cbc:TaxAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
            <cac:TaxSubtotal>
                <cbc:TaxableAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IMPORTE & "</cbc:TaxableAmount>
                <cbc:TaxAmount currencyID ='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
                <cac:TaxCategory>
                    <cbc:Percent>" & comprobante.POR_IGV & "</cbc:Percent>
                    <cbc:TaxExemptionReasonCode>" & comprobante.detalle(x).COD_TIPO_OPERACION & "</cbc:TaxExemptionReasonCode>
                    <cac:TaxScheme>
                        <cbc:ID>1000</cbc:ID>
                        <cbc:Name>IGV</cbc:Name>
                        <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
                    </cac:TaxScheme>
                </cac:TaxCategory>
            </cac:TaxSubtotal>
        </cac:TaxTotal>		
        <cac:Item>
            <cbc:Description><![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]></cbc:Description>
            <cac:SellersItemIdentification>
                    <cbc:ID><![CDATA[" & comprobante.detalle(x).CODIGO & "]]></cbc:ID>
            </cac:SellersItemIdentification>
        </cac:Item>
        <cac:Price>
        <cbc:PriceAmount currencyID='" & comprobante.COD_MONEDA & "'>" & comprobante.detalle(x).PRECIO & "</cbc:PriceAmount>
        </cac:Price>
        </cac:DebitNoteLine>"
            Next

            xml = xml & "</DebitNote>"

            doc.LoadXml(xml)
            doc.Save(ruta & nomArchivo & ".XML")
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function

    'Public Function CPE_GUIA_REMISION(comprobante As BE.CPE_GUIA_REMISION, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
    '    Dim dictionary As Dictionary(Of String, String)
    '    Try
    '        Dim xml As String
    '        Dim doc As New XmlDocument()
    '        xml = "<?xml version='1.0' encoding='iso-8859-1'?>
    '                <DespatchAdvice
    '                    xmlns:ds='http://www.w3.org/2000/09/xmldsig#'
    '                    xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2'
    '                    xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2'
    '                    xmlns:ccts='urn:un:unece:uncefact:documentation:2'
    '                    xmlns:xsd='http://www.w3.org/2001/XMLSchema'
    '                    xmlns:udt='urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2'
    '                    xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2'
    '                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
    '                    xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2'
    '                    xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1'
    '                    xmlns='urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2'>
    '                    <ext:UBLExtensions>
    '                        <ext:UBLExtension>
    '                            <ext:ExtensionContent>
    '                            </ext:ExtensionContent>
    '                        </ext:UBLExtension>
    '                    </ext:UBLExtensions>

    '                    <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
    '                    <cbc:CustomizationID>1.0</cbc:CustomizationID>
    '                    <cbc:ID>" & comprobante.NRO_COMPROBANTE & "</cbc:ID>
    '                    <cbc:IssueDate>" & comprobante.FECHA_DOCUMENTO & "</cbc:IssueDate>
    '                    <cbc:DespatchAdviceTypeCode>" & comprobante.COD_TIPO_DOCUMENTO & "</cbc:DespatchAdviceTypeCode>
    '                    <cbc:Note><![CDATA[" & comprobante.NOTA & "]]></cbc:Note>

    '                    <cac:Signature>
    '                        <cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
    '                  <cac:SignatoryParty>
    '                   <cac:PartyIdentification>
    '                    <cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
    '                   </cac:PartyIdentification>
    '                   <cac:PartyName>
    '                    <cbc:Name>" & comprobante.RAZON_SOCIAL_EMPRESA & "</cbc:Name>
    '                   </cac:PartyName>
    '                  </cac:SignatoryParty>
    '                  <cac:DigitalSignatureAttachment>
    '                   <cac:ExternalReference>
    '                    <cbc:URI>#" & comprobante.NRO_COMPROBANTE & "</cbc:URI>
    '                   </cac:ExternalReference>
    '                  </cac:DigitalSignatureAttachment>
    '                    <cac:Signature>

    '                    <cac:DespatchSupplierParty>
    '                        <cbc:CustomerAssignedAccountID schemeID='" & comprobante.TIPO_DOCUMENTO_EMPRESA & "'>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:CustomerAssignedAccountID>
    '                        <cac:Party>
    '                            <cac:PartyName>
    '                                <cbc:Name>" & comprobante.RAZON_SOCIAL_EMPRESA & "</cbc:Name>
    '                            </cac:PartyName>
    '                            <cac:PartyLegalEntity>
    '                                <cbc:RegistrationName>
    '                                    <![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]>
    '                                </cbc:RegistrationName>
    '                            </cac:PartyLegalEntity>
    '                        </cac:Party>
    '                    </cac:DespatchSupplierParty>

    '                    <cac:DeliveryCustomerParty>
    '                        <cbc:CustomerAssignedAccountID schemeID='" & comprobante.TIPO_DOCUMENTO_CLIENTE & "'>" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:CustomerAssignedAccountID>
    '                        <cac:Party>
    '                            <cac:PartyLegalEntity>
    '                                <cbc:RegistrationName>
    '                                    <![CDATA[" & comprobante.RAZON_SOCIAL_CLIENTE & "]]>
    '                                </cbc:RegistrationName>
    '                            </cac:PartyLegalEntity>
    '                        </cac:Party>
    '                    </cac:DeliveryCustomerParty>

    '                    <cac:Shipment>
    '                        <cbc:ID>" & comprobante.ITEM_ENVIO & "</cbc:ID>
    '                        <cbc:HandlingCode>" & comprobante.COD_MOTIVO_TRASLADO & "</cbc:HandlingCode>
    '                        <cbc:Information>" & comprobante.DESCRIPCION_MOTIVO_TRASLADO & "</cbc:Information>
    '                        <cbc:GrossWeightMeasure unitCode='" & comprobante.COD_UND_PESO_BRUTO & "'>" & comprobante.PESO_BRUTO & "</cbc:GrossWeightMeasure>
    '                        <cbc:TotalTransportHandlingUnitQuantity>" & comprobante.TOTAL_BULTOS & "</cbc:TotalTransportHandlingUnitQuantity>

    '                        <cac:ShipmentStage>
    '                            <cbc:TransportModeCode>" & comprobante.COD_MODALIDAD_TRASLADO & "</cbc:TransportModeCode>
    '                            <cac:TransitPeriod>
    '                                <cbc:StartDate>" & comprobante.FECHA_INICIO & "</cbc:StartDate>
    '                            </cac:TransitPeriod>

    '                            <cac:CarrierParty>
    '                                <cac:PartyIdentification>
    '                                    <cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_TRANSPORTISTA & "'>" & comprobante.NRO_DOCUMENTO_TRANSPORTISTA & "</cbc:ID>
    '                                </cac:PartyIdentification>
    '                                <cac:PartyName>
    '                                    <cbc:Name>
    '                                        <![CDATA[" & comprobante.RAZON_SOCIAL_TRANSPORTISTA & "]]>
    '                                    </cbc:Name>
    '                                </cac:PartyName>
    '                            </cac:CarrierParty>

    '                        <cac:TransportMeans>
    '                            <cac:RoadTransport>
    '                                <cbc:LicensePlateID>" & comprobante.PLACA_VEHICULO & "</cbc:LicensePlateID>
    '                            </cac:RoadTransport>
    '                        </cac:TransportMeans>

    '                        <cac:DriverPerson>
    '                            <cbc:ID schemeID='" & comprobante.COD_TIPO_DOC_CHOFER & "'>" & comprobante.NRO_DOC_CHOFER & "</cbc:ID>
    '                            <cbc:FirstName>""</cbc:FirstName>
    '                        </cac:DriverPerson>

    '                     </cac:ShipmentStage>
    '                        <cac:Delivery>
    '                            <cac:DeliveryAddress>
    '                                <cbc:ID>" & comprobante.COD_UBIGEO_DESTINO & "</cbc:ID>
    '                                <cbc:StreetName>" & comprobante.DIRECCION_DESTINO & "</cbc:StreetName>
    '                            </cac:DeliveryAddress>
    '                        </cac:Delivery>"

    '        If comprobante.PLACA_CARRETA <> "" Then
    '            xml = xml & "<cac:TransportHandlingUnit>
    '                            <cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID>
    '                            <cac:TransportEquipment>
    '                                <cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID>
    '                            </cac:TransportEquipment>
    '                        </cac:TransportHandlingUnit>"
    '        End If

    '        xml = xml & "<cac:OriginAddress>
    '                            <cbc:ID>" & comprobante.COD_UBIGEO_ORIGEN & "</cbc:ID>
    '                            <cbc:StreetName>" & comprobante.DIRECCION_ORIGEN & "</cbc:StreetName>
    '                        </cac:OriginAddress>
    '                    </cac:Shipment>"

    '        For x As Integer = 0 To comprobante.detalle.Count - 1
    '            xml = xml & "<cac:DespatchLine>
    '                                    <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
    '                                    <cbc:DeliveredQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "'>" & comprobante.detalle(x).CANTIDAD & "</cbc:DeliveredQuantity>
    '                                    <cac:OrderLineReference>
    '                                        <cbc:LineID>" & comprobante.detalle(x).ORDER_ITEM & "</cbc:LineID>
    '                                    </cac:OrderLineReference>
    '                                    <cac:Item>
    '                                        <cbc:Name>
    '                                            <![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]>
    '                                        </cbc:Name>
    '                                        <cac:SellersItemIdentification>
    '                                            <cbc:ID>" & comprobante.detalle(x).CODIGO & "</cbc:ID>
    '                                        </cac:SellersItemIdentification>
    '                                    </cac:Item>
    '                                </cac:DespatchLine>"
    '        Next

    '        xml = xml & "</DespatchAdvice>"

    '        doc.LoadXml(xml)
    '        doc.Save(ruta & nomArchivo & ".XML")
    '        dictionary = New Dictionary(Of String, String)
    '        dictionary.Add("flg_rta", "1")
    '        dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
    '        'MsgBox("El XML de la guia se Creó Correctamente. En " & ruta, MsgBoxStyle.Information, "Generacion de Xml")
    '        dictionary.Add("cod_sunat", "")
    '        dictionary.Add("msj_sunat", "")
    '        dictionary.Add("hash_cdr", "")
    '        dictionary.Add("hash_cpe", "")
    '    Catch ex As Exception
    '        dictionary = New Dictionary(Of String, String)
    '        dictionary.Add("flg_rta", "0")
    '        dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
    '        MsgBox("Error al Generar el Archivo XML de la Guia " & ex.Message, MsgBoxStyle.Exclamation, "Generacion de Xml")
    '        dictionary.Add("cod_sunat", "")
    '        dictionary.Add("msj_sunat", "")
    '        dictionary.Add("hash_cdr", "")
    '        dictionary.Add("hash_cpe", "")
    '    End Try
    '    Return dictionary
    'End Function


    Public Function CPE_GUIA_REMISION(comprobante As BE.CPE_GUIA_REMISION, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As New Dictionary(Of String, String)
        Try
            Dim xml As New StringBuilder()

            'Iniciamos el Encabezado XML
            xml.AppendLine("<?xml version='1.0' encoding='utf-8'?>")
            xml.AppendLine("<DespatchAdvice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">")
            xml.AppendLine("<ext:UBLExtensions>")
            xml.AppendLine("<ext:UBLExtension>")
            xml.AppendLine("<ext:ExtensionContent></ext:ExtensionContent>")
            xml.AppendLine("</ext:UBLExtension>")
            xml.AppendLine("</ext:UBLExtensions>")
            xml.AppendLine("<cbc:UBLVersionID>2.1</cbc:UBLVersionID>")
            xml.AppendLine("<cbc:CustomizationID>2.0</cbc:CustomizationID>")
            xml.AppendLine($"<cbc:ID>{comprobante.NRO_COMPROBANTE}</cbc:ID>")
            xml.AppendLine($"<cbc:IssueDate>{comprobante.FECHA_DOCUMENTO}</cbc:IssueDate>")
            xml.AppendLine($"<cbc:IssueTime>{DateTime.Now:HH:mm:ss}</cbc:IssueTime>")
            xml.AppendLine("<cbc:DespatchAdviceTypeCode>09</cbc:DespatchAdviceTypeCode>")
            xml.AppendLine($"<cbc:Note><![CDATA[{comprobante.NOTA}]]></cbc:Note>")

            ' Comprobando si la lista de documentos de referencia existe y tiene elementos.
            If comprobante.ListaDocsReferencia IsNot Nothing AndAlso comprobante.ListaDocsReferencia.Count > 0 Then
                ' Si la lista no está vacía, recorre cada documento y genera su bloque XML.
                ' Añadiendo Documento de Referencia-añadiendo bucle foreach
                For Each docRef As BE.DocumentoReferencia In comprobante.ListaDocsReferencia
                    xml.AppendLine($"<cac:AdditionalDocumentReference>")
                    xml.AppendLine($"<cbc:ID>{docRef.ID_DOC_REF}</cbc:ID>")
                    xml.AppendLine($"<cbc:DocumentTypeCode>{docRef.TipoDocumento_ref}</cbc:DocumentTypeCode>")
                    xml.AppendLine($"<cbc:DocumentType>{docRef.NombreDocumento_ref}</cbc:DocumentType>")
                    xml.AppendLine($"<cac:IssuerParty>")
                    xml.AppendLine($"<cac:PartyIdentification>")
                    xml.AppendLine($"<cbc:ID schemeID=""6"" schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">{comprobante.NRO_DOCUMENTO_EMPRESA}</cbc:ID>")
                    xml.AppendLine($"</cac:PartyIdentification>")
                    xml.AppendLine($"</cac:IssuerParty>")
                    xml.AppendLine($"</cac:AdditionalDocumentReference>")
                Next
            End If

            'Firma de la guia
            xml.AppendLine($"<cac:Signature><cbc:ID>{comprobante.NRO_DOCUMENTO_EMPRESA}</cbc:ID>")
            xml.AppendLine("<cac:SignatoryParty><cac:PartyIdentification><cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID></cac:PartyIdentification></cac:SignatoryParty>")
            xml.AppendLine($"<cac:DigitalSignatureAttachment><cac:ExternalReference><cbc:URI>{comprobante.NRO_DOCUMENTO_EMPRESA}</cbc:URI></cac:ExternalReference></cac:DigitalSignatureAttachment></cac:Signature>")

            ' Supplier Party
            xml.AppendLine("<cac:DespatchSupplierParty><cac:Party><cac:PartyIdentification><cbc:ID schemeID=""6"" schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID></cac:PartyIdentification>")
            xml.AppendLine($"<cac:PartyName><cbc:Name><![CDATA[{comprobante.RAZON_SOCIAL_EMPRESA}]]></cbc:Name></cac:PartyName>")
            xml.AppendLine($"<cac:PartyLegalEntity><cbc:RegistrationName><![CDATA[{comprobante.RAZON_SOCIAL_EMPRESA}]]></cbc:RegistrationName></cac:PartyLegalEntity>")
            xml.AppendLine("</cac:Party></cac:DespatchSupplierParty>")

            ' Customer Party
            xml.AppendLine("<cac:DeliveryCustomerParty><cac:Party><cac:PartyIdentification><cbc:ID schemeID=""" & comprobante.TIPO_DOCUMENTO_CLIENTE & """ schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:ID></cac:PartyIdentification>")
            xml.AppendLine($"<cac:PartyLegalEntity><cbc:RegistrationName><![CDATA[{comprobante.RAZON_SOCIAL_CLIENTE}]]></cbc:RegistrationName></cac:PartyLegalEntity>")
            xml.AppendLine("</cac:Party></cac:DeliveryCustomerParty>")

            ' Shipment , CONSIDERAR EN REEMPLAZO DE 01 , EL CODIGO DEL MOTIVO DE TRASALDO (VENTA 01- COMPRAS 02 - TRASLADO ENTRE EST 04 CATALOG 20
            xml.AppendLine("<cac:Shipment><cbc:ID>SUNAT_Envio</cbc:ID>")
            xml.AppendLine($"<cbc:HandlingCode listAgencyName=""PE:SUNAT"" listName=""Motivo de traslado"" listURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo20"">" & comprobante.COD_MOTIVO_TRASLADO & "</cbc:HandlingCode>")
            xml.AppendLine($"<cbc:GrossWeightMeasure unitCode=""{comprobante.COD_UND_PESO_BRUTO}"">{comprobante.PESO_BRUTO}</cbc:GrossWeightMeasure>")

            If comprobante.COD_MOTIVO_TRASLADO = 7 Then
                xml.AppendLine($"<cbc:TotalTransportHandlingUnitQuantity>{comprobante.TOTAL_BULTOS}</cbc:TotalTransportHandlingUnitQuantity>")
            End If

            xml.AppendLine("<cac:ShipmentStage><cbc:ID>1</cbc:ID>")
            xml.AppendLine($"<cbc:TransportModeCode listAgencyName=""PE:SUNAT"" listName=""Modalidad de traslado"" listURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo18"">" & comprobante.COD_MODALIDAD_TRASLADO & "</cbc:TransportModeCode>")
            xml.AppendLine("<cac:TransitPeriod><cbc:StartDate>" & comprobante.FECHA_INICIO & "</cbc:StartDate></cac:TransitPeriod>")

            If comprobante.COD_MODALIDAD_TRASLADO = "01" Then
                xml.AppendLine("<cac:CarrierParty><cac:PartyIdentification><cbc:ID schemeID=""6"" schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">" & comprobante.NRO_DOCUMENTO_TRANSPORTISTA & "</cbc:ID></cac:PartyIdentification>")
                xml.AppendLine($"<cac:PartyLegalEntity><cbc:RegistrationName><![CDATA[{comprobante.RAZON_SOCIAL_TRANSPORTISTA}]]></cbc:RegistrationName>")
                If Not String.IsNullOrEmpty(comprobante.MTC_TRANSPORTISTA) Then
                    xml.AppendLine($"<cbc:CompanyID>{comprobante.MTC_TRANSPORTISTA}</cbc:CompanyID>")
                End If
                xml.AppendLine("</cac:PartyLegalEntity></cac:CarrierParty>")
            End If

            If comprobante.COD_MODALIDAD_TRASLADO = "02" Then
                'Agregando la etiqueta transportMeans
                'xml.AppendLine("<cac:TransportMeans>")
                'xml.AppendLine($"<cbc:LicensePlateID>{comprobante.PLACA_VEHICULO}</cbc:LicensePlateID>")
                'xml.AppendLine("/cac:TransportMeans")

                xml.AppendLine("<cac:TransportMeans>") ' Etiqueta de apertura
                If Not String.IsNullOrEmpty(comprobante.PLACA_VEHICULO) Then
                    xml.AppendLine("<cac:RoadTransport>")
                    xml.AppendLine($"<cbc:LicensePlateID>{comprobante.PLACA_VEHICULO}</cbc:LicensePlateID>")
                    xml.AppendLine("</cac:RoadTransport>")
                End If
                xml.AppendLine("</cac:TransportMeans>") ' Etiqueta de cierre

                'If Not String.IsNullOrEmpty(comprobante.PLACA_CARRETA) Then
                '    xml.AppendLine("<cac:RoadTransport>")
                '    xml.AppendLine($"<cbc:LicensePlateID>{comprobante.PLACA_CARRETA}</cbc:LicensePlateID>")
                '    xml.AppendLine("</cac:RoadTransport>")
                'End If
                xml.AppendLine("<cac:DriverPerson>")
                xml.AppendLine($"<cbc:ID schemeID='1'>{comprobante.NRO_DOC_CHOFER}</cbc:ID>")
                xml.AppendLine($"<cbc:FirstName>{comprobante.NOMBRE_CHOFER}</cbc:FirstName>")
                xml.AppendLine($"<cbc:FamilyName>{comprobante.APELLIDO_CHOFER}</cbc:FamilyName>")
                xml.AppendLine("<cbc:JobTitle>Principal</cbc:JobTitle>")
                xml.AppendLine($"<cac:IdentityDocumentReference><cbc:ID>{comprobante.LICENCIA_CHOFER}</cbc:ID></cac:IdentityDocumentReference>")
                xml.AppendLine("</cac:DriverPerson>")

                'en caso seg.conductor'
                If Not String.IsNullOrEmpty(comprobante.NRO_DOC_CHOFER_SEC) Then
                    xml.AppendLine("<cac:DriverPerson>")
                    xml.AppendLine($"<cbc:ID schemeID='1'>{comprobante.NRO_DOC_CHOFER_SEC}</cbc:ID>")
                    xml.AppendLine($"<cbc:FirstName><![CDATA[{comprobante.NOMBRE_CHOFER_SEC}]]></cbc:FirstName>")
                    xml.AppendLine($"<cbc:FamilyName><![CDATA[{comprobante.APELLIDO_CHOFER_SEC}]]></cbc:FamilyName>")
                    xml.AppendLine($"<cbc:JobTitle><![CDATA[{"Secundario"}]]></cbc:JobTitle>")
                    xml.AppendLine("<cac:IdentityDocumentReference>")
                    xml.AppendLine($"<cbc:ID>{comprobante.LICENCIA_CHOFER_SEC}</cbc:ID>")
                    xml.AppendLine("</cac:IdentityDocumentReference>")
                    xml.AppendLine("</cac:DriverPerson>")
                End If

                'xml.AppendLine("<cac:TransportHandlingUnit><cac:TransportEquipment><cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID></cac:TransportEquipment></cac:TransportHandlingUnit>")

                '' Incluir TransportHandlingUnit solo si corresponde
                'If Not String.IsNullOrEmpty(comprobante.PLACA_VEHICULO) Then
                '    xml.AppendLine("<cac:TransportHandlingUnit><cac:TransportEquipment><cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID></cac:TransportEquipment></cac:TransportHandlingUnit>")
                'End If

            End If

            xml.AppendLine("</cac:ShipmentStage>")
            xml.AppendLine("<cac:Delivery>")
            xml.AppendLine("<cac:DeliveryAddress><cbc:ID schemeName=""Ubigeos"" schemeAgencyName=""PE:INEI"">" & comprobante.COD_UBIGEO_DESTINO & "</cbc:ID>")
            xml.AppendLine("<cac:AddressLine><cbc:Line>" & comprobante.DIRECCION_DESTINO & "</cbc:Line></cac:AddressLine></cac:DeliveryAddress>")
            xml.AppendLine("<cac:Despatch><cac:DespatchAddress><cbc:ID schemeName=""Ubigeos"" schemeAgencyName=""PE:INEI"">" & comprobante.COD_UBIGEO_ORIGEN & "</cbc:ID>")
            xml.AppendLine("<cac:AddressLine><cbc:Line>" & comprobante.DIRECCION_ORIGEN & "</cbc:Line></cac:AddressLine></cac:DespatchAddress></cac:Despatch>")
            xml.AppendLine("</cac:Delivery>")

            'If comprobante.COD_MODALIDAD_TRASLADO = "2" Then
            '    xml.AppendLine("<cac:TransportHandlingUnit><cac:TransportEquipment><cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID></cac:TransportEquipment></cac:TransportHandlingUnit>")
            'End If
            'If Not String.IsNullOrEmpty(comprobante.PLACA_CARRETA) Then
            '    xml.AppendLine("<cac:RoadTransport>")
            '    xml.AppendLine($"<cbc:LicensePlateID>{comprobante.PLACA_CARRETA}</cbc:LicensePlateID>")
            '    xml.AppendLine("</cac:RoadTransport>")
            'End If
            If comprobante.COD_MODALIDAD_TRASLADO = "02" Then
                xml.AppendLine("<cac:TransportHandlingUnit>")
                xml.AppendLine("<cac:TransportEquipment><cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID>")
                If Not String.IsNullOrEmpty(comprobante.PLACA_CARRETA) Then
                    xml.AppendLine("<cac:AttachedTransportEquipment><cbc:ID>" & comprobante.PLACA_CARRETA & "</cbc:ID>")
                    xml.AppendLine("</cac:AttachedTransportEquipment>")
                End If
                xml.AppendLine("</cac:TransportEquipment>")
                xml.AppendLine("</cac:TransportHandlingUnit>")
            End If

            xml.AppendLine("</cac:Shipment>")

            For x As Integer = 0 To comprobante.detalle.Count - 1
                ' Verificar si el detalle tiene elementos válidos
                If comprobante.detalle(x) IsNot Nothing Then
                    ' Depuración: Imprimir el detalle actual
                    Console.WriteLine($"Generando DespatchLine para el item {comprobante.detalle(x).ITEM}")

                    xml.AppendLine("<cac:DespatchLine>")
                    xml.AppendLine($"<cbc:ID>{comprobante.detalle(x).ITEM}</cbc:ID>")
                    xml.AppendLine($"<cbc:DeliveredQuantity unitCode='NIU' unitCodeListID='UN/ECE rec 20' unitCodeListAgencyName='United Nations Economic Commission for Europe'>{comprobante.detalle(x).CANTIDAD}</cbc:DeliveredQuantity>")
                    xml.AppendLine("<cac:OrderLineReference>")
                    xml.AppendLine($"<cbc:LineID>{comprobante.detalle(x).ORDER_ITEM}</cbc:LineID>")
                    xml.AppendLine("</cac:OrderLineReference>")
                    xml.AppendLine("<cac:Item>")
                    xml.AppendLine($"<cbc:Description>{comprobante.detalle(x).DESCRIPCION}</cbc:Description>")
                    'xml.AppendLine($"<cbc:Name><![CDATA[{comprobante.detalle(x).DESCRIPCION}]]></cbc:Name>")
                    xml.AppendLine($"<cac:SellersItemIdentification><cbc:ID>{comprobante.detalle(x).CODIGO}</cbc:ID></cac:SellersItemIdentification>")
                    xml.AppendLine("</cac:Item>")
                    xml.AppendLine("</cac:DespatchLine>")
                Else
                    ' Depuración: Si no hay datos en el detalle
                    Console.WriteLine($"El detalle en la posición {x} es vacío o nulo")
                End If
            Next

            ' Cierre del XML
            xml.AppendLine("</DespatchAdvice>")
            ' Guardar el XML en el archivo
            ' Guardar el XML sin BOM
            'Dim encoding As New UTF8Encoding(False) ' Sin BOM
            Dim doc As New XmlDocument()
            doc.LoadXml(xml.ToString())
            'Using writer As New StreamWriter(Path.Combine(ruta, $"{nomArchivo}.XML"), False, encoding)
            '    doc.Save(writer)
            'End Using
            doc.Save(Path.Combine(ruta, $"{nomArchivo}.XML"))

            'Crear el XmlWriter con la codificación UTF-8 sin BOM
            'Dim encoding As New UTF8Encoding(False) ' Sin BOM
            'Dim settings As New XmlWriterSettings()
            'settings.Encoding = encoding
            'settings.Indent = True

            'Usar XmlWriter para guardar el archivo sin BOM
            'Using writer As XmlWriter = XmlWriter.Create(Path.Combine(ruta, $"{nomArchivo}.XML"), settings)
            '    writer.WriteRaw(xml.ToString())
            'End Using

            ' Respuesta de éxito
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")

        Catch ex As Exception
            ' Respuesta de error
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function

    Public Function CPE_TRANSPORTISTA(comprobante As BE.CPE_GUIA_REMISION, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As New Dictionary(Of String, String)
        Try
            Dim xml As New StringBuilder()


            'Iniciamos el Encabezado XML
            xml.AppendLine("<?xml version='1.0' encoding='utf-8'?>")
            xml.AppendLine("<DespatchAdvice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">")
            xml.AppendLine("<ext:UBLExtensions>")
            xml.AppendLine("<ext:UBLExtension>")
            xml.AppendLine("<ext:ExtensionContent></ext:ExtensionContent>")
            xml.AppendLine("</ext:UBLExtension>")
            xml.AppendLine("</ext:UBLExtensions>")

            ' Datos generales del documento
            xml.AppendLine("<cbc:UBLVersionID>2.1</cbc:UBLVersionID>")
            xml.AppendLine("<cbc:CustomizationID>2.0</cbc:CustomizationID>")
            xml.AppendLine($"<cbc:ID>{comprobante.NRO_COMPROBANTE}</cbc:ID>")
            xml.AppendLine($"<cbc:IssueDate>{comprobante.FECHA_DOCUMENTO}</cbc:IssueDate>")
            xml.AppendLine($"<cbc:IssueTime>{DateTime.Now:HH:mm:ss}</cbc:IssueTime>")
            xml.AppendLine("<cbc:DespatchAdviceTypeCode>31</cbc:DespatchAdviceTypeCode>")
            xml.AppendLine($"<cbc:Note><![CDATA[{comprobante.NOTA}]]></cbc:Note>")

            'Firma de la guia
            xml.AppendLine($"<cac:Signature><cbc:ID>{comprobante.NRO_DOCUMENTO_EMPRESA}</cbc:ID>")
            xml.AppendLine("<cac:SignatoryParty><cac:PartyIdentification><cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID></cac:PartyIdentification>")
            xml.AppendLine("<cac:PartyName>")
            xml.AppendLine($"<cbc:Name><![CDATA[{comprobante.RAZON_SOCIAL_EMPRESA}]]></cbc:Name>")
            xml.AppendLine("</cac:PartyName></cac:SignatoryParty>") 'se puede agregar opcional'
            xml.AppendLine($"<cac:DigitalSignatureAttachment><cac:ExternalReference><cbc:URI>{comprobante.NRO_DOCUMENTO_EMPRESA}</cbc:URI></cac:ExternalReference></cac:DigitalSignatureAttachment></cac:Signature>")

            'Despacho del transportista( Supplier Party ) DATOS DEL EMISOR(TRANSPORTISTA)
            xml.AppendLine("<cac:DespatchSupplierParty><cac:Party><cac:PartyIdentification><cbc:ID schemeID=""6"" schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID></cac:PartyIdentification>")
            'xml.AppendLine($"<cac:PartyName><cbc:Name><![CDATA[{comprobante.RAZON_SOCIAL_EMPRESA}]]></cbc:Name></cac:PartyName>")
            xml.AppendLine($"<cac:PartyLegalEntity><cbc:RegistrationName><![CDATA[{comprobante.RAZON_SOCIAL_EMPRESA}]]></cbc:RegistrationName></cac:PartyLegalEntity>")
            xml.AppendLine("</cac:Party></cac:DespatchSupplierParty>")

            ' Customer Party se cambia al num 7 scheme ID (DATOS DEL RECEPTOR--(DESTINATARIO)---)
            xml.AppendLine("<cac:DeliveryCustomerParty><cac:Party><cac:PartyIdentification><cbc:ID schemeID=""6"" schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:ID></cac:PartyIdentification>")
            xml.AppendLine($"<cac:PartyLegalEntity><cbc:RegistrationName><![CDATA[{comprobante.RAZON_SOCIAL_CLIENTE}]]></cbc:RegistrationName></cac:PartyLegalEntity>")
            xml.AppendLine("</cac:Party></cac:DeliveryCustomerParty>")

            ' Shipment (DATOS DE QUIEN PAGA EL SERVICIO)
            'ID OBLIGATIRIO POR UBL (1)'
            xml.AppendLine("<cac:Shipment><cbc:ID>SUNAT_Envio</cbc:ID>")
            'xml.AppendLine($"<cbc:HandlingCode listAgencyName=""PE:SUNAT"" listName=""Motivo de traslado"" listURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo20"">01</cbc:HandlingCode>")'
            xml.AppendLine($"<cbc:GrossWeightMeasure unitCode=""KGM"">{comprobante.PESO_BRUTO}</cbc:GrossWeightMeasure>")


            If comprobante.COD_MOTIVO_TRASLADO = 7 Then
                xml.AppendLine($"<cbc:TotalTransportHandlingUnitQuantity>{comprobante.TOTAL_BULTOS}</cbc:TotalTransportHandlingUnitQuantity>")
            End If

            ' Información del transportista
            xml.AppendLine("<cac:ShipmentStage>") '<cbc:ID>1</cbc:ID>'
            'xml.AppendLine($"<cbc:TransportModeCode listAgencyName=""PE:SUNAT"" listName=""Modalidad de traslado"" listURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo18"">0{comprobante.COD_MODALIDAD_TRASLADO}</cbc:TransportModeCode>")'
            xml.AppendLine("<cac:TransitPeriod><cbc:StartDate>" & comprobante.FECHA_INICIO & "</cbc:StartDate></cac:TransitPeriod>")



            ' Datos del transportista
            xml.AppendLine("<cac:CarrierParty>")
            xml.AppendLine("<cac:PartyIdentification>")
            'xml.AppendLine($"<cbc:ID schemeID='6'>{comprobante.NRO_DOCUMENTO_TRANSPORTISTA}</cbc:ID>")
            xml.AppendLine("<cbc:ID schemeID=""""/>")
            xml.AppendLine("</cac:PartyIdentification>")
            xml.AppendLine("<cac:PartyLegalEntity>")
            'xml.AppendLine($"<cbc:RegistrationName><![CDATA[{comprobante.RAZON_SOCIAL_TRANSPORTISTA}]]></cbc:RegistrationName>")
            xml.AppendLine("<cbc:RegistrationName/>") 'dato vacio'
            xml.AppendLine("</cac:PartyLegalEntity>")
            xml.AppendLine("</cac:CarrierParty>")

            ' Información sobre el vehículo y el conductor



            xml.AppendLine("<cac:TransportMeans>")
            If Not String.IsNullOrEmpty(comprobante.PLACA_VEHICULO) Then
                xml.AppendLine("<cac:RoadTransport>")
                xml.AppendLine($"<cbc:LicensePlateID>{comprobante.PLACA_VEHICULO}</cbc:LicensePlateID>")
                xml.AppendLine("</cac:RoadTransport>")
            End If

            If Not String.IsNullOrEmpty(comprobante.PLACA_CARRETA) Then
                xml.AppendLine("<cac:RoadTransport>")
                xml.AppendLine($"<cbc:LicensePlateID>{comprobante.PLACA_CARRETA}</cbc:LicensePlateID>")
                xml.AppendLine("</cac:RoadTransport>")
            End If

            xml.AppendLine("</cac:TransportMeans>")

            xml.AppendLine("<cac:DriverPerson>")
            xml.AppendLine($"<cbc:ID schemeID='1'>{comprobante.NRO_DOC_CHOFER}</cbc:ID>")
            xml.AppendLine($"<cbc:FirstName><![CDATA[{comprobante.NOMBRE_CHOFER}]]></cbc:FirstName>")
            xml.AppendLine($"<cbc:FamilyName><![CDATA[{comprobante.APELLIDO_CHOFER}]]></cbc:FamilyName>")
            xml.AppendLine($"<cbc:JobTitle><![CDATA[{"Principal"}]]></cbc:JobTitle>")
            xml.AppendLine("<cac:IdentityDocumentReference>")
            xml.AppendLine($"<cbc:ID>{comprobante.LICENCIA_CHOFER}</cbc:ID>")
            xml.AppendLine("</cac:IdentityDocumentReference>")

            xml.AppendLine("</cac:DriverPerson>")
            'en caso seg.conductor'
            If Not String.IsNullOrEmpty(comprobante.NRO_DOC_CHOFER_SEC) Then
                xml.AppendLine("<cac:DriverPerson>")
                xml.AppendLine($"<cbc:ID schemeID='1'>{comprobante.NRO_DOC_CHOFER_SEC}</cbc:ID>")
                xml.AppendLine($"<cbc:FirstName><![CDATA[{comprobante.NOMBRE_CHOFER_SEC}]]></cbc:FirstName>")
                xml.AppendLine($"<cbc:FamilyName><![CDATA[{comprobante.APELLIDO_CHOFER_SEC}]]></cbc:FamilyName>")
                xml.AppendLine($"<cbc:JobTitle><![CDATA[{"Secundario"}]]></cbc:JobTitle>")
                xml.AppendLine("<cac:IdentityDocumentReference>")
                xml.AppendLine($"<cbc:ID>{comprobante.LICENCIA_CHOFER_SEC}</cbc:ID>")
                xml.AppendLine("</cac:IdentityDocumentReference>")
                xml.AppendLine("</cac:DriverPerson>")
            End If

            ' Direcciones de Entrega y Despacho
            xml.AppendLine("</cac:ShipmentStage><cac:Delivery>")
            xml.AppendLine("<cac:DeliveryAddress><cbc:ID schemeName=""Ubigeos"" schemeAgencyName=""PE:INEI"">" & comprobante.COD_UBIGEO_DESTINO & "</cbc:ID>")
            xml.AppendLine("<cac:AddressLine><cbc:Line>" & comprobante.DIRECCION_DESTINO & "</cbc:Line></cac:AddressLine></cac:DeliveryAddress>")
            xml.AppendLine("<cac:Despatch><cac:DespatchAddress><cbc:ID schemeName=""Ubigeos"" schemeAgencyName=""PE:INEI"">" & comprobante.COD_UBIGEO_ORIGEN & "</cbc:ID>")
            xml.AppendLine("<cac:AddressLine><cbc:Line>" & comprobante.DIRECCION_ORIGEN & "</cbc:Line></cac:AddressLine></cac:DespatchAddress>")

            'Datos del Remitente'
            xml.AppendLine("<cac:DespatchParty><cac:PartyIdentification><cbc:ID schemeID=""6"" schemeName=""Documento de Identidad"" schemeAgencyName=""PE:SUNAT"" schemeURI=""urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06"">" & comprobante.NRO_DOC_EMPRESA_REMITENTE & "</cbc:ID></cac:PartyIdentification>")
            xml.AppendLine("<cac:PartyLegalEntity><cbc:RegistrationName>" & comprobante.RAZON_SOC_EMP_REMITENTE & " </cbc:RegistrationName></cac:PartyLegalEntity></cac:DespatchParty></cac:Despatch>")
            xml.AppendLine("</cac:Delivery>")

            ' Transporte de la unidad de manejo (TransportHandlingUnit)
            'Si hay placa del camaion (vehiculo)'
            xml.AppendLine("<cac:TransportHandlingUnit>")
            If Not String.IsNullOrEmpty(comprobante.PLACA_VEHICULO) Then
                xml.AppendLine("<cac:TransportEquipment>")
                xml.AppendLine($"<cbc:ID>{comprobante.PLACA_VEHICULO}</cbc:ID>")
                xml.AppendLine("</cac:TransportEquipment>")

            End If

            'Si hay placa de la carreta'
            If Not String.IsNullOrEmpty(comprobante.PLACA_CARRETA) Then
                xml.AppendLine("<cac:TransportEquipment>")
                xml.AppendLine($"<cbc:ID>{comprobante.PLACA_CARRETA}</cbc:ID>")
                xml.AppendLine("</cac:TransportEquipment>")
            End If

            xml.AppendLine("</cac:TransportHandlingUnit>")

            'xml.AppendLine("</cac:ShipmentStage><cac:Delivery>")
            'xml.AppendLine("<cac:DeliveryAddress><cbc:ID schemeName=""Ubigeos"" schemeAgencyName=""PE:INEI"">" & comprobante.COD_UBIGEO_DESTINO & "</cbc:ID>")
            'xml.AppendLine("<cac:AddressLine><cbc:Line>" & comprobante.DIRECCION_DESTINO & "</cbc:Line></cac:AddressLine></cac:DeliveryAddress>")
            'xml.AppendLine("<cac:Despatch><cac:DespatchAddress><cbc:ID schemeName=""Ubigeos"" schemeAgencyName=""PE:INEI"">" & comprobante.COD_UBIGEO_ORIGEN & "</cbc:ID>")
            'xml.AppendLine("<cac:AddressLine><cbc:Line>" & comprobante.DIRECCION_ORIGEN & "</cbc:Line></cac:AddressLine></cac:DespatchAddress></cac:Despatch>")
            'xml.AppendLine("</cac:Delivery>")

            ' Cerrar las etiquetas de envío
            xml.AppendLine("</cac:Shipment>")

            For x As Integer = 0 To comprobante.detalle.Count - 1
                ' Verificar si el detalle tiene elementos válidos
                If comprobante.detalle(x) IsNot Nothing Then
                    ' Depuración: Imprimir el detalle actual
                    Console.WriteLine($"Generando DespatchLine para el item {comprobante.detalle(x).ITEM}")

                    xml.AppendLine("<cac:DespatchLine>")
                    xml.AppendLine($"<cbc:ID>{comprobante.detalle(x).ITEM}</cbc:ID>")
                    xml.AppendLine($"<cbc:DeliveredQuantity unitCode='{comprobante.detalle(x).UNIDAD_MEDIDA}'>{comprobante.detalle(x).CANTIDAD}</cbc:DeliveredQuantity>")
                    xml.AppendLine("<cac:OrderLineReference>")
                    xml.AppendLine($"<cbc:LineID>{comprobante.detalle(x).ORDER_ITEM}</cbc:LineID>")
                    xml.AppendLine("</cac:OrderLineReference>")
                    xml.AppendLine("<cac:Item>")
                    xml.AppendLine($"<cbc:Description>{comprobante.detalle(x).DESCRIPCION}</cbc:Description>")
                    'xml.AppendLine($"<cbc:Name><![CDATA[{comprobante.detalle(x).DESCRIPCION}]]></cbc:Name>")
                    xml.AppendLine($"<cac:SellersItemIdentification><cbc:ID>{comprobante.detalle(x).CODIGO}</cbc:ID></cac:SellersItemIdentification>")
                    xml.AppendLine("</cac:Item>")
                    xml.AppendLine("</cac:DespatchLine>")
                Else
                    ' Depuración: Si no hay datos en el detalle
                    Console.WriteLine($"El detalle en la posición {x} es vacío o nulo")
                End If

            Next

            ' Cierre del XML
            xml.AppendLine("</DespatchAdvice>")
            'Dim encoding As New UTF8Encoding(False) ' Sin BOM
            Dim doc As New XmlDocument()
            doc.LoadXml(xml.ToString())
            doc.Save(Path.Combine(ruta, $"{nomArchivo}.XML"))
            ' Respuesta de éxito
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
            '' Guardamos el archivo XML
            'Dim path As String = path.Combine(ruta, nomArchivo)
            'File.WriteAllText(path, xml.ToString())

            'dictionary.Add("xmlFilePath", path)
            'dictionary.Add("status", "success")
        Catch ex As Exception
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try

        Return dictionary
    End Function


    'Public Function CPE_GUIA_REMISION_TRANSPORTISTA(comprobante As BE.CPE_GUIA_REMISION, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
    '    Dim dictionary As Dictionary(Of String, String)
    '    Try
    '        Dim xml As String
    '        Dim doc As New XmlDocument()
    '        xml = "<?xml version='1.0' encoding='iso-8859-1'?>
    '                <DespatchAdvice
    '                    xmlns:ds='http://www.w3.org/2000/09/xmldsig#'
    '                    xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2'
    '                    xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2'
    '                    xmlns:ccts='urn:un:unece:uncefact:documentation:2'
    '                    xmlns:xsd='http://www.w3.org/2001/XMLSchema'
    '                    xmlns:udt='urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2'
    '                    xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2'
    '                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
    '                    xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2'
    '                    xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1'
    '                    xmlns='urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2'>
    '                    <ext:UBLExtensions>
    '                        <ext:UBLExtension>
    '                            <ext:ExtensionContent>
    '                            </ext:ExtensionContent>
    '                        </ext:UBLExtension>
    '                    </ext:UBLExtensions>

    '                    <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
    '                    <cbc:CustomizationID>1.0</cbc:CustomizationID>
    '                    <cbc:ID>" & comprobante.NRO_COMPROBANTE & "</cbc:ID>
    '                    <cbc:IssueDate>" & comprobante.FECHA_DOCUMENTO & "</cbc:IssueDate>
    '                    <cbc:DespatchAdviceTypeCode>" & comprobante.COD_TIPO_DOCUMENTO & "</cbc:DespatchAdviceTypeCode>
    '                    <cbc:Note><![CDATA[" & comprobante.NOTA & "]]></cbc:Note>

    '                    <cac:DespatchSupplierParty>
    '                        <cbc:CustomerAssignedAccountID schemeID='" & comprobante.TIPO_DOCUMENTO_EMPRESA & "'>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:CustomerAssignedAccountID>
    '                        <cac:Party>
    '                            <cac:PartyName>
    '                                <cbc:Name>" & comprobante.RAZON_SOCIAL_EMPRESA & "</cbc:Name>
    '                            </cac:PartyName>
    '                            <cac:PartyLegalEntity>
    '                                <cbc:RegistrationName>
    '                                    <![CDATA[" & comprobante.RAZON_SOCIAL_EMPRESA & "]]>
    '                                </cbc:RegistrationName>
    '                            </cac:PartyLegalEntity>
    '                        </cac:Party>
    '                    </cac:DespatchSupplierParty>

    '                    <cac:DeliveryCustomerParty>
    '                        <cbc:CustomerAssignedAccountID schemeID='" & comprobante.TIPO_DOCUMENTO_CLIENTE & "'>" & comprobante.NRO_DOCUMENTO_CLIENTE & "</cbc:CustomerAssignedAccountID>
    '                        <cac:Party>
    '                            <cac:PartyLegalEntity>
    '                                <cbc:RegistrationName>
    '                                    <![CDATA[" & comprobante.RAZON_SOCIAL_CLIENTE & "]]>
    '                                </cbc:RegistrationName>
    '                            </cac:PartyLegalEntity>
    '                        </cac:Party>
    '                    </cac:DeliveryCustomerParty>


    '                    <cac:Shipment>
    '                        <cbc:ID>" & comprobante.ITEM_ENVIO & "</cbc:ID>
    '                        <cbc:HandlingCode>" & comprobante.COD_MOTIVO_TRASLADO & "</cbc:HandlingCode>
    '                        <cbc:Information>" & comprobante.DESCRIPCION_MOTIVO_TRASLADO & "</cbc:Information>
    '                        <cbc:GrossWeightMeasure unitCode='" & comprobante.COD_UND_PESO_BRUTO & "'>" & comprobante.PESO_BRUTO & "</cbc:GrossWeightMeasure>
    '                        <cbc:TotalTransportHandlingUnitQuantity>" & comprobante.TOTAL_BULTOS & "</cbc:TotalTransportHandlingUnitQuantity>

    '                        <cac:ShipmentStage>
    '                            <cbc:TransportModeCode>" & comprobante.COD_MODALIDAD_TRASLADO & "</cbc:TransportModeCode>
    '                            <cac:TransitPeriod>
    '                                <cbc:StartDate>" & comprobante.FECHA_INICIO & "</cbc:StartDate>
    '                            </cac:TransitPeriod>
    '                            <cac:CarrierParty>
    '                                <cac:PartyIdentification>
    '                                    <cbc:ID schemeID='" & comprobante.TIPO_DOCUMENTO_TRANSPORTISTA & "'>" & comprobante.NRO_DOCUMENTO_TRANSPORTISTA & "</cbc:ID>
    '                                </cac:PartyIdentification>
    '                                <cac:PartyName>
    '                                    <cbc:Name>
    '                                        <![CDATA[" & comprobante.RAZON_SOCIAL_TRANSPORTISTA & "]]>
    '                                    </cbc:Name>
    '                                </cac:PartyName>
    '                            </cac:CarrierParty>

    '                        <cac:TransportMeans>
    '                            <cac:RoadTransport>
    '                                <cbc:LicensePlateID>" & comprobante.PLACA_VEHICULO & "</cbc:LicensePlateID>
    '                            </cac:RoadTransport>
    '                        </cac:TransportMeans>
    '                        <cac:DriverPerson>
    '                            <cbc:ID schemeID='" & comprobante.COD_TIPO_DOC_CHOFER & "'>" & comprobante.NRO_DOC_CHOFER & "</cbc:ID>
    '                        </cac:DriverPerson>

    '                     </cac:ShipmentStage>

    '                        <cac:Delivery>
    '                            <cac:DeliveryAddress>
    '                                <cbc:ID>" & comprobante.COD_UBIGEO_DESTINO & "</cbc:ID>
    '                                <cbc:StreetName>" & comprobante.DIRECCION_DESTINO & "</cbc:StreetName>
    '                            </cac:DeliveryAddress>
    '                        </cac:Delivery>"

    '        If comprobante.PLACA_CARRETA <> "" Then
    '            xml = xml & "<cac:TransportHandlingUnit>
    '                            <cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID>
    '                            <cac:TransportEquipment>
    '                                <cbc:ID>" & comprobante.PLACA_VEHICULO & "</cbc:ID>
    '                            </cac:TransportEquipment>
    '                        </cac:TransportHandlingUnit>"
    '        End If

    '        xml = xml & "<cac:OriginAddress>
    '                            <cbc:ID>" & comprobante.COD_UBIGEO_ORIGEN & "</cbc:ID>
    '                            <cbc:StreetName>" & comprobante.DIRECCION_ORIGEN & "</cbc:StreetName>
    '                        </cac:OriginAddress>
    '                    </cac:Shipment>"

    '        For x As Integer = 0 To comprobante.detalle.Count - 1
    '            xml = xml & "<cac:DespatchLine>
    '                                    <cbc:ID>" & comprobante.detalle(x).ITEM & "</cbc:ID>
    '                                    <cbc:DeliveredQuantity unitCode='" & comprobante.detalle(x).UNIDAD_MEDIDA & "'>" & comprobante.detalle(x).CANTIDAD & "</cbc:DeliveredQuantity>
    '                                    <cac:OrderLineReference>
    '                                        <cbc:LineID>" & comprobante.detalle(x).ORDER_ITEM & "</cbc:LineID>
    '                                    </cac:OrderLineReference>
    '                                    <cac:Item>
    '                                        <cbc:Name>
    '                                            <![CDATA[" & comprobante.detalle(x).DESCRIPCION & "]]>
    '                                        </cbc:Name>
    '                                        <cac:SellersItemIdentification>
    '                                            <cbc:ID>" & comprobante.detalle(x).CODIGO & "</cbc:ID>
    '                                        </cac:SellersItemIdentification>
    '                                    </cac:Item>
    '                                </cac:DespatchLine>"
    '        Next

    '        xml = xml & "</DespatchAdvice>"

    '        doc.LoadXml(xml)
    '        doc.Save(ruta & nomArchivo & ".XML")
    '        dictionary = New Dictionary(Of String, String)
    '        dictionary.Add("flg_rta", "1")
    '        dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
    '        'MsgBox("El XML de la guia se Creó Correctamente. En " & ruta, MsgBoxStyle.Information, "Generacion de Xml")
    '        dictionary.Add("cod_sunat", "")
    '        dictionary.Add("msj_sunat", "")
    '        dictionary.Add("hash_cdr", "")
    '        dictionary.Add("hash_cpe", "")
    '    Catch ex As Exception
    '        dictionary = New Dictionary(Of String, String)
    '        dictionary.Add("flg_rta", "0")
    '        dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
    '        MsgBox("Error al Generar el Archivo XML de la Guia " & ex.Message, MsgBoxStyle.Exclamation, "Generacion de Xml")
    '        dictionary.Add("cod_sunat", "")
    '        dictionary.Add("msj_sunat", "")
    '        dictionary.Add("hash_cdr", "")
    '        dictionary.Add("hash_cpe", "")
    '    End Try
    '    Return dictionary
    'End Function


    Public Function ResumenBoleta(comprobante As BE.CPE_RESUMEN_BOLETA, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim xml As String
            Dim doc As New XmlDocument()
            xml = "<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
                    <SummaryDocuments xmlns='urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1' 
                    xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' 
                    xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' 
                    xmlns:ds='http://www.w3.org/2000/09/xmldsig#' 
                    xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2' 
                    xmlns:qdt='urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2' 
                    xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1' 
                    xmlns:udt='urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2'>
                    <ext:UBLExtensions>
                    <ext:UBLExtension>
                    <ext:ExtensionContent>
                    </ext:ExtensionContent>
                    </ext:UBLExtension>
                    </ext:UBLExtensions>
                    <cbc:UBLVersionID>2.0</cbc:UBLVersionID>
                    <cbc:CustomizationID>1.1</cbc:CustomizationID>
                    <cbc:ID>" & comprobante.CODIGO & "-" & comprobante.SERIE & "-" & comprobante.SECUENCIA & "</cbc:ID>
                    <cbc:ReferenceDate>" & comprobante.FECHA_REFERENCIA & "</cbc:ReferenceDate>
                    <cbc:IssueDate>" & comprobante.FECHA_DOCUMENTO & "</cbc:IssueDate>
                    <cac:Signature>
                    <cbc:ID>" & comprobante.CODIGO & "-" & comprobante.SERIE & "-" & comprobante.SECUENCIA & "</cbc:ID>
                    <cac:SignatoryParty>
                    <cac:PartyIdentification>
                    <cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
                    </cac:PartyIdentification>
                    <cac:PartyName>
                    <cbc:Name><![CDATA[" & comprobante.RAZON_SOCIAL & "]]></cbc:Name>
                    </cac:PartyName>
                    </cac:SignatoryParty>
                    <cac:DigitalSignatureAttachment>
                    <cac:ExternalReference>
                    <cbc:URI>" & comprobante.CODIGO & "-" & comprobante.SERIE & "-" & comprobante.SECUENCIA & "</cbc:URI>
                    </cac:ExternalReference>
                    </cac:DigitalSignatureAttachment>
                    </cac:Signature>
                    <cac:AccountingSupplierParty>
                    <cbc:CustomerAssignedAccountID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:CustomerAssignedAccountID>
                    <cbc:AdditionalAccountID>6</cbc:AdditionalAccountID>
                    <cac:Party>
                    <cac:PartyLegalEntity>
                    <cbc:RegistrationName><![CDATA[" & comprobante.RAZON_SOCIAL & "]]></cbc:RegistrationName>
                    </cac:PartyLegalEntity>
                    </cac:Party>
                    </cac:AccountingSupplierParty>"
            For x As Integer = 0 To comprobante.detalle.Count - 1
                xml = xml & "<sac:SummaryDocumentsLine>
                    <cbc:LineID>" & comprobante.detalle(x).ITEM & "</cbc:LineID>
                    <cbc:DocumentTypeCode>" & comprobante.detalle(x).TIPO_COMPROBANTE & "</cbc:DocumentTypeCode>
                    <cbc:ID>" & comprobante.detalle(x).NRO_COMPROBANTE & "</cbc:ID>
                    <cac:AccountingCustomerParty>
                    <cbc:CustomerAssignedAccountID>" & comprobante.detalle(x).NRO_DOCUMENTO & "</cbc:CustomerAssignedAccountID>
                    <cbc:AdditionalAccountID>" & comprobante.detalle(x).TIPO_DOCUMENTO & "</cbc:AdditionalAccountID>
                    </cac:AccountingCustomerParty>"
                If (comprobante.detalle(x).TIPO_COMPROBANTE = "07" Or comprobante.detalle(x).TIPO_COMPROBANTE = "08") Then
                    xml = xml & "<cac:BillingReference>
			                        <cac:InvoiceDocumentReference>
				                        <cbc:ID>" & comprobante.detalle(x).NRO_COMPROBANTE_REF & "</cbc:ID>
				                        <cbc:DocumentTypeCode>" & comprobante.detalle(x).TIPO_COMPROBANTE_REF & "</cbc:DocumentTypeCode>
			                        </cac:InvoiceDocumentReference>
		                        </cac:BillingReference>"
                End If
                xml = xml & "<cac:Status>
                    <cbc:ConditionCode>" & comprobante.detalle(x).STATU & "</cbc:ConditionCode>
                    </cac:Status>
                    <sac:TotalAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).TOTAL & "</sac:TotalAmount>
                    <sac:BillingPayment>
                    <cbc:PaidAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).GRAVADA & "</cbc:PaidAmount>
                    <cbc:InstructionID>01</cbc:InstructionID>
                    </sac:BillingPayment>"
                If (comprobante.detalle(x).EXONERADO > 0) Then
                    xml = xml & "<sac:BillingPayment>
                    <cbc:PaidAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).EXONERADO & "</cbc:PaidAmount>
                    <cbc:InstructionID>02</cbc:InstructionID>
                    </sac:BillingPayment>"
                End If

                If (comprobante.detalle(x).INAFECTO > 0) Then
                    xml = xml & "<sac:BillingPayment>
                    <cbc:PaidAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).INAFECTO & "</cbc:PaidAmount>
                    <cbc:InstructionID>03</cbc:InstructionID>
                    </sac:BillingPayment>"
                End If

                If (comprobante.detalle(x).EXPORTACION > 0) Then
                    xml = xml & "<sac:BillingPayment>
                    <cbc:PaidAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).EXPORTACION & "</cbc:PaidAmount>
                    <cbc:InstructionID>04</cbc:InstructionID>
                    </sac:BillingPayment>"
                End If

                If (comprobante.detalle(x).GRATUITAS > 0) Then
                    xml = xml & "<sac:BillingPayment>
                    <cbc:PaidAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).GRATUITAS & "</cbc:PaidAmount>
                    <cbc:InstructionID>05</cbc:InstructionID>
                    </sac:BillingPayment>"
                End If

                If (comprobante.detalle(x).MONTO_CARGO_X_ASIG > 0) Then
                    xml = xml & "<cac:AllowanceCharge>"
                    If (comprobante.detalle(x).CARGO_X_ASIGNACION = 1) Then
                        xml = xml & "<cbc:ChargeIndicator>true</cbc:ChargeIndicator>"
                    Else
                        xml = xml & "<cbc:ChargeIndicator>false</cbc:ChargeIndicator>"
                    End If
                    xml = xml & "<cbc:Amount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).MONTO_CARGO_X_ASIG & "</cbc:Amount>
                    </cac:AllowanceCharge>"
                End If

                If (comprobante.detalle(x).ISC > 0) Then
                    xml = xml & "<cac:TaxTotal>
			        <cbc:TaxAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
			        <cac:TaxSubtotal>
                        <cbc:TaxAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).ISC & "</cbc:TaxAmount>
				        <cac:TaxCategory>
                            <cac:TaxScheme>
                                <cbc:ID>2000</cbc:ID>
                                <cbc:Name>ISC</cbc:Name>
                                <cbc:TaxTypeCode>EXC</cbc:TaxTypeCode>
                            </cac:TaxScheme>
				        </cac:TaxCategory>
                    </cac:TaxSubtotal>
		        </cac:TaxTotal>"
                End If

                xml = xml & "<cac:TaxTotal>
                    <cbc:TaxAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
                    <cac:TaxSubtotal>
                    <cbc:TaxAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).IGV & "</cbc:TaxAmount>
                    <cac:TaxCategory>
                    <cac:TaxScheme>
                    <cbc:ID>1000</cbc:ID>
                    <cbc:Name>IGV</cbc:Name>
                    <cbc:TaxTypeCode>VAT</cbc:TaxTypeCode>
                    </cac:TaxScheme>
                    </cac:TaxCategory>
                    </cac:TaxSubtotal>
                    </cac:TaxTotal>"

                If (comprobante.detalle(x).OTROS > 0) Then
                    xml = xml & "<cac:TaxTotal>
			            <cbc:TaxAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).OTROS & "</cbc:TaxAmount>
			            <cac:TaxSubtotal>
                            <cbc:TaxAmount currencyID='" & comprobante.detalle(x).COD_MONEDA & "'>" & comprobante.detalle(x).OTROS & "</cbc:TaxAmount>
				            <cac:TaxCategory>
                                <cac:TaxScheme>
                                    <cbc:ID>9999</cbc:ID>
                                    <cbc:Name>OTROS</cbc:Name>
                                    <cbc:TaxTypeCode>OTH</cbc:TaxTypeCode>
                                </cac:TaxScheme>
				            </cac:TaxCategory>
                        </cac:TaxSubtotal>
		            </cac:TaxTotal>"
                End If

                xml = xml & "</sac:SummaryDocumentsLine>"
            Next
            xml = xml & "</SummaryDocuments>"
            doc.LoadXml(xml)
            doc.Save(ruta & nomArchivo & ".XML")
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
            MsgBox("El XML del Resumen fue Generado: " & vbCrLf & "Ruta: " & ruta & vbCrLf & "Nombre: " & nomArchivo, MsgBoxStyle.Information, "Archivo XML Generado")
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
            MsgBox("Error Al Crear el Archivo XML del Resumen de boletas.. Mas info.. " & vbCrLf & ex.Message, MsgBoxStyle.Exclamation, "Advertencia de Seguridad")
        End Try
        Return dictionary
    End Function



    Public Function ResumenBaja(comprobante As BE.CPE_BAJA, nomArchivo As String, ruta As String) As Dictionary(Of String, String)
        Dim dictionary As Dictionary(Of String, String)
        Try
            Dim xml As String
            Dim doc As New XmlDocument()
            xml = "<?xml version='1.0' encoding='ISO-8859-1' standalone='no'?>
                    <VoidedDocuments xmlns='urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1' 
                    xmlns:cac='urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2' 
                    xmlns:cbc='urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2' 
                    xmlns:ds='http://www.w3.org/2000/09/xmldsig#' xmlns:ext='urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2' 
                    xmlns:sac='urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1' 
                    xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
	                    <ext:UBLExtensions>
		                    <ext:UBLExtension>
			                    <ext:ExtensionContent>
			                    </ext:ExtensionContent>
		                    </ext:UBLExtension>
	                    </ext:UBLExtensions>
	                    <cbc:UBLVersionID>2.0</cbc:UBLVersionID>
	                    <cbc:CustomizationID>1.0</cbc:CustomizationID>
	                    <cbc:ID>" & comprobante.CODIGO & "-" & comprobante.SERIE & "-" & comprobante.SECUENCIA & "</cbc:ID>
	                    <cbc:ReferenceDate>" & comprobante.FECHA_REFERENCIA & "</cbc:ReferenceDate>
	                    <cbc:IssueDate>" & comprobante.FECHA_BAJA & "</cbc:IssueDate>
	                    <cac:Signature>
		                    <cbc:ID>" & comprobante.CODIGO & "-" & comprobante.SERIE & "-" & comprobante.SECUENCIA & "</cbc:ID>
		                    <cac:SignatoryParty>
			                    <cac:PartyIdentification>
				                    <cbc:ID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:ID>
			                    </cac:PartyIdentification>
			                    <cac:PartyName>
				                    <cbc:Name>" & comprobante.RAZON_SOCIAL & "</cbc:Name>
			                    </cac:PartyName>
		                    </cac:SignatoryParty>
		                    <cac:DigitalSignatureAttachment>
			                    <cac:ExternalReference>
				                    <cbc:URI>" & comprobante.CODIGO & "-" & comprobante.SERIE & "-" & comprobante.SECUENCIA & "</cbc:URI>
			                    </cac:ExternalReference>
		                    </cac:DigitalSignatureAttachment>
	                    </cac:Signature>
	                    <cac:AccountingSupplierParty>
		                    <cbc:CustomerAssignedAccountID>" & comprobante.NRO_DOCUMENTO_EMPRESA & "</cbc:CustomerAssignedAccountID>
		                    <cbc:AdditionalAccountID>" & comprobante.TIPO_DOCUMENTO & "</cbc:AdditionalAccountID>
		                    <cac:Party>
			                    <cac:PartyLegalEntity>
				                    <cbc:RegistrationName>" & comprobante.RAZON_SOCIAL & "</cbc:RegistrationName>
			                    </cac:PartyLegalEntity>
		                    </cac:Party>
	                    </cac:AccountingSupplierParty>"
            For x As Integer = 0 To comprobante.detalle.Count - 1
                xml = xml & "<sac:VoidedDocumentsLine>
		                     <cbc:LineID>" & comprobante.detalle(x).ITEM & "</cbc:LineID>
		                     <cbc:DocumentTypeCode>" & comprobante.detalle(x).TIPO_COMPROBANTE & "</cbc:DocumentTypeCode>
		                     <sac:DocumentSerialID>" & comprobante.detalle(x).SERIE & "</sac:DocumentSerialID>
		                     <sac:DocumentNumberID>" & comprobante.detalle(x).NUMERO & "</sac:DocumentNumberID>
		                     <sac:VoidReasonDescription>" & comprobante.detalle(x).DESCRIPCION & "</sac:VoidReasonDescription>
	                         </sac:VoidedDocumentsLine>"
            Next
            xml = xml & "</VoidedDocuments>"
            doc.LoadXml(xml)
            doc.Save(ruta & nomArchivo & ".XML")
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "1")
            dictionary.Add("mensaje", "EL XML SE CREO CORRECTAMENTE")
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        Catch ex As Exception
            dictionary = New Dictionary(Of String, String)
            dictionary.Add("flg_rta", "0")
            dictionary.Add("mensaje", "ERROR AL CREAR EL XML: " & ex.Message)
            dictionary.Add("cod_sunat", "")
            dictionary.Add("msj_sunat", "")
            dictionary.Add("hash_cdr", "")
            dictionary.Add("hash_cpe", "")
        End Try
        Return dictionary
    End Function





End Class
