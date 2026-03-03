namespace CivilWorks.Domain.Entities;

public class Empresa : BaseEntity
{
    public string Nome { get; set; } = default!;
    public string? Cnpj { get; set; }
}