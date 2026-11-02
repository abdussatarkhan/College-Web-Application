<p align="center">
  <img src="College%20Web%20Application/wwwroot/images/gdc-madyan-logo.png" alt="Government Degree College Madyan Swat Crest" width="130" />
</p>

<h1 align="center">Government Degree College Madyan Swat</h1>
<h3 align="center">Official Institutional Website & Campus Management Portal (CMS + ERP)</h3>

<p align="center">
  <strong>Motto:</strong> <em>رَبِّ زِدْنِي عِلْماً ("O My Lord! Increase me in knowledge")</em><br>
  <strong>Established:</strong> 2006 &bull; <strong>Affiliation:</strong> BISE Swat & University of Swat<br>
  <strong>Department:</strong> Higher Education Department (HED) Khyber Pakhtunkhwa, Pakistan
</p>

<p align="center">
  <a href="#key-features"><img src="https://img.shields.io/badge/Platform-ASP.NET%20Core%2010.0%20MVC-512BD4?logo=dotnet" alt=".NET 10 MVC" /></a>
  <a href="#database-architecture"><img src="https://img.shields.io/badge/ORM-EF%20Core%20Code--First-68217A?logo=csharp" alt="EF Core" /></a>
  <a href="#frontend-experience"><img src="https://img.shields.io/badge/UI-Bootstrap%205.3%20%7C%20Chart.js-7952B3?logo=bootstrap" alt="Bootstrap 5" /></a>
  <a href="#cloud-deployment"><img src="https://img.shields.io/badge/Deploy-Docker%20%7C%20Google%20Cloud%20Run-4285F4?logo=googlecloud" alt="Google Cloud" /></a>
  <a href="#institutional-leadership"><img src="https://img.shields.io/badge/Principal-Mr.%20Rasheed%20Ahmad-0B1B3D?logo=academia" alt="Principal Mr. Rasheed Ahmad" /></a>
  <a href="#palette"><img src="https://img.shields.io/badge/Palette-Royal%20Navy%20%26%20Gold-D4AF37" alt="Royal Navy and Gold" /></a>
</p>

---

## 🏛️ Executive Summary

**Government Degree College Madyan Swat** is a premier public-sector higher education institution serving 5,000+ students and 100+ teaching and administrative faculty members across Upper Swat. 

This enterprise web application delivers a modern institutional public website benchmarked against prestigious university standards (**University of Swat & GIKI**), seamlessly unified with a mission-critical **Campus Management ERP** that enforces Khyber Pakhtunkhwa Higher Education Department (HED) disciplinary mandates and University of Swat / HEC academic regulations.

---

## 👥 Institutional Leadership & Development

- **Principal & Director:** **Mr. Rasheed Ahmad** *(Head of Institution, Government Degree College Madyan Swat)*
- **Lead Full-Stack Solutions Architect:** **Abdussatar Khan** ([@abdussatarkhan](https://github.com/abdussatarkhan))
- **Affiliated Bodies:** 
  - **Intermediate (HSSC):** Board of Intermediate and Secondary Education (BISE) Swat
  - **Undergraduate (BS 4-Year):** University of Swat & Higher Education Commission (HEC) Pakistan

---

## 🚀 Key Features & Modules

### 1. 🎓 Flagship Public Institutional Website (University of Swat Benchmark)
- **Royal Navy & Gold Design System:** Prestigious academic palette built on University of Swat tokens (`#0B1B3D` Royal Navy, `#D4AF37` Gold, crisp white surfaces, full Dark Mode support).
- **Executive Principal Desk:** Professional rectangular framed portrait showcase of Principal **Mr. Rasheed Ahmad** with formal HED KP credential plate and welcome address.
- **Two-Tier Sticky Header:** Top institutional utility bar with Arabic motto (`رَبِّ زِدْنِي عِلْماً`), official affiliations, theme/RTL toggle, and gold CMS portal login pill.
- **Cinematic Campus Drone Background Video:** Web-optimized aerial drone loop (`campus-drone.mp4`) with smooth charcoal glass gradient overlay for crystal-clear typography.
- **Unobstructed Live Updates Ticker:** Real-time scrolling marquee notification bar for marks-based merit lists, admission deadlines, and official HED circulars.
- **6 Accredited BS Academic Departments:**
  1. **Computer Science** (BSCS 4-Year & ICS)
  2. **Microbiology** (BS Microbiology 4-Year)
  3. **Chemistry** (BS Chemistry 4-Year & F.Sc)
  4. **Botany** (BS Botany 4-Year & F.Sc)
  5. **English** (BS English Literature & Linguistics 4-Year)
  6. **Urdu** (BS Urdu Literature 4-Year)
- **Transparent Marks-Based Merit Admissions:** Zero interview bias — 100% evaluated on Total Obtained Marks with live KP HED merit formula preview.
- **Campus Life Tabbed Gallery:** High-resolution aerial stills covering academic blocks, botanical gardens, science laboratories, and student walkways with lightbox modal.

### 2. ⚡ Dual Attendance & Disciplinary Compliance Engine
- **BS 4-Year HEC / University of Swat 75% Rule:**
  - Automated subject-wise attendance tracking:
    $$\text{Attendance \%} = \left(\frac{\text{Present Lectures} + \text{Approved Official Leaves}}{\text{Total Conducted Lectures}}\right) \times 100$$
  - Real-time three-tier status:
    - $\ge 75\%$ &rarr; **Eligible** (Green Badge)
    - $70.0\% - 74.99\%$ &rarr; **Shortage / Condonable by Principal** (Yellow Warning)
    - $< 70.0\%$ &rarr; **Debarred from Final Term Examination** (Red Lockout)
  - Interactive **Shortage Recovery Calculator** showing students the exact consecutive lectures ($x = 3C - 4P$) needed to regain eligibility.
- **HSSC (F.Sc / FA) 3-Day Consecutive Absence Rustication Rule:**
  - Automatically identifies Intermediate students marked absent for 3 consecutive instructional days without approved leave.
  - Changes status to `Rusticated`, locks student portal, and dispatches parent alert.
  - Requires parent in-person campus visit and formal Principal reinstatement with CNIC logging.

### 3. 📝 Controller of Examinations Office & Result Locking
- **Strict Subject-Assignment Lock:** Faculty members can strictly view, mark attendance, and submit marks **only** for courses assigned to them (`TeacherCourseAssignment`).
- **One-Way Result Lock:** Once a faculty member submits semester marks, the record is locked read-only for teachers.
- **Master DMC / Official Transcript Compilation:** Controller of Examinations reviews and verifies all subjects before releasing finalized semester transcripts with SGPA/CGPA and percentage calculations.
- **Official Roll Number Slips:** Printable examination slips displaying live attendance clearance stamps, timetable, and room allocations.

### 4. 📲 Bilingual Parent Communication & WhatsApp Integration
- Automated English + Urdu notification generation for attendance shortages and disciplinary warnings.
- **1-Click Instant WhatsApp Dispatch:**
  `https://wa.me/{guardianPhone}?text={encodedBilingualMessage}`
  Allows clerks to notify parents immediately with zero manual typing.

### 5. 🔐 Role-Based Access Control (RBAC)
Configured with 6 distinct institutional security roles:
1. `SuperAdmin` / `Principal`: Full campus ERP oversight, student reinstatements, condonation approvals.
2. `ControllerExams`: Examination result verification, grade locks, Master DMCs, and roll number slip generation.
3. `HOD`: Departmental curriculum monitoring and teacher subject allocation.
4. `Faculty`: Class attendance marking and semester award list submission.
5. `Clerk`: Admission verification, fee challan processing, and parent notification dispatch.
6. `Student`: Live attendance tracking, fee payment receipts, DMCs, and exam slips.

---

## 🛠️ Technology Stack & Architecture

- **Backend Framework:** ASP.NET Core 10.0 (C# 13) MVC
- **Database & ORM:** Entity Framework Core (Code-First)
  - Production: PostgreSQL / SQL Server
  - In-Memory fallback: Zero-configuration instant local startup
- **Security & Identity:** ASP.NET Core Identity with PBKDF2 password hashing and secure cookies
- **Frontend & Styling:** Razor Views (`.cshtml`), Bootstrap 5.3, Bootstrap Icons, Google Fonts (`Plus Jakarta Sans`, `Amiri`)
- **Visual Analytics:** Chart.js 4.4 for real-time administrative dashboards
- **Containerization:** Multi-stage optimized Dockerfile (`mcr.microsoft.com/dotnet/aspnet:10.0`)
- **Hosting & Infrastructure:** Google Cloud Run, Firebase App Hosting, Docker, Render

---

## 📁 Repository Structure

```text
├── College Web Application/
│   ├── Areas/
│   │   ├── Admin/             # Principal, Exams Controller & Clerk Portals
│   │   ├── Faculty/           # Attendance Marking & Grading Console
│   │   └── Student/           # Student Dashboard, Shortage Recovery & Slips
│   ├── Controllers/           # Public & Authentication Controllers
│   ├── Data/                  # EF Core Context & Institutional DbInitializer
│   ├── Models/                # Domain Entities & ViewModels
│   ├── Views/                 # Razor Views (GIKI Homepage, Mega-Menus, Portals)
│   └── wwwroot/
│       ├── css/               # GIKI Emerald Green (#0B6623) Design System
│       ├── images/            # Official Crest, Principal Portrait, Gallery
│       └── videos/            # Optimized 720p Drone Footage
├── Dockerfile                 # Multi-stage production container build
├── firebase.json              # Firebase Hosting + Cloud Run configuration
├── .gitignore                 # Visual Studio / .NET clean ignore rules
└── README.md                  # Institutional documentation
```

---

## 💻 Local Development Setup

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022 / 2025](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Clone and Run
```bash
# 1. Clone repository
git clone https://github.com/abdussatarkhan/College-Web-Application.git
cd College-Web-Application

# 2. Restore dependencies and run
dotnet restore
dotnet run --project "College Web Application/College Web Application.csproj"
```

Open your browser at `https://localhost:7170` or `http://localhost:5161`.

---

## ☁️ Cloud Deployment (Google Cloud Run / Firebase)

This project is pre-configured for high-concurrency cloud hosting capable of handling **10,000+ simultaneous requests**:

```bash
# Deploy to Google Cloud Run with 1 command:
gcloud run deploy gdc-madyan-portal \
  --source . \
  --region asia-south1 \
  --allow-unauthenticated \
  --port 8080
```

---

## 📄 License & Attribution

&copy; 2006–2026 **Government Degree College Madyan Swat**, Higher Education Department, Government of Khyber Pakhtunkhwa. All rights reserved.
Developed by **Abdussatar Khan** for academic and administrative excellence.
