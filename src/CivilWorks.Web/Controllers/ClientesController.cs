using CivilWorks.Domain.Entities;
using CivilWorks.Infrastructure.Persistence;
using CivilWorks.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Web.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ClientesController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 5) pageSize = 5;
        if (pageSize > 50) pageSize = 50;

        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var query = _db.Clientes.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(c =>
                c.Nome.Contains(q) ||
                (c.Documento != null && c.Documento.Contains(q)) ||
                (c.Email != null && c.Email.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Id = Guid.NewGuid();
        model.CreatedAtUtc = DateTime.UtcNow;
        model.EmpresaId = await _currentUser.GetEmpresaIdAsync();

        _db.Clientes.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Cliente cadastrado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (cliente is null) return NotFound();

        TempData["Success"] = "Cliente atualizado com sucesso!";
        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Cliente model)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (cliente is null) return NotFound();

        if (!ModelState.IsValid) return View(model);

        cliente.Nome = model.Nome;
        cliente.Documento = model.Documento;
        cliente.Email = model.Email;
        cliente.Telefone = model.Telefone;
        cliente.Observacoes = model.Observacoes;
        cliente.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (cliente is null) return NotFound();

        // Soft delete
        cliente.IsDeleted = true;
        cliente.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        
        TempData["Success"] = "Cliente excluído com sucesso!";
        return RedirectToAction(nameof(Index));
    }
}