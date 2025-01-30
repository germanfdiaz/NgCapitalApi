using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.Extensions.Configuration;

using NgCapitalApi.Data;
using NgCapitalApi.Models;
using NgCapitalApi.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using static NgCapitalApi.Models.Cotizacion;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System;
using Newtonsoft.Json.Linq;
using System.Xml.Schema;
using Core.Cotizaciones;
using System.Text.Json.Serialization;
using NgCapitalApi.Core;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly NgCapitalApiDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly HttpClient _httpClient;
    private Cedears _cedears;
    private Bonos _bonos;
    private ObligacionesNegociables _obligacionesNegociables;
    private Acciones _acciones;
    private ObtenerDatosIol _obtenerDatosIol;

    public AuthController( NgCapitalApiDbContext   context
                          ,IConfiguration          configuration
                          ,ILogger<AuthController> logger
                          ,HttpClient              httpClient
                          ,Cedears                 cedears
                          ,Bonos                   bonos
                          ,ObligacionesNegociables obligacionesNegociables
                          ,Acciones acciones
                          ,ObtenerDatosIol         obtenerDatosIol
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
        _obtenerDatosIol         = obtenerDatosIol;
    }

    [HttpPost("signUp")]
    public IActionResult SignUp([FromBody] UserSignUpDto userSignUp)
    {
        try
        {
            _logger.LogInformation("Registro de nuevo usuario. Email " + userSignUp.Email + ", contraseña: " + userSignUp.Password);

            var msg = "";

            // Verificar si el usuario ya existe
            if (_context.Usuarios.Any(u => u.Email == userSignUp.Email))
            {
                msg = "El usuario ya existe.";
                _logger.LogError( msg );
                return BadRequest( new { status = false, message = msg, data = "" });
            }

            // Verifica que la contraseña sea igual a la de confirmacion
            if (userSignUp.Password != userSignUp.ConfirmPassword)
            {
                msg = "La nueva contraseña y su confirmación deben ser iguales";
                _logger.LogError( msg );
                return BadRequest( new { status = false, message = msg, data = "" } );
            }

            // Encriptar la contraseña
            string passwordHash = BCrypt.Net.BCrypt.HashPassword( userSignUp.Password );

            var usuario = new Usuario
            {
                Nombre    = userSignUp.Nombre,
                Email     = userSignUp.Email,
                Password  = passwordHash,
            };
            
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            msg = "Usuario " + userSignUp.Nombre + " registrado existosamente.";
            _logger.LogInformation( msg );
            return Ok( new { status = true, message = msg, data = "" } );

        }
        catch (System.Exception e)
        {
            //throw;
            _logger.LogError( "Error: " + e.Message );  
            return BadRequest( new { status = false, message = e.Message, data = "" } );
        }
       
    }

    [HttpPost("SignIn")]
    public IActionResult SignIn([FromBody] UserSignInDto userSignIn)
    {
        try
        {
            _logger.LogInformation("Inicio de sesión: " + userSignIn.Email + " - " + userSignIn.Password);
            
            var msg = "";

            var user = _context.Usuarios.SingleOrDefault(u => u.Email == userSignIn.Email);
            if (user == null)
            {
                msg = "El usuario " + userSignIn.Email + " no existe.";
                _logger.LogError( msg );
                return Unauthorized( msg );
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userSignIn.Password, user.Password);
            if (!isPasswordValid)
            {
                msg = "La contraseña ingresada es incorrecta";
                _logger.LogError( msg );            
                return BadRequest( new { status = false, message = msg, data = "" } );
            }
                
            // Genero JWT
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

            msg = "Inicio de sesión exitoso de " + user.Nombre;
            _logger.LogInformation( msg );
            return Ok( new { Token = tokenString } );

        }
        catch (System.Exception e)
        {
            //throw;
            _logger.LogError( "Error: " + e.Message );  
            return BadRequest( new { status = false, message = e.Message, data = "" } );
        }
        
    }

    [Authorize]
    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto userChangePassword)
    {
        try
        {
            var msg = "";

             _logger.LogInformation("Cambio de contraseña en usuario. Email " + userChangePassword.Email + ", contraseña vieja: " + userChangePassword.OldPassword + ", contraseña nueva: " + userChangePassword.NewPassword);

            // Verificar si el usuario ya existe
            var user = _context.Usuarios.SingleOrDefault(u => u.Email == userChangePassword.Email);
            if (user == null)
            {
                msg = "El usuario " + userChangePassword.Email + " no existe.";
                _logger.LogError( msg );
                return BadRequest( new { status = false, message = msg, data = ""} );
            }

            // Verificar si la vieja contraseña es correcta
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userChangePassword.OldPassword, user.Password);
            if (!isPasswordValid)
            {
                msg = "La contraseña ingresada es incorrecta para el usuario " + userChangePassword.Email;
                _logger.LogError( msg );
                return BadRequest( new { status = false, message = msg, data = "" } );
            }

            // Verifica que la contraseña no sea la misma que la anterior
            if (userChangePassword.OldPassword == userChangePassword.NewPassword)
            {
                msg = "La nueva contraseña debe ser diferente a la contraseña actual";
                _logger.LogError( msg );
                return BadRequest( new { status = false, message = msg, data = "" } );
            }

            // Verifica que la contraseña sea igual a la de confirmacion
            if (userChangePassword.NewPassword != userChangePassword.ConfirmPassword)
            {
                msg = "La contraseña y la confirmación de contraseña deben ser iguales";
                _logger.LogError( msg );
                return BadRequest( new { status = false, message = msg, data = "" } );
            }

            // Encriptar la contraseña
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userChangePassword.NewPassword);
            
            var usuario = new Usuario
            {
                Id       = user.Id,
                Nombre   = user.Nombre,
                Email    = user.Email,
                Password = passwordHash,
            };
            // Desvincular la entidad existente
            _context.Entry(user).State = EntityState.Detached;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // Solo una forma diferente para el caso en donde no quiera desvincular la entidad
            // using (var newContext = new YourDbContext())
            // {
            //     newContext.Entry(usuario).State = EntityState.Modified;
            //     await newContext.SaveChangesAsync();
            // }
            
            msg = "Contraseña modificada existosamente.";
            _logger.LogInformation(msg);
            return Ok( new { status = true, message = msg, data = "" } );

            
        }
        catch (System.Exception e)
        {
            //throw;
            _logger.LogError("Error: " + e.Message);  
            return BadRequest( new { status = false, message = e.Message, data = "" } );

        }
    }

    [HttpPost("cotizaciones")]
    public async Task<IActionResult> Cotizaciones ()//([FromBody] UserChangePasswordDto userChangePassword)
    {

        try
        {
            // Trunca la tabla
            //await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Cotizaciones");

            await _obtenerDatosIol.Obtener();

            // Obtengo Cedears con descripcion optimizada
            //await _cedears.ObtenerCedear();

            // Obtengo Bonos con descripcion optimizada
            //await _bonos.ObtenerBonos();

            // Obtengo Bonos con descripcion optimizada
            //await _obligacionesNegociables.ObtenerON();

            // Obtengo Bonos con descripcion optimizada
            //await _acciones.ObtenerAcciones();

            return Ok();
        }
        catch (System.Exception e)
        {
            //throw;
            _logger.LogError("Error: " + e.Message);
            return BadRequest(new { status = false, message = e.Message, data = "" });

        }



    }
}