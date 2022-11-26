namespace domain.measures.infrastructure;

public interface ITranslatableString
{
    // Gets the values in the current thread's string
    string ToString();

    // locale can be in the form "en" or "en-us"
    string ToString(string locale);
}
