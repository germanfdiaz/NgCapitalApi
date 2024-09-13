using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NgCapitalApi.Data;
using NgCapitalApi.Models;
using BCrypt.Net;


namespace NgCapitalApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly NgCapitalApiDbContext _context;

        public UsuariosController(NgCapitalApiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Ok( new { status = true, message = "", data = usuarios } );
        }
        
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return BadRequest( new { status = false, message = "Usuario incorrecto.", data = "" } );
                //return NotFound();
            }

            return Ok( new { status = true, message = "", data = usuario } );
            //return usuario;
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            //return Ok( new { status = true, message = "Se creó el dato sobre la tabla usuario  correctamente.", data = usuario } );
            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
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

        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return BadRequest( new { status = false, message = "Usuario inexistente", data = "" } );
                //return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok( new { status = true, message = "Se eliminaron los datos de la tabla usuario correctamente.", data = usuario } );
            //return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}