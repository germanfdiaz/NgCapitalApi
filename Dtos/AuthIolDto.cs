using Newtonsoft.Json;

namespace NgCapitalApi.Dtos
{
    public class AuthIolDto
    { 
        public string ? Username { get; set; }
        public string ? Password { get; set; }
        public string ? GrantType { get; set; }
        public string ? UrlLogin { get; set; }
        public string ? UrlCotizacionesCedearsArgentinaTodos { get; set; }
        public string ? UrlCotizacionesBonosArgentinaTodos { get; set; }
        public string ? UrlCotizacionesOnArgentinaTodos { get; set; }
        public string? UrlCotizacionesAccionesArgentinaTodos { get; set; }
    }

    public class IolLoginResponse 
    {
        [JsonProperty("access_token")]
        public string ? AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string ? TokenType { get; set; }
        [JsonProperty(".expires")]
        public DateTime ? Expires { get; set; }

    }
}