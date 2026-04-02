# HTTPS Setup for `rexengswagger.duckdns.org` on Windows VPS

Goal:

- expose the API publicly at `https://rexengswagger.duckdns.org`
- keep the legacy IP endpoint available at `http://103.146.22.206:5000`
- let `Caddy` handle HTTPS and reverse proxy traffic to `Kidzgo.API`

## 1. Current code state

The codebase is already configured to use:

- `ClientSettings.ApiUrl = https://rexengswagger.duckdns.org`
- profile verification emails that point to the HTTPS backend
- anonymous `GET /api/profiles/{id}/reactivate-and-update` for clicks from email

Relevant files:

- [ProfileCreatedDomainEventHandler.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.Application/Profiles/CreateProfile/ProfileCreatedDomainEventHandler.cs)
- [ProfileController.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.API/Controllers/ProfileController.cs)
- [ClientSettings.cs](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/Kidzgo.Infrastructure/Shared/ClientSettings.cs)
- [Caddyfile](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/deploy/caddy/Caddyfile)
- [deploy-win.ps1](/c:/Users/ADMIN/RiderProjects/Kidzgo_Be/deploy-win.ps1)

## 2. VPS prerequisites

Inbound firewall should allow:

- TCP `80`
- TCP `443`
- TCP `5000` if the frontend still uses the legacy IP endpoint

## 3. Run API in dual-access mode

On the VPS, the API should bind to:

```powershell
$env:ASPNETCORE_URLS="http://0.0.0.0:5000"
```

This keeps both of these working at the same time:

- `http://103.146.22.206:5000/swagger/index.html`
- `https://rexengswagger.duckdns.org/swagger/index.html`

`Caddy` can still proxy to `127.0.0.1:5000` because the app is listening on all interfaces, including loopback.

## 4. Install Caddy

Run this on the VPS:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\install-caddy-win.ps1
```

The script will:

- download `caddy.exe`
- write the `Caddyfile`
- register the Windows service `caddy` with `sc.exe`
- start the service

If you previously ran an older version of the script and saw:

```text
unknown command "service" for "caddy"
```

pull the latest code and run `.\scripts\install-caddy-win.ps1` again. The updated script no longer depends on `caddy service ...`.

## 5. Verify

Check both URLs:

```text
http://103.146.22.206:5000/swagger/index.html
https://rexengswagger.duckdns.org/swagger/index.html
```

Quick checks:

```powershell
Get-Service caddy
Invoke-WebRequest https://rexengswagger.duckdns.org/swagger/index.html
Invoke-WebRequest http://103.146.22.206:5000/swagger/index.html
```

## 6. If HTTPS is not working yet

Check:

1. `rexengswagger.duckdns.org` points to `103.146.22.206`
2. ports `80/443` are open
3. no other process is using `80/443`
4. `Kidzgo.API` is reachable on port `5000`

Caddy log example:

```powershell
Get-WinEvent -LogName Application | Where-Object { $_.ProviderName -like "*caddy*" } | Select-Object -First 20
```

## 7. Recommended deployment flow

- publish the app to `C:\apps\kidzgo-api`
- keep `Kidzgo.API` running on port `5000`
- keep `Caddy` as the HTTPS entrypoint for the domain
- keep port `5000` open only if the old frontend still depends on the IP-based URL

Traffic flow:

`https://rexengswagger.duckdns.org` -> `Caddy` -> `http://127.0.0.1:5000`

Legacy flow:

`http://103.146.22.206:5000` -> `Kidzgo.API`
