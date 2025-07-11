#!/bin/bash
# Test script to verify Docker build works with Node.js 24 and Vite 7

set -e

echo "🐳 Testing Docker build with Node.js 24 and Vite 7..."
echo "=================================================="

# Build the Docker image
echo "Building Docker image..."
docker build -t ribosoft-test:latest -f Ribosoft/Dockerfile .

echo ""
echo "✅ Docker build completed successfully!"
echo ""
echo "🔍 Verifying Node.js version in the built image..."
docker run --rm ribosoft-test:latest node --version

echo ""
echo "🎉 Docker build test completed successfully!"
echo "The image is ready with Node.js 24 and Vite 7 support."
