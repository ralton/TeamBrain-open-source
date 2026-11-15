# ==============================================================================
# TeamBrain VS Code Extension — Prerequisite Setup Script (Windows PowerShell)
# ==============================================================================

Write-Host "Checking prerequisites for TeamBrain VS Code Extension development & packaging..." -ForegroundColor Cyan

# 1. Check Node.js
try {
    $nodeVersion = node -v 2>$null
    if ($nodeVersion) {
        Write-Host "[OK] Node.js is installed: $nodeVersion" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Node.js executable ('node') was not found in PATH." -ForegroundColor Yellow
        Write-Host "         Please install Node.js (v20+) from https://nodejs.org/" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[WARNING] Unable to check Node.js version." -ForegroundColor Yellow
}

# 2. Check npm
try {
    $npmVersion = npm -v 2>$null
    if ($npmVersion) {
        Write-Host "[OK] npm is installed: v$npmVersion" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] npm was not found in PATH." -ForegroundColor Yellow
    }
} catch {
    Write-Host "[WARNING] Unable to check npm version." -ForegroundColor Yellow
}

# 3. Check VS Code CLI ('code')
try {
    $codeVersion = code -v 2>$null
    if ($codeVersion) {
        Write-Host "[OK] VS Code CLI ('code') is installed." -ForegroundColor Green
    } else {
        Write-Host "[INFO] VS Code CLI ('code') not found in PATH (Optional for building VSIX packages)." -ForegroundColor Gray
    }
} catch {
    Write-Host "[INFO] VS Code CLI check skipped." -ForegroundColor Gray
}

# 4. Check & install @vscode/vsce globally
Write-Host "`nChecking VS Code Extension packaging tool (@vscode/vsce)..." -ForegroundColor Cyan
try {
    $vsceVersion = vsce --version 2>$null
    if ($vsceVersion) {
        Write-Host "[OK] @vscode/vsce is installed globally: v$vsceVersion" -ForegroundColor Green
    } else {
        Write-Host "[ACTION] Installing @vscode/vsce globally via npm..." -ForegroundColor Yellow
        npm install -g @vscode/vsce
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] Successfully installed @vscode/vsce globally!" -ForegroundColor Green
        } else {
            Write-Host "[ERROR] Failed to install @vscode/vsce globally. You can also use npx @vscode/vsce package." -ForegroundColor Red
        }
    }
} catch {
    Write-Host "[INFO] Skipping global vsce installation check." -ForegroundColor Gray
}

# 5. Install Extension project dependencies
$extensionDir = Join-Path -Path $PSScriptRoot -ChildPath "..\extensions\vscode"
if (Test-Path $extensionDir) {
    Write-Host "`nInstalling dependencies in extensions/vscode..." -ForegroundColor Cyan
    Push-Location $extensionDir
    try {
        npm install
        Write-Host "[OK] Extension dependencies installed successfully!" -ForegroundColor Green
    } catch {
        Write-Host "[WARNING] Failed to run npm install in extensions/vscode automatically." -ForegroundColor Yellow
    } finally {
        Pop-Location
    }
}

Write-Host "`n==============================================================================" -ForegroundColor Green
Write-Host "Setup Check Complete!" -ForegroundColor Green
Write-Host "To package the extension into an independent .vsix file for developers, run:" -ForegroundColor White
Write-Host "  cd extensions/vscode" -ForegroundColor Yellow
Write-Host "  npx @vscode/vsce package" -ForegroundColor Yellow
Write-Host "==============================================================================" -ForegroundColor Green
