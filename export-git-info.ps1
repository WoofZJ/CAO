param(
    [string]$OutputFile = "CAO.Client/wwwroot/timeline.json",
    [string]$Branch = "main"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "Exporting Git commit records..." -ForegroundColor Green

if (-not (Test-Path ".git")) {
    Write-Host "Error: Current directory is not a Git repository!" -ForegroundColor Red
    exit 1
}

try {
    $gitLogFormat = "%H|%ad|%s|%b"
    $gitCommand = "git log --pretty=format:`"$gitLogFormat`" --date=iso $Branch"
    Write-Host "Executing command: $gitCommand" -ForegroundColor Yellow
    $commitLines = Invoke-Expression $gitCommand
    if (-not $commitLines) {
        Write-Host "Warning: No commit records found" -ForegroundColor Yellow
        $commits = @()
    } else {
        $commits = @()
        $commitIndex = 0
        foreach ($line in $commitLines) {
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }
            $commitIndex++
            if ($commitIndex % 10 -eq 0) {
                Write-Host "Processing commit $commitIndex..." -ForegroundColor Gray
            }
            $parts = $line -split '\|', 4
            if ($parts.Count -ge 3) {
                $commitHash = $parts[0]
                $dateString = $parts[1]
                $isoDate = try {
                    $parsedDate = [DateTime]::ParseExact($dateString, "yyyy-MM-dd HH:mm:ss zzz", $null)
                    $parsedDate.ToString("yyyy-MM-ddTHH:mm:sszzz")
                } catch {
                    $dateString
                }
                $statsOutput = git show --stat --format="" $commitHash
                $insertions = 0
                $deletions = 0
                $filesChanged = 0
                if ($statsOutput) {
                    $statsLines = $statsOutput | Where-Object { $_ -and $_ -notmatch "^\s*$" }
                    if ($statsLines) {
                        $filesChanged = ($statsLines | Where-Object { $_ -match "\|" }).Count
                        $summaryLine = $statsLines | Where-Object { $_ -match "(\d+)\s+insertions?\(\+\)|(\d+)\s+deletions?\(\-\)" } | Select-Object -Last 1
                        if ($summaryLine) {
                            if ($summaryLine -match "(\d+)\s+insertions?\(\+\)") {
                                $insertions = [int]$matches[1]
                            }
                            if ($summaryLine -match "(\d+)\s+deletions?\(\-\)") {
                                $deletions = [int]$matches[1]
                            }
                        }
                    }
                }
                $commit = @{
                    hash = $commitHash
                    date = $isoDate
                    message = $parts[2]
                    body = if ($parts.Count -gt 3) { $parts[3] } else { "" }
                    stats = @{
                        filesChanged = $filesChanged
                        insertions = $insertions
                        deletions = $deletions
                    }
                }
                $commits += $commit
            }
        }
    }
    Write-Host "Fetching Git tags..." -ForegroundColor Yellow
    $tagLines = git tag -l --sort=-version:refname
    $tags = @()
    if ($tagLines) {
        foreach ($tagName in $tagLines) {
            if ([string]::IsNullOrWhiteSpace($tagName)) {
                continue
            }
            try {
                $tagInfo = git for-each-ref --format="%(refname:short)|%(taggerdate:iso)|%(subject)|%(body)" "refs/tags/$tagName"
                if ($tagInfo -and $tagInfo -match '\|') {
                    $tagParts = $tagInfo -split '\|', 4
                    if ($tagParts.Count -ge 3) {
                        $tagDate = $tagParts[1]
                        $tagMessage = $tagParts[2]
                        $tagBody = if ($tagParts.Count -gt 3) { $tagParts[3] } else { "" }
                        $tagIsoDate = try {
                            if ($tagDate) {
                                $parsedTagDate = [DateTime]::ParseExact($tagDate, "yyyy-MM-dd HH:mm:ss zzz", $null)
                                $parsedTagDate.ToString("yyyy-MM-ddTHH:mm:sszzz")
                            } else { "" }
                        } catch {
                            $tagDate
                        }
                        $tagCommitHash = git rev-list -n 1 $tagName
                        $tag = @{
                            name = $tagName
                            date = $tagIsoDate
                            message = $tagMessage
                            body = $tagBody
                            commit = $tagCommitHash
                        }
                        $tags += $tag
                    }
                }
            } catch {
                Write-Host "Warning: Could not process tag $tagName" -ForegroundColor Yellow
            }
        }
    }
    Write-Host "Found $($tags.Count) tags" -ForegroundColor Cyan
    $output = @{
        exportInfo = @{
            exportDate = Get-Date -Format "yyyy-MM-ddTHH:mm:sszzz"
            totalCommits = $commits.Count
            totalTags = $tags.Count
        }
        commits = $commits
        tags = $tags
    }
    $jsonOutput = $output | ConvertTo-Json -Depth 10 -Compress:$false
    $jsonOutput | Out-File -FilePath $OutputFile -Encoding UTF8
    Write-Host "Successfully exported $($commits.Count) commit records and $($tags.Count) tags to $OutputFile" -ForegroundColor Green
    Write-Host "File size: $((Get-Item $OutputFile).Length) bytes" -ForegroundColor Cyan
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host "`nExport completed!" -ForegroundColor Green