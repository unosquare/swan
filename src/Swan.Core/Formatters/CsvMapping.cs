namespace Swan.Formatters
{
    internal class CsvMapping<TReader, TTarget>
    {
        internal delegate void ApplyMapping<TSource, TDestination>(CsvMapping<TSource, TDestination> mapping, TDestination instance);

        public CsvMapping(TReader reader, string heading, string targetName, ApplyMapping<TReader, TTarget> apply)
        {
            Heading = heading;
            TargetName = targetName;
            Reader = reader;
            Apply = apply;
        }

        public string Heading { get; }

        public string TargetName { get; }

        public TReader Reader { get; }

        public ApplyMapping<TReader, TTarget> Apply { get; }
    }
}
