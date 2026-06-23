using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace businessEntities
{
    //ˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆ
    //Creado por     : Jose Zambrano  (25/06/2018)
    //ˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆˆ
    // <summary> Entidad = CPE_DETALLE </summary> //
    public class CPE_GUIA_REMISION_DETALLE
    {
        public Nullable<int> ID_DETALLE { get; set; }
        public Nullable<int> ID_CABECERA { get; set; }
        public Nullable<int> ITEM { get; set; }
        public string UNIDAD_MEDIDA { get; set; }
        public Nullable<decimal> CANTIDAD { get; set; }
        public Nullable<int> ORDER_ITEM { get; set; }//TAG (OrderLineReference) se colocara el mismo dato que del item
        public string CODIGO { get; set; }
        public string DESCRIPCION { get; set; }
    }
}