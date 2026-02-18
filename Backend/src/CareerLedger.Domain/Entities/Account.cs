namespace CareerLedger.Domain.Entities;

/// <summary>
/// Represents a user account. Single-user for MVP, supports multi-user in future.
/// </summary>
public class Account
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; private set; }

    /// <summary>Email address (unique, used for login)</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>BCrypt hashed password (never store plaintext!)</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Whether account is active (for soft delete or suspension)</summary>
    public bool IsActive { get; private set; }

    /// <summary>Account creation timestamp (UTC)</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Navigation property to applications owned by this account</summary>
    public ICollection<Application> Applications { get; private set; } = new List<Application>();

    // Private constructor for EF Core
    private Account() { }

    /// <summary>
    /// Factory method to create a new account with validation.
    /// </summary>
    /// <param name="email">Email address (must be valid format)</param>
    /// <param name="passwordHash">BCrypt hashed password</param>
    /// <returns>New Account instance</returns>
    /// <exception cref="ArgumentException">If email is invalid or password hash is empty</exception>
    public static Account Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (email.Length < 5 || email.Length > 255)
            throw new ArgumentException("Email must be between 5 and 255 characters", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Email must be a valid email address", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        return new Account
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Deactivate account (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivate account
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
    }
}
