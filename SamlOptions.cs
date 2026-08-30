namespace EntraSamlLab;

public sealed class SamlOptions
{
    public const string SectionName = "Saml";

    public string ApplicationName { get; set; } = "Entra SAML Lab";
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string AcsUrl { get; set; } = string.Empty;
    public string MetadataUrl { get; set; } = string.Empty;
    public string IdentityProviderEntityId { get; set; } = string.Empty;
    public string IdentityProviderLoginUrl { get; set; } = string.Empty;
    public string IdentityProviderLogoutUrl { get; set; } = string.Empty;
    public string IdentityProviderMetadataUrl { get; set; } = string.Empty;

    public bool HasServiceProviderConfiguration =>
        HasAbsoluteUri(PublicBaseUrl) &&
        HasAbsoluteUri(EntityId) &&
        HasAbsoluteUri(AcsUrl) &&
        HasAbsoluteUri(MetadataUrl);

    public bool HasIdentityProviderConfiguration =>
        HasAbsoluteUri(IdentityProviderEntityId) &&
        (HasAbsoluteUri(IdentityProviderLoginUrl) || HasAbsoluteUri(IdentityProviderMetadataUrl));

    private static bool HasAbsoluteUri(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out _);
}