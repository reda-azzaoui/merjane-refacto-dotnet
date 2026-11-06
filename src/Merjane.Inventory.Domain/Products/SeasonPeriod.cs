namespace Merjane.Inventory.Domain.Products;

/// <summary>Period of the year during which a seasonal product can be sold.</summary>
public sealed record SeasonPeriod
{
    public SeasonPeriod(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("A season cannot end before it starts.", nameof(endDate));
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public DateTime StartDate { get; }

    public DateTime EndDate { get; }

    /// <summary>Sales are open strictly between the two bounds: neither the start date nor the end date is included.</summary>
    public bool IsOpenOn(DateTime day) => day > StartDate && day < EndDate;

    public bool HasNotStartedAt(DateTime moment) => StartDate > moment;

    public bool IsOverAt(DateTime moment) => moment > EndDate;
}
