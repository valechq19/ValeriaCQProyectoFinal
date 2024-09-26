using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoFinalAPI.Entities
{
    public class ArticuloEnt
    {
        public int id { get; set; }
        public string Codigo { get; set; } 
        public string Nombre { get; set; } 
        public decimal Precio { get; set; } 
        public bool AplicaIVA { get; set; }
       
    }
}