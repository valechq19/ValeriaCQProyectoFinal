using Newtonsoft.Json;
using ProyectoFinalAPI.Entities;
using ProyectoFinalAPI.ModeloDB;
using ProyectoFinalAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ProyectoFinalAPI.Controllers
{
    public class UsuariosController : ApiController
    {
        HomeModel model = new HomeModel();



        [HttpPost]
        [AllowAnonymous]
        [Route("api/ValidarUsuario")]
        public UsuariosEnt ValidarUsuario(UsuariosEnt entidad)
        {
            return model.ValidarUsuario(entidad);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/BuscarTodosLosArticulos")]
        public IHttpActionResult BuscarTodosLosArticulos()
        {
            try
            {
                var resultado = model.BuscarTodosLosArticulos();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/AgregarArticulo")]
        public int AgregarArticulo(ArticuloEnt entidad)
        {
            return model.AgregarArticulo(entidad);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/BuscarNombreProducto")]
        public ResultadoBusqueda BuscarNombreProducto()
        {
        
            var codigoEnt = new CodigoEnt();

            
            using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                var json = reader.ReadToEnd();
                codigoEnt = JsonConvert.DeserializeObject<CodigoEnt>(json);
            }

           
            string codigo1 = codigoEnt?.codigo; 
            int cantidad = codigoEnt?.cantidad ?? 0;

        
            if (string.IsNullOrEmpty(codigo1))
            {
                return new ResultadoBusqueda
                {
                    Mensaje = "El código del producto no puede estar vacío o ser nulo.",
                    Facturas = new List<FacturaEnt>() 
                };
            }

           
            if (cantidad <= 0)
            {
                return new ResultadoBusqueda
                {
                    Mensaje = "La cantidad debe ser mayor que cero.",
                    Facturas = new List<FacturaEnt>() 
                };
            }

           
            return model.BuscarNombreProducto(codigo1, cantidad); 
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/EliminarProducto")]
        public IHttpActionResult EliminarProducto()
        {
            var codigoEnt = new CodigoEnt();

           
            using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                var json = reader.ReadToEnd();
                codigoEnt = JsonConvert.DeserializeObject<CodigoEnt>(json);
            }

            string codigoEli = codigoEnt?.codigo;

            
            if (string.IsNullOrEmpty(codigoEli))
            {
                return BadRequest("El código del producto no puede estar vacío o ser nulo.");
            }

            try
            {
                model.EliminarProducto(codigoEli); 
                return Ok(); 
            }
            catch (Exception ex)
            {
                
                return InternalServerError(new Exception($"Error al eliminar el producto: {ex.Message}"));
            }
        }

        [HttpGet]
        [Route("api/ObtenerVerFacturas")]
        public async Task<IHttpActionResult> ObtenerVerFacturas() 
        {
            try
            {
          
                var resultado = await model.ObtenerVerFacturasAsync();

                if (resultado != null)
                {
                    return Ok(resultado); 
                }

                return NotFound(); 
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); 
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/EliminarArticulo")]
        public IHttpActionResult EliminarArticulo()
        {
            var codigoEnt = new CodigoEnt();


            using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                var json = reader.ReadToEnd();
                codigoEnt = JsonConvert.DeserializeObject<CodigoEnt>(json);
            }

            string codigoArt = codigoEnt?.codigo;


            if (string.IsNullOrEmpty(codigoArt))
            {
                return BadRequest("El código del artículo no puede estar vacío o ser nulo.");
            }

            try
            {
                model.EliminarArticulo(codigoArt);
                return Ok();
            }
            catch (Exception ex)
            {

                return InternalServerError(new Exception($"Error al eliminar el artículo: {ex.Message}"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/EditarArticulo")]
        public IHttpActionResult EditarArticulo()
        {
            var articuloEnt = new ArticuloEnt();

            using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                var json = reader.ReadToEnd();
                articuloEnt = JsonConvert.DeserializeObject<ArticuloEnt>(json);
            }

            string codigoEdit = articuloEnt?.Codigo;
            string nuevoNombre = articuloEnt?.Nombre;
            decimal nuevoPrecio = articuloEnt?.Precio ?? 0;

            if (string.IsNullOrEmpty(codigoEdit))
            {
                return BadRequest("El código del artículo no puede estar vacío o ser nulo.");
            }

            if (string.IsNullOrEmpty(nuevoNombre))
            {
                return BadRequest("El nombre del artículo no puede estar vacío.");
            }

            if (nuevoPrecio <= 0)
            {
                return BadRequest("El precio del artículo debe ser mayor que cero.");
            }

            try
            {
                model.EditarArticulo(codigoEdit, nuevoNombre, nuevoPrecio);
                return Ok("Artículo editado con éxito.");
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"Error al editar el artículo: {ex.Message}"));
            }
        }

    }
}











