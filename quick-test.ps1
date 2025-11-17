# Quick test to see what the app actually detects

Write-Host "Testing shader mode detection..." -ForegroundColor Cyan
Write-Host ""

# Build with verbose output
dotnet build --configuration Debug --verbosity detailed 2>&1 | Select-String "DisplayShaderHook|CopyNativeDll|Native DLL"

Write-Host ""
Write-Host "Files in Debug output:" -ForegroundColor Yellow
Get-ChildItem "bin\Debug\net8.0-windows\*.dll" | ForEach-Object {
    Write-Host "  $($_.Name) - $($_.Length) bytes" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Now run the app and check the status display at the top"
Write-Host "It should show either:" -ForegroundColor Yellow
Write-Host "  'Shader Mode: Active (Hook v1)' - if DLL detected ?" -ForegroundColor Green
Write-Host "  'Shader Mode: Not Available'     - if DLL not detected ?" -ForegroundColor Red
