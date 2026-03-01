#!/bin/bash
cleanup() {
    if [ -n "$HEALTH_PID" ] && kill -0 "$HEALTH_PID" 2>/dev/null; then
        kill "$HEALTH_PID" 2>/dev/null
        wait "$HEALTH_PID" 2>/dev/null
    fi
}
trap cleanup EXIT

python3 -c "
import http.server, json, sys, os
class H(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps({'status':'Healthy','phase':'starting','timestamp':''}).encode())
    def do_HEAD(self):
        self.send_response(200)
        self.end_headers()
    def log_message(self, format, *args):
        print(f'[EARLY-HEALTH] {args[0]}', flush=True)
s = http.server.HTTPServer(('0.0.0.0', 5000), H)
print('[EARLY-HEALTH] Ready on :5000', flush=True)
s.serve_forever()
" &
HEALTH_PID=$!

sleep 1
echo "[STARTUP] Early health responder running (PID=$HEALTH_PID), building app..."

cd /home/runner/workspace/src/Net4Courier.Web

echo "[STARTUP] Starting .NET application..."
kill "$HEALTH_PID" 2>/dev/null
wait "$HEALTH_PID" 2>/dev/null
sleep 0.5

exec dotnet bin/Debug/net8.0/Net4Courier.Web.dll --urls http://0.0.0.0:5000
