using CapitalPos.Cpe.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapitalPos.Cpe.Api.Data;

public class CpeDbContext : DbContext
{
    public const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=CapitalPosCpeDb;Username=postgres;Password=postgres";

    public CpeDbContext(DbContextOptions<CpeDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(DefaultConnectionString);
        }
    }

    public DbSet<Emisor> Emisores => Set<Emisor>();
    public DbSet<EmisorCertificado> EmisorCertificados => Set<EmisorCertificado>();
    public DbSet<EmisorCredencialSol> EmisorCredencialesSol => Set<EmisorCredencialSol>();
    public DbSet<ComprobanteEmitido> ComprobantesEmitidos => Set<ComprobanteEmitido>();
    public DbSet<CdrRegistro> CdrRegistros => Set<CdrRegistro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Emisor>(entity =>
        {
            entity.ToTable("Emisores");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Ruc).HasMaxLength(11).IsRequired();
            entity.HasIndex(x => x.Ruc).IsUnique();
            entity.Property(x => x.RazonSocial).HasMaxLength(500).IsRequired();
            entity.Property(x => x.NombreComercial).HasMaxLength(500);
            entity.Property(x => x.Ubigeo).HasMaxLength(6);
            entity.Property(x => x.Direccion).HasMaxLength(500);
            entity.Property(x => x.Departamento).HasMaxLength(100);
            entity.Property(x => x.Provincia).HasMaxLength(100);
            entity.Property(x => x.Distrito).HasMaxLength(100);
            entity.Property(x => x.AmbienteDefault).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ProveedorEnvio).HasMaxLength(50).IsRequired();
            entity.Property(x => x.UrlOverrideBeta).HasMaxLength(500);
            entity.Property(x => x.UrlOverrideProduccion).HasMaxLength(500);
        });

        modelBuilder.Entity<EmisorCertificado>(entity =>
        {
            entity.ToTable("EmisorCertificados");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PfxCiphertext).IsRequired();
            entity.Property(x => x.PasswordCiphertext).IsRequired();
            entity.Property(x => x.HuellaThumbprint).HasMaxLength(128);

            entity.HasOne(x => x.Emisor)
                .WithMany(x => x.Certificados)
                .HasForeignKey(x => x.EmisorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.EmisorId, x.Activo });
        });

        modelBuilder.Entity<EmisorCredencialSol>(entity =>
        {
            entity.ToTable("EmisorCredencialesSol");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Ambiente).HasMaxLength(20).IsRequired();
            entity.Property(x => x.UsuarioSolCiphertext).IsRequired();
            entity.Property(x => x.ClaveSolCiphertext).IsRequired();

            entity.HasOne(x => x.Emisor)
                .WithMany(x => x.CredencialesSol)
                .HasForeignKey(x => x.EmisorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.EmisorId, x.Ambiente, x.Activo });
        });

        modelBuilder.Entity<ComprobanteEmitido>(entity =>
        {
            entity.ToTable("ComprobantesEmitidos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Ruc).HasMaxLength(11).IsRequired();
            entity.Property(x => x.TipoComprobante).HasMaxLength(2).IsRequired();
            entity.Property(x => x.Serie).HasMaxLength(4).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Mensaje).HasMaxLength(2000);
            entity.Property(x => x.Hash).HasMaxLength(128);
            entity.Property(x => x.NombreXml).HasMaxLength(260);
            entity.Property(x => x.NombreZip).HasMaxLength(260);
            entity.Property(x => x.NombreCdr).HasMaxLength(260);
            entity.Property(x => x.RazonSocialEmisor).HasMaxLength(500);
            entity.Property(x => x.TipoDocumentoCliente).HasMaxLength(2);
            entity.Property(x => x.NumeroDocumentoCliente).HasMaxLength(20);
            entity.Property(x => x.RazonSocialCliente).HasMaxLength(500);
            entity.Property(x => x.Moneda).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.SistemaOrigen).HasMaxLength(100);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);

            entity.HasIndex(x => new { x.Ruc, x.TipoComprobante, x.Serie, x.Correlativo })
                .IsUnique();

            entity.HasOne(x => x.Emisor)
                .WithMany(x => x.Comprobantes)
                .HasForeignKey(x => x.EmisorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CdrRegistro>(entity =>
        {
            entity.ToTable("CdrRegistros");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CodigoSunat).HasMaxLength(20);
            entity.Property(x => x.MensajeSunat).HasMaxLength(2000);
            entity.Property(x => x.NombreArchivo).HasMaxLength(260);

            entity.HasOne(x => x.ComprobanteEmitido)
                .WithMany(x => x.Cdrs)
                .HasForeignKey(x => x.ComprobanteEmitidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
