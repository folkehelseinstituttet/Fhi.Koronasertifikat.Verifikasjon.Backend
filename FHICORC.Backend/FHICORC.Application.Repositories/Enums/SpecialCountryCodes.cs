namespace FHICORC.Application.Repositories.Enums
{
    /// <summary>
    /// Special country codes used for storing public keys until UK certificates can be retrieved from EU DGC Gateway
    /// Observe, that 'UK' is used as a prefix here to exclude the public keys from being deleted by the normal EU DGC certificate storing,
    /// and that the codes are not real country codes per https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes
    /// </summary>
    public enum SpecialCountryCodes
    {
        UK,
        UK_NI
    }
}
