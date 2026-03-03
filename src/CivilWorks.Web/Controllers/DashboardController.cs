using CivilWorks.Domain.Enums;
using CivilWorks.Infrastructure.Persistence;
using CivilWorks.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DashboardController(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var totalClientes = await _db.Clientes.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .CountAsync();

        var obrasAtivasQuery = _db.Obras.AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && o.Status == StatusObra.EmAndamento);

        var totalObrasAtivas = await obrasAtivasQuery.CountAsync();

        var somaOrcamentoPrevistoAtivas = await obrasAtivasQuery
            .SumAsync(o => (decimal?)o.OrcamentoPrevisto) ?? 0m;

        // Custo real: soma de despesas nas obras ativas
        var idsObrasAtivas = await obrasAtivasQuery.Select(o => o.Id).ToListAsync();

        var custoRealAtivas = await _db.ObraLancamentos.AsNoTracking()
            .Where(l => l.EmpresaId == empresaId && l.Tipo == TipoLancamento.Despesa)
            .Join(
                _db.Obras.AsNoTracking().Where(o => o.EmpresaId == empresaId && o.Status == StatusObra.EmAndamento),
                l => l.ObraId,
                o => o.Id,
                (l, o) => l.Valor
            )
            .SumAsync(v => (decimal?)v) ?? 0m;

        // Contar obras "estouradas" (custo > previsto)
        // (consulta simples e segura; otimiza depois se precisar)
        var gastosPorObra = await _db.ObraLancamentos.AsNoTracking()
            .Where(l => l.EmpresaId == empresaId
                        && idsObrasAtivas.Contains(l.ObraId)
                        && l.Tipo == TipoLancamento.Despesa)
            .GroupBy(l => l.ObraId)
            .Select(g => new { ObraId = g.Key, Total = g.Sum(x => x.Valor) })
            .ToListAsync();

        var orcamentoPorObra = await _db.Obras.AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && idsObrasAtivas.Contains(o.Id))
            .Select(o => new { o.Id, o.OrcamentoPrevisto })
            .ToListAsync();

        var orcMap = orcamentoPorObra.ToDictionary(x => x.Id, x => x.OrcamentoPrevisto);
        var obrasEstouradas = gastosPorObra.Count(x => orcMap.TryGetValue(x.ObraId, out var prev) && x.Total > prev);

        ViewBag.TotalClientes = totalClientes;
        ViewBag.TotalObrasAtivas = totalObrasAtivas;
        ViewBag.OrcamentoPrevistoAtivas = somaOrcamentoPrevistoAtivas;
        ViewBag.CustoRealAtivas = custoRealAtivas;
        ViewBag.ObrasEstouradas = obrasEstouradas;

        return View();
    }
}