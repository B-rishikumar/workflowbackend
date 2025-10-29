namespace WorkflowManagement.Shared.Constants;

public static class ConfigurationKeys
{
    public const string ConnectionStringDefault = "ConnectionStrings:DefaultConnection";
    public const string ConnectionStringRedis = "ConnectionStrings:Redis";
    public const string DatabaseProvider = "DatabaseProvider";
    
    public const string JwtSettingsSection = "JwtSettings";
    public const string JwtSecretKey = "JwtSettings:SecretKey";
    public const string JwtIssuer = "JwtSettings:Issuer";
    public const string JwtAudience = "JwtSettings:Audience";
    public const string JwtExpiryMinutes = "JwtSettings:ExpiryMinutes";
    
    public const string SmtpSettingsSection = "SmtpSettings";
    public const string SmtpHost = "SmtpSettings:Host";
    public const string SmtpPort = "SmtpSettings:Port";
    public const string SmtpUsername = "SmtpSettings:Username";
    public const string SmtpPassword = "SmtpSettings:Password";
    public const string SmtpFromEmail = "SmtpSettings:FromEmail";
    public const string SmtpFromName = "SmtpSettings:FromName";
    
    public const string FileStorageSection = "FileStorage";
    public const string FileStorageBasePath = "FileStorage:BasePath";
    public const string FileStorageMaxSizeBytes = "FileStorage:MaxSizeBytes";
    
    public const string AllowedOrigins = "AllowedOrigins";
}
