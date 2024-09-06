namespace NgCapitalApi.Dtos
{
    public class UserSignUpDto
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}