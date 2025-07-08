namespace CAO.Shared.Dtos;

public record VisitGetResponse
(
    int PageVisits,
    int SiteVisits,
    int Visitors,
    int MonthlyPageVisits,
    int MonthlySiteVisits,
    int MonthlyVisitors
);