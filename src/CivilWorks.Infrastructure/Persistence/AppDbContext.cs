using CivilWorks.Domain.Entities;
using CivilWorks.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CivilWorks.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Obra> Obras => Set<Obra>();
    public DbSet<ObraLancamentoFinanceiro> ObraLancamentos => Set<ObraLancamentoFinanceiro>();
    public DbSet<ObraHistorico> ObraHistoricos => Set<ObraHistorico>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Empresa
        builder.Entity<Empresa>(e =>
        {
            e.ToTable("Empresas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cnpj).HasMaxLength(20);
        });

        // ApplicationUser custom fields
        builder.Entity<ApplicationUser>(u =>
        {
            u.Property(x => x.Nome).HasMaxLength(150);
            u.Property(x => x.EmpresaId);
            u.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // Cliente
        builder.Entity<Cliente>(c =>
        {
            c.ToTable("Clientes");
            c.HasKey(x => x.Id);

            c.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            c.Property(x => x.Documento).HasMaxLength(30);
            c.Property(x => x.Email).HasMaxLength(150);
            c.Property(x => x.Telefone).HasMaxLength(30);
            c.Property(x => x.Observacoes).HasMaxLength(1000);

            c.HasIndex(x => new { x.EmpresaId, x.Nome });
            c.HasIndex(x => new { x.EmpresaId, x.Documento });

            // Soft delete global (opcional, mas bom)
            c.HasQueryFilter(x => !x.IsDeleted);
        });

        // Obras
        builder.Entity<Obra>(o =>
        {
            o.ToTable("Obras");
            o.HasKey(x => x.Id);

            o.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            o.Property(x => x.Descricao).HasMaxLength(2000);

            o.Property(x => x.OrcamentoPrevisto).HasColumnType("decimal(18,2)");
            o.Property(x => x.ProgressoPercentual).IsRequired();

            o.HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

            o.HasIndex(x => new { x.EmpresaId, x.Status });
            o.HasIndex(x => new { x.EmpresaId, x.ClienteId });
            o.HasIndex(x => new { x.EmpresaId, x.Nome });

            o.HasQueryFilter(x => !x.IsDeleted);
        });

        // Lancamentos
        builder.Entity<ObraLancamentoFinanceiro>(l =>
        {
            l.ToTable("ObraLancamentos");
            l.HasKey(x => x.Id);

            l.Property(x => x.Valor).HasColumnType("decimal(18,2)");
            l.Property(x => x.Categoria).HasMaxLength(80);
            l.Property(x => x.Descricao).HasMaxLength(500);

            l.HasOne(x => x.Obra)
            .WithMany(o => o.Lancamentos)
            .HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Cascade);

            l.HasIndex(x => new { x.EmpresaId, x.ObraId, x.Data });
            l.HasIndex(x => new { x.EmpresaId, x.Tipo });

            l.HasQueryFilter(x => !x.IsDeleted);
        });

        // Historico
        builder.Entity<ObraHistorico>(h =>
        {
            h.ToTable("ObraHistoricos");
            h.HasKey(x => x.Id);

            h.Property(x => x.Evento).HasMaxLength(120).IsRequired();
            h.Property(x => x.Detalhes).HasMaxLength(2000);

            h.HasOne(x => x.Obra)
            .WithMany(o => o.Historico)
            .HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Cascade);

            h.HasIndex(x => new { x.EmpresaId, x.ObraId });

            // não precisa queryfilter, mas pode ter
        });
    }
}