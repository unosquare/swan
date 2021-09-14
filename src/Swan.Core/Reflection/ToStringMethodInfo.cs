using System;
using System.Reflection;

namespace Swan.Reflection
{
    internal sealed class ToStringMethodInfo : CommonMethodInfo
    {
        public ToStringMethodInfo(ITypeInfo typeInfo)
            : base(typeInfo, nameof(byte.TryParse))
        {
            // placeholder
        }

        protected override MethodInfo? RetriveMethodInfo(ITypeInfo typeInfo, string methodName) =>
            typeInfo.NativeType.GetMethod(methodName, new[] { typeof(IFormatProvider) }) ??
            typeInfo.NativeType.GetMethod(methodName, Array.Empty<Type>());
    }
}
