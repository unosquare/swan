using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swan.Reflection
{
    internal abstract class CommonMethodInfo
    {
        protected CommonMethodInfo(ITypeProxy typeInfo, string methodName)
        {
            MethodName = methodName;
            Method = null;
            Parameters = Array.Empty<ParameterInfo>();

            try
            {
                Method = RetriveMethodInfo(typeInfo, methodName);
                if (Method is null)
                    return;

                Parameters = Method.GetParameters();
            }
            catch
            {
                // ignore
            }
        }

        public string MethodName { get; }

        public MethodInfo? Method { get; }

        public IReadOnlyList<ParameterInfo> Parameters { get; }

        protected abstract MethodInfo? RetriveMethodInfo(ITypeProxy typeInfo, string methodName);
    }
}
