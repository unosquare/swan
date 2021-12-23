namespace Swan.Formatters;

internal class CsvMapping<TContainer, TTarget>
{
    internal delegate void ApplyMapping<TSource, TDestination>(CsvMapping<TSource, TDestination> mapping, TDestination instance);

    public CsvMapping(TContainer container, string heading, string targetName, ApplyMapping<TContainer, TTarget> apply)
    {
        Heading = heading;
        TargetName = targetName;
        Container = container;
        Apply = apply;
    }

    public string Heading { get; }

    public string TargetName { get; }

    public TContainer Container { get; }

    public ApplyMapping<TContainer, TTarget> Apply { get; }
}
