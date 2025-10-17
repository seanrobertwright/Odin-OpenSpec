# Debug launcher for Odin - OpenSpec
Write-Host "Starting Odin - OpenSpec..." -ForegroundColor Green

try {
    $exePath = "Odin - OpenSpec\bin\Debug\net8.0-windows10.0.19041.0\Odin - OpenSpec.exe"
    
    if (-not (Test-Path $exePath)) {
        Write-Host "Executable not found at: $exePath" -ForegroundColor Red
        Write-Host "Building project first..." -ForegroundColor Yellow
        dotnet build "Odin - OpenSpec/Odin - OpenSpec.csproj"
    }
    
    Write-Host "Launching application..." -ForegroundColor Cyan
    $process = Start-Process -FilePath $exePath -PassThru -Wait
    
    Write-Host "Application exited with code: $($process.ExitCode)" -ForegroundColor $(if ($process.ExitCode -eq 0) { "Green" } else { "Red" })
    
    if ($process.ExitCode -ne 0) {
        Write-Host "Application crashed or exited with an error." -ForegroundColor Red
        Write-Host "Check Windows Event Viewer for crash details:" -ForegroundColor Yellow
        Write-Host "  eventvwr.msc -> Windows Logs -> Application" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Error launching application: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host "`nPress any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
