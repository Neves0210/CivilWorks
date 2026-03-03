using CivilWorks.Domain.Entities;
using CivilWorks.Domain.Enums;
using CivilWorks.Infrastructure.Persistence;
using CivilWorks.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Web.Controllers;

[Authorize]
public class ObrasController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ObrasController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // ✅ Todos logados podem ver listagem
    [Authorize(Roles = "Admin,Engenheiro,Funcionario")]
    public async Task<IActionResult> Index(string? q, StatusObra? status, int page = 1, int pageSize = 10)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var query = _db.Obras.AsNoTracking()
            .Include(o => o.Cliente)
            .Where(o => o.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(o =>
                o.Nome.Contains(q) ||
                (o.Cliente != null && o.Cliente.Nome.Contains(q)));
        }

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    // ✅ Apenas Admin/Engenheiro criam
    [Authorize(Roles = "Admin,Engenheiro")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadClientesAsync();
        return View(new Obra { DataInicio = DateTime.Today });
    }

    [Authorize(Roles = "Admin,Engenheiro")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Obra model)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        if (!ModelState.IsValid)
        {
            await LoadClientesAsync(model.ClienteId);
            return View(model);
        }

        model.Id = Guid.NewGuid();
        model.EmpresaId = empresaId;
        model.CreatedAtUtc = DateTime.UtcNow;

        _db.Obras.Add(model);

        _db.ObraHistoricos.Add(new ObraHistorico
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ObraId = model.Id,
            Evento = "Obra criada",
            Detalhes = $"Obra '{model.Nome}' criada com orçamento previsto {model.OrcamentoPrevisto:N2}.",
            CreatedAtUtc = DateTime.UtcNow,
            UsuarioId = await _currentUser.GetUserIdAsync()
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Obra cadastrada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // ✅ Apenas Admin/Engenheiro editam
    [Authorize(Roles = "Admin,Engenheiro")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var obra = await _db.Obras
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (obra is null) return NotFound();

        await LoadClientesAsync(obra.ClienteId);
        return View(obra);
    }

    [Authorize(Roles = "Admin,Engenheiro")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Obra model)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var obra = await _db.Obras
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (obra is null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadClientesAsync(model.ClienteId);
            return View(model);
        }

        obra.Nome = model.Nome;
        obra.Descricao = model.Descricao;
        obra.ClienteId = model.ClienteId;
        obra.Status = model.Status;
        obra.DataInicio = model.DataInicio;
        obra.DataPrevisaoTermino = model.DataPrevisaoTermino;
        obra.OrcamentoPrevisto = model.OrcamentoPrevisto;
        obra.ProgressoPercentual = Math.Clamp(model.ProgressoPercentual, 0, 100);
        obra.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Obra atualizada com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // ✅ Apenas Admin exclui
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var obra = await _db.Obras
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (obra is null) return NotFound();

        obra.IsDeleted = true;
        obra.UpdatedAtUtc = DateTime.UtcNow;

        _db.ObraHistoricos.Add(new ObraHistorico
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ObraId = obra.Id,
            Evento = "Obra excluída",
            Detalhes = $"Obra '{obra.Nome}' marcada como excluída.",
            CreatedAtUtc = DateTime.UtcNow,
            UsuarioId = await _currentUser.GetUserIdAsync()
        });

        await _db.SaveChangesAsync();

        TempData["Warning"] = "Obra excluída com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // ✅ Todos logados podem ver detalhes
    [Authorize(Roles = "Admin,Engenheiro,Funcionario")]
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var obra = await _db.Obras.AsNoTracking()
            .Include(o => o.Cliente)
            .FirstOrDefaultAsync(o => o.Id == id && o.EmpresaId == empresaId);

        if (obra is null) return NotFound();

        var lancamentos = await _db.ObraLancamentos.AsNoTracking()
            .Where(l => l.EmpresaId == empresaId && l.ObraId == id)
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.CreatedAtUtc)
            .ToListAsync();

        var historico = await _db.ObraHistoricos.AsNoTracking()
            .Where(h => h.EmpresaId == empresaId && h.ObraId == id)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Take(50)
            .ToListAsync();

        ViewBag.Historico = historico;

        var despesas = lancamentos.Where(x => x.Tipo == CivilWorks.Domain.Enums.TipoLancamento.Despesa).Sum(x => x.Valor);
        var receitas = lancamentos.Where(x => x.Tipo == CivilWorks.Domain.Enums.TipoLancamento.Receita).Sum(x => x.Valor);

        var despesasPorCategoria = lancamentos
            .Where(x => x.Tipo == CivilWorks.Domain.Enums.TipoLancamento.Despesa)
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Categoria) ? "Sem categoria" : x.Categoria!.Trim())
            .Select(g => new { Categoria = g.Key, Total = g.Sum(x => x.Valor) })
            .OrderByDescending(x => x.Total)
            .ToList();

        ViewBag.DespesasPorCategoria = despesasPorCategoria;

        var estourou = despesas > obra.OrcamentoPrevisto && obra.OrcamentoPrevisto > 0;
        ViewBag.Excesso = estourou ? (despesas - obra.OrcamentoPrevisto) : 0m;

        ViewBag.Lancamentos = lancamentos;
        ViewBag.TotalDespesas = despesas;
        ViewBag.TotalReceitas = receitas;
        ViewBag.Saldo = receitas - despesas;

        return View(obra);
    }

    private async Task LoadClientesAsync(Guid? selected = null)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var clientes = await _db.Clientes.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId)
            .OrderBy(c => c.Nome)
            .ToListAsync();

        ViewBag.Clientes = new SelectList(clientes, "Id", "Nome", selected);
        ViewBag.StatusList = new SelectList(Enum.GetValues<StatusObra>().Select(s => new { Id = (int)s, Nome = s.ToString() }),
            "Id", "Nome", selectedValue: null);
    }
}