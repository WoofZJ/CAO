namespace CAO.Shared.Dtos;

public record VisitRecordRequest
(
    string UserAgent,
    string Origin,
    string Path,
    string Query,
    string Referer,
    Guid VisitorId,
    Guid SessionId
);