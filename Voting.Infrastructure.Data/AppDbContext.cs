using Microsoft.EntityFrameworkCore;
using Voting.Domain.Aggregates;
using Voting.Domain.Entities;

namespace Voting.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<VotingSessionAggregate> VotingSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -------------------------------
            // 1) User
            // -------------------------------
            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("Users");
                builder.HasKey(u => u.Id);

                builder.Property(u => u.Email)
                       .IsRequired()
                       .HasMaxLength(256);

                builder.Property(u => u.BlockchainAddress)
                       .IsRequired()
                       .HasMaxLength(128);

                builder.Property(u => u.Role)
                       .HasConversion<string>()
                       .IsRequired();

                builder.Property(u => u.VerificationLevel)
                       .HasConversion<int>()
                       .IsRequired();
            });

            // -------------------------------
            // 2) VotingSessionAggregate
            // -------------------------------
            modelBuilder.Entity<VotingSessionAggregate>(builder =>
            {
                builder.ToTable("VotingSessions");
                builder.HasKey(v => v.Id);

                builder.Property(v => v.AdminUserId).IsRequired();
                builder.Property(v => v.Mode)
                       .HasConversion<string>()
                       .IsRequired();
                builder.Property(v => v.RequiredVerificationLevel)
                       .HasConversion<int>()
                       .IsRequired();
                builder.Property(v => v.VotingActive).IsRequired();
                builder.Property(v => v.StartTimeUtc);
                builder.Property(v => v.EndTimeUtc);

                // -------------------------------
                // 2.1) Candidates — отдельная таблица
                // -------------------------------
                builder.OwnsMany(v => v.Candidates, nav =>
                {
                    nav.ToTable("VotingSessionCandidates");
                    nav.WithOwner().HasForeignKey("SessionId");
                    nav.HasKey("SessionId", nameof(Candidate.Id));

                    nav.Property<uint>(nameof(Candidate.Id))
                       .HasColumnName("CandidateId")
                       .ValueGeneratedNever();

                    nav.Property(c => c.Name)
                       .IsRequired()
                       .HasMaxLength(200);

                    nav.Property(c => c.Description)
                       .HasDefaultValue(string.Empty);

                    nav.Property(c => c.VoteCount)
                       .IsRequired();
                });

                // говорим EF, что за навигацией Candidates скрыто поле _candidates
                builder.Navigation(v => v.Candidates)
                       .HasField("_candidates")
                       .UsePropertyAccessMode(PropertyAccessMode.Field);

                // -------------------------------
                // 2.2) RegisteredUserIds как uuid[]
                // -------------------------------
                builder.Property(v => v.RegisteredUserIds)
                       .HasConversion(
                           v => v.ToArray(),
                           v => (IReadOnlyCollection<Guid>)v.ToList().AsReadOnly())
                       .HasColumnType("uuid[]")
                       .HasField("_registeredUserIds");

                // -------------------------------
                // 2.3) VotedUserIds как uuid[]
                // -------------------------------
                builder.Property(v => v.VotedUserIds)
                       .HasConversion(
                           v => v.ToArray(),
                           v => (IReadOnlyCollection<Guid>)v.ToList().AsReadOnly())
                       .HasColumnType("uuid[]")
                       .HasField("_votedUserIds");
            });
        }
    }
}
