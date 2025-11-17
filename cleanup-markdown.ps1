# Markdown File Cleanup and Organization Script
# Moves temporary/development markdown files to archive, keeps essential docs

Write-Host "=== Markdown File Cleanup ===" -ForegroundColor Cyan
Write-Host ""

# Create archive directory structure
$archiveDir = "docs\archive"
$devNotesDir = "$archiveDir\development-notes"
$fixesDir = "$archiveDir\fixes-applied"
$uiChangesDir = "$archiveDir\ui-changes"

Write-Host "Creating archive directory structure..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $devNotesDir -Force | Out-Null
New-Item -ItemType Directory -Path $fixesDir -Force | Out-Null
New-Item -ItemType Directory -Path $uiChangesDir -Force | Out-Null
Write-Host "? Archive directories created" -ForegroundColor Green
Write-Host ""

# Files to KEEP in root (essential documentation)
$keepInRoot = @(
    "README.md",
    "LICENSE",
    "CHANGELOG.md",
    "QUICKSTART.md"
)

# Files to KEEP in docs (current documentation)
$keepInDocs = @(
    "docs\BUILD_INSTRUCTIONS.md",
    "docs\IMPLEMENTATION_STATUS.md",
    "docs\TECHNICAL_LIMITATIONS.md",
    "docs\ICON.md",
    "docs\WINDOW_CLOSING_FIX.md"
)

# Development/completion notes (archive)
$devNotes = @(
    "COMPLETION_SUMMARY.md",
    "MISSION_COMPLETE.md",
    "MISSION_ACCOMPLISHED.md",
    "PROJECT_SUMMARY.md",
    "INDEX.md",
    "START_HERE.md",
    "GETTING_STARTED.md"
)

# Fix/issue resolution docs (archive)
$fixDocs = @(
    "CRASH_FIX_COMPLETE.md",
    "CRASH_RECOVERY_VALIDATION.md",
    "DIAGNOSTIC_LOGGING_ADDED.md",
    "EMOJI_FIX_COMPLETE.md",
    "INFINITE_RETRY_FIXED.md",
    "INITIALIZATION_FIX_COMPLETE.md",
    "LIVE_CONFIG_AND_PREVIEW_COMPLETE.md",
    "NO_ADMIN_REQUIRED_FIX.md",
    "PERFORMANCE_OPTIMIZATION_COMPLETE.md",
    "PREVIEW_AND_CONFIG_UPDATE_ISSUES.md",
    "QUESTION_MARK_ICON_FIXED.md",
    "SHADER_MODE_FIX.md",
    "SHADER_MODE_VERIFICATION.md",
    "SYSTEM_TRAY_RESTORED.md",
    "UNHOOKING_COMPLETE.md",
    "UNHOOKING_CRITICAL_ISSUE.md"
)

# UI development docs (archive)
$uiDocs = @(
    "UI_CHANGES_SUMMARY.md",
    "UI_IMPLEMENTATION_CHECKLIST.md",
    "UI_IMPLEMENTATION_COMPLETE.md",
    "UI_IMPROVEMENT_PROPOSAL.md",
    "UI_REDESIGN_COMPLETE.md",
    "UI_REDESIGN_SUMMARY.md",
    "UI_REFACTOR_COMPLETE.md",
    "TEST_UI_NOW.md"
)

# Implementation/feature completion docs (archive)
$implDocs = @(
    "BUILD_SUCCESS_SUMMARY.md",
    "CLEANUP_COMPLETE.md",
    "CLEARTYPE_AUTOMATIC_FALLBACK.md",
    "COMMUNITY_RESPONSE.md",
    "CONFIGURATION.md",
    "DEPLOY_NOW.md",
    "DEPLOYMENT_GUIDE.md",
    "INJECTION_IMPLEMENTATION_COMPLETE.md",
    "INSTANT_APPLY_COMPLETE.md",
    "MODERN_UI_DEPLOYED.md",
    "PRODUCTION_CHECKLIST.md",
    "PRODUCTION_READY.md",
    "QUICK_REFERENCE.md",
    "QUICK_START_UNIVERSAL.md",
    "README_UPDATE_COMPLETE.md",
    "TEST_INITIALIZATION_FIX.md",
    "UNIVERSAL_INJECTION_COMPLETE.md",
    "UNIVERSAL_INJECTION_SUMMARY.md"
)

# Obsolete docs from old implementation (archive)
$obsoleteDocs = @(
    "docs\OPTION_B_SUMMARY.md",
    "docs\ROADMAP.md",
    "docs\PRODUCTION_DEPLOYMENT.md"
)

# Move files function
function Move-ToArchive {
    param(
        [string[]]$files,
        [string]$destination,
        [string]$category
    )
    
    Write-Host "Moving $category..." -ForegroundColor Yellow
    $moved = 0
    foreach ($file in $files) {
        if (Test-Path $file) {
            $fileName = Split-Path $file -Leaf
            $dest = Join-Path $destination $fileName
            Move-Item -Path $file -Destination $dest -Force
            Write-Host "  ? $fileName" -ForegroundColor Gray
            $moved++
        }
    }
    Write-Host "  Moved $moved files" -ForegroundColor Green
    Write-Host ""
}

# Execute moves
Move-ToArchive -files $devNotes -destination $devNotesDir -category "Development notes"
Move-ToArchive -files $fixDocs -destination $fixesDir -category "Fix documentation"
Move-ToArchive -files $uiDocs -destination $uiChangesDir -category "UI change documentation"
Move-ToArchive -files $implDocs -destination $devNotesDir -category "Implementation notes"
Move-ToArchive -files $obsoleteDocs -destination "$archiveDir\obsolete" -category "Obsolete documentation"

# Create archive index
Write-Host "Creating archive index..." -ForegroundColor Yellow
$indexContent = @"
# Documentation Archive

This directory contains historical documentation from the development process.

## Directory Structure

- **development-notes/** - Project completion summaries, implementation notes
- **fixes-applied/** - Documentation of bugs fixed and issues resolved
- **ui-changes/** - UI redesign and refactoring documentation
- **obsolete/** - Outdated documentation from old implementations

## Current Documentation

For current, up-to-date documentation, see:

- `/README.md` - Project overview and quick start
- `/QUICKSTART.md` - Detailed setup guide
- `/docs/BUILD_INSTRUCTIONS.md` - How to build from source
- `/docs/IMPLEMENTATION_STATUS.md` - Current feature status
- `/docs/TECHNICAL_LIMITATIONS.md` - Known limitations

## Development History

These archived documents provide a historical record of:
- Features implemented
- Bugs fixed
- UI iterations
- Performance optimizations
- Architecture decisions

They are kept for reference but may not reflect the current state of the application.

---

**Last Updated:** $(Get-Date -Format "yyyy-MM-dd")
"@

Set-Content -Path "$archiveDir\README.md" -Value $indexContent
Write-Host "? Archive index created" -ForegroundColor Green
Write-Host ""

# Update main README to reflect cleanup
Write-Host "Summary of cleanup:" -ForegroundColor Cyan
Write-Host "  ? Kept in root: $($keepInRoot.Count) essential files" -ForegroundColor Green
Write-Host "  ? Kept in docs: $($keepInDocs.Count) current docs" -ForegroundColor Green
Write-Host "  ? Archived: Development notes, fix logs, UI changes" -ForegroundColor Green
Write-Host ""

# Show what's left in root
Write-Host "Files remaining in root directory:" -ForegroundColor Cyan
Get-ChildItem -Path . -Filter "*.md" -File | ForEach-Object {
    Write-Host "  • $($_.Name)" -ForegroundColor Gray
}
Write-Host ""

Write-Host "=== Cleanup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Archive location: $archiveDir" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the archived files in docs\archive\" -ForegroundColor Gray
Write-Host "  2. Update README.md links if needed" -ForegroundColor Gray
Write-Host "  3. Commit the cleanup with: git add . && git commit -m 'docs: cleanup and organize markdown files'" -ForegroundColor Gray
