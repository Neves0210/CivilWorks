using CivilWorks.Domain.Abstractions;
using CivilWorks.Domain.Enums;

namespace CivilWorks.Domain.Entities;

public class ObraLancamentoFinanceiro : BaseEntity, IHasEmpresa
{
    public Guid EmpresaId { get; set; }

    public Guid ObraId { get; set; }
    public Obra? Obra { get; set; }

    public TipoLancamento Tipo { get; set; } = TipoLancamento.Despesa;

    public decimal Valor { get; set; }
    public DateTime Data { get; set; }

    public string? Categoria { get; set; }   // Material, Mão de obra, Extra...
    public string? Descricao { get; set; }

    public bool IsDeleted { get; set; } = false;
}