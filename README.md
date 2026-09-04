# FMT 飛貓科技 VPN 客戶端

![FMT VPN Client](Assets/fmt-client.png)

FMT 飛貓科技 VPN 客戶端是一套精簡的 Windows VPN 連線工具，可匯入 FMT `.conf` 設定、快速連線，並在系統匣背景執行。

## 操作說明

1. 下載並執行 `FMT-VPN-Client-v1.1.0-win-x64.exe`。
2. Windows 詢問權限時選擇「是」。VPN 連線需要系統管理員權限。
3. 點選「匯入 CONF」，選擇管理者提供的 `.conf` 設定檔。
4. 從「連線設定」選擇要使用的設定，按下「連線」。
5. 顯示綠色「已連線」代表 VPN 已建立；再次按下按鈕即可中斷連線。
6. 按最小化會進入 Windows 系統匣，VPN 連線繼續保持；按右上角 `X` 或系統匣的「中斷連線並結束」，會先偵測並移除所有 FMT VPN 隧道再退出。

## 設定檔管理

- 匯入的設定會保存於 `%LOCALAPPDATA%\FMTClient\Configurations`。
- 同名設定再次匯入時會更新原有設定。
- 按下「刪除」會同時移除程式內的項目與本機保存的 `.conf`。
- VPN 連線期間無法刪除或切換設定，請先中斷連線。

## 系統需求

- Windows 10 或 Windows 11（64 位元）
- FMT VPN 網路元件
- 連線時需要系統管理員權限

## v1.1.0 功能

- 匯入及保存 FMT CONF
- 一鍵連線／中斷
- 即時連線狀態
- 設定刪除與更新
- Windows 系統匣背景執行
- 內嵌 VPN 影像介面，固定使用 `http://10.0.1.20:8889/`，可輸入並保存不同的影像分頁路徑
- FMT 飛貓科技品牌圖示與介面
- VPN 影像分頁設定與內嵌影像視窗
- 關閉程式時自動偵測並完整移除 FMT VPN 隧道

## 從原始碼建置

需要 .NET 9 SDK：

```powershell
dotnet build .\FeimaoTunnel.csproj --configuration Release
```

Copyright © 2026 FMT 飛貓科技
