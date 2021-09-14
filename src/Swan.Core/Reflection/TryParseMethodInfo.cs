using System;
using System.Globalization;
using System.Reflection;

namespace Swan.Reflection
{
    internal sealed class TryParseMethodInfo : CommonMethodInfo
    {
        public TryParseMethodInfo(ITypeInfo typeInfo)
            : base(typeInfo, nameof(byte.TryParse))
        {
            // placeholder
        }

        protected override MethodInfo? RetriveMethodInfo(ITypeInfo typeInfo, string methodName) =>
            typeInfo.UnderlyingType.NativeType.GetMethod(MethodName,
                new[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider), typeInfo.UnderlyingType.NativeType.MakeByRefType() }) ??
            typeInfo.UnderlyingType.NativeType.GetMethod(MethodName,
                new[] { typeof(string), typeInfo.UnderlyingType.NativeType.MakeByRefType() });
    }
}
