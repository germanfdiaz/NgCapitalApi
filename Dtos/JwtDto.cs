namespace NgCapitalApi.Dtos
{
    public class JwtDto
    { 
        public required string Key { get; set; }
        public required string Algorithm { get; set; }
        public int Expiration { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
    }
}