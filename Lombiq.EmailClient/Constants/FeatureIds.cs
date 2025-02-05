namespace Lombiq.EmailClient.Constants;

public static class FeatureIds
{
    public const string Area = "Lombiq.EmailClient";

    public const string Default = Area;
    public const string Imap = Area + "." + nameof(Imap);
    public const string EmailSync = Area + "." + nameof(EmailSync);
}
