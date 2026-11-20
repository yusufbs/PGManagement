using System.Diagnostics.Metrics;

namespace PGManagement.API.Observability;

public class PGMetrics
{
    public const string MeterName = "PGManagement.API";
    private readonly Meter _meter;

    public Counter<long> BillsGeneratedCounter { get; }
    public Counter<long> PaymentsReceivedCounter { get; }
    public Histogram<double> PaymentAmountHistogram { get; }

    public PGMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);

        BillsGeneratedCounter = _meter.CreateCounter<long>(
            "pg_bills_generated_total",
            description: "Total number of bills generated");

        PaymentsReceivedCounter = _meter.CreateCounter<long>(
            "pg_payments_received_total",
            description: "Total number of payments recorded");

        PaymentAmountHistogram = _meter.CreateHistogram<double>(
            "pg_payment_amount",
            unit: "INR",
            description: "Distribution of payment amounts received");
    }
}
