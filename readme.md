# Instrucciones para trabajar con .NET Core en Visual Studio Code
---

Para comenzar a trabajar con las últimas versiones de .NET Core en Visual Studio Code, sigue estos pasos:

### Instalación de Visual Studio Code:
Descarga e instala Visual Studio Code desde su sitio oficial.

### Instalación del SDK de .NET Core:
Asegúrate de tener instalado el SDK de .NET Core. Puedes descargarlo desde el sitio de .NET. Esto es esencial para compilar y ejecutar aplicaciones .NET.

 * https://dotnet.microsoft.com/es-es/download

### Configuración del entorno:
Abre Visual Studio Code y asegúrate de que el terminal esté configurado para usar el SDK de .NET. Puedes verificar esto ejecutando el comando **dotnet --version** en el terminal integrado de VS Code.

### Extensiones necesarias:
Para mejorar tu experiencia de desarrollo, instala las siguientes extensiones:
* C#: Esta es la extensión principal que proporciona soporte para C# y .NET Core. Te ayudará con la autocompletación, la depuración y la navegación del código. 
* C# Extensions: Mejora la funcionalidad de la extensión principal y ofrece características adicionales para trabajar con C#.
* NuGet Package Manager: Facilita la gestión de paquetes NuGet directamente desde el IDE.
* .Net Install Tool
* C# Dev Kit
* C# XML Documentation Comments

## Crear un nuevo proyecto:
Puedes crear un nuevo proyecto de .NET Core utilizando el terminal de VS Code. Ejecuta el siguiente comando:

* **dotnet new console -n NombreDelProyecto**
* **dotnet new worker -n NombreDelProyecto**
* **dotnet new webapi -n NombreDelProyecto**

Esto creará un nuevo proyecto de consola / job / webapi  en una carpeta llamada "NombreDelProyecto".

**Nota:** Para el caso en donde se cree un webapi, seria conveniente usar la instrucción **dotnet new webapi --use-controllers -n NombreDelProyecto** para que no realice instalaciones minimas (para versiones superiores a la 8).


### Ejecutar el proyecto:
Navega a la carpeta del proyecto y ejecuta el siguiente comando para compilar y ejecutar tu aplicación:

 * **dotnet run**


## Configuración de Entity Framework

En esta configuración vamos a basarnos en el nombre de proyecto **PruebaEF** de una webapi.

1)  Agregar entity framework  
     * **dotnet add package Microsoft.EntityFrameworkCore**
     * **dotnet add package Microsoft.EntityFrameworkCore.Design**

2) Agregar proveedor para Mysql
    * **dotnet add package Pomelo.EntityFrameworkCore.MySql**
  

Nota: Posibles errores de configuración.

Verificar que al configurar entityFramework se guarden las referencias en el archivo **PruebasEF.csproj**, sino agregamos las siguientes lineas.
``` [C#]
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
  <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="6.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.0" />
</ItemGroup>
```
Es probable que tengas que restaurar los paquetes, para esto desde la terminal correr el siguiente script **dotnet restore** y reiniciar VSCode.
   
3) Crear Models
     * Agregar la carpeta **Models** dentro del directorio principal.
    * Crear el archivo de la tabla deseada, en este caso Usuario.cs
    * Agregar al archivo la configuracion de la tabla de la base de datos.
``` [C#]
namespace PruebaEF.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
    }
}
```

1) Crear DbContext
    * Se debe crear la carpeta **Data** dentro del directorio principal.
    * Crear archivo NombreDelProyectoDbContext.cs (PruebaEF).
    * Agregar al archivo el contexto para la tabla, en este caso usuario

``` [C#]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PruebaEF.Models;
using Pomelo.EntityFrameworkCore;

namespace PruebaEF.Data
{
    public class PruebaEFDbContext : DbContext
    {
        public PruebaEFDbContext(DbContextOptions<PruebaEFDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios   
        { get; set; }
    }
}
```

5) Configuración appsettings.json

    Agregar la siguiente configuracion al archivo con los datos de la base.
``` [C#]
"ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=MiBaseDeDatos;User=root;Password=tu_contraseña;"
    }
```
6) Registrar el DbContext en Program.cs
   * Agregar el using del dbcontext y el de entityFramework y además las lineas de configuración.

``` [C#]
using Microsoft.EntityFrameworkCore;
using PruebaEF.Data;

// Add services to the container.
builder.Services.AddDbContext<PruebaEFDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));
```
7) Crear controlador con CRUD

    * Creamos el archivo UsuarioControllers.cs dentro de la carpeta Controllers.
    * Agregamos el siguiente código.
``` [C#]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaEF.Data;
using PruebaEF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiProyectoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly PruebaEFDbContext _context;

        public UsuariosController(PruebaEFDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
```

8) Migración y creación de la base de datos
    * **dotnet ef migrations add InitialCreate**
    * **dotnet ef database update**

9) Probar el funcionamiento
    * **dotnet run**
