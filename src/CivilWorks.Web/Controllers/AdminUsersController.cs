using CivilWorks.Infrastructure.Identity;
using CivilWorks.Infrastructure.Persistence;
using CivilWorks.Web.Models.Admin;
using CivilWorks.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AdminUsersController(
        AppDbContext db,
        ICurrentUser currentUser,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _db = db;
        _currentUser = currentUser;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        // lista usuários da empresa
        var users = await _db.Users.AsNoTracking()
            .Where(u => u.EmpresaId == empresaId)
            .OrderBy(u => u.Nome)
            .ToListAsync();

        // carrega roles de cada user (simples; otimiza depois se precisar)
        var result = new List<(ApplicationUser User, string Role)>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add((u, roles.FirstOrDefault() ?? "-"));
        }

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await EnsureRolesAsync();
        return View(new CreateUserVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserVm vm)
    {
        await EnsureRolesAsync();

        if (!ModelState.IsValid)
            return View(vm);

        var empresaId = await _currentUser.GetEmpresaIdAsync();

        // evita duplicado
        var exists = await _userManager.FindByEmailAsync(vm.Email);
        if (exists is not null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Já existe um usuário com esse email.");
            return View(vm);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Nome = vm.Nome,
            UserName = vm.Email,
            Email = vm.Email,
            EmailConfirmed = true,
            IsActive = true,
            EmpresaId = empresaId
        };

        var create = await _userManager.CreateAsync(user, vm.Password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors)
                ModelState.AddModelError("", e.Description);

            return View(vm);
        }

        // role
        var addRole = await _userManager.AddToRoleAsync(user, vm.Role);
        if (!addRole.Succeeded)
        {
            foreach (var e in addRole.Errors)
                ModelState.AddModelError("", e.Description);

            return View(vm);
        }

        TempData["Success"] = "Usuário criado com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var empresaId = await _currentUser.GetEmpresaIdAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.EmpresaId == empresaId);
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = user.IsActive ? "Usuário ativado." : "Usuário desativado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task EnsureRolesAsync()
    {
        string[] roles = ["Admin", "Engenheiro", "Funcionario"];

        foreach (var r in roles)
        {
            if (!await _roleManager.RoleExistsAsync(r))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(r));
        }

        ViewBag.Roles = roles;
    }
}