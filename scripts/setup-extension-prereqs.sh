#!/usr/bin/env bash
# ==============================================================================
# TeamBrain VS Code Extension — Prerequisite Setup Script (Linux/macOS)
# ==============================================================================

echo "Checking prerequisites for TeamBrain VS Code Extension development & packaging..."

# 1. Check Node.js
if command -v node >/dev/null 2>&1; then
    echo "[OK] Node.js is installed: $(node -v)"
else
    echo "[WARNING] Node.js executable ('node') was not found in PATH."
    echo "         Please install Node.js (v20+) from https://nodejs.org/"
fi

# 2. Check npm
if command -v npm >/dev/null 2>&1; then
    echo "[OK] npm is installed: v$(npm -v)"
else
    echo "[WARNING] npm was not found in PATH."
fi

# 3. Check @vscode/vsce
echo ""
echo "Checking VS Code Extension packaging tool (@vscode/vsce)..."
if command -v vsce >/dev/null 2>&1; then
    echo "[OK] @vscode/vsce is installed globally: v$(vsce --version)"
else
    echo "[ACTION] Installing @vscode/vsce globally via npm..."
    npm install -g @vscode/vsce || echo "[ERROR] Failed to install @vscode/vsce globally."
fi

# 4. Install Extension dependencies
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
EXT_DIR="${SCRIPT_DIR}/../extensions/vscode"

if [ -d "$EXT_DIR" ]; then
    echo ""
    echo "Installing dependencies in extensions/vscode..."
    cd "$EXT_DIR" && npm install && echo "[OK] Extension dependencies installed successfully!"
fi

echo ""
echo "=============================================================================="
echo "Setup Check Complete!"
echo "To package the extension into an independent .vsix file for developers, run:"
echo "  cd extensions/vscode"
echo "  npx @vscode/vsce package"
echo "=============================================================================="
