using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NgCapitalApi.Data;
using NgCapitalApi.Models;
using BCrypt.Net;
using Microsoft.VisualBasic;


namespace NgCapitalApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly NgCapitalApiDbContext       _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController( NgCapitalApiDbContext       context
                                  ,ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var msg = "";
            try
            {
                msg = "Consulta sobre todos los usuarios. ";
                _logger.LogInformation( msg );
                var usuarios = await _context.Usuarios.ToListAsync();
                return Ok( new { status = true, message = "", data = usuarios } );
            }
            catch (System.Exception e)
            {
                msg = "Error al consultar por todos los usuarios";
                _logger.LogError( msg );
                return NotFound( new { status = false, message = msg + e.Message, data = "" } );
            }
        }
        
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var msg = "";
            try
            {
                msg = "Consulta sobre el id de usuario: " + id;
                _logger.LogInformation( msg );
                
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return BadRequest( new { status = false, message = "Usuario incorrecto.", data = "" } );
                }

                return Ok( new { status = true, message = "", data = usuario } );
            }
            catch (System.Exception e)
            {
                msg = "Error al consultar por el id de usuario: " + id;
                _logger.LogError( msg );
                return NotFound( new { status = false, message = msg + e.Message, data = "" } );
            }
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            var msg = "";
            try
            {
                msg = "Se agrega nuevo usuario. ";
                _logger.LogInformation( msg );

                usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                //return Ok( new { status = true, message = "Se creó el dato sobre la tabla usuario  correctamente.", data = usuario } );
                return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
            }
            catch (System.Exception e)
            {
                msg = "Error al agregar usuario: ";
                _logger.LogError( msg );
                return NotFound( new { status = false, message = msg + e.Message, data = "" } );
            }
            
        }

        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            var msg = "";
            try
            {
                msg = "Se modifica usuario " + id;
                _logger.LogInformation( msg );

                if (id != usuario.Id)
                {
                    return BadRequest( new { status = false, message = "Usuario incorrecto.", data = "" } );
                }

                var user = await _context.Usuarios.FindAsync(id);

                if (user != null) 
                {
                    //user.Id = usuario.Id;
                    user.Nombre = usuario.Nombre == "" ? user.Nombre : usuario.Nombre;
                    user.Email = usuario.Email == "" ? user.Email : usuario.Email;

                    _context.Entry(user).State = EntityState.Modified;
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        if (!UsuarioExists(id))
                        {
                            //return NotFound();
                            return BadRequest( new { status = false, message = "Usuario inexistente.", data = "" } );
                        }
                        else
                        {
                            return BadRequest( new { status = false, message = "Error: " + e.Message, data = "" } );
                            //throw;
                        }
                    }
                    return Ok( new { status = true, message = "Se actualizaron los datos de la tabla usuario correctamente.", data = user } );
                    //return NoContent();
                }
                else
                {
                    return BadRequest( new { status = false, message = "", data = "" } );
                }
            }
            catch (System.Exception e)
            {
                msg = "Error al modificar usuario " + id;
                _logger.LogError( msg );
                return NotFound( new { status = false, message = msg + e.Message, data = "" } );
            }
                        
        }

        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var msg = "";
            try
            {
                msg = "Se elimina al usuario " + id;
                _logger.LogInformation( msg );

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return BadRequest( new { status = false, message = "Usuario inexistente", data = "" } );
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return Ok( new { status = true, message = "Se eliminaron los datos de la tabla usuario correctamente.", data = usuario } );
            }
            catch (System.Exception e)
            {
                msg = "Error al eliminar usuario " + id;
                _logger.LogError( msg );
                return NotFound( new { status = false, message = msg + e.Message, data = "" } );
            }            
            
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}