using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NgCapitalApi.Dtos;
using NgCapitalApi.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using NgCapitalApi.Data;
using System.Net.Http.Headers;

namespace NgCapitalApi.Core
{
    public class ObligacionesNegociables
    {
        private readonly NgCapitalApiDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly HttpClient _httpClient;
        public ObligacionesNegociables( NgCapitalApiDbContext context
                                       , IConfiguration configuration
                                       , ILogger<AuthController> logger
                                       , HttpClient httpClient
                                      )
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task ObtenerON(string token, DateTime fechaRegistro)
        {
            try
            {
                /*var authIol = _configuration.GetSection("AuthIol").Get<AuthIolDto>();

                var requestBody = new
                {
                    username = authIol.Username,
                    password = authIol.Password,
                    grant_type = authIol.GrantType
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(authIol.UrlLogin, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<IolLoginResponse>(responseContent);*/

                var authIol = _configuration.GetSection("AuthIol").Get<AuthIolDto>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var responseCotizaciones = await _httpClient.GetAsync(authIol.UrlCotizacionesOnArgentinaTodos/*, content*/);

                    if (responseCotizaciones.IsSuccessStatusCode)
                    {
                        var responseContentCotizaciones = await responseCotizaciones.Content.ReadAsStringAsync();

                        string json = responseContentCotizaciones;

                        JObject originalJson = JObject.Parse(json);
                        JArray titulos = (JArray)originalJson["titulos"];
                        JArray transformedTitulos = new JArray();

                        foreach (var titulo in titulos)
                        {
                            JObject transformedTitulo = new JObject();
                            transformedTitulo["instrumento"] = "ON";
                            transformedTitulo["simbolo"] = titulo["simbolo"];
                            var puntas = titulo["puntas"];
                            if (puntas != null && puntas.HasValues)
                            {
                                transformedTitulo["puntaCantidadCompra"] = puntas["cantidadCompra"];
                                transformedTitulo["puntaPrecioCompra"] = puntas["precioCompra"];
                                transformedTitulo["puntaPrecioVenta"] = puntas["precioVenta"];
                                transformedTitulo["puntaCantidadVenta"] = puntas["cantidadVenta"];
                            }
                            else
                            {
                                transformedTitulo["puntaCantidadCompra"] = null;
                                transformedTitulo["puntaPrecioCompra"] = null;
                                transformedTitulo["puntaPrecioVenta"] = null;
                                transformedTitulo["puntaCantidadVenta"] = null;
                            }
                            transformedTitulo["ultimoPrecio"] = titulo["ultimoPrecio"];
                            transformedTitulo["variacionPorcentual"] = titulo["variacionPorcentual"];
                            transformedTitulo["apertura"] = titulo["apertura"];
                            transformedTitulo["maximo"] = titulo["maximo"];
                            transformedTitulo["minimo"] = titulo["minimo"];
                            transformedTitulo["ultimoCierre"] = titulo["ultimoCierre"];
                            transformedTitulo["volumen"] = titulo["volumen"];
                            transformedTitulo["cantidadOperaciones"] = titulo["cantidadOperaciones"];
                            transformedTitulo["fecha"] = titulo["fecha"];
                            transformedTitulo["tipoOpcion"] = titulo["tipoOpcion"];
                            transformedTitulo["precioEjercicio"] = titulo["precioEjercicio"];
                            transformedTitulo["fechaVencimiento"] = titulo["fechaVencimiento"];
                            transformedTitulo["mercado"] = titulo["mercado"];
                            transformedTitulo["moneda"] = titulo["moneda"].ToString() == "1" ? "ARS" : "USD"; 
                            transformedTitulo["descripcion"] = titulo["descripcion"].ToString().ToUpper().Trim().Replace("  ", " ").Replace("U$S CG", "").Replace("$ CG", "").Replace(" VTO ", " ").Replace(" VTO. ", " ").Replace(" V.", " ").Replace(".", "").Replace(",", "").Replace("(", "").Replace(")", "").Trim();
                            transformedTitulo["descripcionOriginal"] = titulo["descripcion"].ToString().ToUpper().Trim();
                            transformedTitulo["plazo"] = titulo["plazo"];
                            transformedTitulo["laminaMinima"] = titulo["laminaMinima"];
                            transformedTitulo["lote"] = titulo["lote"];
                            transformedTitulo["fechaRegistro"] = fechaRegistro;

                        transformedTitulos.Add(transformedTitulo);
                        }

                        JObject resultJson = new JObject
                        {
                            ["titulos"] = transformedTitulos
                        };

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };

                        string transformedJson = JsonConvert.SerializeObject(resultJson, Formatting.Indented);
                        //string transformedJson = resultJson.ToString(Formatting.Indented);

                        Console.WriteLine("Transformed JSON: ");
                        Console.WriteLine(transformedJson);

                        //return Ok(transformedJson);
                        //Cotizacion cotizaciones = JsonSerializer.Deserialize<Cotizacion>(transformedJson);
                        Cotizacion cotizaciones = JsonConvert.DeserializeObject<Cotizacion>(transformedJson);

                        
                        _context.Cotizaciones.AddRange(cotizaciones.Titulos);
                        await _context.SaveChangesAsync();


                        //List<Cotizacion> cotizaciones = JsonSerializer.Deserialize<List<Cotizacion>>(responseContentCotizaciones, opcionesJson);

                        //return Ok(responseContentCotizaciones);
                        //return true; //transformedJson;

                    }
                    else
                    {
                        _logger.LogError("Error cotizaciones:" + responseCotizaciones.RequestMessage);
                        //return false;

                    }

              /*  }
                else
                {
                    _logger.LogError("Error: " + response.RequestMessage);
                   // return false;

                }*/
            }
            catch (System.Exception e)
            {
                _logger.LogError("Error: " + e.Message);
                //return false;

            }
        }
    }
}
