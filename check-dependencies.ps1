# Dependency Checker for Odin - OpenSpec
Write-Host "Checking WinUI 3 Dependencies..." -ForegroundColor Cyan

# Check .NET Runtime
Write-Host "`n1. Checking .NET 8 Runtime..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✓ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ .NET 8 SDK not found!" -ForegroundColor Red
}

# Check Windows App SDK
Write-Host "`n2. Checking Windows App SDK Runtime..." -ForegroundColor Yellow
$appRuntime = Get-AppxPackage -Name "Microsoft.WindowsAppRuntime.1.*" | Select-Object -First 1
if ($appRuntime) {
    Write-Host "   ✓ Windows App SDK Runtime found: $($appRuntime.Version)" -ForegroundColor Green
} else {
    Write-Host "   ✗ Windows App SDK 1.8 Runtime NOT installed!" -ForegroundColor Red
    Write-Host "   → Download from: https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads" -ForegroundColor Yellow
}

# Check Visual C++ Redistributables
Write-Host "`n3. Checking Visual C++ Redistributables..." -ForegroundColor Yellow
$vcRedist = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\VisualStudio\*\VC\Runtimes\*" -ErrorAction SilentlyContinue
if ($vcRedist) {
    Write-Host "   ✓ VC++ Redistributables found" -ForegroundColor Green
    $vcRedist | ForEach-Object { 
        Write-Host "     - $($_.PSChildName): Version $($_.Version)" -ForegroundColor Gray
    }
} else {
    Write-Host "   ⚠ VC++ Redistributables might be missing" -ForegroundColor Yellow
    Write-Host "   → Download from: https://aka.ms/vs/17/release/vc_redist.x64.exe" -ForegroundColor Yellow
}

# Check DLL dependencies using dumpbin (if available)
Write-Host "`n4. Attempting to list DLL dependencies..." -ForegroundColor Yellow
$exePath = "Odin - OpenSpec\bin\Debug\net8.0-windows10.0.19041.0\Odin - OpenSpec.exe"
if (Test-Path $exePath) {
    Write-Host "   Executable found at: $exePath" -ForegroundColor Gray
    
    # Try to run with detailed error
    Write-Host "   Attempting to launch..." -ForegroundColor Gray
    try {
        $process = Start-Process -FilePath $exePath -PassThru -Wait -WindowStyle Hidden
        if ($process.ExitCode -eq -1073741515) {
            Write-Host "   ✗ Exit code 0xc000027b - DLL not found" -ForegroundColor Red
            Write-Host "   This usually means:" -ForegroundColor Yellow
            Write-Host "   - Windows App SDK Runtime is not installed" -ForegroundColor Yellow
            Write-Host "   - Or VC++ Redistributables are missing" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ✗ Failed to launch: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   ✗ Executable not found!" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RECOMMENDATION:" -ForegroundColor Green -BackgroundColor Black
Write-Host "For development, use Visual Studio 2022" -ForegroundColor White
Write-Host "Press F5 to run - it handles all dependencies automatically" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan
