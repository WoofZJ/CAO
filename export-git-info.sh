#!/bin/bash

OUTPUT_FILE="${1:-timeline.json}"
BRANCH="${2:-main}"

echo "Exporting Git commit records..."

if [ ! -d ".git" ]; then
    echo "Error: Current directory is not a Git repository!"
    exit 1
fi

# Use Node.js for robust JSON generation and Git parsing
# This avoids dependency on jq and handles complex string escaping correctly
node -e "
const fs = require('fs');
const { execSync } = require('child_process');

const outputFile = '$OUTPUT_FILE';
const branch = '$BRANCH';

try {
    // 1. Export Info
    const exportInfo = {
        exportDate: new Date().toISOString(),
        totalCommits: 0,
        totalTags: 0
    };

    // 2. Commits
    console.log('Fetching Git commits...');
    const separator = '|||SEP|||';
    const endMarker = '==END==';
    // Format: Hash|Date|Subject|Body
    const logCmd = \`git log --pretty=format:\"%H\${separator}%ad\${separator}%s\${separator}%b\${separator}\${endMarker}\" --date=iso-strict \${branch}\`;
    
    // Increase buffer size for large logs (50MB)
    const logOutput = execSync(logCmd, { maxBuffer: 1024 * 1024 * 50 }).toString();
    
    const commits = [];
    const rawCommits = logOutput.split(endMarker);
    
    let commitIndex = 0;
    for (const raw of rawCommits) {
        if (!raw.trim()) continue;
        
        const parts = raw.split(separator);
        if (parts.length < 4) continue;
        
        commitIndex++;
        if (commitIndex % 10 === 0) {
            process.stdout.write(\`Processing commit \${commitIndex}...\r\`);
        }

        const hash = parts[0].trim();
        const date = parts[1].trim();
        const message = parts[2].trim();
        const body = parts[3].trim();
        
        // Get stats
        let filesChanged = 0;
        let insertions = 0;
        let deletions = 0;
        
        try {
            const statsCmd = \`git show --stat --format=\"\" \${hash}\`;
            const statsOutput = execSync(statsCmd).toString();
            const lines = statsOutput.split('\\n').filter(l => l.trim());
            
            if (lines.length > 0) {
                const summaryLine = lines[lines.length - 1];
                const filesMatch = summaryLine.match(/(\d+)\s+files?\s+changed/);
                if (filesMatch) filesChanged = parseInt(filesMatch[1]);
                
                const insMatch = summaryLine.match(/(\d+)\s+insertions?\(\+\)/);
                if (insMatch) insertions = parseInt(insMatch[1]);
                
                const delMatch = summaryLine.match(/(\d+)\s+deletions?\(\-\)/);
                if (delMatch) deletions = parseInt(delMatch[1]);
            }
        } catch (e) {
            // ignore stats error
        }

        commits.push({
            hash,
            date,
            message,
            body,
            stats: {
                filesChanged,
                insertions,
                deletions
            }
        });
    }
    console.log(\`\nProcessed \${commits.length} commits.\`);

    // 3. Tags
    console.log('Fetching Git tags...');
    const tags = [];
    try {
        const tagsOutput = execSync('git tag -l --sort=-version:refname').toString();
        const tagNames = tagsOutput.split('\\n').filter(t => t.trim());
        
        for (const tagName of tagNames) {
            try {
                const fmt = '%(refname:short)|%(taggerdate:iso)|%(subject)|%(body)';
                const tagInfoCmd = \`git for-each-ref --format=\"\${fmt}\" \"refs/tags/\${tagName}\"\`;
                const tagInfo = execSync(tagInfoCmd).toString().trim();
                
                if (tagInfo) {
                    const parts = tagInfo.split('|');
                    if (parts.length >= 3) {
                        const name = parts[0];
                        const date = parts[1];
                        const message = parts[2];
                        const body = parts.slice(3).join('|');
                        
                        const commitCmd = \`git rev-list -n 1 \${tagName}\`;
                        const commit = execSync(commitCmd).toString().trim();
                        
                        tags.push({
                            name,
                            date,
                            message,
                            body,
                            commit
                        });
                    }
                }
            } catch (e) {
                console.error(\`Warning: Could not process tag \${tagName}\`);
            }
        }
    } catch (e) {
        console.error('Error fetching tags:', e.message);
    }
    console.log(\`Found \${tags.length} tags.\`);

    // 4. Final Output
    exportInfo.totalCommits = commits.length;
    exportInfo.totalTags = tags.length;

    const output = {
        exportInfo,
        commits,
        tags
    };

    fs.writeFileSync(outputFile, JSON.stringify(output, null, 2));
    console.log(\`Successfully exported to \${outputFile}\`);
    console.log(\`File size: \${fs.statSync(outputFile).size} bytes\`);

} catch (e) {
    console.error('Error:', e);
    process.exit(1);
}
"
