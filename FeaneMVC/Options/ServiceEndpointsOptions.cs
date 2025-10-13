namespace FeaneMVC.Options;

public class ServiceEndpointsOptions
{
    public const string SectionName = "ServiceEndpoints";

    public string MenuService { get; set; } = string.Empty;

    public string ReservationService { get; set; } = string.Empty;
}
