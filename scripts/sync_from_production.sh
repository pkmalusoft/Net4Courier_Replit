#!/bin/bash  bash scripts/sync_from_production.sh
set -e

echo "============================================="
echo "  Net4Courier - Production Data Sync"
echo "  Syncs Gateex production DB → Development DB"
echo "============================================="
echo ""

if [ -z "$GATEEX_DB_HOST" ] || [ -z "$GATEEX_DB_USER" ] || [ -z "$GATEEX_DB_PASSWORD" ] || [ -z "$GATEEX_DB_NAME" ]; then
    echo "ERROR: Gateex production database secrets are not set."
    echo "Required: GATEEX_DB_HOST, GATEEX_DB_PORT, GATEEX_DB_USER, GATEEX_DB_PASSWORD, GATEEX_DB_NAME"
    exit 1
fi

if [ -z "$PGHOST" ] || [ -z "$PGUSER" ] || [ -z "$PGPASSWORD" ] || [ -z "$PGDATABASE" ]; then
    echo "ERROR: Development database environment variables are not set."
    echo "Required: PGHOST, PGPORT, PGUSER, PGPASSWORD, PGDATABASE"
    exit 1
fi

GATEEX_PORT="${GATEEX_DB_PORT:-5432}"
DEV_PORT="${PGPORT:-5432}"
DUMP_FILE="/tmp/gateex_production_dump.sql"

echo "Source:  $GATEEX_DB_HOST / $GATEEX_DB_NAME"
echo "Target:  $PGHOST / $PGDATABASE"
echo ""

read -p "This will OVERWRITE all data in the development database. Continue? (y/N): " confirm
if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
    echo "Cancelled."
    exit 0
fi

echo ""
echo "[1/3] Dumping production database..."
echo "      This may take a few minutes depending on data size..."
PGPASSWORD="$GATEEX_DB_PASSWORD" pg_dump \
    -h "$GATEEX_DB_HOST" \
    -p "$GATEEX_PORT" \
    -U "$GATEEX_DB_USER" \
    -d "$GATEEX_DB_NAME" \
    --no-owner \
    --no-privileges \
    --clean \
    --if-exists \
    -F p \
    > "$DUMP_FILE"

DUMP_SIZE=$(du -h "$DUMP_FILE" | cut -f1)
echo "      Done! Dump size: $DUMP_SIZE"

echo ""
echo "[2/3] Restoring to development database..."
echo "      Dropping and recreating tables..."
PGPASSWORD="$PGPASSWORD" psql \
    -h "$PGHOST" \
    -p "$DEV_PORT" \
    -U "$PGUSER" \
    -d "$PGDATABASE" \
    -f "$DUMP_FILE" \
    --quiet \
    2>&1 | grep -i "error" || true

echo "      Done!"

echo ""
echo "[3/3] Cleaning up..."
rm -f "$DUMP_FILE"
echo "      Temporary dump file removed."

echo ""
echo "============================================="
echo "  Sync Complete!"
echo "  Development database now has latest"
echo "  production data from Gateex."
echo ""
echo "  NOTE: Restart the application to pick up"
echo "  any schema changes."
echo "============================================="
