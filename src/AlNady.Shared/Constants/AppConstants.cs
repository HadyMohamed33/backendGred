namespace AlNady.Shared.Constants;

public static class AppConstants
{
    public static class Roles
    {
        public const string Player = "Player";
        public const string Trainer = "Trainer";
        public const string Academy = "Academy";
        public const string Admin = "Admin";
    }

    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireTrainer = "RequireTrainer";
        public const string RequireAcademy = "RequireAcademy";
        public const string RequirePlayer = "RequirePlayer";
        public const string RequireTrainerOrAcademy = "RequireTrainerOrAcademy";
    }

    public static class Cache
    {
        public const string UserPrefix = "user:";
        public const string ProgramPrefix = "program:";
        public const string TrainerPrefix = "trainer:";
        public const string AcademyPrefix = "academy:";
        public const int DefaultExpirationMinutes = 15;
    }

    public static class Jwt
    {
        public const int AccessTokenExpirationMinutes = 60;
        public const int RefreshTokenExpirationDays = 30;
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
    }

    public static class Rating
    {
        public const int MinValue = 1;
        public const int MaxValue = 5;
        public const int EditWindowHours = 24;
    }

    public static class Refund
    {
        public const int FullRefundDaysBeforeProgram = 7;
        public const int PartialRefundDaysBeforeProgram = 3;
        public const decimal PartialRefundPercentage = 0.5m;
    }
}
