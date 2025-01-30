namespace NgCapitalApi.Dtos
{
    public class UserSignInDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class UserSignUpDto
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    public class UserChangePasswordDto
    {
        public required string Email { get; set; }
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}