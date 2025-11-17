# Monitor DLL injection in real-time
Write-Host "Monitoring DisplayShaderHook.dll injection..." -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
Write-Host ""

while ($true) {
    $procs = Get-Process | Where-Object {
        try {
            $_.Modules.ModuleName -contains "DisplayShaderHook.dll"
        } catch {
            $false
        }
    }
    
    Clear-Host
    Write-Host "=== DisplayShaderHook.dll Monitor ===" -ForegroundColor Cyan
    Write-Host ""
    
    if ($procs) {
        Write-Host "DLL loaded in $($procs.Count) process(es):" -ForegroundColor Green
        Write-Host ""
        foreach ($proc in $procs) {
            Write-Host "  ? $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Green
        }
    } else {
        Write-Host "DLL not currently loaded in any process" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "Last updated: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray
    Write-Host "Press Ctrl+C to stop monitoring" -ForegroundColor Gray
    
    Start-Sleep -Seconds 2
}
