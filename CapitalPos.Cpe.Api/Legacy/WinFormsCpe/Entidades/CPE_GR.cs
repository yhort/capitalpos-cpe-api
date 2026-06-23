using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace businessEntities
{
    public class CPE_GR
    {
        //=================DATOS DE ENVIO=================
        public Nullable<int> ITEM_ENVIO { get; set; }

        public string COD_MOTIVO_TRASLADO { get; set; }//TAG (HandlingCode) catalogo 20
        public string DESCRIPCION_MOTIVO_TRASLADO { get; set; }//TAG (Information) catalogo 20
        public string COD_UND_PESO_BRUTO { get; set; }//TAG (GrossWeightMeasure) catalogo 3
        public Nullable<decimal> PESO_BRUTO { get; set; }
        public Nullable<int> TOTAL_BULTOS { get; set; }//TAG (TotalTransportHandlingUnitQuantity) cantidad total de bultos o palet's
    }
}
