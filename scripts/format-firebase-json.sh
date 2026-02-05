#!/bin/bash
# Script để format Firebase Service Account JSON cho appsettings.json
# Usage: ./format-firebase-json.sh path/to/firebase-service-account.json

if [ -z "$1" ]; then
    echo "Error: Please provide JSON file path"
    echo "Usage: ./format-firebase-json.sh path/to/firebase-service-account.json"
    exit 1
fi

JSON_FILE="$1"

if [ ! -f "$JSON_FILE" ]; then
    echo "Error: File not found: $JSON_FILE"
    exit 1
fi

echo "Reading JSON file: $JSON_FILE"

# Read and escape JSON
# Use jq to compact JSON, then escape quotes
if command -v jq &> /dev/null; then
    ESCAPED=$(cat "$JSON_FILE" | jq -c . | sed 's/"/\\"/g')
else
    # Fallback: simple escape without jq
    ESCAPED=$(cat "$JSON_FILE" | tr -d '\n' | sed 's/"/\\"/g' | sed 's/\\n/\\\\n/g')
fi

echo ""
echo "=== Formatted JSON String ==="
echo "$ESCAPED"
echo ""
echo "=== Copy the above string and paste into appsettings.json ==="
echo 'Under "FCM": { "ServiceAccountJson": "..." }'

# Try to copy to clipboard
if command -v xclip &> /dev/null; then
    echo "$ESCAPED" | xclip -selection clipboard
    echo "✅ JSON string has been copied to clipboard!"
elif command -v pbcopy &> /dev/null; then
    echo "$ESCAPED" | pbcopy
    echo "✅ JSON string has been copied to clipboard!"
else
    echo "⚠️  Could not copy to clipboard. Please copy manually."
fi

