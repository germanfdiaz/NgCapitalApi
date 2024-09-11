using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

using NgCapitalApi.Data;
using NgCapitalApi.Models;
using NgCapitalApi.Dtos;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly NgCapitalApiDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController( NgCapitalApiDbContext   context
                          ,IConfiguration          configuration
                          ,ILogger<AuthController> logger
                        )
    {
        _context       = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("signUp")]
    public IActionResult SignUp([FromBody] UserSignUpDto userSignUp)
    {
        _logger.LogInformation("Registro de nuevo usuario. Email " + userSignUp.Email + ", contraseña: " + userSignUp.Password);

        // Verificar si el usuario ya existe
        if (_context.Usuarios.Any(u => u.Email == userSignUp.Email))
        {
            _logger.LogError("El usuario ya existe.");
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

        _logger.LogInformation("Usuario " + userSignUp.Nombre + "registrado existosamente.");
        return Ok("Usuario registrado exitosamente.");
    }

    [HttpPost("SignIn")]
    public IActionResult SignIn([FromBody] UserSignInDto userSignIn)
    {
        _logger.LogInformation("Inicio de sesión: " + userSignIn.Email + " - " + userSignIn.Password);

        var user = _context.Usuarios.SingleOrDefault(u => u.Email == userSignIn.Email);
        if (user == null)
        {
            _logger.LogError("El usuario " + userSignIn.Email + " no existe.");
            return Unauthorized();
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userSignIn.Password, user.Password);
        if (isPasswordValid)
        {
            // La contraseña es correcta
            var jwt = _configuration.GetSection("Jwt").Get<JwtDto>();
            var tokenHandler = new JwtSecurityTokenHandler();
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

            _logger.LogInformation("Inicio de sesión exitoso de " + user.Nombre);
            return Ok(new { Token = tokenString });
        }
        else
        {
            _logger.LogError("La contraseña ingresada es incorrecta");
            // return new
            // {
            //     success = false,
            //     message = "",
            //     result =  ""
            // };
            
            return Unauthorized("La contraseña ingresada es incorrecta");
        }
    }
}