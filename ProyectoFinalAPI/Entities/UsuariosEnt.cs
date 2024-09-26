using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoFinalAPI.Entities
{
    public class UsuariosEnt
    {
        public long id { get; set; }

        public string usuario { get; set; }
        public string Contrasena { get; set; }
        public string Token { get; set; }
    }
}