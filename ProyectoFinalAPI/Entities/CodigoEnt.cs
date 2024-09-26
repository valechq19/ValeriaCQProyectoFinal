using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoFinalAPI.Entities
{
    public class CodigoEnt
    {
        public string codigo { get; set; }
        public string codigoIngre { get; set; }

        public int cantidad { get; set; }
    }
}