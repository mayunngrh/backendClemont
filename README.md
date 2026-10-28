# Clemont Loan Demo — Backend API

Backend REST API untuk aplikasi "Clemont Loan Demo" yang dibangun menggunakan ASP.NET Core Web API, Entity Framework Core dengan SQLite, dan ASP.NET Core Identity.

---

## 1. Versi SDK & Framework
- **.NET SDK**: `9.0.319` (`net9.0`)
- **ASP.NET Core Runtime**: `9.0.21`
- **EF Core & Identity Packages**: `9.0.2`
  - `Microsoft.EntityFrameworkCore.Sqlite` (9.0.2)
  - `Microsoft.EntityFrameworkCore.Design` (9.0.2)
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (9.0.2)

---

## 2. Struktur Proyek
```
backend/
├── Controllers/
│   ├── AuthController.cs            # CSRF, Login, Logout, Me
│   └── LoanApplicationsController.cs# GET list, GET detail, POST pengajuan
├── Data/
│   ├── ApplicationDbContext.cs      # EF Core DbContext + Identity
│   └── DbInitializer.cs             # Idempotent seeding (User demo & 5 pengajuan pinjaman)
├── DTOs/
│   ├── AuthDtos.cs                  # UserDto, LoginRequestDto, LoginResponseDto
│   └── LoanApplicationDtos.cs       # Request & Response DTOs + Business Rules Validations
├── Migrations/                      # EF Core Code-First Migrations
│   ├── 20261025145920_InitialCreate.cs
│   └── ApplicationDbContextModelSnapshot.cs
├── Models/
│   ├── ApplicationUser.cs           # Custom IdentityUser (FullName)
│   └── LoanApplication.cs          # Entity model pengajuan pinjaman
├── Services/
│   └── ApplicationNumberGenerator.cs# Generator nomor unik (LN-yyyyMMdd-xxx)
├── appsettings.json                 # Konfigurasi koneksi SQLite (clemont_loan.db)
├── ClemontLoan.Api.csproj
├── ClemontLoan.Api.http             # HTTP Client test requests & dokumentasi alur antiforgery
└── README.md
```

---

## 3. Akun Demo Khusus Development
Seed data dijalankan secara otomatis dan idempoten pada saat API pertama kali berjalan dalam environment `Development`:
- **Email**: `staff@clemont.test`
- **Password**: `Belajar123!`
- **Nama Petugas**: `Budi Santoso`
- **User ID**: `usr-clemont-001`

Data pengajuan pinjaman awal (5 pengajuan sintetis dari akun demo) juga otomatis di-seed ke dalam database `clemont_loan.db` sehingga data tetap tersimpan saat API dimatikan.

---

## 4. Port dan URL API
- **HTTP**: `http://localhost:5088`
- **HTTPS**: `https://localhost:7118` (jika SSL certificate lokal dikonfigurasi)
- **Swagger UI (Klasik)**: `http://localhost:5088/swagger`
- **Scalar UI (Modern)**: `http://localhost:5088/scalar/v1`
- **OpenAPI Schema (JSON)**: `http://localhost:5088/openapi/v1.json`

---

## 5. Prasyarat & Perintah Setup

Pastikan path .NET SDK tersedia di terminal. Jika .NET terinstal di folder user (`~/.dotnet`), tambahkan ke environment variable:
```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

### Install EF Core CLI Tool (jika belum ada)
```bash
dotnet tool install --global dotnet-ef
```

### Build Solusi
```bash
dotnet build
```

### Menjalankan Database Migration
Database SQLite (`clemont_loan.db`) dibuat dan dimigrasi secara otomatis saat aplikasi dijalankan, atau dapat dijalankan secara manual:
```bash
dotnet ef database update
```

### Menjalankan Backend API
```bash
dotnet run --urls "http://localhost:5088"
```

---

## 6. Alur Autentikasi Cookie & Antiforgery
1. **GET `/api/auth/csrf`**:
   - Menghasilkan cookie HttpOnly `.AspNetCore.Antiforgery` (token server) dan cookie `XSRF-TOKEN` (bisa dibaca oleh Angular HttpClient).
2. **Pengiriman Permintaan Mutasi (POST)**:
   - Header request wajib menyertakan: `X-XSRF-TOKEN: <isi-cookie-XSRF-TOKEN>`.
   - Browser menyertakan cookie `.AspNetCore.Antiforgery` dan `.ClemontLoan.Auth` secara otomatis (`credentials: 'include'`).
   - Endpoint login, logout, dan create loan application terlindungi dengan atribut `[ValidateAntiForgeryToken]`.
3. **Respon Akses Tidak Sah**:
   - Jika belum login atau sesi berakhir, server mengembalikan status `401 Unauthorized` dengan format `ProblemDetails` (bukan redirect HTML login).
   - Jika resource tidak ditemukan atau bukan milik pengguna login, server mengembalikan `404 Not Found`.

---

## 7. Hasil Pengujian Endpoint yang Dilakukan
Pengujian komprehensif telah dijalankan secara otomatis dengan hasil sebagai berikut:
1. **GET `/api/loan-applications` (Tanpa Sesi)**: Mengembalikan `401 Unauthorized` dengan format JSON `ProblemDetails`.
2. **GET `/api/auth/csrf`**: Berhasil mengembalikan `204 No Content` dan menetapkan cookie `XSRF-TOKEN` serta `.AspNetCore.Antiforgery`.
3. **POST `/api/auth/login` (Password Salah)**: Mengembalikan status `401 Unauthorized` dengan pesan umum `"Email atau password salah."`.
4. **POST `/api/auth/login` (Kredensial Demo Benar)**: Mengembalikan `200 OK` dengan payload user demo dan menetapkan cookie sesi `.ClemontLoan.Auth`.
5. **GET `/api/auth/me`**: Mengembalikan profil akun demo yang sedang login (`usr-clemont-001`, `Budi Santoso`).
6. **GET `/api/loan-applications`**: Mengembalikan 5 item pengajuan pinjaman awal terpaginasi (totalCount = 5) dengan urutan terbaru (`createdAt` desc).
7. **GET `/api/loan-applications?search=fauzi`**: Berhasil memfilter pengajuan berdasarkan kata kunci (ditemukan 1 item: Ahmad Fauzi).
8. **GET `/api/loan-applications/la-seed-001`**: Mengembalikan detail data pengajuan Siti Rahmawati.
9. **GET `/api/loan-applications/non-existent`**: Mengembalikan `404 Not Found` ProblemDetails.
10. **POST `/api/loan-applications` (Validasi Gagal)**: Berhasil menangkap seluruh pelanggaran batas (NIK != 16 digit, nomor HP < 10 digit, nominal di luar batas 1jt-100jt, tenor di luar 6/12/24/36 bulan, panjang deskripsi tujuan) dengan mengembalikan `400 Bad Request` RFC 7807 ProblemDetails.
11. **POST `/api/loan-applications` (Tanpa Token Antiforgery)**: Mengembalikan `400 Bad Request` menolak request tanpa header `X-XSRF-TOKEN`.
12. **POST `/api/loan-applications` (Data Valid)**: Berhasil menyimpan data dengan format nomor otomatis `LN-20261025-xxx`, status `submitted`, dan mengembalikan `201 Created`.
13. **POST `/api/auth/logout`**: Mengembalikan `204 No Content` dan menghapus sesi autentikasi.
14. **GET `/api/auth/me` setelah Logout**: Mengembalikan `401 Unauthorized` mengonfirmasi sesi telah dihapus sepenuhnya.

## 8. Integrasi frontend Angular

Frontend sudah menggunakan endpoint backend melalui Angular HttpClient. Jalankan profil `http` pada port 5088 dan `npm start` dari folder frontend pada port 4200. Proxy `frontend/src/proxy.conf.json` meneruskan `/api/**` ke backend; browser memakai cookie pada origin frontend sehingga development tidak memerlukan CORS lintas origin.

Login mengambil CSRF sebelum POST dan memperbaruinya sesudah sign-in. Guard memulihkan pengguna lewat `/api/auth/me` setelah refresh. Detail dan pencarian mengikuti endpoint backend; pencarian mendukung nama serta nomor pengajuan. Panduan lengkap tersedia di `../frontend/README.md`.

### Kompatibilitas Swagger UI

Paket `Swashbuckle.AspNetCore.SwaggerUI` menggunakan versi `10.3.1` agar dapat menampilkan dokumen OpenAPI `3.0.4` yang dihasilkan backend. Versi `6.9.0` sebelumnya menolak versi dokumen tersebut. Jika browser masih menampilkan "Unable to render this definition" setelah update, restart backend lalu lakukan hard refresh (`Cmd+Shift+R` di macOS).
