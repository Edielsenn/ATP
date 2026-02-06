using CNHVirtualAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CNHVirtualAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<Plano> Planos { get; set; }
    public DbSet<PlanoRecurso> PlanoRecursos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Assinatura> Assinaturas { get; set; }
    public DbSet<WebhookLog> WebhookLogs { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<Configuracao> Configuracoes { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações de precisão decimal
        modelBuilder.Entity<Plano>()
            .Property(p => p.Preco)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Plano>()
            .Property(p => p.PrecoPromocional)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorTotal)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorDesconto)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pedido>()
            .Property(p => p.ValorFinal)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pagamento>()
            .Property(p => p.Valor)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Pagamento>()
            .Property(p => p.ValorRecebido)
            .HasPrecision(10, 2);

        // Configurações de relacionamentos
        modelBuilder.Entity<PlanoRecurso>()
            .HasOne(pr => pr.Plano)
            .WithMany(p => p.Recursos)
            .HasForeignKey(pr => pr.PlanoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Plano)
            .WithMany(pl => pl.Pedidos)
            .HasForeignKey(p => p.PlanoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pagamento>()
            .HasOne(p => p.Pedido)
            .WithMany(pe => pe.Pagamentos)
            .HasForeignKey(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assinatura>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Assinaturas)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assinatura>()
            .HasOne(a => a.Plano)
            .WithMany(p => p.Assinaturas)
            .HasForeignKey(a => a.PlanoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assinatura>()
            .HasOne(a => a.Pedido)
            .WithMany(p => p.Assinaturas)
            .HasForeignKey(a => a.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email);

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.Numero)
            .IsUnique();

        modelBuilder.Entity<Pagamento>()
            .HasIndex(p => p.AsaasPaymentId);

        modelBuilder.Entity<Configuracao>()
            .HasIndex(c => c.Chave)
            .IsUnique();
    }
}
