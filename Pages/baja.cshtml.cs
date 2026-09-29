using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace des1lab3.Pages
{
    public class BajaModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public BajaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public List<int> Seleccionados { get; set; } = new();

        public List<ProductoOpcion> Productos { get; set; } = new();

        public string Mensaje { get; set; } = "";
        public bool Correcto { get; set; }

        public void OnGet()
        {
            CargarProductos();
        }

        public IActionResult OnPost()
        {
            if (Seleccionados == null || Seleccionados.Count == 0)
            {
                Mensaje = "Seleccione un producto.";
                CargarProductos();
                return Page();
            }

            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();

                    foreach (int id in Seleccionados)
                    {
                        string consulta = "DELETE FROM productos WHERE idProducto = @id";

                        using (SqlCommand comando = new SqlCommand(consulta, conexion))
                        {
                            comando.Parameters.AddWithValue("@id", id);
                            comando.ExecuteNonQuery();
                        }
                    }
                }

                Mensaje = "Se eliminaron los productos seleccionados.";
                Correcto = true;
            }
            catch (Exception)
            {
                Mensaje = "No se pudo eliminar el producto.";
            }

            CargarProductos();
            return Page();
        }

        private void CargarProductos()
        {
            Productos = new List<ProductoOpcion>();
            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                conexion.Open();
                string consulta = "SELECT idProducto, nombre FROM productos";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        Productos.Add(new ProductoOpcion
                        {
                            IdProducto = lector.GetInt32(0),
                            Nombre = lector.GetString(1)
                        });
                    }
                }
            }
        }
    }
}