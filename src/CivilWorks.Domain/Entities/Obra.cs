using CivilWorks.Domain.Abstractions;
using CivilWorks.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CivilWorks.Domain.Entities;

public class Obra : BaseEntity, IHasEmpresa
{
    public Guid EmpresaId { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string Nome { get; set; } = default!;
    public string? Descricao { get; set; }

    public StatusObra Status { get; set; } = StatusObra.Planejamento;

    public DateTime DataInicio { get; set; }
    public DateTime? DataPrevisaoTermino { get; set; }

    [Required(ErrorMessage = "Informe o orçamento previsto.")]
    [Range(0.0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero.")]
    public decimal OrcamentoPrevisto { get; set; }
    // Por enquanto manual (0..100). Depois pode virar por tarefas/etapas.
    public int ProgressoPercentual { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;

    public ICollection<ObraLancamentoFinanceiro> Lancamentos { get; set; } = new List<ObraLancamentoFinanceiro>();
    public ICollection<ObraHistorico> Historico { get; set; } = new List<ObraHistorico>();
}