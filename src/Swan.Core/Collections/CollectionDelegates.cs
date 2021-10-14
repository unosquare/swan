namespace Swan.Collections
{
    using Swan.Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class CollectionDelegates
    {
        private readonly Lazy<Action?> ClearLazy;
        private readonly Lazy<Action<object?>?> RemoveLazy;
        private readonly Lazy<Action<object?>?> AddValueLazy;
        private readonly Lazy<Action<object, object?>?> AddKeyValueLazy;
        private readonly Lazy<Func<object?, bool>?> ContainsLazy;
        private readonly Lazy<Func<object?, bool>?> ContainsKeyLazy;
        private readonly Lazy<Action<int, object?>?> InsertLazy;
        private readonly Lazy<Action<int>?> RemoveAtLazy;
        private readonly Lazy<Func<int, object?>?> IndexerIntGetterLazy;
        private readonly Lazy<Action<int, object?>?> IndexerIntSetterLazy;
        private readonly Lazy<Func<object, object?>?> IndexerKeyGetterLazy;
        private readonly Lazy<Action<object, object?>?> IndexerKeySetterLazy;

        public CollectionDelegates(IEnumerable target, ICollectionInfo info)
        {
            ClearLazy = new(() => info.SourceType.TryFindPublicMethod(nameof(IList.Clear), null, out var method)
                ? method.CreateDelegate<Action>(target)
                : default, true);

            RemoveLazy = new(() =>
            {
                var elementType = info.IsDictionary ? info.KeysType.NativeType : info.ValuesType.NativeType;
                var parameterTypes = new[] { elementType };

                if (!info.SourceType.TryFindPublicMethod(nameof(IList.Remove), parameterTypes, out var method))
                    return default;

                elementType = method.GetParameters().First().ParameterType;
                var valueParameter = Expression.Parameter(typeof(object), "value");
                var body = Expression.Call(
                    Expression.Constant(target), method, Expression.Convert(valueParameter, elementType));
                return Expression.Lambda<Action<object?>>(body, valueParameter).Compile();
            }, true);

            RemoveAtLazy = new(() =>
            {
                var parameterTypes = new[] { typeof(int) };
                if (!info.SourceType.TryFindPublicMethod(nameof(IList.RemoveAt), parameterTypes, out var method))
                    return default;

                var valueParameter = Expression.Parameter(typeof(int), "value");
                var body = Expression.Call(
                    Expression.Constant(target), method, valueParameter);
                return Expression.Lambda<Action<int>>(body, valueParameter).Compile();
            }, true);

            AddValueLazy = new(() =>
            {
                if (info.IsDictionary)
                    return default;

                var elementType = info.ValuesType.NativeType;
                var parameterTypes = new[] { elementType };

                if (!info.SourceType.TryFindPublicMethod(nameof(IList.Add), parameterTypes, out var method))
                    return default;

                elementType = method.GetParameters().First().ParameterType;
                var valueParameter = Expression.Parameter(typeof(object), "value");
                var body = Expression.Call(
                    Expression.Constant(target), method, Expression.Convert(valueParameter, elementType));
                return Expression.Lambda<Action<object?>>(body, valueParameter).Compile();
            }, true);

            AddKeyValueLazy = new(() =>
            {
                if (!info.IsDictionary)
                    return default;

                var keysType = info.KeysType.NativeType;
                var valuesType = info.ValuesType.NativeType;
                var parameterTypes = new[] { keysType, valuesType };

                if (!info.SourceType.TryFindPublicMethod(nameof(IDictionary.Add), parameterTypes, out var method))
                    return default;

                var keyParameter = Expression.Parameter(typeof(object), "key");
                var valueParameter = Expression.Parameter(typeof(object), "value");

                keysType = method.GetParameters().First().ParameterType;
                valuesType = method.GetParameters().Last().ParameterType;

                var body = Expression.Call(
                    Expression.Constant(target), method,
                    Expression.Convert(keyParameter, keysType),
                    Expression.Convert(valueParameter, valuesType));
                return Expression.Lambda<Action<object, object?>>(body, keyParameter, valueParameter).Compile();
            }, true);

            ContainsLazy = new(() =>
            {
                var elementType = info.IsDictionary ? info.KeysType.NativeType : info.ValuesType.NativeType;
                var parameterTypes = new[] { elementType };

                if (!info.SourceType.TryFindPublicMethod(nameof(IDictionary.Contains), parameterTypes, out var method))
                    return default;

                elementType = method.GetParameters().First().ParameterType;
                var valueParameter = Expression.Parameter(typeof(object), "value");

                var body = Expression.Call(
                    Expression.Constant(target), method,
                    Expression.Convert(valueParameter, elementType));
                return Expression.Lambda<Func<object?, bool>>(body, valueParameter).Compile();
            }, true);

            ContainsKeyLazy = new(() =>
            {
                if (!info.IsDictionary)
                    return default;

                var elementType = info.KeysType.NativeType;
                var parameterTypes = new[] { elementType };

                if (!info.SourceType.TryFindPublicMethod(nameof(IDictionary<int, int>.ContainsKey), parameterTypes, out var method))
                    return default;

                elementType = method.GetParameters().First().ParameterType;
                var valueParameter = Expression.Parameter(typeof(object), "value");

                var body = Expression.Call(
                    Expression.Constant(target), method,
                    Expression.Convert(valueParameter, elementType));
                return Expression.Lambda<Func<object?, bool>>(body, valueParameter).Compile();
            }, true);

            InsertLazy = new(() =>
            {
                if (info.IsDictionary)
                    return default;

                var elementType = info.KeysType.NativeType;
                var parameterTypes = new[] { typeof(int), elementType };

                if (!info.SourceType.TryFindPublicMethod(nameof(IList<int>.Insert), parameterTypes, out var method))
                    return default;

                elementType = method.GetParameters().Last().ParameterType;
                var indexParameter = Expression.Parameter(typeof(int), "index");
                var valueParameter = Expression.Parameter(typeof(object), "value");

                var body = Expression.Call(
                    Expression.Constant(target), method,
                    indexParameter, Expression.Convert(valueParameter, elementType));
                return Expression.Lambda<Action<int, object?>>(body, indexParameter, valueParameter).Compile();

            }, true);

            IndexerIntGetterLazy = new(() =>
            {
                var allProperties = info.SourceType.NativeType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToArray();

                var indexer = allProperties
                    .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                    .FirstOrDefault(c =>
                        c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType == typeof(int));

                if (indexer is null)
                    return default;

                var argument = Expression.Parameter(typeof(int), "index");
                var property = Expression.Convert(
                    Expression.Property(Expression.Constant(target), indexer.Property, argument),
                    typeof(object));

                var getter = Expression
                    .Lambda<Func<int, object?>>(property, argument)
                    .Compile();

                return getter;

            }, true);

            IndexerKeyGetterLazy = new(() =>
            {
                var allProperties = info.SourceType.NativeType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToArray();

                var indexer = allProperties
                    .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                    .FirstOrDefault(c => c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType != typeof(int));

                if (indexer is null)
                    return default;

                var argument = Expression.Parameter(typeof(object), "index");
                var conversionType = indexer.IndexParameters[0].ParameterType;

                var property = Expression.Convert(
                    Expression.Property(Expression.Constant(target),
                        indexer.Property,
                        Expression.Convert(argument, conversionType)),
                    typeof(object));

                var getter = Expression
                    .Lambda<Func<object, object?>>(property, argument)
                    .Compile();

                return getter;
            }, true);

            IndexerIntSetterLazy = new(() =>
            {
                var allProperties = info.SourceType.NativeType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToArray();

                var indexer = allProperties
                    .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                    .FirstOrDefault(c =>
                        c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType == typeof(int));

                if (indexer is null)
                    return default;

                var valueArgument = Expression.Parameter(typeof(object), "value");
                var indexArgument = Expression.Parameter(typeof(int), "index");

                var body = Expression.Assign(
                    Expression.Property(Expression.Constant(target), indexer.Property, indexArgument),
                    Expression.Convert(valueArgument, indexer.Property.PropertyType));

                var setter = Expression
                    .Lambda<Action<int, object?>>(body, indexArgument, valueArgument)
                    .Compile();

                return setter;

            }, true);

            IndexerKeySetterLazy = new(() =>
            {
                var allProperties = info.SourceType.NativeType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .ToArray();

                var indexer = allProperties
                    .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                    .FirstOrDefault(c => c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType != typeof(int));

                if (indexer is null)
                    return default;

                var valueArgument = Expression.Parameter(typeof(object), "value");
                var keyArgument = Expression.Parameter(typeof(object), "key");
                var keyType = indexer.IndexParameters[0].ParameterType;

                var body = Expression.Assign(
                    Expression.Property(
                        Expression.Constant(target),
                        indexer.Property,
                        Expression.Convert(keyArgument, keyType)),
                    Expression.Convert(valueArgument, indexer.Property.PropertyType));

                var setter = Expression
                    .Lambda<Action<object, object?>>(body, keyArgument, valueArgument)
                    .Compile();

                return setter;

            }, true);
        }

        public Action? Clear => ClearLazy.Value;

        public Action<object?>? Remove => RemoveLazy.Value;

        public Action<object?>? AddValue => AddValueLazy.Value;

        public Action<object, object?>? AddKeyValue => AddKeyValueLazy.Value;

        public Func<object?, bool>? Contains => ContainsLazy.Value;

        public Func<object?, bool>? ContainsKey => ContainsKeyLazy.Value;

        public Action<int, object?>? Insert => InsertLazy.Value;

        public Action<int>? RemoveAt => RemoveAtLazy.Value;

        public Func<int, object?>? IndexerIntGetter => IndexerIntGetterLazy.Value;

        public Func<object, object?>? IndexerObjGetter => IndexerKeyGetterLazy.Value;

        public Action<int, object?>? IndexerIntSetter => IndexerIntSetterLazy.Value;

        public Action<object, object?>? IndexerObjSetter => IndexerKeySetterLazy.Value;
    }
}
