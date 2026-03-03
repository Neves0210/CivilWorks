using CivilWorks.Domain.Entities;
using CivilWorks.Infrastructure.Identity;
using CivilWorks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        // 1) Roles (garante que existem)
        string[] roles = ["Admin", "Engenheiro", "Funcionario", "CEO"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // 2) Empresa padrão (se não existir)
        var empresaNome = "Empresa Padrão";
        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Nome == empresaNome);

        if (empresa is null)
        {
            empresa = new Empresa
            {
                Nome = empresaNome,
                Cnpj = null
            };

            db.Empresas.Add(empresa);
            await db.SaveChangesAsync();
        }

        // 3) Usuário Admin padrão (se não existir)
        var adminEmail = "admin@civilworks.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Nome = "Administrador",
                EmpresaId = empresa.Id,
                IsActive = true
            };

            // Troque a senha depois (apenas seed de dev)
            var createResult = await userManager.CreateAsync(adminUser, "Admin@123");

            if (!createResult.Succeeded)
            {
                var errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                throw new Exception($"Falha ao criar Admin: {errors}");
            }
        }

        // 4) Garante Role Admin
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}