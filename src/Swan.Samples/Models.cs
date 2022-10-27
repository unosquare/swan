namespace Swan.Samples;

public interface ITenantRecord
{
    /// <summary>
    /// Gets the unique id of a tenant.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets the column name of the record identifier.
    /// </summary>
    string RecordIdFieldName { get; }

    /// <summary>
    /// Gets the value of the unique record identifier within a tenant.
    /// </summary>
    object RecordIdValue { get; }
}

public interface ITenantRecord<T> : ITenantRecord
{
    /// <summary>
    /// Gets the typed value of the unique record id.
    /// </summary>
    new T RecordIdValue { get; }
}

public record Organisation : ITenantRecord<Guid>
{
    public Guid TenantId { get; set; }

    public Guid OrganisationId { get; set; }

    public string? ApiKey { get; set; }

    public string? Name { get; set; }

    public bool PaysTax { get; set; }

    public string? Version { get; set; }

    public string? OrganisationType { get; set; }

    public string? OrganisationEntityType { get; set; }

    public string? BaseCurrency { get; set; }

    public string? CountryCode { get; set; }

    public bool IsDemoCompany { get; set; }

    public string? OrganisationStatus { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? EmployerIdentificationNumber { get; set; }

    public string? TaxNumber { get; set; }

    public int FinancialYearEndDay { get; set; }

    public int FinancialYearEndMonth { get; set; }

    public string? SalesTaxBasis { get; set; }

    public string? SalesTaxPeriod { get; set; }

    public string? DefaultSalesTax { get; set; }

    public string? DefaultPurchasesTax { get; set; }

    public DateTime? PeriodLockDate { get; set; }

    public DateTime? EndOfYearLockDate { get; set; }

    public DateTime CreatedDateUtc { get; set; }

    public string? Timezone { get; set; }

    public string? ShortCode { get; set; }

    public string? Edition { get; set; }

    string ITenantRecord.RecordIdFieldName => nameof(OrganisationId);

    object ITenantRecord.RecordIdValue => OrganisationId;

    Guid ITenantRecord<Guid>.RecordIdValue => OrganisationId;
}
