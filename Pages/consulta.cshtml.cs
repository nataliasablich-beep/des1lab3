using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace des1lab3.Pages
{
    public class ConsultaModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ConsultaModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ProductoItem> Productos { get; set; } = new();

        public void OnGet()
        {
            string cadena = _configuration.GetConnectionString("RinconDelMate") ?? "";

            using (SqlConnection conexion = new SqlConnection(cadena))
            {
                conexion.Open();
                string consulta = @"SELECT p.idProducto, p.nombre, p.precio, c.descripcion
                                    FROM productos p
                                    INNER JOIN categorias c ON p.categoria = c.idCategoria";

                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        Productos.Add(new ProductoItem
                        {
                            IdProducto = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            Precio = lector.GetDecimal(2),
                            Categoria = lector.GetString(3)
                        });
                    }
                }
            }
        }
    }

    public class ProductoItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = "";
    }
}