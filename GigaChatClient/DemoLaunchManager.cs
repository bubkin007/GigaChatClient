using System;
using GigaChatClient.Models;

namespace GigaChatClient;

public sealed class DemoLaunchManager
{
    private readonly DemoLaunchSettings _settings;

    public DemoLaunchManager(DemoLaunchSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
    }

    public bool IsActive => _settings.Enabled;

    public VerificationResult ApprovePassport(PassportData passport)
    {
        ArgumentNullException.ThrowIfNull(passport);
        var reference = ComposePassportReference(passport);
        if (!_settings.Enabled)
        {
            return VerificationResult.Pending("Demo mode disabled", reference);
        }
        if (!_settings.AutoApprovePassport)
        {
            return VerificationResult.Pending("Manual passport verification required", reference);
        }
        return VerificationResult.Approved("Passport auto-approved in demo mode", reference);
    }

    public VerificationResult AcceptConfirmationCode(string code, VerificationScenario scenario)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var reference = $"{scenario}:{code}";
        if (string.Equals(code, _settings.UniversalConfirmationCode, StringComparison.Ordinal))
        {
            return VerificationResult.Approved($"Universal confirmation code accepted for {scenario}", reference);
        }
        if (!_settings.Enabled)
        {
            return VerificationResult.Rejected("Demo mode disabled", reference);
        }
        return VerificationResult.Rejected("Confirmation code mismatch", reference);
    }

    private static string ComposePassportReference(PassportData passport)
    {
        if (!string.IsNullOrWhiteSpace(passport.Series) && !string.IsNullOrWhiteSpace(passport.Number))
        {
            return $"{passport.Series}-{passport.Number}";
        }
        if (!string.IsNullOrWhiteSpace(passport.Number))
        {
            return passport.Number;
        }
        return Guid.NewGuid().ToString("N");
    }
}
