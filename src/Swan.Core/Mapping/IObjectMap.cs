namespace Swan.Mapping;

using Swan.Reflection;
using System.Linq.Expressions;

/// <summary>
/// A delegate providing values and receiving a instance to produce values from.
/// </summary>
/// <param name="instance">An instance of an object.</param>
/// <returns>The value extracted from the instance.</returns>
public delegate object? SourceValueProvider(object instance);

/// <summary>
/// Simple interface for an object map
/// in which a target and a source type is defined and it's just
/// a dictionary of properties and value providers.
/// </summary>
public interface IObjectMap : IDictionary<IPropertyProxy, SourceValueProvider>
{
    /// <summary>
    /// Gets the type of the target.
    /// </summary>
    ITypeInfo TargetType { get; }

    /// <summary>
    /// Gets the type of the source.
    /// </summary>
    ITypeInfo SourceType { get; }

    /// <summary>
    /// Creates an instance of the target type and evaluates the <see cref="SourceValueProvider"/>
    /// delegates for each of the mapped properties. The target type must have a parameterless constructor.
    /// </summary>
    /// <param name="source">The source object passed to each of the value provider delegates.</param>
    /// <returns>An instance of <see cref="TargetType"/> with mapped values from the source object.</returns>
    object Apply(object source);

    /// <summary>
    /// Evaluates the <see cref= "SourceValueProvider" /> delegate
    /// associated with each of the mapped properties, setting the target properties if possible.
    /// </summary>
    /// <param name="source">The source object passed to the value provider delegate.</param>
    /// <param name="target">The target object to write to.</param>
    /// <returns>The target object with applied properties.</returns>
    object Apply(object source, object target);
}

/// <summary>
/// Generic version of the 
/// </summary>
/// <typeparam name="TSource">The type used to read properties from.</typeparam>
/// <typeparam name="TTarget">The type used to write properties to.</typeparam>
public interface IObjectMap<TSource, TTarget> : IObjectMap
{
    /// <summary>
    /// Creates an instance of the target type and evaluates the <see cref="SourceValueProvider"/>
    /// delegates for each of the mapped properties. The target type must have a parameterless constructor.
    /// </summary>
    /// <param name="source">The source object passed to each of the value provider delegates.</param>
    /// <returns>An instance of the target type with mapped values from the source object.</returns>
    TTarget Apply(TSource source);

    /// <summary>
    /// Evaluates the <see cref= "SourceValueProvider" /> delegate
    /// associated with each of the mapped properties, setting the target properties if possible.
    /// </summary>
    /// <param name="source">The source object passed to the value provider delegate.</param>
    /// <param name="target">The target object to write to.</param>
    /// <returns>The target object with applied properties.</returns>
    TTarget Apply(TSource source, TTarget target);

    /// <summary>
    /// Adds or replaces a path to populate a target via a source delegate.
    /// </summary>
    /// <typeparam name="TTargetMember">The type of the target property.</typeparam>
    /// <typeparam name="TSourceMember">The type of the source property.</typeparam>
    /// <param name="targetPropertyExpression">The expression containing the target property to write.</param>
    /// <param name="valueProvider">The delegate the consumes and assigns the source property.</param>
    /// <returns>This instance of the object map to provide a fluent API.</returns>
    IObjectMap<TSource, TTarget> Add<TTargetMember, TSourceMember>(
        Expression<Func<TTarget, TTargetMember>> targetPropertyExpression,
        Func<TSource, TSourceMember> valueProvider);

    /// <summary>
    /// Removes a property path from this map.
    /// </summary>
    /// <typeparam name="TTargetProperty">The type of the target property.</typeparam>
    /// <param name="targetPropertyExpression">The expression containing the target property to write.</param>
    /// <returns>This instance of the object map to provide a fluent API.></returns>
    IObjectMap<TSource, TTarget> Remove<TTargetProperty>(
        Expression<Func<TTarget, TTargetProperty>> targetPropertyExpression);
}
