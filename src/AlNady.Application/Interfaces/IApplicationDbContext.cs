using AlNady.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Interfaces;

/// <summary>
/// Application-layer abstraction over the EF Core DbContext.
/// Infrastructure implements this — Application handlers depend only on this interface.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Trainer> Trainers { get; }
    DbSet<Academy> Academies { get; }
    DbSet<Certificate> Certificates { get; }
    DbSet<TrainingProgram> TrainingPrograms { get; }
    DbSet<Form> Forms { get; }
    DbSet<FormField> FormFields { get; }
    DbSet<FormResponse> FormResponses { get; }
    DbSet<FieldValue> FieldValues { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Cancellation> Cancellations { get; }
    DbSet<Rating> Ratings { get; }
    DbSet<RatingAspect> RatingAspects { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Session> Sessions { get; }
    DbSet<VerificationCode> VerificationCodes { get; }
    DbSet<EventLog> EventLogs { get; }
    DbSet<Blacklist> Blacklists { get; }
    DbSet<UserPreference> UserPreferences { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
