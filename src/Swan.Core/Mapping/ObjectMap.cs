namespace Swan.Mapping;

using Reflection;

/// <summary>
/// Provides a basic implementation of a <see cref="IObjectMap"/>
/// It's basically a dictionary of target properties to value providers.
/// </summary>
internal class ObjectMap : ConcurrentDictionary<IPropertyProxy, SourceValueProvider>, IObjectMap
{
    /// <summary>
    /// Creates a new instance of the <see cref="ObjectMap"/> class.
    /// It also populates a default map of properties that have the same names
    /// and compatible types.
    /// </summary>
    /// <param name="context">The parent object mapper containing all other maps.</param>
    /// <param name="sourceType">The source type for this object map.</param>
    /// <param name="targetType">The target type for this object map.</param>
    public ObjectMap(ObjectMapper context, ITypeInfo sourceType, ITypeInfo targetType)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));

        foreach (var targetProperty in TargetType.Properties())
        {
            if (!targetProperty.CanWrite)
                continue;

            if (!SourceType.TryFindProperty(targetProperty.PropertyName, out var sourceProperty))
                continue;

            if (!sourceProperty.CanRead)
                continue;

            if (!targetProperty.IsAssignableFrom(sourceProperty))
                continue;

            this[targetProperty] = (source) => sourceProperty.TryRead(source, out var value)
                ? value
                : targetProperty.DefaultValue;
        }
    }

    protected ObjectMapper Context { get; }

    /// <inheritdoc />
    public ITypeInfo TargetType { get; }

    /// <inheritdoc />
    public ITypeInfo SourceType { get; }

    /// <inheritdoc />
    public virtual object Apply(object source) =>
        Apply(source, TargetType.CreateInstance());

    /// <inheritdoc />
    public virtual object Apply(object source, object target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.GetType() != TargetType.NativeType)
            throw new ArgumentException($"Parameter {nameof(target)} must be of type '{TargetType.FullName}'");

        foreach (var (targetProperty, valueProvider) in this)
        {
            var sourceValue = valueProvider.Invoke(source);
            targetProperty.TryWrite(target, sourceValue);
        }

        return target;
    }
}
