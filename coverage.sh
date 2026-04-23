#!/usr/bin/env bash
# coverage.sh – runs PromptTests with code coverage and generates an HTML report.
#
# Usage:
#   ./coverage.sh [options]
#
# Options:
#   -c, --configuration  Build configuration: Debug (default) or Release.
#   -o, --output         Directory for the HTML report.
#                        Defaults to tests/PromptTests/coverage-report.
#   -r, --open           Open the HTML report in the default browser when done.
#   -h, --help           Show this help message.
#
# Examples:
#   ./coverage.sh
#   ./coverage.sh --configuration Release --open
#   ./coverage.sh -o /tmp/my-report

set -euo pipefail

# ── Defaults ──────────────────────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIGURATION="Debug"
REPORT_DIR="${SCRIPT_DIR}/tests/PromptTests/coverage-report"
OPEN_REPORT=false

# ── Argument parsing ──────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
    case "$1" in
        -c|--configuration)
            CONFIGURATION="$2"; shift 2 ;;
        -o|--output)
            REPORT_DIR="$2"; shift 2 ;;
        -r|--open)
            OPEN_REPORT=true; shift ;;
        -h|--help)
            sed -n '2,20p' "$0" | sed 's/^# \?//'
            exit 0 ;;
        *)
            echo "Unknown option: $1" >&2; exit 1 ;;
    esac
done

TEST_PROJECT="${SCRIPT_DIR}/tests/PromptTests/PromptTests.csproj"
COBERTURA_DIR="${SCRIPT_DIR}/tests/PromptTests/coverage"

# ── Colour helpers ────────────────────────────────────────────────────────────
cyan()   { printf '\033[0;36m%s\033[0m\n' "$*"; }
green()  { printf '\033[0;32m%s\033[0m\n' "$*"; }
yellow() { printf '\033[0;33m%s\033[0m\n' "$*"; }

# ── 1. Clean previous artefacts ───────────────────────────────────────────────
rm -rf "${COBERTURA_DIR}" "${REPORT_DIR}"
mkdir -p "${COBERTURA_DIR}" "${REPORT_DIR}"

# ── 2. Run tests with coverlet data collector ─────────────────────────────────
echo ""
cyan "==> Running tests with coverage collection..."

dotnet test "${TEST_PROJECT}" \
    --configuration "${CONFIGURATION}" \
    --collect:"XPlat Code Coverage" \
    --results-directory "${COBERTURA_DIR}" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# ── 3. Locate the generated coverage.cobertura.xml ────────────────────────────
COBERTURA_FILE=$(find "${COBERTURA_DIR}" -name 'coverage.cobertura.xml' | head -n 1)

if [[ -z "${COBERTURA_FILE}" ]]; then
    echo "ERROR: Could not find coverage.cobertura.xml. Check that coverlet.collector is installed." >&2
    exit 1
fi

green "==> Coverage data: ${COBERTURA_FILE}"

# ── 4. Ensure ReportGenerator is available ────────────────────────────────────
if ! command -v reportgenerator &> /dev/null; then
    yellow "==> Installing ReportGenerator global tool..."
    dotnet tool install --global dotnet-reportgenerator-globaltool

    # Add the .NET tools directory to PATH for this session
    DOTNET_TOOLS="${HOME}/.dotnet/tools"
    export PATH="${PATH}:${DOTNET_TOOLS}"
fi

# ── 5. Generate HTML report ───────────────────────────────────────────────────
echo ""
cyan "==> Generating HTML report..."

reportgenerator \
    "-reports:${COBERTURA_FILE}" \
    "-targetdir:${REPORT_DIR}" \
    "-reporttypes:Html;TextSummary" \
    "-assemblyfilters:+interactiveCLI"

# ── 6. Print text summary ─────────────────────────────────────────────────────
SUMMARY="${REPORT_DIR}/Summary.txt"
if [[ -f "${SUMMARY}" ]]; then
    echo ""
    cyan "==> Coverage summary:"
    cat "${SUMMARY}"
fi

echo ""
green "==> HTML report written to: ${REPORT_DIR}"

# ── 7. Optionally open the report ─────────────────────────────────────────────
if [[ "${OPEN_REPORT}" == true ]]; then
    INDEX="${REPORT_DIR}/index.html"
    if [[ -f "${INDEX}" ]]; then
        if command -v xdg-open &> /dev/null; then
            xdg-open "${INDEX}"
        elif command -v open &> /dev/null; then
            open "${INDEX}"          # macOS fallback
        else
            yellow "==> Could not open browser automatically. Report is at: ${INDEX}"
        fi
    fi
fi
