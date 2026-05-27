namespace XdtBox.LicenseIssuer;

public sealed class LicenseIssuerException : Exception
{
    public LicenseIssuerException(string message)
        : base(message)
    {
    }

    public LicenseIssuerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
