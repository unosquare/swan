namespace Swan.Reflection;

internal sealed class TryParseMethodInfo
{
    private delegate bool TryParseDelegate(string input, out object? value);

    private const string TryParseMethodName = nameof(int.TryParse);
    private const string ParseMethodName = nameof(int.Parse);
    private const NumberStyles AnyStyle = NumberStyles.Any;

    private static readonly string[] FalseStrings = { "False", "false", "FALSE", "0", "no", "NO" };
    private static readonly IFormatProvider InvariantCulture = CultureInfo.InvariantCulture;
    private static readonly Type StringType = typeof(string);
    private static readonly Type NumberStylesType = typeof(NumberStyles);
    private static readonly Type FormatProviderType = typeof(IFormatProvider);

    private readonly ITypeInfo ParentType;
    private readonly TryParseDelegate MethodCall;

    public TryParseMethodInfo(ITypeInfo typeInfo)
    {
        ParentType = typeInfo;
        TryParseDelegate? delegateCall = null;

        try
        {
            delegateCall = BuildDelegate();
        }
        catch
        {
            // ignore
        }

        IsNative = delegateCall is not null;
        MethodCall = delegateCall ?? new((string input, out object? result) =>
        {
            result = ParentType.DefaultValue;
            return false;
        });
    }

    public bool IsNative { get; }

    public bool Invoke(string? input, out object? result)
    {
        result = ParentType.DefaultValue;
        if (input is null)
            return false;

        if (ParentType.NativeType == typeof(string))
        {
            result = input;
            return true;
        }

        var normalizedInput = input.Trim();
        return !string.IsNullOrWhiteSpace(normalizedInput) &&
               MethodCall.Invoke(normalizedInput, out result);
    }

    private TryParseDelegate? BuildDelegate()
    {
        var backingType = ParentType.BackingType.NativeType;
        var defaultValue = ParentType.DefaultValue;

        if (backingType == typeof(double))
            return (string input, out object? value) =>
            {
                value = defaultValue;
                if (!double.TryParse(input, AnyStyle, InvariantCulture, out var parsedValue))
                    return false;

                value = parsedValue;
                return true;
            };

        if (backingType == typeof(float))
            return (string input, out object? value) =>
            {
                value = defaultValue;
                if (!float.TryParse(input, AnyStyle, InvariantCulture, out var parsedValue))
                    return false;

                value = parsedValue;
                return true;
            };

        if (backingType == typeof(bool))
            return (string input, out object? value) =>
            {
                value = !FalseStrings.Contains(input);
                return true;
            };

        if (ParentType.IsEnum)
            return (string input, out object? value) => Enum.TryParse(ParentType.EnumType!.NativeType, input, true, out value);

        if (ParentType.IsNumeric)
            return (string input, out object? value) =>
            {
                value = defaultValue;
                var builder = new StringBuilder(input.Length);
                foreach (var c in input.AsSpan())
                {
                    if (!char.IsDigit(c) &&
                        c != '.' &&
                        c != 'e' &&
                        c != 'E' &&
                        c != '-' &&
                        c != '+')
                        continue;

                    builder.Append(c);
                }

                if (!decimal.TryParse(builder.ToString(), AnyStyle, InvariantCulture, out var parsedValue))
                    return false;

                if (ParentType.IsAssignableFrom(typeof(decimal)))
                {
                    value = parsedValue;
                    return true;
                }

                try
                {
                    value = Convert.ChangeType(parsedValue, backingType, InvariantCulture);
                    return true;
                }
                catch
                {
                    return false;
                }
            };

        var outputType = backingType.MakeByRefType();

        if (ParentType.BackingType.TryFindStaticMethod(TryParseMethodName, new[] { StringType, NumberStylesType, FormatProviderType, outputType }, out var method))
            return (string input, out object? value) =>
            {
                value = defaultValue;
                var parameters = new object?[] { input, AnyStyle, InvariantCulture, null };
                if (method.Invoke(null, parameters) is not bool result)
                    return false;

                value = parameters[^1];
                return result;
            };

        if (ParentType.BackingType.TryFindStaticMethod(TryParseMethodName, new[] { StringType, FormatProviderType, outputType }, out method))
            return (string input, out object? value) =>
            {
                value = defaultValue;
                var parameters = new object?[] { input, InvariantCulture, null };
                if (method.Invoke(null, parameters) is not bool result)
                    return false;

                value = parameters[^1];
                return result;
            };

        if (ParentType.BackingType.TryFindStaticMethod(TryParseMethodName, new[] { StringType, outputType }, out method))
            return (string input, out object? value) =>
            {
                value = defaultValue;
                var parameters = new object?[] { input, null };
                if (method.Invoke(null, parameters) is not bool result)
                    return false;

                value = parameters[^1];
                return result;
            };

        if (ParentType.BackingType.TryFindStaticMethod(ParseMethodName, new[] { StringType, FormatProviderType }, out method))
            return (string input, out object? value) =>
            {
                value = ParentType.DefaultValue;
                var parameters = new object?[] { input, InvariantCulture };

                try
                {
                    value = method.Invoke(null, parameters);
                    return true;
                }
                catch
                {
                        // ignore
                    }

                return false;
            };

        if (ParentType.BackingType.TryFindStaticMethod(ParseMethodName, new[] { StringType }, out method))
            return (string input, out object? value) =>
            {
                value = defaultValue;
                var parameters = new object?[] { input };

                try
                {
                    value = method.Invoke(null, parameters);
                    return true;
                }
                catch
                {
                        // ignore
                    }

                return false;
            };

        return default;
    }
}
