# 🏕️ CampSync

A full-stack **Camp Management and Workforce Administration System** built with **ASP.NET Core Web API, ASP.NET Core MVC, Entity Framework Core, and SQL Server**.

CampSync provides a centralized platform for managing **camps, workers, supervisors, attendance, rooms, beds, buses, routes, leave requests, and operational reports** through a secure role-based architecture.

---

## 📸 Application Screenshots

## 🔐 Authentication & Access Control

<p align="center">
  <img src="Screenshorts/Registration.JPG" width="420"/>
  <img src="Screenshorts/Login.JPG" width="420"/>
</p>

### 🔒 Controlled Registration

One of the key security features of CampSync is its **controlled registration model**.

- Only an **Admin** can register through the public registration process.
- The system allows **only one Admin account** to be registered.
- Other roles such as **Supervisor, Worker, and Driver cannot self-register**.
- Operational users are created and managed by authorized administrators.
- Role-based authorization determines which modules and operations are accessible to each user.

This prevents unauthorized users from creating privileged accounts and keeps user administration under centralized control.

---

# 👑 Admin Module

The Admin has centralized control over the CampSync system and manages the major operational modules.

## 🏕️ Camp Management

<p align="center">
  <img src="Screenshorts/camps dashboard_a.JPG" width="420"/>
   <img src="Screenshorts/edit camp_A.JPG" width="420"/>
    <img src="Screenshorts/create camps_a.JPG" width="420"/>
     <img src="Screenshorts/Delete camp_a.JPG" width="420"/>
    
</p>

Admin can:

- Create camps
- View camp details
- Update camp information
- Manage camp-related data
- View individual camp records

---

## 👥 User & Worker Management

<p align="center">
  <img src="Screenshorts/Supervisor_User_A.JPG" width="420"/>
  <img src="Screenshorts/Worker Dashboard_a.JPG" width="420"/>
  <img src="Screenshorts/Create Worker_a.JPG" width="420"/>
  <img src="Screenshorts/Delet worker_a.JPG" width="420"/>
</p>

Admin manages the system workforce and controls the creation of operational users.

---

## 👨‍💼 Supervisor Management

<p align="center">
  <img src="Screenshots/Admin/Supervisor/SupervisorList.JPG" width="420"/>
  <img src="Screenshots/Admin/Supervisor/CreateSupervisor.JPG" width="420"/>
  <img src="Screenshots/Admin/Supervisor/SupervisorDetails.JPG" width="420"/>
</p>

Admin can:

- Create supervisors
- View supervisor information
- Manage supervisor records
- Assign supervisors to camps

---

## 🛏️ Room Management

<p align="center">
  <img src="Screenshots/Admin/Room/RoomList.JPG" width="420"/>
  <img src="Screenshots/Admin/Room/CreateRoom.JPG" width="420"/>
  <img src="Screenshots/Admin/Room/EditRoom.JPG" width="420"/>
  <img src="Screenshots/Admin/Room/RoomById.JPG" width="420"/>
</p>

Room management provides structured accommodation management for camps.

---

## 🛏️ Bed Management

<p align="center">
  <img src="Screenshots/Admin/Bed/BedList.JPG" width="420"/>
  <img src="Screenshots/Admin/Bed/CreateBed.JPG" width="420"/>
  <img src="Screenshots/Admin/Bed/EditBed.JPG" width="420"/>
  <img src="Screenshots/Admin/Bed/AvailableBeds.JPG" width="420"/>
</p>

Admin can manage:

- Beds
- Bed availability
- Room occupancy
- Camp accommodation resources

---

## 🚌 Bus Management

<p align="center">
  <img src="Screenshots/Admin/Bus/BusList.JPG" width="420"/>
  <img src="Screenshots/Admin/Bus/CreateBus.JPG" width="420"/>
  <img src="Screenshots/Admin/Bus/EditBus.JPG" width="420"/>
  <img src="Screenshots/Admin/Bus/BusById.JPG" width="420"/>
</p>

Transportation management includes:

- Bus records
- Bus capacity
- Driver information
- Bus assignments
- Bus utilization

---

## 📍 Route & Stop Management

<p align="center">
  <img src="Screenshots/Admin/RouteStop/RouteStopList.JPG" width="420"/>
  <img src="Screenshots/Admin/RouteStop/CreateRouteStop.JPG" width="420"/>
  <img src="Screenshots/Admin/RouteStop/EditRouteStop.JPG" width="420"/>
  <img src="Screenshots/Admin/RouteStop/ReorderRouteStop.JPG" width="420"/>
</p>

Admin can configure transportation routes and maintain the order of route stops.

---

## 📅 Attendance Management

<p align="center">
  <img src="Screenshots/Admin/Attendance/AttendanceList.JPG" width="420"/>
  <img src="Screenshots/Admin/Attendance/MarkAttendance.JPG" width="420"/>
  <img src="Screenshots/Admin/Attendance/MarkByCamp.JPG" width="420"/>
  <img src="Screenshots/Admin/Attendance/AttendanceSummary.JPG" width="420"/>
</p>

Attendance functionality provides:

- Attendance marking
- Camp-based attendance
- Attendance summaries
- Attendance percentage calculations
- Attendance history

---

## 🏖️ Leave Management

<p align="center">
  <img src="Screenshots/Admin/Leave/PendingLeaves.JPG" width="420"/>
  <img src="Screenshots/Admin/Leave/LeaveSummary.JPG" width="420"/>
  <img src="Screenshots/Admin/Leave/MyLeaves.JPG" width="420"/>
</p>

Admin can monitor and manage leave-related information and approval workflows.

---

## 📊 Reports & Dashboard

<p align="center">
  <img src="Screenshots/Admin/Reports/Dashboard.JPG" width="600"/>
</p>

The reporting dashboard provides an overview of important operational information across the system.

---

# 👨‍💼 Supervisor Module

Supervisors are responsible for managing day-to-day operations within their assigned camp.

## 🏕️ Camp Operations

<p align="center">
  <img src="Screenshots/Supervisor/Camp/CampList.JPG" width="420"/>
  <img src="Screenshots/Supervisor/Camp/CampDetails.JPG" width="420"/>
</p>

Supervisors can access information related to their assigned camp.

---

## 👷 Worker Management

<p align="center">
  <img src="Screenshots/Supervisor/Worker/WorkerList.JPG" width="420"/>
  <img src="Screenshots/Supervisor/Worker/WorkerDetails.JPG" width="420"/>
</p>

Supervisors can manage and monitor workers associated with their operational scope.

---

## 📋 Attendance Management

<p align="center">
  <img src="Screenshots/Supervisor/Attendance/AttendanceList.JPG" width="420"/>
  <img src="Screenshots/Supervisor/Attendance/MarkByCamp.JPG" width="420"/>
  <img src="Screenshots/Supervisor/Attendance/AttendanceSummary.JPG" width="420"/>
</p>

Supervisors can:

- Mark attendance
- View attendance records
- Manage camp-based attendance
- View attendance summaries

---

## 🏖️ Leave Management

<p align="center">
  <img src="Screenshots/Supervisor/Leave/PendingLeaves.JPG" width="420"/>
  <img src="Screenshots/Supervisor/Leave/LeaveSummary.JPG" width="420"/>
</p>

Supervisors can review and process leave requests according to their authorization level.

---

# 🚌 Driver Module

The Driver module provides functionality related to transportation operations.

## 🚌 Assigned Bus

<p align="center">
  <img src="Screenshots/Driver/Bus/MyBus.JPG" width="420"/>
  <img src="Screenshots/Driver/Bus/BusDetails.JPG" width="420"/>
</p>

Drivers can access information about their assigned transportation resources.

---

## 📍 Routes & Stops

<p align="center">
  <img src="Screenshots/Driver/Route/RouteStops.JPG" width="420"/>
  <img src="Screenshots/Driver/Route/RouteDetails.JPG" width="420"/>
</p>

Drivers can view the routes and stops relevant to their transportation operations.

---

## 📋 Driver Attendance

<p align="center">
  <img src="Screenshots/Driver/Attendance/Attendance.JPG" width="420"/>
  <img src="Screenshots/Driver/Attendance/MyAttendance.JPG" width="420"/>
</p>

Driver-specific attendance records are handled separately from general workforce attendance.

---

# 👷 Worker Module

Workers have access to their personal information and workforce-related services.

## 👤 Profile

<p align="center">
  <img src="Screenshots/Worker/Profile/MyProfile.JPG" width="420"/>
</p>

Workers can view their profile and personal information.

---

## 📅 My Attendance

<p align="center">
  <img src="Screenshots/Worker/Attendance/MyAttendance.JPG" width="420"/>
  <img src="Screenshots/Worker/Attendance/AttendanceSummary.JPG" width="420"/>
</p>

Workers can view their attendance records and attendance summaries.

---

## 🏖️ Leave Management

<p align="center">
  <img src="Screenshots/Worker/Leave/ApplyLeave.JPG" width="420"/>
  <img src="Screenshots/Worker/Leave/MyLeaves.JPG" width="420"/>
</p>

Workers can:

- Apply for leave
- View submitted leave requests
- Track leave status
- Review their leave history

---

# ✨ Key Features

- 🔐 JWT-based authentication
- 🔒 BCrypt password hashing
- 👑 Controlled Admin registration
- 🚫 Restricted self-registration for operational roles
- 🛡️ Role-based authorization
- 👥 User and workforce management
- 🏕️ Camp management
- 👨‍💼 Supervisor management
- 👷 Worker management
- 🚌 Bus management
- 📍 Route and route-stop management
- 🛏️ Room management
- 🛏️ Bed and occupancy management
- 📅 Attendance management
- 📊 Attendance summaries and percentage calculations
- 🏖️ Leave application and approval workflow
- 📈 Dashboard and operational reporting
- 🗄️ Entity Framework Core Code-First architecture
- 🔄 RESTful Web API architecture
- 🔐 Sensitive configuration excluded from source control

---

# 🛠️ Tech Stack

## Backend

- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt.Net
- RESTful API

## Frontend

- ASP.NET Core MVC
- .NET 8
- Razor Views
- Bootstrap
- HTML5
- CSS3
- JavaScript
- jQuery

## Development Tools

- Visual Studio 2022
- Git
- GitHub
- Entity Framework Core Migrations
- SQL Server

---

# 🗄️ Database Schema

CampSync uses **SQL Server** with **Entity Framework Core Code-First** architecture.

The main entities include:

| Entity | Description |
|---|---|
| User | Authentication, identity and role information |
| Camp | Camp information and management |
| Worker | Workforce records |
| Supervisor | Supervisor information and camp assignment |
| Attendance | Worker attendance records |
| DriverAttendance | Driver-specific attendance records |
| Leave | Leave requests and approval information |
| Room | Camp room information |
| Bed | Bed and occupancy information |
| Bus | Transportation and bus information |
| RouteStop | Transportation route stops and ordering |

Entity relationships and database changes are maintained through **Entity Framework Core migrations**.

---

# 🔄 CampSync Lifecycle

```text
Admin Registration
        │
        ▼
   Admin Login
        │
        ▼
   User Management
        │
        ├───────────────┐
        ▼               ▼
   Camp Creation   Supervisor Assignment
        │               │
        └───────┬───────┘
                ▼
        Worker Management
                │
        ┌───────┼────────┐
        ▼       ▼        ▼
      Rooms    Beds     Buses
        │       │        │
        └───────┼────────┘
                ▼
        Daily Operations
                │
        ┌───────┼──────────┐
        ▼       ▼          ▼
   Attendance  Leave   Transportation
        │       │          │
        └───────┼──────────┘
                ▼
        Reports & Dashboard
