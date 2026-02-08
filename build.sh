#!/bin/bash
set -e

echo "=== Restoring NuGet packages ==="
dotnet restore Net4Courier.sln

echo "=== Cleaning static assets ==="
cd src/Net4Courier.Web
rm -rf wwwroot/_content
rm -f wwwroot/Net4Courier.Web.styles.css

echo "=== Building project ==="
dotnet build -c Debug

echo "=== Copying MudBlazor static assets ==="
mkdir -p wwwroot/_content/MudBlazor
cp ~/.nuget/packages/mudblazor/7.16.0/staticwebassets/MudBlazor.min.css wwwroot/_content/MudBlazor/
cp ~/.nuget/packages/mudblazor/7.16.0/staticwebassets/MudBlazor.min.js wwwroot/_content/MudBlazor/

echo "=== Copying scoped CSS (if available) ==="
cp obj/Debug/net8.0/scopedcss/bundle/Net4Courier.Web.styles.css wwwroot/ 2>/dev/null || echo "Scoped CSS not found, skipping"

echo "=== Build complete ==="
