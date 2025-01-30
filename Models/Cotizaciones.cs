using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NgCapitalApi.Models
{
    public class Cotizacion
    { 
        [JsonPropertyName("titulos")] public List<Titulo> Titulos { get; set; } 
    }
    public class Titulo 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 
        public string Instrumento { get; set; }
        public string Simbolo { get; set; }

        [JsonProperty("puntaCantidadCompra")]
        public decimal? PuntaCantidadCompra { get; set; }

        [JsonProperty("puntaPrecioCompra")]
        public decimal? PuntaPrecioCompra { get; set; }

        [JsonProperty("puntaPrecioVenta")]
        public decimal? PuntaPrecioVenta { get; set; }

        [JsonProperty("puntaCantidadVenta")]
        public decimal? PuntaCantidadVenta { get; set; }
        public decimal? UltimoPrecio { get; set; }
        public decimal? VariacionPorcentual { get; set; } 
        public decimal? Apertura { get; set; } 
        public decimal? Maximo { get; set; } 
        public decimal? Minimo { get; set; } 
        public decimal? UltimoCierre { get; set; } 
        public double? Volumen { get; set; } 
        public decimal? CantidadOperaciones { get; set; } 
        public DateTime? Fecha { get; set; } 
        public string? TipoOpcion { get; set; } 
        public decimal? PrecioEjercicio { get; set; } 
        public DateTime? FechaVencimiento { get; set; } 
        public string? Mercado { get; set; } 
        public string? Moneda { get; set; } 
        public string? Descripcion { get; set; }
        public string? DescripcionOriginal { get; set; }
        public string? Plazo { get; set; } 
        public int? LaminaMinima { get; set; }

        [JsonProperty("lote")]
        public int? Lote { get; set; } 
        public DateTime? FechaRegistro { get; set; }

    }
}
