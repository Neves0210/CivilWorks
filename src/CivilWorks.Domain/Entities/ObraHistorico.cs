using CivilWorks.Domain.Abstractions;

namespace CivilWorks.Domain.Entities;

public class ObraHistorico : BaseEntity, IHasEmpresa
{
    public Guid EmpresaId { get; set; }

    public Guid ObraId { get; set; }
    public Obra? Obra { get; set; }

    public string Evento { get; set; } = default!;
    public string? Detalhes { get; set; }

    public Guid? UsuarioId { get; set; }
}