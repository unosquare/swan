namespace Swan.Formatters;

using System.IO;

/// <summary>
/// Provides a base class for reading schema aware <see cref="CsvReader"/> streams
/// where headings are used to map and transform specific target members.
/// </summary>
public abstract class CsvReader<TReader, TLine> : CsvReaderBase<TLine>
    where TReader : CsvReaderBase<TLine>
{
    private readonly Dictionary<string, int> _headings = new(64);

    /// <summary>
    /// Creates a new instance of the <see cref="CsvReader{TReader,TLine}"/> class.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <param name="separatorChar">The field separator character.</param>
    /// <param name="escapeChar">The escape character.</param>
    /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
    /// <param name="trimsValues">True to trim field values as they are read and parsed.</param>
    /// <param name="trimsHeadings">True to trim heading values as they are read and parsed.</param>
    protected CsvReader(Stream stream,
        Encoding? encoding,
        char separatorChar,
        char escapeChar,
        bool leaveOpen,
        bool trimsValues,
        bool trimsHeadings)
        : base(stream, encoding, separatorChar, escapeChar, leaveOpen, trimsValues)
    {
        TrimsHeadings = trimsHeadings;
    }

    /// <summary>
    /// Gets a value indicating whether heading names are trimmed as they
    /// are read and parsed from the underlying stream.
    /// </summary>
    public bool TrimsHeadings { get; }

    /// <summary>
    /// Gets a dictionary of headings and their corresponding indices.
    /// If not headings have been read, the 
    /// method.
    /// </summary>
    public IReadOnlyDictionary<string, int> Headings
    {
        get
        {
            RequireHeadings(false);
            return _headings;
        }
    }

    /// <summary>
    /// Gets the index of the given heading name.
    /// </summary>
    /// <param name="heading">The name of the heading.</param>
    /// <returns>Returns a valid field index or -1 for invalid or not found.</returns>
    public virtual int IndexOf(string heading) =>
        !Headings.TryGetValue(heading, out var index) ? -1 : index;

    /// <summary>
    /// Gets the string value of a given field name.
    /// </summary>
    /// <param name="heading">The field name (heading) to retrieve the data from.</param>
    /// <param name="value">The return value.</param>
    /// <returns></returns>
    public virtual bool TryGetValue(string heading, out string value) =>
        TryGetValue(IndexOf(heading), out value);

    /// <summary>
    /// When the underlying stream has no headings, call this method for the
    /// current reader to become schema-aware.
    /// </summary>
    /// <param name="names">The heading names.</param>
    /// <returns>This instance so that a fluent API is available.</returns>
    public TReader SetHeadings(params string[] names)
    {
        if (_headings.Any())
            throw new InvalidOperationException($"The {nameof(Headings)} have already been set.");

        if (names is null || names.Length <= 0)
            throw new ArgumentException("Headings must contain at least one element.", nameof(names));

        for (var i = 0; i < names.Length; i++)
            _headings[names[i]] = i;

        OnHeadingsRead(names);
        return (this as TReader)!;
    }

    /// <summary>
    /// Parses a set of literals from the current positions of the underlying
    /// stream just as MoveNext but does not increment the Count.
    /// and does not set Values property.
    /// </summary>
    /// <param name="skipCount">The number of records to skip.</param>
    /// <returns>This instance so that a fluent API is available.</returns>
    public new TReader Skip(int skipCount = 1)
    {
        base.Skip(skipCount);
        return (this as TReader)!;
    }

    /// <summary>
    /// This method gets called after the the <see cref="RequireHeadings"/> method is called
    /// and it may be used to build an initial map of properties for a structured reader.
    /// </summary>
    /// <param name="headings">Provides the headings that were read from the stream.</param>
    protected abstract void OnHeadingsRead(IReadOnlyList<string> headings);

    /// <summary>
    /// If no headings have been set, this method automatically reads them.
    /// The method throws if proper conditions are not met.
    /// </summary>
    /// <param name="moveNext">If after reading the headings the pointer to the current record should be advanced.</param>
    protected void RequireHeadings(bool moveNext)
    {
        if (_headings.Any())
            return;

        if (Values is null)
        {
            if (!MoveNext(TrimsHeadings) || Values is null || Values.Count <= 0)
                throw new InvalidOperationException("Unable to read headings from the underlying stream.");
        }
        
        SetHeadings(Values.ToArray());

        if (moveNext)
            MoveNext();
    }
}
