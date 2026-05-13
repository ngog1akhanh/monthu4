param(
    [int]$ApiPort = 5276,
    [switch]$ListOnly
)

$ErrorActionPreference = "Stop"

function Find-Adb {
    $command = Get-Command adb -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = @(
        $env:ANDROID_HOME,
        $env:ANDROID_SDK_ROOT,
        (Join-Path $env:LOCALAPPDATA "Android\Sdk"),
        "C:\Program Files (x86)\Android\android-sdk"
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($root in $candidates) {
        $adb = Join-Path $root "platform-tools\adb.exe"
        if (Test-Path $adb) {
            return $adb
        }
    }

    throw "adb.exe was not found. Install Android SDK Platform Tools or add adb to PATH."
}

$adbPath = Find-Adb
Write-Host "Using adb: $adbPath"

$deviceLines = & $adbPath devices | Select-Object -Skip 1 | Where-Object { $_.Trim() -ne "" }
$devices = @()
foreach ($line in $deviceLines) {
    $parts = $line -split "\s+"
    if ($parts.Count -ge 2 -and $parts[1] -eq "device") {
        $devices += $parts[0]
    }
    elseif ($parts.Count -ge 2) {
        Write-Host "Skipping $($parts[0]): $($parts[1])"
    }
}

if ($devices.Count -eq 0) {
    Write-Host "No authorized Android device/emulator found."
    Write-Host "Check USB debugging, accept the RSA prompt on the phone, then run this script again."
    exit 0
}

Write-Host "Authorized devices:"
foreach ($device in $devices) {
    Write-Host " - $device"
}

if ($ListOnly) {
    exit 0
}

foreach ($device in $devices) {
    Write-Host "Mapping device $device localhost:$ApiPort -> PC localhost:$ApiPort"
    & $adbPath -s $device reverse "tcp:$ApiPort" "tcp:$ApiPort" | Out-Null
}

Write-Host "Android local setup complete."
Write-Host "Run API on http://localhost:$ApiPort, then the mobile app can use http://127.0.0.1:$ApiPort/."
