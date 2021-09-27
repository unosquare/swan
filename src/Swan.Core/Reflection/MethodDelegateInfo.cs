#pragma warning disable CA1031 // Do not catch general exception types
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Swan.Reflection
{
    internal sealed class MethodDelegateInfo
    {
        private readonly Lazy<IReadOnlyList<ParameterInfo>> _parametersLazy;

        private MethodDelegateInfo(ITypeInfo parentType, MethodInfo method, Type[] methodTypes)
        {
            ParentType = parentType;
            Method = method;
            MethodTypes = methodTypes;
            _parametersLazy = new(() => Method.GetParameters(), true);
        }

        public IReadOnlyList<Type> MethodTypes { get; }

        /// <summary>
        /// Gets the type proxy that owns this property proxy.
        /// </summary>
        public ITypeInfo ParentType { get; }

        public MethodInfo Method { get; }

        public IReadOnlyList<ParameterInfo> Parameters => _parametersLazy.Value;

        public Delegate Delegate { get; private set; }

        public static bool TryCreate<T>(TypeInfo parentType,
            bool useUnderlying,
            string methodName,
            Type[] types,
            [MaybeNullWhen(false)]out MethodDelegateInfo methodInfo)
            where T : Delegate
        {
            methodInfo = default;
            try
            {
                var targetType = useUnderlying ? parentType.UnderlyingType : parentType;
                var method = targetType.NativeType.GetMethod(methodName, types);
                if (method is null)
                    return false;

                methodInfo = new(targetType, method, types)
                {
                    Delegate = method.CreateDelegate<T>()
                };

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#pragma warning restore CA1031 // Do not catch general exception types