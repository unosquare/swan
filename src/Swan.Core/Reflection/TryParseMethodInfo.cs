using System;
using System.Globalization;
using System.Linq;

namespace Swan.Reflection
{
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

        private readonly ITypeInfo _parentType;
        private readonly Type _nativeType;
        private readonly TryParseDelegate MethodCall;

        public TryParseMethodInfo(ITypeInfo typeInfo)
        {
            _parentType = typeInfo;
            _nativeType = typeInfo.UnderlyingType.NativeType;

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
                result = _parentType.DefaultValue;
                try
                {
                    result = Convert.ChangeType(input, _nativeType, InvariantCulture);
                    return true;
                }
                catch
                {
                    // ignore
                }

                return false;
            });
        }

        public bool IsNative { get; }

        public bool Invoke(string? input, out object? result)
        {
            result = _parentType.DefaultValue;
            if (input is null)
                return false;

            var normalizedInput = input.Trim();
            return !string.IsNullOrWhiteSpace(normalizedInput) &&
                   MethodCall.Invoke(normalizedInput, out result);
        }

        private TryParseDelegate? BuildDelegate()
        {
            if (_nativeType == typeof(double))
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
                    if (!double.TryParse(input, AnyStyle, InvariantCulture, out var parsedValue))
                        return false;

                    value = parsedValue;
                    return true;
                };

            if (_nativeType == typeof(float))
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
                    if (!float.TryParse(input, AnyStyle, InvariantCulture, out var parsedValue))
                        return false;

                    value = parsedValue;
                    return true;
                };

            if (_parentType.IsNumeric)
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
                    if (!decimal.TryParse(input, AnyStyle, InvariantCulture, out var parsedValue))
                        return false;

                    try
                    {
                        value = Convert.ChangeType(parsedValue, _nativeType, InvariantCulture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                };

            if (_nativeType == typeof(bool))
                return (string input, out object? value) =>
                {
                    value = !FalseStrings.Contains(input);
                    return true;
                };

            var outputType = _nativeType.MakeByRefType();

            var method = _nativeType.GetMethod(TryParseMethodName, new[] { StringType, NumberStylesType, FormatProviderType, outputType });
            if (method is not null)
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
                    var parameters = new object?[] { input, AnyStyle, InvariantCulture, null };
                    if (method.Invoke(null, parameters) is not bool result)
                        return false;

                    value = parameters[^1];
                    return result;
                };

            method = _nativeType.GetMethod(TryParseMethodName, new[] { StringType, FormatProviderType, outputType });
            if (method is not null)
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
                    var parameters = new object?[] { input, InvariantCulture, null };
                    if (method.Invoke(null, parameters) is not bool result)
                        return false;

                    value = parameters[^1];
                    return result;
                };

            method = _nativeType.GetMethod(TryParseMethodName, new[] { StringType, outputType });
            if (method is not null)
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
                    var parameters = new object?[] { input, null };
                    if (method.Invoke(null, parameters) is not bool result)
                        return false;

                    value = parameters[^1];
                    return result;
                };

            method = _nativeType.GetMethod(ParseMethodName, new[] { StringType, FormatProviderType });
            if (method is not null)
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
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

            method = _nativeType.GetMethod(ParseMethodName, new[] { StringType });
            if (method is not null)
                return (string input, out object? value) =>
                {
                    value = _parentType.DefaultValue;
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
}
