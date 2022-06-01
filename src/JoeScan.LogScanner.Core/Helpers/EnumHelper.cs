namespace JoeScan.LogScanner.Core.Helpers;

public static class EnumHelper
{
    public static T FromString<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value);
    }
}
