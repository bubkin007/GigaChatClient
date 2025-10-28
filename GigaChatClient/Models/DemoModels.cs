using System;

namespace GigaChatClient.Models;

public enum VerificationStatus
{
    Approved,
    Pending,
    Rejected
}

public enum VerificationScenario
{
    Passport,
    Sms,
    Email,
    Manual
}

public sealed class DemoLaunchSettings
{
    public bool Enabled { get; init; } = true;

    public bool AutoApprovePassport { get; init; } = true;

    public string UniversalConfirmationCode { get; init; } = "6699";

    public static DemoLaunchSettings CreateDefault() => new();
}

public sealed class PassportData
{
    public required string Series { get; init; }

    public required string Number { get; init; }

    public string? Issuer { get; init; }

    public DateTime? IssueDate { get; init; }

    public string? OwnerFullName { get; init; }
}

public sealed class VerificationResult
{
    public VerificationStatus Status { get; }

    public string Message { get; }

    public DateTimeOffset Timestamp { get; }

    public string Reference { get; }

    private VerificationResult(VerificationStatus status, string message, DateTimeOffset timestamp, string reference)
    {
        Status = status;
        Message = message;
        Timestamp = timestamp;
        Reference = reference;
    }

    public static VerificationResult Approved(string message, string reference) => Create(VerificationStatus.Approved, message, reference);

    public static VerificationResult Pending(string message, string reference) => Create(VerificationStatus.Pending, message, reference);

    public static VerificationResult Rejected(string message, string reference) => Create(VerificationStatus.Rejected, message, reference);

    private static VerificationResult Create(VerificationStatus status, string message, string reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        return new VerificationResult(status, message, DateTimeOffset.UtcNow, reference);
    }
}
