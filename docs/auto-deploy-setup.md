# Hướng dẫn Setup Auto-Deploy cho Windows VPS

Có 3 cách để tự động deploy khi có code mới trên GitHub main branch. Chọn cách phù hợp nhất:

## Cách 1: Scheduled Task trên VPS (Đơn giản nhất - Khuyến nghị)

**Ưu điểm:** Không cần config GitHub Secrets, không cần SSH, hoạt động ngay.

**Nhược điểm:** Check định kỳ (mỗi 5-10 phút), không deploy ngay lập tức.

### Bước 1: Tạo Scheduled Task

1. Mở **Task Scheduler** trên VPS (Windows + R → gõ `taskschd.msc`)
2. Click **Create Task** (bên phải)
3. Tab **General**:
   - Name: `Kidzgo Auto-Deploy`
   - Description: `Tự động check và deploy khi có code mới`
   - ✅ Run whether user is logged on or not
   - ✅ Run with highest privileges
4. Tab **Triggers** → **New**:
   - Begin the task: `On a schedule`
   - Settings: `Repeat task every` → chọn `5 minutes` hoặc `10 minutes`
   - Duration: `Indefinitely`
   - ✅ Enabled
5. Tab **Actions** → **New**:
   - Action: `Start a program`
   - Program/script: `powershell.exe`
   - Add arguments: `-ExecutionPolicy Bypass -File "C:\Users\Administrator\Desktop\Projects\Kidzgo\auto-deploy-check.ps1"`
   - Start in: `C:\Users\Administrator\Desktop\Projects\Kidzgo`
6. Tab **Settings**:
   - ✅ Allow task to be run on demand
   - ✅ Run task as soon as possible after a scheduled start is missed
   - If the task fails, restart every: `1 minute` (tối đa 3 lần)
7. Click **OK** → nhập password Administrator nếu được hỏi

### Bước 2: Test thủ công

Chạy thử script để đảm bảo hoạt động:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\auto-deploy-check.ps1
```

Nếu không có code mới, sẽ báo "No new commits". Nếu có code mới, sẽ tự động deploy.

### Bước 3: Kiểm tra log

Log được ghi vào: `C:\apps\kidzgo-api\auto-deploy.log`

```powershell
Get-Content C:\apps\kidzgo-api\auto-deploy.log -Tail 20
```

---

## Cách 2: GitHub Actions với SSH (Deploy ngay lập tức)

**Ưu điểm:** Deploy ngay khi push code, có log trên GitHub Actions.

**Nhược điểm:** Cần enable SSH trên Windows VPS và config GitHub Secrets.

### Bước 1: Enable SSH trên Windows VPS

1. Mở **Settings** → **Apps** → **Optional Features**
2. Tìm **OpenSSH Server** → **Install** (nếu chưa có)
3. Mở PowerShell (Admin):

```powershell
# Start SSH service
Start-Service sshd
Set-Service -Name sshd -StartupType 'Automatic'

# Mở firewall
New-NetFirewallRule -Name sshd -DisplayName 'OpenSSH Server (sshd)' -Enabled True -Direction Inbound -Protocol TCP -Action Allow -LocalPort 22

# Kiểm tra
Get-Service sshd
```

### Bước 2: Config GitHub Secrets

Vào GitHub repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**:

- `VPS_HOST`: IP VPS (ví dụ: `103.146.22.206`)
- `VPS_USERNAME`: Username Windows (ví dụ: `Administrator`)
- `VPS_PASSWORD`: Password Windows
- `VPS_SSH_PORT`: `22` (hoặc port SSH bạn đã đổi)

### Bước 3: Uncomment workflow file

File `.github/workflows/deploy-windows.yml` đã được tạo sẵn. Chỉ cần uncomment và push lên GitHub.

---

## Cách 3: GitHub Webhook (Nâng cao)

Tạo một API endpoint trên VPS để nhận webhook từ GitHub và trigger deploy. Phức tạp hơn nhưng linh hoạt nhất.

---

## So sánh các cách

| Cách | Thời gian deploy | Độ phức tạp | Khuyến nghị |
|------|----------------|-------------|-------------|
| Scheduled Task | 5-10 phút sau khi push | ⭐ Dễ | ✅ **Khuyến nghị** |
| GitHub Actions | Ngay lập tức | ⭐⭐ Trung bình | Nếu cần deploy ngay |
| GitHub Webhook | Ngay lập tức | ⭐⭐⭐ Khó | Nếu cần custom logic |

---

## Troubleshooting

### Scheduled Task không chạy

1. Kiểm tra Task Scheduler → **Task Scheduler Library** → tìm task `Kidzgo Auto-Deploy`
2. Click phải → **Run** để test thủ công
3. Xem **History** tab để xem lỗi (nếu có)

### Script báo lỗi git

Đảm bảo VPS có quyền truy cập GitHub:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
git remote -v
git fetch origin main
```

Nếu dùng private repo, cần config SSH key hoặc Personal Access Token.

### Service không restart được

Kiểm tra service có đang chạy không:

```powershell
Get-Service KidzgoAPI
```

Nếu service không tồn tại, chạy lại bước tạo service trong hướng dẫn deploy ban đầu.

