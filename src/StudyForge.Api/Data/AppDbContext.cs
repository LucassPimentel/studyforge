using Microsoft.EntityFrameworkCore;
using StudyForge.Api.Entities;

namespace StudyForge.Api.Data;

/// <summary>
/// Contexto EF Core do StudyForge, persistido em SQLite (studyforge.db).
/// Mapeia as entidades <see cref="Deck"/> e <see cref="Card"/> com relação 1..* e
/// exclusão em cascata.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Deck> Decks => Set<Deck>();

    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Deck>(deck =>
        {
            deck.HasKey(d => d.Id);
            deck.Property(d => d.Name).IsRequired();

            deck.HasMany(d => d.Cards)
                .WithOne(c => c.Deck)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Card>(card =>
        {
            card.HasKey(c => c.Id);
            card.Property(c => c.Question).IsRequired();
            card.Property(c => c.Answer).IsRequired();
        });
    }
}
