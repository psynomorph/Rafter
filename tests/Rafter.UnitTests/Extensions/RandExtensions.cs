namespace Rafter.UnitTests.Extensions;

internal static class RandExtensions
{
    public static TEnum NextEnumValue<TEnum>(this Random random) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        return random.NextArrayElement(values);
    }

    public static T NextArrayElement<T>(this Random random, T[] values)
    {
        var index = random.Next(values.Length);
        return values[index];
    }
}
