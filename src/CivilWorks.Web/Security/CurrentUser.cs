using CivilWorks.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CivilWorks.Web.Security;

public interface ICurrentUser
{
    Task<Guid> GetEmpresaIdAsync();
    Task<Guid> GetUserIdAsync();
}

public class CurrentUser : ICurrentUser
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> GetUserIdAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("Sem HttpContext/User.");

        var user = await _userManager.GetUserAsync(principal)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        return user.Id;
    }

    public async Task<Guid> GetEmpresaIdAsync()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("Sem HttpContext/User.");

        var user = await _userManager.GetUserAsync(principal)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        if (user.EmpresaId is null)
            throw new InvalidOperationException("Usuário sem Empresa vinculada.");

        return user.EmpresaId.Value;
    }
}