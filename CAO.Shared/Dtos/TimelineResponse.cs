namespace CAO.Shared.Dtos;

public record CommitDto(
    string Message,
    string Subject,
    int FilesChanged,
    int Insertions,
    int Deletions,
    DateTime Date
);

public record TimelineResponse(
    string Version,
    string Message,
    DateTime Date,
    List<CommitDto> Commits
);