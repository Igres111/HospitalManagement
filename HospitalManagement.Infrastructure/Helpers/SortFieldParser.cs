namespace HospitalManagement.Infrastructure.Helpers
{
    public static class SortFieldParser
    {
        public static TEnum Parse<TEnum>(string? sortBy, TEnum defaultValue) where TEnum : struct, Enum
        {
            return Enum.TryParse<TEnum>(sortBy, ignoreCase: true, out var parsed)
                ? parsed
                : defaultValue;
        }
    }
}