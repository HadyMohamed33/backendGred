namespace AlNady.Domain.Enums;

public enum EventType
{
    Login = 0,
    Logout = 1,
    Register = 2,
    PasswordChange = 3,
    PasswordReset = 4,
    EmailVerification = 5,
    ProfileUpdate = 6,
    PaymentInitiated = 7,
    PaymentCompleted = 8,
    PaymentFailed = 9,
    RefundProcessed = 10,
    EnrollmentSubmitted = 11,
    EnrollmentCancelled = 12,
    ProgramCreated = 13,
    ProgramUpdated = 14,
    ProgramDeleted = 15,
    AdminApprovedTrainer = 16,
    AdminRejectedTrainer = 17,
    AdminApprovedAcademy = 18,
    AdminRejectedAcademy = 19,
    UserBlacklisted = 20,
    UserRemovedFromBlacklist = 21,
    CertificateUploaded = 22,
    RatingSubmitted = 23,
    RatingEdited = 24,
    SecurityEvent = 25,
    ProgramPublished = 26
}
