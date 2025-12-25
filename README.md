# Chinese Chess (Xiangqi) - WPF Application

## Giới thiệu
Đây là đồ án cuối kỳ cho môn học **Lập Trình Trực Quan (IT008)** tại **Trường Đại học Công nghệ Thông tin - ĐHQG TP.HCM (UIT)**.

Dự án là một ứng dụng Cờ Tướng (Chinese Chess) được xây dựng trên nền tảng **.NET (C#)** sử dụng **WPF (Windows Presentation Foundation)**. Ứng dụng tập trung vào việc áp dụng kiến trúc MVVM để quản lý trạng thái game , tích hợp Engine AI và xử lý đồng bộ dữ liệu thời gian thực cho tính năng chơi trực tuyến.

## Tính năng chính

### 1. Single Player (Đấu với Máy)
- **Tích hợp Engine Pikafish:** Sử dụng [Pikafish](https://github.com/official-pikafish/Pikafish) - một trong những engine Cờ Tướng mã nguồn mở mạnh nhất hiện nay (dựa trên kiến trúc NNUE của Stockfish).
- **Giao thức UCI:** Giao tiếp giữa GUI và Engine thông qua UCI (Universal Chess Interface) protocol, đảm bảo hiệu suất tính toán nước đi tối ưu.
- **Độ khó tùy chỉnh:** Hỗ trợ các chế độ từ dễ đến khó dựa trên độ sâu tìm kiếm (depth) và thời gian suy nghĩ của Engine.

### 2. Online Multiplayer (Đấu Online)
- **Hạ tầng Backend:** Sử dụng **Firebase Realtime Database** làm máy chủ trung gian.
- **Cơ chế hoạt động:**
    - **Matchmaking:** Hệ thống tạo phòng và ghép cặp người chơi tự động hoặc theo ID phòng.
    - **Data Sync:** Đồng bộ hóa nước đi (FEN string), trạng thái bàn cờ và tín hiệu điều khiển (cầu hòa, xin thua) theo thời gian thực .
    - **Chat System:** Tích hợp khung chat trực tuyến giữa hai người chơi trong thời gian thực.

### 3. Giao diện & Trải nghiệm (UI/UX)
- Thiết kế theo phong cách Modern UI với Dark Theme chủ đạo.
- Animation mượt mà khi di chuyển quân cờ và tương tác bàn cờ.
- Hỗ trợ Settings: Tùy chỉnh âm thanh, theme bàn cờ và quân cờ.

## Kiến trúc & Công nghệ

* **Ngôn ngữ:** C# (.NET 6.0)
* **Framework:** WPF (Windows Presentation Foundation)
* **Mô hình thiết kế:** MVVM (Model-View-ViewModel) - Tách biệt logic xử lý nghiệp vụ khỏi giao diện người dùng, giúp code dễ bảo trì và mở rộng.
* **Database:** Google Firebase Realtime Database.
* **AI Engine:** Pikafish (NNUE).
* **Thư viện hỗ trợ:**
    - *Newtonsoft.Json* (Xử lý dữ liệu JSON trao đổi với Firebase).
    - *System.Windows.Interactivity* (Hỗ trợ MVVM Command binding).

## Yêu cầu cài đặt

* **OS:** Windows 10/11 (64-bit).
* **IDE:** Visual Studio 2022.
* **Runtime:** .NET 6.0 SDK trở lên.

## Hướng dẫn chạy chương trình

1.  **Clone repository:**
    ```bash
    git clone [https://github.com/AnhGam/Co_Tuong.git](https://github.com/AnhGam/Co_Tuong.git)
    ```
2.  **Mở project:** Khởi động Visual Studio 2022 và mở file `Chinese Chess.sln`.
3.  **Cấu hình Engine:**
    - Đảm bảo file thực thi của Pikafish (`pikafish.exe`) và file mạng nơ-ron (`pikafish.nnue`) đã được đặt đúng trong thư mục `Assets/Engine/` hoặc thư mục `bin/Debug` sau khi build.
4.  **Build & Run:** Nhấn `F5` để biên dịch và chạy ứng dụng.

## Tác giả

**Đồ án môn học IT008 - UIT**
* **Nguyễn Minh Anh** - [AnhGam](https://github.com/AnhGam)
* **Dương Nguyễn Phú Quý** -[QusyPlus](https://github.com/QusyPlus)

---
*Lưu ý: Dự án sử dụng Pikafish engine tuân theo giấy phép GPLv3. Vui lòng tham khảo file LICENSE trong thư mục Engine để biết thêm chi tiết.*
