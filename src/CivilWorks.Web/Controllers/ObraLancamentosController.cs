using CivilWorks.Domain.Entities;
using CivilWorks.Domain.Enums;
using CivilWorks.Infrastructure.Persistence;
using CivilWorks.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Web.Controllers;

[Authorize]
public class ObraLancamentosController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ObraLancamentosController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid obraId)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var obra = await _db.Obras.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == obraId && o.EmpresaId == empresaId);

        if (obra is null) return NotFound();

        ViewBag.ObraId = obraId;
        ViewBag.ObraNome = obra.Nome;

        return View(new ObraLancamentoFinanceiro
        {
            ObraId = obraId,
            Data = DateTime.Today,
            Tipo = TipoLancamento.Despesa
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid obraId, ObraLancamentoFinanceiro model)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var obra = await _db.Obras
            .FirstOrDefaultAsync(o => o.Id == obraId && o.EmpresaId == empresaId);

        if (obra is null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.ObraId = obraId;
            ViewBag.ObraNome = obra.Nome;
            return View(model);
        }

        model.Id = Guid.NewGuid();
        model.EmpresaId = empresaId;
        model.ObraId = obraId;
        model.CreatedAtUtc = DateTime.UtcNow;

        _db.ObraLancamentos.Add(model);

        _db.ObraHistoricos.Add(new ObraHistorico
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ObraId = obraId,
            Evento = "Lançamento criado",
            Detalhes = $"{model.Tipo} {model.Valor:N2} ({model.Categoria}) em {model.Data:dd/MM/yyyy}",
            CreatedAtUtc = DateTime.UtcNow,
            UsuarioId = await _currentUser.GetUserIdAsync()
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Lançamento adicionado!";
        return RedirectToAction("Details", "Obras", new { id = obraId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid obraId)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var lanc = await _db.ObraLancamentos
            .FirstOrDefaultAsync(l => l.Id == id && l.EmpresaId == empresaId);

        if (lanc is null) return NotFound();

        lanc.IsDeleted = true;
        lanc.UpdatedAtUtc = DateTime.UtcNow;

        _db.ObraHistoricos.Add(new ObraHistorico
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ObraId = obraId,
            Evento = "Lançamento excluído",
            Detalhes = $"{lanc.Tipo} {lanc.Valor:N2} ({lanc.Categoria}) em {lanc.Data:dd/MM/yyyy}",
            CreatedAtUtc = DateTime.UtcNow,
            UsuarioId = await _currentUser.GetUserIdAsync()
        });

        await _db.SaveChangesAsync();

        TempData["Warning"] = "Lançamento excluído.";
        return RedirectToAction("Details", "Obras", new { id = obraId });
    }
}