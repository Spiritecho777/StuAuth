# StuAuth

**Local TOTP/HOTP authenticator**  
No cloud. No sync. No tracking. Everything stays on your device.1

Manual backups only. Full control.

### Features
- Offline OTP generation (TOTP & HOTP)
- Folders & account organization
- Google Authenticator import (QR scan, text file, protobuf migration)
- Local encrypted storage (AES-256 + PBKDF2)
- Optional LAN HTTP server (port 19755) to share accounts between your devices
- Languages: French (main), English, Breton & Japanese (Google Translate for fun)

### Security notes
- Key derived from a generated GUID (stored locally)
- No master password yet (improvement possible)
- LAN server only – not exposed to internet

### Downloads / Builds

**Mobile (MAUI – Android)**  
- APK prebuilt: [download here](https://github.com/Spiritecho777/StuAuthMobile/release)  
- Or build yourself: clone → open in VS/Code with MAUI → run

**Desktop (WPF – Windows)**  
- Prebuilt EXE: [download here](https://github.com/545d0c2f-c21e-4ce4-a22e-7ff704c6725e)
- Or build yourself: clone → Visual Studio → build)

**Web Extension (Javascript – Firefox)**  
- XPI Prebuilt: [download here](https://github.com/Spiritecho777/StuAuthWeb/release](https://github.com/Spiritecho777/StuAuthWeb/tree/main/release) 

### License
MIT – do whatever you want, no warranty.

Made for me, shared, features not planned.  
If you find it useful: star, fork.

Spiritecho – 2026
