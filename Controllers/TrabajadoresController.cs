using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TrabajadoresController : Controller
    {
        private readonly TrabajadoresPruebaContext _context;

        public TrabajadoresController(TrabajadoresPruebaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string filtroSexo)
        {
            var trabajadores = await _context.Trabajadores
                .Include(t => t.IdDepartamentoNavigation)
                .Include(t => t.IdProvinciaNavigation)
                .Include(t => t.IdDistritoNavigation)
                .Where(t => string.IsNullOrEmpty(filtroSexo) || t.Sexo == filtroSexo)
                .Select(t => new TrabajadorDTO
                {
                    Id = t.Id,
                    TipoDocumento = t.TipoDocumento,
                    NumeroDocumento = t.NumeroDocumento,
                    Nombres = t.Nombres,
                    Sexo = t.Sexo,
                    Departamento = t.IdDepartamentoNavigation.NombreDepartamento,
                    Provincia = t.IdProvinciaNavigation.NombreProvincia,
                    Distrito = t.IdDistritoNavigation.NombreDistrito
                })
                .ToListAsync();

            ViewBag.FiltroSexo = filtroSexo;
            return View(trabajadores);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departamentos = await _context.Departamentos.ToListAsync();
            ViewBag.Provincias = new List<Provincium>();
            ViewBag.Distritos = new List<Distrito>();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Trabajadore trabajador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trabajador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departamentos = await _context.Departamentos.ToListAsync();
            return View(trabajador);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var trabajador = await _context.Trabajadores.FindAsync(id);
            if (trabajador == null)
            {
                return NotFound();
            }
            ViewBag.Departamentos = await _context.Departamentos.ToListAsync();
            ViewBag.Provincias = await _context.Provincia.Where(p => p.IdDepartamento == trabajador.IdDepartamento).ToListAsync();
            ViewBag.Distritos = await _context.Distritos.Where(d => d.IdProvincia == trabajador.IdProvincia).ToListAsync();
            return View(trabajador);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Trabajadore trabajador)
        {
            if (id != trabajador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trabajador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrabajadorExists(trabajador.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departamentos = await _context.Departamentos.ToListAsync();
            return View(trabajador);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var trabajador = await _context.Trabajadores
                .Include(t => t.IdDepartamentoNavigation)
                .Include(t => t.IdProvinciaNavigation)
                .Include(t => t.IdDistritoNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trabajador == null)
            {
                return NotFound();
            }

            return View(trabajador);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trabajador = await _context.Trabajadores.FindAsync(id);
            if (trabajador != null)
            {
                _context.Trabajadores.Remove(trabajador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetProvincias(int departamentoId)
        {
            var provincias = await _context.Provincia
                .Where(p => p.IdDepartamento == departamentoId)
                .Select(p => new { p.Id, p.NombreProvincia })
                .ToListAsync();
            return Ok(provincias);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistritos(int provinciaId)
        {
            var distritos = await _context.Distritos
                .Where(d => d.IdProvincia == provinciaId)
                .Select(d => new { d.Id, d.NombreDistrito })
                .ToListAsync();
            return Ok(distritos);
        }

        private bool TrabajadorExists(int id)
        {
            return _context.Trabajadores.Any(e => e.Id == id);
        }
    }

    public class TrabajadorDTO
    {
        public int Id { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? Nombres { get; set; }
        public string? Sexo { get; set; }
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
    }
} 