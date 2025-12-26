namespace Shared.Extensions
{
    public static class ExtensionString
    {
        public static Guid ToGuid(this string value)
        {
            try
            {
                return Guid.Parse(value);
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }
}
