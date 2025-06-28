namespace TesorosChoco.API.Configuration;

/// <summary>
/// Consolidated application settings
/// Provides strongly-typed configuration binding for all application settings
/// </summary>
public class AppSettings
{
    public JwtSettings Jwt { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}

/// <summary>
/// JWT authentication settings
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// Database connection settings
/// </summary>
public class DatabaseSettings
{
    public string DefaultConnection { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
}

/// <summary>
/// Email service settings
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

/// <summary>
/// Cache settings
/// </summary>
public class CacheSettings
{
    public string? RedisConnection { get; set; }
    public int DefaultExpirationMinutes { get; set; } = 60;
    public bool EnableDistributedCache { get; set; } = false;
}

/// <summary>
/// Security settings
/// </summary>
public class SecuritySettings
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public int MaxRequestSizeBytes { get; set; } = 1048576; // 1MB
    public int RateLimitRequestsPerMinute { get; set; } = 100;
    public bool EnableHsts { get; set; } = true;
    public bool RequireHttps { get; set; } = true;
}
