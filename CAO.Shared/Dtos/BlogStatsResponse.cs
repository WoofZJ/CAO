namespace CAO.Shared.Dtos;

public record BlogStatsResponse
(
    int BlogCount,
    int MonthlyCreatedCount,
    int MonthlyUpdatedCount,
    int TagCount,
    string MostUsedTag,
    int UsedCount,
    int MessageCount
);