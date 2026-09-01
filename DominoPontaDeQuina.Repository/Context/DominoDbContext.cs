using DominoPontaDeQuina.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Context;

public class DominoDbContext : DbContext
{
    public const string ConnectionStringPadrao = "Data Source=domino.db";

    public DominoDbContext()
    {
    }

    public DominoDbContext(DbContextOptions<DominoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;

    public DbSet<Jogador> Jogadores { get; set; } = null!;

    public DbSet<Partida> Partidas { get; set; } = null!;

    public DbSet<TimePartida> TimesPartida { get; set; } = null!;

    public DbSet<ParticipacaoPartida> ParticipacoesPartida { get; set; } = null!;

    public DbSet<Rodada> Rodadas { get; set; } = null!;

    public DbSet<Jogada> Jogadas { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(ConnectionStringPadrao);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurarUsuario(modelBuilder);
        ConfigurarJogador(modelBuilder);
        ConfigurarPartida(modelBuilder);
        ConfigurarTimePartida(modelBuilder);
        ConfigurarParticipacaoPartida(modelBuilder);
        ConfigurarRodada(modelBuilder);
        ConfigurarJogada(modelBuilder);
    }

    private static void ConfigurarUsuario(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasMany(usuario => usuario.Jogadores)
            .WithOne(jogador => jogador.Usuario)
            .HasForeignKey(jogador => jogador.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarJogador(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jogador>()
            .HasIndex(jogador => new { jogador.UsuarioId, jogador.NomeExibicao })
            .IsUnique();

        modelBuilder.Entity<Jogador>()
            .HasMany(jogador => jogador.Participacoes)
            .WithOne(participacao => participacao.Jogador)
            .HasForeignKey(participacao => participacao.JogadorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Jogador>()
            .HasMany(jogador => jogador.Jogadas)
            .WithOne(jogada => jogada.Jogador)
            .HasForeignKey(jogada => jogada.JogadorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Jogador>()
            .HasMany(jogador => jogador.RodadasVencidas)
            .WithOne(rodada => rodada.JogadorVencedor)
            .HasForeignKey(rodada => rodada.JogadorVencedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigurarPartida(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Partida>()
            .Property(partida => partida.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Partida>()
            .HasIndex(partida => partida.Status);

        modelBuilder.Entity<Partida>()
            .HasIndex(partida => partida.IniciadoEm);

        modelBuilder.Entity<Partida>()
            .HasMany(partida => partida.Participacoes)
            .WithOne(participacao => participacao.Partida)
            .HasForeignKey(participacao => participacao.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Partida>()
            .HasMany(partida => partida.Times)
            .WithOne(time => time.Partida)
            .HasForeignKey(time => time.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Partida>()
            .HasMany(partida => partida.Rodadas)
            .WithOne(rodada => rodada.Partida)
            .HasForeignKey(rodada => rodada.PartidaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarTimePartida(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TimePartida>()
            .HasIndex(time => new { time.PartidaId, time.Nome })
            .IsUnique();

        modelBuilder.Entity<TimePartida>()
            .HasMany(time => time.Participacoes)
            .WithOne(participacao => participacao.Time)
            .HasForeignKey(participacao => participacao.TimePartidaId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigurarParticipacaoPartida(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParticipacaoPartida>()
            .HasIndex(participacao => new { participacao.PartidaId, participacao.JogadorId })
            .IsUnique();
    }

    private static void ConfigurarRodada(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rodada>()
            .Property(rodada => rodada.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Rodada>()
            .Property(rodada => rodada.TipoFinalizacao)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<Rodada>()
            .HasIndex(rodada => new { rodada.PartidaId, rodada.Numero })
            .IsUnique();

        modelBuilder.Entity<Rodada>()
            .HasMany(rodada => rodada.Jogadas)
            .WithOne(jogada => jogada.Rodada)
            .HasForeignKey(jogada => jogada.RodadaId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarJogada(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jogada>()
            .Property(jogada => jogada.Lado)
            .HasConversion<string>()
            .HasMaxLength(10);

        modelBuilder.Entity<Jogada>()
            .HasIndex(jogada => new { jogada.RodadaId, jogada.Sequencia })
            .IsUnique();
    }
}
