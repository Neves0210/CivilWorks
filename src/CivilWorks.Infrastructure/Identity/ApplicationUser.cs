using Microsoft.AspNetCore.Identity;

namespace CivilWorks.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? EmpresaId { get; set; } // pode ser null até você amarrar o fluxo de cadastro
    public bool IsActive { get; set; } = true;

    public string? Nome { get; set; }
}