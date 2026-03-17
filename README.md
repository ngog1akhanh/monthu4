# TourGuideApp

TourGuideApp là ứng dụng mobile hỗ trợ khách du lịch khám phá nhà hàng và địa điểm ăn uống bằng cách quét **QR Code**. Sau khi quét, ứng dụng sẽ hiển thị thông tin nhà hàng, menu, hình ảnh và phát audio giới thiệu bằng nhiều ngôn ngữ.

---

## Features

* 📷 Scan QR Code để xem thông tin nhà hàng
* 🌐 Hỗ trợ đa ngôn ngữ
* 🔊 Phát audio giới thiệu địa điểm
* 🍜 Hiển thị menu món ăn
* 🖼️ Hiển thị hình ảnh nhà hàng
* 🎟️ Nhận coupon khuyến mãi khi quét QR

---

## Technology Stack

* **.NET MAUI** – phát triển ứng dụng mobile đa nền tảng
* **SQLite** – database lưu trữ dữ liệu
* **C#** – ngôn ngữ lập trình chính
* **QR Code Scanner** – quét mã QR để truy cập thông tin

---

## Database Design

Database được thiết kế theo mô hình quan hệ để quản lý:

* Users
* Restaurants
* Restaurant Translations (đa ngôn ngữ)
* Menus
* QR Codes
* Coupons
* Coupon Scans

### ERD (Entity Relationship Diagram)

![Database ERD](database/TourGuideApp_ERD.pdf)

### Database Schema

Chi tiết cấu trúc database nằm trong file:

database/schema.sql

---

## Project Structure

```
TourGuideApp
│
├─ TourGuideApp/                # .NET MAUI project
│   └─ Resources
│       └─ Raw
│           └─ TourGuide.db     # SQLite database used by the app
│
├─ database/
│   ├─ schema.sql               # SQL schema definition
│   └─ ERD.png                  # Database ERD diagram
│
└─ README.md
```

---

## How It Works

1. Người dùng mở ứng dụng.
2. Quét **QR Code** tại nhà hàng hoặc địa điểm.
3. Ứng dụng truy xuất thông tin từ database.
4. Hiển thị:

   * Tên nhà hàng
   * Mô tả
   * Hình ảnh
   * Menu
5. Phát **audio giới thiệu địa điểm** theo ngôn ngữ đã chọn.

---

## Future Improvements

* Thêm bản đồ hiển thị vị trí nhà hàng
* Hệ thống đăng nhập bằng Google
* Backend API để quản lý dữ liệu từ web admin
* Thống kê lượt quét QR

---

## Author
Vu_Hoang_Phuong
Project developed as part of a mobile application development project.
