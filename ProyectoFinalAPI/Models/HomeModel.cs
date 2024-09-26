
using ProyectoFinalAPI.App_Start;
using ProyectoFinalAPI.Entities;
using ProyectoFinalAPI.ModeloDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace ProyectoFinalAPI.Models
{
    public class HomeModel
    {
        TokenGenerator modelToken = new TokenGenerator();
        public UsuariosEnt ValidarUsuario(UsuariosEnt entidad)
        {
            using (var conexion = new PruebaPracEntities())
            {

                var datosBD = (from x in conexion.Usuario
                               where x.Usuario1 == entidad.usuario
                                  && x.Contrasena == entidad.Contrasena
                               select x).FirstOrDefault();

                if (datosBD != null)
                {
                    UsuariosEnt respuesta = new UsuariosEnt();
                    respuesta.id = datosBD.Id;
                    respuesta.usuario = datosBD.Usuario1;
                    respuesta.Contrasena = datosBD.Contrasena;
                    respuesta.Token = modelToken.GenerateTokenJwt(datosBD.Id.ToString());
                    return respuesta;
                }

                return null;
            }
        }

        public ResultadoBusqueda BuscarTodosLosArticulos()
        {
            var resultado = new ResultadoBusqueda
            {
                Articulos = new List<ArticuloEnt>()
            };

            using (var conexion = new PruebaPracEntities())
            {
                using (SqlConnection conn = new SqlConnection(conexion.Database.Connection.ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT id, Codigo, Nombre, Precio, AplicaIVA FROM Articulos", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var articulo = new ArticuloEnt
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Codigo = reader["Codigo"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Precio = reader.IsDBNull(reader.GetOrdinal("Precio"))
                                            ? 0m
                                            : reader.GetDecimal(reader.GetOrdinal("Precio")),
                                    AplicaIVA = reader.GetBoolean(reader.GetOrdinal("AplicaIVA"))
                                };
                                resultado.Articulos.Add(articulo);
                            }
                        }
                    }
                }
            }

            return resultado;
        }

        public int AgregarArticulo(ArticuloEnt entidad)
        {
            using (var conexion = new PruebaPracEntities())
            {
                return conexion.AgregarArticulo(entidad.Codigo, entidad.Nombre, entidad.Precio, entidad.AplicaIVA);
            }
        }

        private string _codigoProducto;

        public ResultadoBusqueda BuscarNombreProducto(string codigo1, int cantidad)
        {
            string nombreProducto = null; 
            decimal totalTodo = 0;
            _codigoProducto = codigo1; 
            var resultado = new ResultadoBusqueda
            {
                Facturas = new List<FacturaEnt>()
            };

            using (var conexion = new PruebaPracEntities())
            {
                using (SqlConnection conn = new SqlConnection(conexion.Database.Connection.ConnectionString))
                {
            
                    using (SqlCommand cmd = new SqlCommand("SELECT Nombre FROM Articulos WHERE Codigo = @codigo1", conn))
                    {
                        cmd.Parameters.Add("@codigo1", SqlDbType.NVarChar).Value = codigo1;

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                nombreProducto = reader["Nombre"].ToString();
                            }
                        }
                    }

                    
                    if (string.IsNullOrEmpty(nombreProducto))
                    {
                        throw new Exception("Producto no encontrado.");
                    }

                    
                    using (SqlCommand cmd = new SqlCommand("SP_AgregarProductoAFactura", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Codigo", SqlDbType.VarChar).Value = codigo1;
                        cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;

                        
                        SqlParameter totalTodoParam = new SqlParameter("@TotalTodo", SqlDbType.Decimal)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(totalTodoParam);

                       
                        cmd.ExecuteNonQuery(); 

                        
                        totalTodo = (decimal)totalTodoParam.Value; 
                    }

                   
                    using (SqlCommand cmd = new SqlCommand("SELECT Codigo, Nombre, Precio, Cantidad, Total, Iva FROM Factura", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var factura = new FacturaEnt
                                {
                                    Codigo = reader["Codigo"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Precio = reader.IsDBNull(reader.GetOrdinal("Precio"))
                                        ? 0m 
                                        : reader.GetDecimal(reader.GetOrdinal("Precio")),
                                    Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
                                    Total = reader.IsDBNull(reader.GetOrdinal("Total"))
                                        ? 0m  
                                        : reader.GetDecimal(reader.GetOrdinal("Total")),
                                    Iva = reader.GetBoolean(reader.GetOrdinal("Iva")),
                                };
                                resultado.Facturas.Add(factura); 
                            }
                        }
                    }
                }
            }

          
            resultado.NombreProducto = nombreProducto;
            resultado.TotalTodo = totalTodo; 

            return resultado; 
        }

        public void EliminarProducto(string codigoEli)
        {
            using (var conexion = new PruebaPracEntities())
            {
                using (SqlConnection conn = new SqlConnection(conexion.Database.Connection.ConnectionString))
                {
                    
                    string query = "DELETE FROM Factura WHERE Codigo = @Codigo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@Codigo", SqlDbType.VarChar).Value = codigoEli;

                        conn.Open();
                        try
                        {
                            int filasAfectadas = cmd.ExecuteNonQuery();

                            if (filasAfectadas > 0)
                            {
                                Console.WriteLine("Producto eliminado con éxito.");
                            }
                            else
                            {
                                Console.WriteLine("No se encontró un producto con el código especificado.");
                            }
                        }
                        catch (SqlException ex)
                        {
                            
                            Console.WriteLine($"Error al eliminar el producto: {ex.Message}");
                        }
                    }
                }
            }
        }

        public async Task<ResultadoBusqueda> ObtenerVerFacturasAsync()
        {
            var resultado = new ResultadoBusqueda
            {
                VerFactura = new List<FacturaEnt>()
            };

            using (var conexion = new PruebaPracEntities())
            {
                using (SqlConnection conn = new SqlConnection(conexion.Database.Connection.ConnectionString))
                {
                    await conn.OpenAsync(); 

                    using (SqlCommand cmd = new SqlCommand("SELECT Codigo, Nombre, Precio, Cantidad, Total, Iva FROM Factura", conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) 
                        {
                            while (await reader.ReadAsync())
                            {
                                var factura = new FacturaEnt
                                {
                                    Codigo = reader["Codigo"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Precio = reader.IsDBNull(reader.GetOrdinal("Precio"))
                                        ? 0m
                                        : reader.GetDecimal(reader.GetOrdinal("Precio")),
                                    Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
                                    Total = reader.IsDBNull(reader.GetOrdinal("Total"))
                                        ? 0m
                                        : reader.GetDecimal(reader.GetOrdinal("Total")),
                                    Iva = reader.GetBoolean(reader.GetOrdinal("Iva")),
                                };
                                resultado.VerFactura.Add(factura);
                            }
                        }
                    }

                    resultado.TotalTodo = resultado.VerFactura.Sum(f => f.Total);
                }
            }

            return resultado;
        }


        public void EliminarArticulo(string codigoArt)
        {
            using (var conexion = new PruebaPracEntities())
            {
                
                using (SqlConnection conn = new SqlConnection(conexion.Database.Connection.ConnectionString))
                {
                    
                    string query = "DELETE FROM Articulos WHERE Codigo = @Codigo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        
                        cmd.Parameters.Add("@Codigo", SqlDbType.VarChar).Value = codigoArt;

                        conn.Open(); 
                        try
                        {
                            
                            int filasAfectadas = cmd.ExecuteNonQuery();

                           
                            if (filasAfectadas > 0)
                            {
                                Console.WriteLine("Artículo eliminado con éxito.");
                            }
                            else
                            {
                                Console.WriteLine("No se encontró un artículo con el código especificado.");
                            }
                        }
                        catch (SqlException ex)
                        {
                          
                            Console.WriteLine($"Error al eliminar el artículo: {ex.Message}");
                        }
                    }
                }
            }
        }

        public void EditarArticulo(string codigoEdit, string nuevoNombre, decimal nuevoPrecio)
        {
            using (var conexion = new PruebaPracEntities())
            {
                using (SqlConnection conn = new SqlConnection(conexion.Database.Connection.ConnectionString))
                {

                    string query = "SELECT * FROM Articulos WHERE Codigo = @Codigo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {

                        cmd.Parameters.Add("@Codigo", SqlDbType.VarChar).Value = codigoEdit;

                        conn.Open();


                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();

                                string nombreActual = reader["Nombre"].ToString();
                                decimal precioActual = Convert.ToDecimal(reader["Precio"]);
                                bool ivaActual = Convert.ToBoolean(reader["AplicaIVA"]); 
                                
                                reader.Close();
                            
                                string updateQuery = "UPDATE Articulos SET Nombre = @NuevoNombre, Precio = @NuevoPrecio WHERE Codigo = @Codigo";

                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                {
                                    
                                    updateCmd.Parameters.Add("@NuevoNombre", SqlDbType.VarChar).Value = nuevoNombre;
                                    updateCmd.Parameters.Add("@NuevoPrecio", SqlDbType.Decimal).Value = nuevoPrecio;
                                    updateCmd.Parameters.Add("@Codigo", SqlDbType.VarChar).Value = codigoEdit; 

                                  
                                    int filasAfectadas = updateCmd.ExecuteNonQuery();

                                    if (filasAfectadas > 0)
                                    {
                                        Console.WriteLine("Artículo editado con éxito.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("No se pudo editar el artículo.");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("No se encontró un artículo con el código especificado.");
                            }
                        }
                    }
                }
            }
        }


    }
}















