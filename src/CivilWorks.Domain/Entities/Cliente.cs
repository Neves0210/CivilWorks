using CivilWorks.Domain.Abstractions;

namespace CivilWorks.Domain.Entities;

public class Cliente : BaseEntity, IHasEmpresa
{
    public Guid EmpresaId { get; set; }

    public string Nome { get; set; } = default!;
    public string? Documento { get; set; } // CPF/CNPJ (texto)
    public string? Email { get; set; }
    public string? Telefone { get; set; }

    public string? Observacoes { get; set; }

    public bool IsDeleted { get; set; } = false;
}