# Нативные библиотеки туннеля

Положи сюда **две DLL** (x64) - они копируются рядом с `MatchaLab.exe` при сборке:

```
native/
  tunnel.dll      ← встраиваемый туннель AmneziaWG (форк wireguard-windows)
  wintun.dll      ← виртуальный адаптер Wintun (подписанный драйвер WireGuard)
```

## Где взять

### wintun.dll
1. Скачать https://www.wintun.net (архив `wintun-*.zip`).
2. Взять `bin/amd64/wintun.dll`.

### tunnel.dll (именно AmneziaWG, не ванильный WireGuard!)
Обычный `tunnel.dll` от WireGuard **не понимает** параметры обфускации (Jc/Jmin/Jmax, S1/S2, H1-H4)
и не поднимет наш конфиг. Нужен форк **amneziawg-windows**. Варианты:

- **Собрать** из https://github.com/amnezia-vpn/amneziawg-windows (нужны Go + mingw, `build.bat` → `amd64/tunnel.dll`).
- **Извлечь из установленного AmneziaVPN** (Windows): в папке программы
  (`C:\Program Files\AmneziaVPN\...`) есть `tunnel.dll` и `wintun.dll` - скопировать оба.

> ВАЖНО: у форка amneziawg-windows сигнатура `WireGuardTunnelService(confContent, tunnelName)` -
> два аргумента (ТЕКСТ конфига + имя туннеля), не путь к файлу, как у ванильного wireguard-windows.
> `NativeTunnel.cs` и сервисный режим в `Program.cs` уже под это заточены.

## Проверка
После `dotnet build` в `bin\Debug\net8.0\` должны появиться `tunnel.dll` и `wintun.dll`.
Приложение запускать **от администратора** (манифест уже требует этого) - иначе службу-туннель
и адаптер создать нельзя.
