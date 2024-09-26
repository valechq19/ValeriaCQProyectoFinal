using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoFinalAPI.Entities
{
    public class ResultadoBusqueda
    {
        public string NombreProducto { get; set; }
        public decimal TotalTodo { get; set; }
        public List<FacturaEnt> Facturas { get; set; }
        public List<ArticuloEnt> Articulos { get; set; }
        public string Mensaje { get; set; }
        public List<FacturaEnt> VerFactura { get; set; } 
        
    }
}