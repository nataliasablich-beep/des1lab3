using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace des1lab3.Pages
{
    public class AltaModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AltaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string Nombre { get; set; } = "";

        [BindProperty]
        public string Precio { get; set; } = "";

        [BindProperty]
        public int Categoria { get; set; }

        public List<CategoriaItem> Categorias { get; set; } = new();
        public bool Correcto { get; set; } = true;
        public string Mensaje { get; set; } = "";

        public void OnGet()
        {
            CargarCategorias();
        }

        public IActionResult OnPost()
        {
            CargarCategorias();

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                Mensaje = "Ingrese el nombre.";
                Correcto = false;
                return Page();
            }

            if (!decimal.TryParse(Precio, out decimal precio) || precio <= 0)
            {
                Mensaje = "Ingrese un precio valido.";
                Correcto = false;
                return Page();
            }

            if (Categoria == 0)
            {
                Mensaje = "Seleccione una categoria.";
                Correcto = false;
                return Page();
            }

            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    string consulta = "INSERT INTO productos (nombre, precio, categoria) VALUES (@nombre, @precio, @categoria)";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", Nombre);
                        comando.Parameters.AddWithValue("@precio", Precio);
                        comando.Parameters.AddWithValue("@categoria", Categoria);
                        comando.ExecuteNonQuery();
                    }
                }

                Mensaje = "Producto guardado correctamente.";
                Nombre = "";
                Precio = "";
                Categoria = 0;
            }
            catch (Exception)
            {
                Mensaje = "No se pudo guardar el producto.";
                Correcto = false;
            }

            return Page();
        }

        private void CargarCategorias()
        {
            Categorias = new List<CategoriaItem>();
            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                conexion.Open();
                string consulta = "SELECT idCategoria, descripcion FROM categorias";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        Categorias.Add(new CategoriaItem
                        {
                            IdCategoria = lector.GetInt32(0),
                            Descripcion = lector.GetString(1)
                        });
                    }
                }
            }
        }
    }

    public class CategoriaItem
    {
        public int IdCategoria { get; set; }
        public string Descripcion { get; set; } = "";
    }
}