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
    public class ObtenerDatosIol
    {
        private readonly NgCapitalApiDbContext     _context;
        private readonly IConfiguration            _configuration;
        private readonly ILogger<AuthController>   _logger;
        private readonly HttpClient                _httpClient;
        private Cedears                            _cedears;
        private Bonos                              _bonos;
        private ObligacionesNegociables            _obligacionesNegociables;
        private Acciones                           _acciones;

        public ObtenerDatosIol( NgCapitalApiDbContext context
                               ,IConfiguration configuration
                               ,ILogger<AuthController> logger
                               ,HttpClient httpClient
                               ,Cedears cedears
                               ,Bonos bonos
                               ,ObligacionesNegociables obligacionesNegociables
                               ,Acciones acciones
                              )
        {
            _context                 = context;
            _configuration           = configuration;
            _logger                  = logger;
            _httpClient              = httpClient;
            _cedears                 = cedears;
            _bonos                   = bonos;
            _obligacionesNegociables = obligacionesNegociables;
            _acciones                = acciones;
        }

        public async Task Obtener()
        {
            try
            {
                var authIol = _configuration.GetSection("AuthIol").Get<AuthIolDto>();

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
                    var loginResponse = JsonConvert.DeserializeObject<IolLoginResponse>(responseContent);

                    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Cotizaciones");

                    DateTime fechaRegistro = DateTime.Now;

                    // Obtengo Cedears con descripcion optimizada
                    await _cedears.ObtenerCedear(loginResponse.AccessToken, fechaRegistro);

                    // Obtengo Bonos con descripcion optimizada
                    await _bonos.ObtenerBonos(loginResponse.AccessToken, fechaRegistro);

                    // Obtengo Bonos con descripcion optimizada
                    await _obligacionesNegociables.ObtenerON(loginResponse.AccessToken, fechaRegistro);

                    // Obtengo Bonos con descripcion optimizada
                    await _acciones.ObtenerAcciones(loginResponse.AccessToken, fechaRegistro);
                }
                else
                {
                    _logger.LogError("Error: " + response.RequestMessage);
                   // return false;

                }
            }
            catch (System.Exception e)
            {
                _logger.LogError("Error: " + e.Message);
                //return false;

            }
        }
    }
}
