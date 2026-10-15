#!/usr/bin/env bash
# One-click start: checks prerequisites, then runs the Aspire AppHost.
# The AppHost starts PostgreSQL (Docker), the API, and the Angular web app, and opens the Aspire dashboard.
set -euo pipefail
cd "$(dirname "$0")"

require() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing prerequisite: $1 — $2" >&2
    exit 1
  fi
}

require dotnet "install .NET 10 SDK: https://dotnet.microsoft.com/download"
require node   "install Node.js 22.12+ : https://nodejs.org"
require npm    "comes with Node.js"
require docker "install Docker Desktop and make sure it is running"

if ! docker info >/dev/null 2>&1; then
  echo "Docker is installed but not running. Start Docker Desktop and try again." >&2
  exit 1
fi

exec dotnet run --project src/SlotBooking.AppHost/SlotBooking.AppHost.csproj
