using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AlNady.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Blacklist> Blacklists => Set<Blacklist>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<Academy> Academies => Set<Academy>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormResponse> FormResponses => Set<FormResponse>();
    public DbSet<FieldValue> FieldValues => Set<FieldValue>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Cancellation> Cancellations => Set<Cancellation>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<RatingAspect> RatingAspects => Set<RatingAspect>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is AlNady.Domain.Common.AuditableEntity &&
                        e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            var entity = (AlNady.Domain.Common.AuditableEntity)entry.Entity;
            if (entry.State == EntityState.Added)
                entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
