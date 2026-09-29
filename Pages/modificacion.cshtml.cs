
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace des1lab3.Pages
{
    public class ModificacionModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ModificacionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public int IdSeleccionado { get; set; }

        [BindProperty]
        public int IdProducto { get; set; }

        [BindProperty]
        public string Nombre { get; set; } = "";

        [BindProperty]
        public string Precio { get; set; } = "";

        [BindProperty]
        public int Categoria { get; set; }

        [BindProperty]
        public string NombreOriginal { get; set; } = "";

        [BindProperty]
        public string PrecioOriginal { get; set; } = "";

        [BindProperty]
        public int CategoriaOriginal { get; set; }

        public List<ProductoOpcion> Productos { get; set; } = new();

        public List<CategoriaItem> Categorias { get; set; } = new();

        public string Mensaje { get; set; } = "";

        public void OnGet()
        {
            CargarListas();
        }

        public IActionResult OnPostCargar()
        {
            CargarListas();

            if (IdSeleccionado == 0)
            {
                Mensaje = "Seleccione un producto.";
                return Page();
            }

            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                conexion.Open();
                string consulta = "SELECT nombre, precio, categoria FROM productos WHERE idProducto = @id";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@id", IdSeleccionado);

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (!lector.Read())
                        {
                            Mensaje = "No se encontro el producto.";
                            return Page();
                        }

                        IdProducto = IdSeleccionado;
                        Nombre = lector.GetString(0);
                        Precio = lector.GetDecimal(1).ToString();
                        Categoria = lector.GetInt32(2);

                        NombreOriginal = Nombre;
                        PrecioOriginal = Precio;
                        CategoriaOriginal = Categoria;
                    }
                }
            }

            return Page();
        }

        public IActionResult OnPostGuardar()
        {
            CargarListas();
            IdSeleccionado = IdProducto;

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                Mensaje = "Ingrese el nombre.";
                return Page();
            }

            if (!decimal.TryParse(Precio, out decimal precio) || precio <= 0)
            {
                Mensaje = "Ingrese un precio valido.";
                return Page();
            }

            if (Categoria == 0)
            {
                Mensaje = "Seleccione una categoria.";
                return Page();
            }

            decimal.TryParse(PrecioOriginal, out decimal precioOriginal);


            if (Nombre.Trim() == NombreOriginal.Trim()
                && precio == precioOriginal
                && Categoria == CategoriaOriginal)
            {
                Mensaje = "Debe modificar al menos un campo.";
                return Page();
            }

            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(cadena))
                {
                    conexion.Open();
                    string consulta = @"UPDATE productos
                                        SET nombre = @nombre, precio = @precio, categoria = @categoria
                                        WHERE idProducto = @id";

                    using (SqlCommand comando = new SqlCommand(consulta, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", Nombre.Trim());
                        comando.Parameters.AddWithValue("@precio", precio);
                        comando.Parameters.AddWithValue("@categoria", Categoria);
                        comando.Parameters.AddWithValue("@id", IdProducto);
                        comando.ExecuteNonQuery();
                    }
                }

                NombreOriginal = Nombre.Trim();
                PrecioOriginal = Precio;
                CategoriaOriginal = Categoria;
                Nombre = Nombre.Trim();
                Mensaje = "Producto modificado correctamente.";
            }
            catch (Exception)
            {
                Mensaje = "No se pudo modificar el producto.";
            }

            return Page();
        }

        private void CargarListas()
        {
            Productos = new List<ProductoOpcion>();
            Categorias = new List<CategoriaItem>();
            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand("SELECT idProducto, nombre FROM productos", conexion))
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

                using (SqlCommand comando = new SqlCommand("SELECT idCategoria, descripcion FROM categorias", conexion))
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

    public class ProductoOpcion
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
    }
}