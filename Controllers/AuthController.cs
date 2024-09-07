using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NgCapitalApi.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NgCapitalApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using NgCapitalApi.Models;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly NgCapitalApiDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController( NgCapitalApiDbContext context
                          ,IConfiguration        configuration
                        )
    {
        _context       = context;
        _configuration = configuration;
    }

    [HttpPost("signUp")]
    public IActionResult SignUp([FromBody] UserSignUpDto userSignUp)
    {
        // Verificar si el usuario ya existe
        if (_context.Usuarios.Any(u => u.Email == userSignUp.Email))
        {
            return BadRequest("El usuario ya existe.");
        }

        // Encriptar la contraseña
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(userSignUp.Password);

        var usuario = new Usuario
        {
            Nombre = userSignUp.Nombre,
            Email = userSignUp.Email,
            Password = passwordHash,
        };
        
        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        return Ok("Usuario registrado exitosamente.");
    }

    [HttpPost("SignIn")]
    public IActionResult SignIn([FromBody] UserSignInDto userSignIn)
    {
        var user = _context.Usuarios.SingleOrDefault(u => u.Email == userSignIn.Email);
        if (user == null)
            return Unauthorized();

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userSignIn.Password, user.Password);
        if (isPasswordValid)
        {
            // La contraseña es correcta
            var jwt = _configuration.GetSection("Jwt").Get<JwtDto>();
            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes("GermanFernandoDiaz20304246309110");//Encoding.ASCII.GetBytes("GermanFernandoDiaz20304246309110883");//_configuration["Jwt:Key"]==""?"GermanFernandoDiaz20304246309110883":"GermanFernandoDiaz20304246309110883");
            //var key = Encoding.UTF8.GetBytes("GermanFernandoDiaz20304246309110");
            var key = Encoding.UTF8.GetBytes(jwt!.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Aca se guarda todo lo que queres que tenga el token
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())//,
                    //new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject)
                    //new Claim(JwtRegisteredClaimNames.Jti, Guid.BewGuid().ToString()),
                    //new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    //new Claim("id", userLogin.id),
                    //new Claim("email", userLogin.Email),
                }),
                Expires = DateTime.UtcNow.AddHours(jwt.Expiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) //.HmacSha256Signature
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
        else
        {
            // return new
            // {
            //     success = false,
            //     message = "",
            //     result =  ""
            // };
            return Unauthorized();
        }
    }
}