using CivilWorks.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CivilWorks.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = ["Admin", "Engenheiro", "Funcionario"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        await EnsureUserAsync(userManager, "admin@civilworks.local", "Admin");
        await EnsureUserAsync(userManager, "engenheiro@civilworks.local", "Engenheiro");
        await EnsureUserAsync(userManager, "funcionario@civilworks.local", "Funcionario");
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true,
                Nome = role // ou "Usuário " + role
                // EmpresaId = ??? (se for obrigatório, veja observação abaixo)
            };

            var create = await userManager.CreateAsync(user, "CivilWorks@123");
            if (!create.Succeeded)
            {
                var errors = string.Join(" | ", create.Errors.Select(e => e.Description));
                throw new Exception($"Falha ao criar usuário {email}: {errors}");
            }
        }
        else
        {
            // garante campos importantes
            user.EmailConfirmed = true;
            user.IsActive = true;
            user.UserName = email;
            user.Email = email;

            await userManager.UpdateAsync(user);

            // RESETAR SENHA (DEV) — garante que você consegue logar sempre
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, "CivilWorks@123");
            if (!reset.Succeeded)
            {
                var errors = string.Join(" | ", reset.Errors.Select(e => e.Description));
                throw new Exception($"Falha ao resetar senha {email}: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
} 