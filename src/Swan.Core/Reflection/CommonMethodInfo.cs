#pragma warning disable CA1031 // Do not catch general exception types
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swan.Reflection
{
    internal abstract class CommonMethodInfo
    {
        private readonly Lazy<MethodInfo?> MethodLazy;
        private readonly Lazy<IReadOnlyList<ParameterInfo>> ParametersLazy;

        protected CommonMethodInfo(ITypeInfo typeInfo, string methodName)
        {
            MethodName = methodName;
            MethodLazy = new(() =>
            {
                try
                {
                    return RetriveMethodInfo(typeInfo, methodName);
                }
                catch
                {
                    return default;
                }
            }, true);

            ParametersLazy = new(() =>
            {
                var result = Array.Empty<ParameterInfo>();
                try
                {
                    if (Method is null)
                        return result;

                    return Method.GetParameters();
                }
                catch
                {
                    return result;
                }
            }, true);
        }

        public string MethodName { get; }

        public MethodInfo? Method => MethodLazy.Value;

        public IReadOnlyList<ParameterInfo> Parameters => ParametersLazy.Value;

        protected abstract MethodInfo? RetriveMethodInfo(ITypeInfo typeInfo, string methodName);
    }
}
#pragma warning restore CA1031 // Do not catch general exception types