namespace domain.measures.infrastructure;

public class SimpleTranslatableString : ITranslatableString
{
    private readonly string enValue;
    private readonly string itValue;

    public SimpleTranslatableString(string enValue, string itValue)
    {
        this.enValue = enValue;
        this.itValue = itValue;
    }

    // Gets the values in the current thread's string
    public override string ToString()
    {
        return itValue;
    }

    // locale can be in the form "en" or "en-us"
    public string ToString(string locale)
    {
        switch (locale.ToLowerInvariant())
        {
            case "it":
            case "it-it":
            case "": return itValue;

            default: return enValue;
        }
    }
}
