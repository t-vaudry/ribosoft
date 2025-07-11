#!/bin/bash
# Setup script to ensure Node.js 24 is active via nvm
# Run this before building: source ./setup-node.sh

export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"  # This loads nvm
[ -s "$NVM_DIR/bash_completion" ] && \. "$NVM_DIR/bash_completion"  # This loads nvm bash_completion

# Use Node.js 24
nvm use 24

echo "Node.js version: $(node --version)"
echo "npm version: $(npm --version)"
echo ""
echo "Ready to build with Vite 7! Run:"
echo "  npm run build:all:dev    # Development build"
echo "  npm run build:all        # Production build"
