# Fix DLL initialization failure

Write-Host "Fixing DLL initialization failure..." -ForegroundColor Cyan
Write-Host ""

# Apply dllmain fix
Write-Host "[1/2] Updating Native/DisplayShaderHook/dllmain.cpp..." -ForegroundColor Yellow
Copy-Item "Native\DisplayShaderHook\dllmain_fixed.cpp" "Native\DisplayShaderHook\dllmain.cpp" -Force
Write-Host "  ? Fixed DllMain to defer initialization" -ForegroundColor Green

# Apply ShaderService fix
Write-Host ""
Write-Host "[2/2] Updating Services/ShaderService.cs..." -ForegroundColor Yellow
Copy-Item "Services\ShaderService_fixed.cs" "Services\ShaderService.cs" -Force
Write-Host "  ? Updated to call InitializeHook()" -ForegroundColor Green

Write-Host ""
Write-Host "? Fixes applied!" -ForegroundColor Green
Write-Host ""
Write-Host "Now rebuild:" -ForegroundColor Cyan
Write-Host "  1. Rebuild C++ project (Release)" -ForegroundColor White
Write-Host "  2. Rebuild C# project (Debug)" -ForegroundColor White
Write-Host "  3. Press F5 to test" -ForegroundColor White
Write-Host ""

Write-Host "Quick rebuild command:" -ForegroundColor Yellow
Write-Host '  msbuild Native\DisplayShaderHook\DisplayShaderHook.vcxproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild; dotnet build --configuration Debug' -ForegroundColor White
