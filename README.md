# CampSync

> A full-stack camp management system for managing workforce, accommodation, attendance, transportation, leave workflows, and operational reporting through a centralized web platform.

## Overview

CampSync is a full-stack management application designed to centralize and simplify day-to-day camp operations. The system provides separate backend API and frontend MVC applications with role-based access control and a relational SQL Server database.

The application covers core operational areas including camp management, worker and supervisor management, attendance tracking, accommodation management, leave workflows, bus management, route-stop management, and reporting.

## Key Features

### Authentication & Authorization
- User registration and login
- JWT-based authentication
- Role-based access control
- Protected application modules and endpoints
- Access-denied handling for unauthorized users
- User profile management

### Camp Management
- Create, view, update, and delete camps
- View camp-specific information
- Manage camp-related resources
- Assign supervisors to camps

### Worker Management
- Create and manage worker records
- View worker details and lists
- Update and delete worker information
- Worker-specific attendance and leave management

### Supervisor Management
- Create and manage supervisors
- Assign supervisors to camps
- Manage supervisor-related operations
- Role-based supervisor access

### Attendance Management
- Mark worker attendance
- Camp-based attendance management
- Individual attendance records
- Attendance history
- Attendance summaries
- Attendance percentage calculations
- Driver attendance tracking

### Leave Management
- Submit leave applications
- View personal leave requests
- Manage pending leave requests
- Approve or reject leave applications
- Track leave decisions
- Leave summaries and trends
- Worker-specific leave records

### Accommodation Management
- Room management
- Bed management
- Room and bed availability tracking
- Bed occupancy information
- Camp-specific room organization
- Unique room numbering per camp

### Bus & Route Management
- Create and manage buses
- View bus information
- Bus utilization tracking
- Driver attendance management
- Create and manage route stops
- Bulk route-stop creation
- Reorder route stops
- Update and delete route stops

### Reporting & Dashboard
- Centralized operational dashboard
- Attendance summaries
- Leave statistics
- Leave trends
- Bus utilization information
- Bed occupancy information
- Overall camp operational summaries

## Architecture

CampSync follows a separated frontend/backend architecture:

```text
                    CampSync
                       |
          +------------+------------+
          |                         |
      Frontend                   Backend
   ASP.NET Core MVC          ASP.NET Core Web API
          |                         |
    Razor Views              Controllers / DTOs
          |                         |
          +-----------+-------------+
                      |
              Entity Framework Core
                      |
                  SQL Server
