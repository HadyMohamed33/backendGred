# Software Requirements Specification
## for
# AlNady Sports Training Platform

---

| Field | Details |
|---|---|
| **Version** | 2.0 |
| **Status** | Approved |
| **Prepared by** | AlNady Development Team |
| **Organization** | AlNady Corporation |
| **Date** | June 2026 |
| **Document ID** | SRS-ALNADY-2.0 |

---

## Revision History

| Name | Date | Reason for Changes | Version |
|---|---|---|---|
| AlNady Development Team | December 2025 | Initial document creation | 1.0 |
| AlNady Development Team | June 2026 | Full architecture migration from Node.js/MongoDB to ASP.NET Core 9 / SQL Server / Clean Architecture | 2.0 |

---

## Table of Contents

1. Introduction
   - 1.1 Purpose
   - 1.2 Document Conventions
   - 1.3 Intended Audience and Reading Suggestions
   - 1.4 Product Scope
   - 1.5 References

2. Overall Description
   - 2.1 Product Perspective
   - 2.2 Product Functions
   - 2.3 User Classes and Characteristics
   - 2.4 Operating Environment
   - 2.5 Design and Implementation Constraints
   - 2.6 User Documentation
   - 2.7 Assumptions and Dependencies

3. External Interface Requirements
   - 3.1 User Interfaces
   - 3.2 Hardware Interfaces
   - 3.3 Software Interfaces
   - 3.4 Communications Interfaces

4. System Features
   - 4.1 User Registration and Authentication
   - 4.2 Training Program Discovery and Enrollment
   - 4.3 Profile Management System
   - 4.4 Rating and Feedback System
   - 4.5 Payment and Transaction Management
   - 4.6 Administrative Control System
   - 4.7 Security and Session Management

5. Other Nonfunctional Requirements
   - 5.1 Performance Requirements
   - 5.2 Safety Requirements
   - 5.3 Security Requirements
   - 5.4 Software Quality Attributes
   - 5.5 Business Rules

6. Architecture Description
   - 6.1 Clean Architecture Overview
   - 6.2 Domain Layer
   - 6.3 Application Layer
   - 6.4 Infrastructure Layer
   - 6.5 API (Presentation) Layer
   - 6.6 Dependency Flow

7. Backend Implementation Details
   - 7.1 ASP.NET Core 9 Web API
   - 7.2 Entity Framework Core and Database Access
   - 7.3 Repository Pattern and Unit of Work
   - 7.4 Authentication and Authorization
   - 7.5 Validation with FluentValidation
   - 7.6 Object Mapping with AutoMapper
   - 7.7 Dependency Injection
   - 7.8 Logging with Serilog
   - 7.9 API Documentation with Swagger/OpenAPI
   - 7.10 Error Handling and Global Exception Middleware
   - 7.11 API Design Principles
   - 7.12 Coding Standards and Best Practices

8. Database Design
   - 8.1 Database Engine
   - 8.2 Database Schema
   - 8.3 Entity Relationships

9. Frontend Architecture
   - 9.1 Technology Stack
   - 9.2 Application Structure

10. Deployment
    - 10.1 Target Environments
    - 10.2 Backend Deployment
    - 10.3 Database Deployment
    - 10.4 Frontend Deployment
    - 10.5 Production Considerations

11. Other Requirements

Appendix A: Glossary  
Appendix B: Analysis Models  
Appendix C: To Be Determined List

---

---

# 1. Introduction

## 1.1 Purpose

This Software Requirements Specification (SRS) document describes the functional and non-functional requirements for the **AlNady Sports Training Platform**, Version 2.0. The system is designed to connect sports trainees (Players) with certified trainers and academies, facilitating training program discovery, enrollment, payment processing, and post-training feedback.

This document covers the complete system scope including user management, training program management, payment processing, and administrative controls. It also documents the backend architecture, which has been fully redesigned and implemented using **ASP.NET Core 9 Web API** following **Clean Architecture** principles, with **SQL Server** as the relational database engine and a **React / TypeScript** frontend.

This version supersedes Version 1.0 entirely with respect to all technology-stack and implementation details.

## 1.2 Document Conventions

This document follows the IEEE Std 830-1998 standard for Software Requirements Specifications. The following conventions apply:

| Convention | Meaning |
|---|---|
| **REQ-XXX** | Uniquely identified functional requirement |
| **SHALL** | Mandatory requirement |
| **SHOULD** | Recommended but not mandatory |
| **MAY** | Optional feature |
| *Italics* | Technical terms defined in the Glossary |
| `Monospace` | Code identifiers, class names, endpoints, or commands |

Priority levels used:
- **High** – Critical for system operation; must be implemented in first release
- **Medium** – Important functionality; implemented in primary releases
- **Low** – Desirable but deferrable to later releases

## 1.3 Intended Audience and Reading Suggestions

This document is intended for the following audiences:

| Audience | Recommended Sections |
|---|---|
| **Software Developers** | Sections 4, 6, 7, 8, 9 |
| **Project Managers** | Sections 1, 2, 5, 10 |
| **Testers / QA Engineers** | Sections 4, 5, 7 |
| **Database Administrators** | Sections 7.2, 7.3, 8 |
| **Security Engineers** | Sections 5.3, 7.4, 7.5 |
| **Stakeholders / Business Owners** | Sections 1, 2, 4, 5 |
| **DevOps / Deployment Engineers** | Sections 10, 7.8, 7.9 |

Recommended reading sequence: Section 1 → Section 2 → Section 4 → Section 6 → Section 7 → Section 8 → Section 5 → Section 10 → Appendices.

## 1.4 Product Scope

**AlNady** is a web-based platform that connects sports enthusiasts with professional trainers and academies across Egypt. The system enables:

- **Players (Users)** to search for training programs, enroll in sessions, process payments, provide ratings, and manage training schedules.
- **Trainers and Academies** to manage professional profiles, create training programs, handle enrollments, and receive performance feedback.
- **Administrators** to verify trainer/academy credentials, moderate content, manage blacklists, and oversee the platform.

The platform automates and digitizes what was previously a manual, fragmented process of training program discovery and enrollment, providing a scalable, secure, and reliable digital infrastructure.

## 1.5 References

| Reference | Document |
|---|---|
| ERD | AlNady Entity Relationship Diagram (ERD.txt) |
| DFD | AlNady Data Flow Diagram (Last DFD.txt) |
| UC | AlNady Use Cases (usecases_markdown.md) |
| FR | AlNady Functional Requirements (Functional Requirements.md) |
| IEEE 830 | IEEE Recommended Practice for Software Requirements Specifications |
| ASP.NET Core 9 | Microsoft ASP.NET Core 9 Official Documentation |
| EF Core | Entity Framework Core Documentation |
| Clean Architecture | Robert C. Martin, "Clean Architecture: A Craftsman's Guide" |

---

---

# 2. Overall Description

## 2.1 Product Perspective

AlNady is a standalone web application operating as a **Software-as-a-Service (SaaS)** platform. The system is composed of two independently deployable components:

1. **Backend API** – An ASP.NET Core 9 Web API built following Clean Architecture principles, exposing RESTful endpoints consumed by the frontend and any future third-party integrators.
2. **Frontend Application** – A React/TypeScript single-page application (SPA) built with Vite, communicating exclusively with the backend through versioned RESTful APIs.

The system interfaces with the following external services:

| External Service | Purpose |
|---|---|
| Email Service (SendGrid or equivalent) | Account verification codes, password reset codes, enrollment notifications, monthly rating reminders |
| Payment Gateway (Tap or equivalent) | Enrollment payment processing and refund handling |
| Location / Geolocation Service | Distance calculation in search results |
| QR Code Library | National ID QR code scanning and identity verification |
| Cloud File Storage (Azure Blob Storage or AWS S3) | Certificate document and profile image storage |

The system replaces manual training program discovery and enrollment processes with an automated, scalable, and auditable digital platform. The backend is entirely self-contained and does not depend on any server-side rendering framework; all presentation logic resides in the React frontend.

## 2.2 Product Functions

The major functions of AlNady are:

| Function | Description | Priority |
|---|---|---|
| User Registration & Authentication | Account creation, email verification, login/logout, JWT issuance, password recovery | High |
| Profile Management | Trainer/academy profile creation and editing with mandatory admin verification | High |
| Training Program Discovery | Search and filter trainers/academies by location, price, rating, and sports specialization | High |
| Program Enrollment | Form-based enrollment with QR code identity verification and integrated payment processing | High |
| Rating System | Multi-aspect (1–5 star) rating and feedback with 24-hour edit window and monthly reminders | Medium |
| Cancellation Management | Refund processing based on configurable cancellation policies | Medium |
| Administrative Controls | User verification, content moderation, blacklist management, audit dashboards | Medium |
| Recommendation Engine | Personalized recommendations based on user preferences, enrollment history, and ratings | Low |

## 2.3 User Classes and Characteristics

| User Class | Description | Technical Proficiency | Primary Interface |
|---|---|---|---|
| **Player / User** | Sports enthusiast seeking training. Registered and verified account holder. | Low to Medium | Search, Enrollment, Rating interfaces |
| **Trainer** | Individual certified sports trainer offering programs. May have low technical proficiency. | Low to Medium | Profile management, Program creation, Enrollment management |
| **Academy** | Sports institution (club, gym, academy) with potentially multiple trainers. | Medium | All trainer features plus batch program management |
| **Admin** | System administrator responsible for platform oversight and content governance. | High | Administrative dashboard, verification, audit logs |
| **Guest** | Unregistered visitor. No authentication required. | Any | Search and browse only (read-only access to approved profiles) |

## 2.4 Operating Environment

### 2.4.1 Backend Environment

| Component | Technology |
|---|---|
| **Runtime** | .NET 9 (C#) |
| **Framework** | ASP.NET Core 9 Web API |
| **Architecture** | Clean Architecture (Domain / Application / Infrastructure / API layers) |
| **ORM** | Entity Framework Core 9 (Code First) |
| **Database** | Microsoft SQL Server 2022 (or Azure SQL Database) |
| **Authentication** | ASP.NET Identity + JWT Bearer Tokens |
| **Logging** | Serilog (structured logging with file and console sinks) |
| **API Documentation** | Swagger / OpenAPI 3.0 via Swashbuckle |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **Hosting** | Azure App Service or Render |

### 2.4.2 Frontend Environment

| Component | Technology |
|---|---|
| **Language** | TypeScript |
| **Framework** | React 18 |
| **Build Tool** | Vite |
| **Styling** | Tailwind CSS |
| **Routing** | React Router v6 |
| **HTTP Client** | Axios |
| **Hosting** | Vercel |

### 2.4.3 Database Environment

| Component | Technology |
|---|---|
| **Engine** | SQL Server 2022 / Azure SQL Database |
| **Schema Management** | Entity Framework Core Migrations (Code First) |
| **Query Language** | LINQ (via EF Core) / Raw SQL for complex queries |

### 2.4.4 Supported Client Environments

| Browser | Minimum Version |
|---|---|
| Google Chrome | 90+ |
| Mozilla Firefox | 88+ |
| Safari | 14+ |
| Microsoft Edge | 90+ |

The frontend application is responsive and supports desktop, tablet, and mobile viewports.

## 2.5 Design and Implementation Constraints

| Constraint | Details |
|---|---|
| **Regulatory Compliance** | Must comply with Egypt data protection regulations and applicable privacy laws |
| **Payment Integration** | Payment processing must support Egyptian payment methods (Tap or equivalent) |
| **National ID Verification** | Must integrate with Egypt National ID QR code standards for identity verification |
| **Language Support** | The system shall support both Arabic (RTL) and English languages |
| **HTTPS Enforcement** | All communications must use TLS 1.2 or higher; no plaintext HTTP in production |
| **File Upload Limits** | Document uploads (certificates, IDs) are limited to 10 MB per file; accepted formats are PDF, JPG, and PNG |
| **Architecture Constraint** | The backend must strictly follow Clean Architecture; no direct database access from the API or Application layers without passing through the Infrastructure layer |
| **Dependency Injection** | All services, repositories, and cross-cutting concerns must be registered and resolved via the ASP.NET Core built-in DI container |
| **SOLID Principles** | All code shall adhere to SOLID principles with clear Separation of Concerns |

## 2.6 User Documentation

The following documentation shall be provided:

- Online contextual help system integrated into the application
- User manuals for each user class (Player, Trainer, Academy, Admin)
- Swagger/OpenAPI interactive API documentation accessible at `/swagger`
- FAQ section addressing common issues for each user class
- Video tutorials for key functionalities (enrollment, profile setup)

## 2.7 Assumptions and Dependencies

### 2.7.1 Assumptions

- Users have reliable internet access sufficient for web application usage.
- Users have valid email accounts that can receive verification and notification emails.
- Trainers and academies possess the certification documents required for profile verification.
- The external payment gateway service is available and reliable during enrollment operations.
- The email service provider (SendGrid or equivalent) is available for transactional email delivery.
- Administrators have sufficient time and capacity to review and approve pending trainer/academy profiles within a reasonable SLA.

### 2.7.2 Dependencies

| Dependency | Purpose |
|---|---|
| Email service provider | Transactional emails (verification, password reset, notifications) |
| Third-party payment gateway (Tap or equivalent) | Payment processing and refund handling |
| Cloud file storage (Azure Blob Storage or AWS S3) | Certificate and image storage |
| Location/geolocation service API | Distance calculation in search |
| QR code scanning library | National ID verification |
| .NET 9 SDK | Backend compilation and runtime |
| SQL Server / Azure SQL | Persistent data storage |

---

---

# 3. External Interface Requirements

## 3.1 User Interfaces

The system shall provide the following user interfaces, all implemented as React/TypeScript components:

| Interface | Description |
|---|---|
| **Responsive Web Application** | Works on desktop, tablet, and mobile devices via Tailwind CSS responsive utilities |
| **Role-Based Dashboards** | Separate dashboard views for Player, Trainer, Academy, and Admin roles |
| **Search Interface** | Advanced filtering with keyword, location-based, price-range, and rating filters |
| **Profile Management Interface** | Multi-step forms for profile creation and editing, with document upload capability |
| **Payment Interface** | Secure payment forms integrating with the external payment gateway SDK |
| **Rating Interface** | Star-based rating form with optional aspect ratings and text comment |
| **Administrative Interface** | Comprehensive management panel for profile verification, blacklist management, and audit logs |
| **Authentication Forms** | Login, registration, forgot-password, and email-verification screens |

All user interfaces shall comply with **WCAG 2.1 AA** accessibility guidelines. Arabic (RTL) layout shall be fully supported.

## 3.2 Hardware Interfaces

| Hardware Interface | Description |
|---|---|
| **Camera / QR Scanner** | The system shall request camera access on mobile and desktop browsers to scan National ID QR codes via the browser's MediaDevices API |
| **GPS / Location Services** | The system shall request the browser's Geolocation API for distance-based search filtering |
| **File System** | The system shall support file selection dialogs for document uploads (certificates, profile images) with client-side format and size validation before upload |

## 3.3 Software Interfaces

| Interface | Details |
|---|---|
| **Email Service API** | SendGrid (or equivalent) REST API for transactional email delivery. Configured via API key stored in application secrets. |
| **Payment Gateway API** | Tap (or equivalent) REST API for credit card charging and refund processing. Communication via HTTPS with API key authentication. |
| **Location Service API** | Google Maps Geocoding API or OpenStreetMap Nominatim for geocoding addresses and calculating distances. |
| **Cloud File Storage** | Azure Blob Storage or AWS S3 SDK for uploading and retrieving certificate documents and profile images. Files are referenced by URL in the SQL Server database. |
| **ASP.NET Identity** | Built-in ASP.NET Core Identity framework for user management, password hashing, and role management. Backed by SQL Server via EF Core. |

## 3.4 Communications Interfaces

| Protocol / Standard | Usage |
|---|---|
| **HTTPS (TLS 1.2+)** | All client-server and server-to-third-party communications |
| **RESTful HTTP/JSON** | Primary API communication protocol between React frontend and ASP.NET Core backend |
| **JWT Bearer Tokens** | Authentication mechanism for all protected API endpoints |
| **SMTP over TLS** | Outgoing transactional email via external email service provider |
| **JSON** | Standard data format for all API requests and responses |
| **WebSocket (optional)** | For real-time notifications in future releases |

---

---

# 4. System Features

---

## 4.1 User Registration and Authentication

### 4.1.1 Description and Priority

Enables new users to create accounts on the AlNady platform, verify their email addresses, log in to receive a JWT, and recover access to their accounts via a password reset flow. **Priority: High.**

### 4.1.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| User submits registration form | System validates inputs (FluentValidation), checks email uniqueness via ASP.NET Identity, sends verification code, creates user record in SQL Server |
| User submits verification code | System validates code, activates account, redirects to login |
| User submits login credentials | System validates credentials via ASP.NET Identity, generates JWT and refresh token, returns token to client |
| User submits forgot-password email | System generates reset code, sends via email service |
| User submits new password with reset code | System validates code expiry, hashes new password via ASP.NET Identity's `PasswordHasher`, updates record |

### 4.1.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-101 | The system SHALL allow new users to register by providing email, password, full name, phone number, and role (Player / Trainer / Academy). |
| REQ-102 | The system SHALL validate all registration inputs using FluentValidation rules before persisting any data. |
| REQ-103 | The system SHALL verify email uniqueness using ASP.NET Identity before creating a user record. |
| REQ-104 | The system SHALL generate a time-limited (15-minute) email verification code and deliver it via the configured email service. |
| REQ-105 | The system SHALL allow users to request a resend of the verification code; the previous code SHALL be invalidated upon resend. |
| REQ-106 | The system SHALL hash all passwords using ASP.NET Identity's built-in `PasswordHasher<TUser>` (PBKDF2 with HMACSHA256) before storage. |
| REQ-107 | The system SHALL create the user record in SQL Server via EF Core only after successful email verification. |
| REQ-108 | The system SHALL assign the selected role (Player / Trainer / Academy) to the user via ASP.NET Identity's role management upon activation. |
| REQ-109 | The system SHALL generate a JWT (JSON Web Token) upon successful login, including the user's ID, email, and roles as claims. |
| REQ-110 | The system SHALL generate a refresh token upon login to allow token renewal without re-authentication. |
| REQ-111 | The system SHALL update the user's last login timestamp upon each successful authentication. |
| REQ-112 | The system SHALL redirect the authenticated user to the role-specific dashboard. |
| REQ-113 | The system SHALL validate the email format and check existence in the database during the forgot-password flow. |
| REQ-114 | The system SHALL generate a time-limited (30-minute) password reset code and deliver it via the email service. |
| REQ-115 | The system SHALL reject password reset attempts using expired or already-used reset codes. |
| REQ-116 | The system SHALL validate that the new password meets strength requirements (minimum 8 characters, at least one uppercase, one digit, one special character). |
| REQ-117 | The system SHALL log all authentication events (registration, login, logout, failed attempts, password resets) via Serilog. |

---

## 4.2 Training Program Discovery and Enrollment

### 4.2.1 Description and Priority

Enables users to search for training programs offered by verified trainers and academies using configurable filter criteria, and allows authenticated players to enroll in selected programs through a structured form with identity verification and payment processing. **Priority: High.**

### 4.2.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| User applies search filters | System executes parameterized LINQ query via EF Core against SQL Server; applies filters (sport, location, price, rating); returns ranked results |
| User selects a program and clicks Enroll | System displays enrollment form with program terms retrieved from SQL Server |
| User submits QR-scanned National ID | System validates National ID QR code data and checks blacklist status |
| User submits payment details | System processes payment via payment gateway API; on success, creates enrollment record and decrements available slots atomically within a Unit of Work transaction |

### 4.2.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-201 | The system SHALL allow searching trainers/academies by sport specialization, location, price range, and minimum rating. |
| REQ-202 | The system SHALL calculate and display distance from the user's current geolocation when the browser provides location data. |
| REQ-203 | The system SHALL validate the National ID through QR code scanning during the enrollment form submission. |
| REQ-204 | The system SHALL check user blacklist status against the Blacklist entity before allowing enrollment to proceed to payment. |
| REQ-205 | The system SHALL process payments immediately through the integrated payment gateway upon form submission. |
| REQ-206 | The system SHALL reduce the program's available slot count atomically (within a Unit of Work transaction) upon successful payment confirmation. |
| REQ-207 | The system SHALL create an `EnrollmentRequest` record with status `Approved` in SQL Server upon successful payment. |
| REQ-208 | The system SHALL send an enrollment confirmation notification to the user and a new enrollment notification to the trainer/academy via the email service. |
| REQ-209 | The system SHALL validate the user's form responses against the program's enrollment requirements before proceeding to payment. |
| REQ-210 | The system SHALL support program capacity management; programs SHALL automatically transition to status `Filled` when available slots reach zero. |
| REQ-211 | The system SHALL notify enrolled users whose payment codes expire without completion. |
| REQ-212 | The system SHALL prevent duplicate enrollment requests by a single user for the same program. |

---

## 4.3 Profile Management System

### 4.3.1 Description and Priority

Allows trainers and academies to create and maintain professional profiles including biographical information, sports specializations, available time slots, training locations, pricing, certification documents, and age/gender preferences. All profile changes require admin verification before becoming publicly visible. **Priority: High.**

### 4.3.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| Trainer/Academy submits profile creation | System validates inputs (FluentValidation), stores profile data in SQL Server via Repository, stores uploaded documents in cloud storage, sets status to `PendingAdminApproval`, notifies admin |
| Admin approves profile | System updates `IsVerifiedByAdmin` to `true` via Repository within Unit of Work, notifies trainer/academy, profile becomes visible in search results |
| Admin rejects profile | System stores rejection reason, notifies trainer/academy with reason, profile remains hidden |

### 4.3.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-301 | The system SHALL allow trainers/academies to create detailed profiles including about section, sports specialization, available times, training location, pricing, and age/gender preferences. |
| REQ-302 | The system SHALL require admin verification (`IsVerifiedByAdmin = true`) before profiles become publicly visible in search results. |
| REQ-303 | The system SHALL support certificate document uploads; accepted formats are PDF, JPG, and PNG; maximum file size is 10 MB per file. |
| REQ-304 | The system SHALL validate uploaded file formats and sizes on the server side (ASP.NET Core middleware) before storing to cloud storage. |
| REQ-305 | The system SHALL allow trainers/academies to specify age categories and gender preferences for programs. |
| REQ-306 | The system SHALL track and expose profile verification status (`Pending`, `Approved`, `Rejected`) via the `IsVerifiedByAdmin` boolean and related status fields. |
| REQ-307 | The system SHALL send an automatic notification to admins when a new profile or profile update is submitted for review. |
| REQ-308 | The system SHALL maintain a running average rating for each trainer and academy, recalculated each time a new rating is submitted. |
| REQ-309 | The system SHALL allow trainers/academies to resubmit corrected profiles after an admin rejection. |
| REQ-310 | The system SHALL require re-approval if a substantive profile modification (certifications, specialization, location) is submitted. |

---

## 4.4 Rating and Feedback System

### 4.4.1 Description and Priority

Enables players who have completed a training program to submit ratings (1–5 stars) and optional text reviews for trainers and academies, including granular aspect ratings. The system enforces rating eligibility, supports a 24-hour edit window, flags inappropriate content, and issues monthly reminders for unrated completed programs. **Priority: Medium.**

### 4.4.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| User selects a program date for rating | System verifies enrollment and program date have passed; displays rating form |
| User submits rating and review | System validates input (FluentValidation), persists `Rating` and `RatingAspect` records via Repository, recalculates average rating in `Trainer` or `Academy` entity, notifies trainer/academy |
| User edits rating within 24 hours | System updates existing records, recalculates average rating |
| Monthly scheduler runs | System identifies enrolled users with unrated past programs and sends reminder emails |

### 4.4.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-401 | The system SHALL allow only enrolled players who have reached the program date to submit ratings. |
| REQ-402 | The system SHALL accept ratings on a 1–5 star scale with an optional text comment. |
| REQ-403 | The system SHALL support detailed aspect ratings stored as `RatingAspect` records linked to the main `Rating` record. |
| REQ-404 | The system SHALL allow a player to edit their rating exactly once, within 24 hours of the original submission. |
| REQ-405 | The system SHALL automatically recalculate and update the `average_rating` field in the `Trainer` and/or `Academy` entity after every new or updated rating submission. |
| REQ-406 | The system SHALL automatically send monthly reminder notifications to users who have completed programs but have not yet submitted a rating. |
| REQ-407 | The system SHALL flag reviews containing inappropriate content for admin moderation review. |
| REQ-408 | The system SHALL notify the trainer/academy via email when a new review is submitted on their profile. |

---

## 4.5 Payment and Transaction Management

### 4.5.1 Description and Priority

Handles all financial transactions including enrollment payments, refund processing, and transaction audit trails. Integrates with an external payment gateway. **Priority: High.**

### 4.5.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| User submits payment | Backend calls payment gateway API; gateway returns success/failure; on success, enrollment record is committed via Unit of Work |
| User or trainer initiates cancellation | System checks cancellation policy timing, calculates refund amount, calls payment gateway refund API, updates enrollment status, sends notifications |

### 4.5.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-501 | The system SHALL process credit/debit card payments through the integrated payment gateway (Tap or equivalent). |
| REQ-502 | The system SHALL support multiple payment statuses: `Pending`, `Completed`, `Failed`, `Refunded`. |
| REQ-503 | The system SHALL calculate refund amounts based on the following cancellation policy: 100% refund if cancelled more than 48 hours before session; 50% if between 24 and 48 hours; 0% if less than 24 hours before session. |
| REQ-504 | The system SHALL record and store the payment gateway's transaction reference ID for all transactions. |
| REQ-505 | The system SHALL prevent duplicate payment processing using idempotency keys. |
| REQ-506 | The system SHALL maintain a complete audit trail for all financial transactions, including amount, timestamp, status, and gateway reference. |
| REQ-507 | The system SHALL NOT store raw credit card numbers or full card data; only tokenized references provided by the payment gateway SHALL be stored. |

---

## 4.6 Administrative Control System

### 4.6.1 Description and Priority

Provides system administrators with tools to verify trainer/academy credentials, moderate user-generated content, manage blacklists, monitor platform activity, and maintain system health. **Priority: Medium.**

### 4.6.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| Admin reviews pending profile | System retrieves profile and documents via Repository; admin makes approve/reject decision; system updates database via Unit of Work and sends notification |
| Admin adds user to blacklist | System creates blacklist record with user ID, reason, and timestamp; future enrollment requests from this user are rejected |
| Admin reviews flagged review | System retrieves flagged content; admin approves, edits, or deletes the review |

### 4.6.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-601 | The system SHALL provide a dedicated administrative dashboard interface for reviewing and verifying trainer/academy profiles. |
| REQ-602 | The system SHALL allow admins to approve or reject profile submissions; rejected submissions SHALL include a mandatory rejection reason. |
| REQ-603 | The system SHALL maintain a user blacklist with reason tracking, effective date, and optional expiry date. |
| REQ-604 | The system SHALL automatically flag trainer/academy profiles that accumulate three or more cancellations within a 30-day rolling window for admin review. |
| REQ-605 | The system SHALL maintain comprehensive audit logs of all admin actions (approvals, rejections, blacklist changes) via Serilog. |
| REQ-606 | The system SHALL allow admins to moderate (approve, edit, or remove) flagged ratings and reviews. |
| REQ-607 | The system SHALL provide a metrics dashboard showing key platform statistics (registrations, enrollments, ratings, cancellations). |
| REQ-608 | The system SHALL require re-authentication (elevated JWT claim or policy) for sensitive admin operations. |

---

## 4.7 Security and Session Management

### 4.7.1 Description and Priority

Manages user authentication, token-based session management, and role-based access control for all protected resources. **Priority: High.**

### 4.7.2 Stimulus / Response Sequences

| Stimulus | Response |
|---|---|
| Client requests a protected endpoint | ASP.NET Core JWT middleware extracts and validates Bearer token; if valid, populates `ClaimsPrincipal`; authorization policies evaluated |
| Client presents expired JWT | System returns HTTP 401 Unauthorized; client must use refresh token to obtain new JWT |
| Client presents refresh token | System validates refresh token in database; if valid, issues new JWT and rotates refresh token |
| User logs out | System invalidates refresh token record in database |

### 4.7.3 Functional Requirements

| ID | Requirement |
|---|---|
| REQ-701 | The system SHALL implement JWT Bearer authentication with configurable expiration (default: 60 minutes for access token). |
| REQ-702 | The system SHALL implement refresh token rotation: each use of a refresh token issues a new refresh token and invalidates the old one. |
| REQ-703 | The system SHALL enforce role-based access control (RBAC) using ASP.NET Core `[Authorize(Roles = "...")]` attributes and policy-based authorization where appropriate. |
| REQ-704 | The system SHALL log all access attempts, including authenticated successes and failed/unauthorized attempts, via Serilog. |
| REQ-705 | The system SHALL enforce rate limiting on authentication endpoints to prevent brute-force attacks. |
| REQ-706 | The system SHALL enforce HTTPS for all API communications in production environments. |
| REQ-707 | The system SHALL implement CORS policies limiting API access to authorized frontend origins. |
| REQ-708 | The system SHALL validate all incoming request payloads using FluentValidation; invalid inputs SHALL return HTTP 400 Bad Request with structured error details. |
| REQ-709 | The system SHALL implement CSRF protection considerations for state-changing operations. |

---

---

# 5. Other Nonfunctional Requirements

## 5.1 Performance Requirements

| ID | Requirement |
|---|---|
| REQ-801 | The system SHALL support at least 10,000 concurrent users during peak hours without degradation of service. |
| REQ-802 | Page load times SHALL not exceed 3 seconds for 95% of requests under normal load conditions. |
| REQ-803 | Search query results SHALL be returned within 2 seconds for 99% of queries. |
| REQ-804 | Payment processing SHALL complete within 10 seconds for 99% of transactions (inclusive of external gateway call). |
| REQ-805 | The system SHALL maintain 99.5% uptime excluding scheduled maintenance windows. |
| REQ-806 | All database queries SHALL be optimized with appropriate EF Core indexes and query patterns to achieve sub-second response times for single-record operations. |
| REQ-807 | The ASP.NET Core API response time (excluding external service calls) SHALL not exceed 500 milliseconds for 95% of requests. |
| REQ-808 | EF Core queries SHALL be reviewed for N+1 query patterns; `Include()` and projection strategies SHALL be applied where appropriate. |

## 5.2 Safety Requirements

| ID | Requirement |
|---|---|
| REQ-901 | The system SHALL NOT store sensitive payment information beyond tokenized references provided by the payment gateway. |
| REQ-902 | The system SHALL encrypt all sensitive user data at rest using SQL Server Transparent Data Encryption (TDE) or Azure SQL encryption. |
| REQ-903 | The system SHALL maintain automated database backup procedures with a Recovery Point Objective (RPO) of 24 hours and a Recovery Time Objective (RTO) of 4 hours. |
| REQ-904 | The system SHALL maintain complete audit trails for all financial transactions, retaining them for a minimum of 7 years. |
| REQ-905 | The system SHALL validate all user inputs server-side using FluentValidation to prevent malicious data submission regardless of client-side validation results. |

## 5.3 Security Requirements

| ID | Requirement |
|---|---|
| REQ-1001 | All communications SHALL use TLS 1.2 or higher; plaintext HTTP SHALL redirect to HTTPS in production. |
| REQ-1002 | The system SHALL implement rate limiting using ASP.NET Core middleware (e.g., `AspNetCoreRateLimit`) on all authentication and sensitive API endpoints. |
| REQ-1003 | User passwords SHALL be hashed using ASP.NET Identity's `PasswordHasher<TUser>` (PBKDF2 with HMACSHA256, 10,000+ iterations). |
| REQ-1004 | JWT access tokens SHALL expire after 60 minutes of issuance; refresh tokens SHALL expire after 7 days. |
| REQ-1005 | The system SHALL implement proper CORS policies restricting API access to approved frontend origins via `builder.Services.AddCors()` configuration. |
| REQ-1006 | The system SHALL implement SQL injection protection through EF Core parameterized queries; raw SQL SHALL only be used with explicit parameterization. |
| REQ-1007 | The system SHALL implement XSS protection by ensuring all API responses carry appropriate content-type headers and that the frontend sanitizes displayed user content. |
| REQ-1008 | File uploads SHALL be validated server-side for MIME type and file size; stored files SHALL be scanned for malware before being accessible. |
| REQ-1009 | All security-related events (failed logins, unauthorized access, admin actions) SHALL be logged via Serilog with structured context data. |
| REQ-1010 | Administrative functions SHALL require elevated authorization policies enforced via ASP.NET Core policy-based authorization. |

## 5.4 Software Quality Attributes

| ID | Attribute | Requirement |
|---|---|---|
| REQ-1101 | **Usability** | The system SHALL achieve an 85% task completion success rate in user testing for core workflows (search, enroll, rate). |
| REQ-1102 | **Reliability** | The system SHALL achieve a mean time between failures (MTBF) of 720 hours or greater. |
| REQ-1103 | **Maintainability** | The codebase SHALL maintain 80% unit test coverage for Application layer use cases and Domain entities. Tests SHALL use xUnit and Moq. |
| REQ-1104 | **Portability** | The backend SHALL run on any platform supporting .NET 9 runtime (Linux, Windows, macOS); Docker containerization SHALL be supported. |
| REQ-1105 | **Scalability** | The system SHALL support horizontal scaling (multiple API instances behind a load balancer) with stateless JWT authentication enabling session-independent scaling. |
| REQ-1106 | **Interoperability** | The system SHALL expose a documented RESTful API (Swagger/OpenAPI) for potential third-party integration. |
| REQ-1107 | **Testability** | All Application layer use cases and Domain entities SHALL be designed to support unit testing with mock implementations of repository interfaces. |
| REQ-1108 | **Observability** | The system SHALL emit structured logs (via Serilog), expose health check endpoints (`/health`), and support integration with application performance monitoring tools. |

## 5.5 Business Rules

| Rule | Description |
|---|---|
| **BR-001: Email Verification** | Email verification is mandatory before a user account becomes active; unverified accounts cannot log in. |
| **BR-002: Profile Visibility** | Trainer and academy profiles require admin approval (`IsVerifiedByAdmin = true`) before appearing in public search results. |
| **BR-003: Payment Policy** | Full payment is required at enrollment; partial payments are not supported. |
| **BR-004: Cancellation Refund** | 100% refund if cancelled more than 48 hours before session; 50% if between 24 and 48 hours; 0% if less than 24 hours. |
| **BR-005: Rating Eligibility** | Only players who have enrolled in and completed (reached program date for) a training program may submit a rating for it. |
| **BR-006: Rating Edit Window** | A player may edit their rating once, within 24 hours of original submission. |
| **BR-007: Blacklist Policy** | Three program cancellations within a 30-day rolling window results in a 90-day enrollment blacklist for the responsible party. |
| **BR-008: Re-approval Requirement** | Substantive profile modifications (certifications, specialization, pricing, location) require re-approval by admin before changes become visible. |
| **BR-009: Capacity Management** | Programs automatically transition to `Filled` status when available slot count reaches zero. |
| **BR-010: Rating Reminder** | Monthly automated reminders are sent to enrolled players who have not yet rated a completed program. |

---

---

# 6. Architecture Description

## 6.1 Clean Architecture Overview

The AlNady backend is implemented following **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) as described by Robert C. Martin. This architectural style organizes code into concentric layers with a strict unidirectional dependency rule:

> **Outer layers depend on inner layers. Inner layers do not depend on outer layers.**

This principle ensures that the core business logic (Domain and Application layers) remains independent of infrastructure concerns (databases, external services, frameworks) and can be tested in complete isolation.

```
┌──────────────────────────────────────────────┐
│               API Layer                       │
│  (Controllers, Middleware, Swagger, DI Setup) │
│                    │                          │
│                    ▼                          │
│           Application Layer                   │
│  (Use Cases, DTOs, Interfaces, Validators,    │
│   AutoMapper Profiles, Service Interfaces)    │
│                    │                          │
│                    ▼                          │
│            Domain Layer                       │
│  (Entities, Domain Enums, Business Rules,     │
│   Repository Interfaces, Domain Events)       │
│                    │                          │
│                    ▼                          │
│         Infrastructure Layer                  │
│  (EF Core DbContext, Migrations, Repository   │
│   Implementations, External Services,         │
│   Identity Configuration, Email, Storage)     │
└──────────────────────────────────────────────┘
```

The Domain layer has **no dependencies** on any other layer or external library. The Application layer depends only on Domain. Infrastructure depends on both Domain and Application (to implement interfaces defined there). The API layer depends on Application and Infrastructure only for DI registration.

## 6.2 Domain Layer

**Project:** `AlNady.Domain`

The Domain layer is the core of the application. It contains:

| Component | Description |
|---|---|
| **Entities** | `ApplicationUser`, `Trainer`, `Academy`, `TrainingProgram`, `EnrollmentRequest`, `Rating`, `RatingAspect`, `Certificate` — pure C# classes representing the business model |
| **Enums** | `UserRole` (Player, Trainer, Academy, Admin), `ProgramStatus` (Available, Filled, Cancelled), `EnrollmentStatus` (WaitingForPayment, Approved, Rejected) |
| **Repository Interfaces** | `IGenericRepository<T>`, `ITrainerRepository`, `IAcademyRepository`, `IEnrollmentRepository`, etc. — contracts that Infrastructure must implement |
| **Unit of Work Interface** | `IUnitOfWork` — contract for transactional data operations |
| **Domain Events (optional)** | Event types for decoupled business logic notification |

The Domain layer has **zero external NuGet dependencies** beyond base .NET libraries. This guarantees maximum portability and testability.

## 6.3 Application Layer

**Project:** `AlNady.Application`

The Application layer contains all application-specific business logic, orchestrating Domain entities through use cases (also called interactors or application services).

| Component | Description |
|---|---|
| **Use Cases / Services** | `AuthService`, `TrainerService`, `AcademyService`, `EnrollmentService`, `RatingService`, `AdminService` — orchestrate repository calls and domain logic |
| **DTOs (Data Transfer Objects)** | `RegisterDto`, `LoginDto`, `TrainerProfileDto`, `EnrollmentRequestDto`, `RatingDto` — define the API contract shapes |
| **AutoMapper Profiles** | Mapping configurations between Domain entities and DTOs |
| **FluentValidation Validators** | `RegisterValidator`, `LoginValidator`, `EnrollmentValidator`, `RatingValidator` — input validation rules |
| **Service Interfaces** | `IAuthService`, `IEmailService`, `IPaymentService`, `IFileStorageService` — abstractions for external service calls |
| **Response Models** | Standardized `ApiResponse<T>` wrapper used across all endpoints |

The Application layer depends on Domain but has no knowledge of EF Core, SQL Server, or any external service implementation.

## 6.4 Infrastructure Layer

**Project:** `AlNady.Infrastructure`

The Infrastructure layer provides concrete implementations of all interfaces defined in Domain and Application layers.

| Component | Description |
|---|---|
| **`ApplicationDbContext`** | EF Core `DbContext` with all `DbSet<T>` properties; configures entity relationships via Fluent API in `OnModelCreating` |
| **EF Core Migrations** | Code-First migration files managing the SQL Server schema lifecycle |
| **Repository Implementations** | Concrete classes implementing `IGenericRepository<T>` and specialized repository interfaces using EF Core LINQ queries |
| **`UnitOfWork` Implementation** | Coordinates multiple repository operations within a single `DbContext` transaction via `SaveChangesAsync()` |
| **Identity Configuration** | `AddIdentity<ApplicationUser, IdentityRole>()` setup with password policies, lockout settings, and token providers |
| **Email Service** | `EmailService` implementing `IEmailService` using SendGrid SDK or SMTP client |
| **File Storage Service** | `AzureBlobStorageService` or `S3StorageService` implementing `IFileStorageService` |
| **Payment Service** | `TapPaymentService` implementing `IPaymentService` using payment gateway HTTP client |
| **JWT Token Service** | `JwtTokenService` implementing `IJwtTokenService` using `System.IdentityModel.Tokens.Jwt` |
| **Serilog Configuration** | Logging pipeline setup with sinks (file, console, optional cloud sink) |

## 6.5 API (Presentation) Layer

**Project:** `AlNady.API`

The API layer is the entry point of the backend application. It is the only layer that references ASP.NET Core hosting directly.

| Component | Description |
|---|---|
| **Controllers** | `AuthController`, `TrainerController`, `AcademyController`, `EnrollmentController`, `RatingController`, `AdminController`, `ProgramController` — thin controllers that delegate to Application service interfaces |
| **Middleware** | `GlobalExceptionMiddleware` — catches unhandled exceptions and returns standardized error responses |
| **`Program.cs`** | Registers all services in the DI container; configures middleware pipeline; sets up Swagger, CORS, authentication, authorization, rate limiting |
| **Swagger Configuration** | Swashbuckle setup with JWT Bearer scheme support; accessible at `/swagger` in non-production |
| **Health Checks** | `/health` endpoint for liveness probes in deployment environments |
| **`appsettings.json`** | Configuration for database connection string, JWT settings, email settings, payment gateway keys, cloud storage settings |

Controllers are intentionally thin — they receive HTTP requests, call Application service methods, and return `ActionResult<ApiResponse<T>>` responses. No business logic resides in controllers.

## 6.6 Dependency Flow

The dependency flow strictly follows the Clean Architecture rule:

```
API Layer
  │ depends on ▼
Application Layer
  │ depends on ▼
Domain Layer
  ▲ implemented by
Infrastructure Layer
  │ registered in
API Layer (DI Registration in Program.cs)
```

This means:
- The **Domain layer** has no dependencies on any other layer.
- The **Application layer** depends only on Domain (through interfaces).
- The **Infrastructure layer** implements Domain interfaces using EF Core and external services.
- The **API layer** depends on Application (to call services) and Infrastructure (only for DI registration in `Program.cs`).
- **Dependency Inversion Principle** is the key mechanism: high-level modules (Application) define interfaces, and low-level modules (Infrastructure) implement them.

---

---

# 7. Backend Implementation Details

## 7.1 ASP.NET Core 9 Web API

The backend is built on **ASP.NET Core 9**, Microsoft's open-source, cross-platform web framework for building HTTP APIs.

Key characteristics of the AlNady API implementation:

- **Minimal Hosting Model**: Uses the streamlined `Program.cs` with `WebApplication.CreateBuilder()` and the middleware pipeline.
- **Controller-Based API**: Uses `[ApiController]` attribute for automatic model state validation, binding source inference, and ProblemDetails responses.
- **Stateless Architecture**: The API is completely stateless; all session state is encoded in the JWT token, enabling horizontal scalability without sticky sessions.
- **JSON Serialization**: Uses `System.Text.Json` (built-in) with camelCase property naming and `JsonStringEnumConverter` for enum serialization.
- **Health Checks**: Implements `IHealthCheck` endpoints using `Microsoft.Extensions.Diagnostics.HealthChecks` for monitoring.

## 7.2 Entity Framework Core and Database Access

### 7.2.1 Code First Approach

AlNady uses **Entity Framework Core 9** in **Code First** mode. This means:
- The SQL Server database schema is **derived from C# entity classes** — not the other way around.
- Database tables, columns, relationships, and constraints are defined through C# class definitions and optional Fluent API configurations in `ApplicationDbContext.OnModelCreating()`.
- Schema changes are managed through **EF Core Migrations** (`dotnet ef migrations add <Name>`, `dotnet ef database update`).

### 7.2.2 DbContext

`ApplicationDbContext` inherits from `IdentityDbContext<ApplicationUser>` (which itself inherits from `DbContext`). It exposes the following `DbSet<T>` properties:

```csharp
public DbSet<Trainer>           Trainers           { get; set; }
public DbSet<Academy>           Academies          { get; set; }
public DbSet<TrainingProgram>   TrainingPrograms   { get; set; }
public DbSet<EnrollmentRequest> EnrollmentRequests { get; set; }
public DbSet<Rating>            Ratings            { get; set; }
public DbSet<RatingAspect>      RatingAspects      { get; set; }
public DbSet<Certificate>       Certificates       { get; set; }
```

`ApplicationUser` extends `IdentityUser` with platform-specific fields (`FullName`, `Phone`, `ProfileImage`, `CreatedAt`). The base `IdentityDbContext` automatically manages the `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, and related Identity tables.

### 7.2.3 Migrations

EF Core Migrations track every incremental schema change as a versioned C# class. Key commands:

| Command | Purpose |
|---|---|
| `dotnet ef migrations add InitialCreate` | Creates initial migration from entity model |
| `dotnet ef database update` | Applies pending migrations to the database |
| `dotnet ef migrations remove` | Removes the last unapplied migration |

Migration files are committed to source control alongside application code, providing a complete, auditable history of schema evolution.

### 7.2.4 Entity Relationships (Fluent API)

Relationships between entities are configured in `OnModelCreating()` using EF Core's Fluent API:

```csharp
// One-to-One: ApplicationUser ↔ Trainer
modelBuilder.Entity<Trainer>()
    .HasOne(t => t.User)
    .WithOne()
    .HasForeignKey<Trainer>(t => t.UserId);

// One-to-Many: Trainer → TrainingPrograms
modelBuilder.Entity<TrainingProgram>()
    .HasOne(p => p.Trainer)
    .WithMany(t => t.Programs)
    .HasForeignKey(p => p.TrainerId)
    .OnDelete(DeleteBehavior.Restrict);

// One-to-Many: Rating → RatingAspects (composite PK)
modelBuilder.Entity<RatingAspect>()
    .HasKey(ra => new { ra.RatingId, ra.AspectName });
```

### 7.2.5 LINQ Queries

All data access uses **LINQ (Language Integrated Query)** through EF Core, which translates LINQ expressions to optimized SQL queries:

```csharp
// Example: Search trainers with filters
var trainers = await _context.Trainers
    .Include(t => t.User)
    .Where(t => t.IsVerifiedByAdmin
             && t.SpecializationSports.Contains(sport)
             && t.AverageRating >= minRating)
    .OrderByDescending(t => t.AverageRating)
    .Skip(pageNumber * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

## 7.3 Repository Pattern and Unit of Work

### 7.3.1 Generic Repository

The **Repository Pattern** abstracts data access behind interfaces, decoupling the Application layer from EF Core specifics:

```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

Specialized repositories extend this interface with domain-specific query methods:

```csharp
public interface ITrainerRepository : IGenericRepository<Trainer>
{
    Task<IReadOnlyList<Trainer>> SearchAsync(string? sport, decimal? minPrice,
                                             decimal? maxPrice, decimal? minRating);
    Task<Trainer?> GetByUserIdAsync(string userId);
}
```

### 7.3.2 Unit of Work

The **Unit of Work Pattern** ensures that multiple repository operations are committed atomically within a single database transaction:

```csharp
public interface IUnitOfWork : IDisposable
{
    ITrainerRepository    Trainers    { get; }
    IAcademyRepository    Academies   { get; }
    IEnrollmentRepository Enrollments { get; }
    IRatingRepository     Ratings     { get; }
    Task<int> SaveChangesAsync();
}
```

The concrete `UnitOfWork` implementation wraps a single `ApplicationDbContext` instance shared across all repositories during a request. Calling `SaveChangesAsync()` commits all pending changes atomically.

**Example use in Application Service:**
```csharp
public async Task<ApiResponse<bool>> EnrollUserAsync(EnrollmentRequestDto dto)
{
    // Validate, check blacklist, process payment...
    var enrollment = _mapper.Map<EnrollmentRequest>(dto);
    await _unitOfWork.Enrollments.AddAsync(enrollment);
    program.AvailableSlots--;                        // Update in same UoW context
    await _unitOfWork.SaveChangesAsync();             // Single atomic commit
    return ApiResponse<bool>.Success(true);
}
```

## 7.4 Authentication and Authorization

### 7.4.1 ASP.NET Identity

**ASP.NET Core Identity** provides a complete membership system for managing users, passwords, roles, and claims:

- `UserManager<ApplicationUser>` — creates users, validates passwords, manages roles
- `SignInManager<ApplicationUser>` — handles sign-in with password verification
- `RoleManager<IdentityRole>` — manages role creation and assignment
- **Password Hashing**: Uses PBKDF2 with HMACSHA256 (10,000 iterations minimum) — exceeds OWASP recommendations
- **Account Lockout**: Configurable lockout after N failed attempts prevents brute-force
- **Email Confirmation**: Built-in token provider for email confirmation flows

Configuration in `Program.cs`:
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit           = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength         = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### 7.4.2 JWT Authentication

Upon successful login, the system issues a **JSON Web Token (JWT)** containing:

| Claim | Value |
|---|---|
| `sub` | User ID (ASP.NET Identity GUID) |
| `email` | User's email address |
| `role` | User's assigned role(s) |
| `jti` | Unique token identifier (for revocation) |
| `exp` | Expiration timestamp (60 minutes from issuance) |
| `iss` | Token issuer (configured API domain) |
| `aud` | Token audience (configured frontend domain) |

The JWT is signed using **HMAC-SHA256** with a secret key stored in application secrets (never in source code). The frontend sends the JWT in the `Authorization: Bearer <token>` header with every protected API request.

### 7.4.3 Refresh Token

A **Refresh Token** is issued alongside the JWT:
- Stored as a hashed value in the SQL Server database with expiry timestamp (7 days)
- Sent to the client as an HTTP-only cookie or in the response body
- Used to obtain a new JWT without requiring the user to re-enter credentials
- **Token Rotation**: Each use of a refresh token issues a new refresh token and invalidates the old one (preventing replay attacks)

### 7.4.4 Role-Based Authorization

ASP.NET Core's authorization system enforces access based on user roles:

```csharp
[Authorize(Roles = "Admin")]
[HttpPost("admin/verify-profile/{trainerId}")]
public async Task<IActionResult> VerifyTrainerProfile(int trainerId) { ... }

[Authorize(Roles = "Player")]
[HttpPost("enroll")]
public async Task<IActionResult> EnrollInProgram([FromBody] EnrollmentRequestDto dto) { ... }
```

### 7.4.5 Policy-Based Authorization

For fine-grained authorization beyond simple role checks, ASP.NET Core **Policy-Based Authorization** is used:

```csharp
builder.Services.AddAuthorization(options => {
    options.AddPolicy("VerifiedTrainer", policy =>
        policy.RequireRole("Trainer")
              .RequireClaim("IsVerified", "true"));
});
```

Policies allow expressing complex authorization rules (e.g., "must be a verified trainer AND the owner of this resource") cleanly and testably.

## 7.5 Validation with FluentValidation

**FluentValidation** is used for all incoming request validation in the Application layer. It provides a fluent, strongly-typed API for defining validation rules:

```csharp
public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
        RuleFor(x => x.FullName)
            .NotEmpty().MaximumLength(100);
        RuleFor(x => x.Role)
            .IsInEnum();
    }
}
```

FluentValidation is integrated with ASP.NET Core's model validation pipeline via `AddFluentValidationAutoValidation()`, so validation errors are automatically returned as HTTP 400 responses with structured error messages before the controller action is even invoked.

**Benefits over Data Annotations:**
- Validation rules are defined separately from DTOs (separation of concerns)
- Complex cross-field validation rules are expressible cleanly
- Rules are fully unit-testable
- Validators are injected via the DI container, supporting dependencies if needed

## 7.6 Object Mapping with AutoMapper

**AutoMapper** is used to eliminate boilerplate mapping code between Domain entities and DTOs:

```csharp
public class TrainerMappingProfile : Profile
{
    public TrainerMappingProfile()
    {
        CreateMap<Trainer, TrainerProfileDto>()
            .ForMember(dest => dest.FullName,
                       opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Email,
                       opt => opt.MapFrom(src => src.User.Email));

        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.UserName,
                       opt => opt.MapFrom(src => src.Email));
    }
}
```

AutoMapper profiles are registered in the Application layer and injected via DI. The `IMapper` interface is injected into Application services. This ensures that entity-to-DTO transformations are centralized, consistent, and testable.

## 7.7 Dependency Injection

ASP.NET Core has a **built-in Dependency Injection (DI) container** that is used throughout the entire application. All services, repositories, validators, and cross-cutting concerns are registered in `Program.cs` and resolved automatically:

```csharp
// Infrastructure registrations
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();

// Application service registrations
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// External service registrations
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(TrainerMappingProfile).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
```

**Service Lifetimes Used:**

| Lifetime | Registration | Usage |
|---|---|---|
| `Scoped` | `AddScoped<>()` | One instance per HTTP request; used for `DbContext`, repositories, application services |
| `Singleton` | `AddSingleton<>()` | One instance for the application lifetime; used for caches, configuration objects |
| `Transient` | `AddTransient<>()` | New instance each time requested; used for lightweight stateless services |

This approach enforces the **Dependency Inversion Principle**: high-level modules depend on abstractions, not on concrete implementations.

## 7.8 Logging with Serilog

**Serilog** is used as the structured logging library throughout the backend. Unlike traditional string-based logging, Serilog captures log events as structured data objects, enabling powerful querying and analysis.

Configuration in `Program.cs`:
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File("logs/alnady-.log",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();
```

**What is logged:**
- All HTTP requests and responses (via Serilog's `RequestLogging` middleware)
- Authentication events (login, logout, failed attempts, token issuance)
- Authorization failures (unauthorized access attempts)
- Admin actions (approvals, rejections, blacklist changes)
- Financial transaction events (payment initiation, confirmation, failure, refund)
- Application errors and unhandled exceptions (with full stack trace)
- Rating and enrollment lifecycle events

Serilog uses **structured message templates** to associate searchable properties with log events:
```csharp
_logger.LogInformation("User {UserId} enrolled in Program {ProgramId} at {EnrollmentTime}",
    userId, programId, DateTime.UtcNow);
```

## 7.9 API Documentation with Swagger / OpenAPI

**Swashbuckle.AspNetCore** generates an **OpenAPI 3.0** specification and serves an interactive **Swagger UI** at `/swagger`. This provides:

- Complete documentation of all API endpoints, request/response schemas
- Interactive testing interface for developers and testers
- JWT Bearer token authentication support in the Swagger UI
- Grouped endpoints by controller/feature area

Configuration:
```csharp
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo {
        Title   = "AlNady API",
        Version = "v1",
        Description = "AlNady Sports Training Platform REST API"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Type   = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    // Include XML comments from controllers and DTOs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});
```

Swagger UI is only enabled in Development and Staging environments; it is disabled in Production to minimize attack surface.

## 7.10 Error Handling and Global Exception Middleware

A **Global Exception Handling Middleware** intercepts all unhandled exceptions and returns standardized error responses, preventing stack traces and internal details from leaking to clients:

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate  _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await WriteErrorResponse(context, ex.Errors);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await WriteErrorResponse(context, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = 401;
            await WriteErrorResponse(context, "Unauthorized.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await WriteErrorResponse(context, "An internal server error occurred.");
        }
    }
}
```

### 7.10.1 Standardized API Response

All API responses use a consistent `ApiResponse<T>` wrapper:

```csharp
public class ApiResponse<T>
{
    public bool   IsSuccess { get; set; }
    public string Message   { get; set; }
    public T?     Data      { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Success")
        => new() { IsSuccess = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
        => new() { IsSuccess = false, Message = message, Errors = errors };
}
```

**Standard HTTP Status Codes used:**

| Code | Meaning | When Used |
|---|---|---|
| 200 OK | Success | Successful GET, PUT, PATCH |
| 201 Created | Resource created | Successful POST (user registration, enrollment) |
| 204 No Content | Success, no body | Successful DELETE |
| 400 Bad Request | Validation error | FluentValidation failures, invalid input |
| 401 Unauthorized | Authentication required | Missing or invalid JWT |
| 403 Forbidden | Authorization denied | Valid JWT but insufficient role/policy |
| 404 Not Found | Resource not found | Entity not found in database |
| 409 Conflict | State conflict | Duplicate email, duplicate enrollment |
| 422 Unprocessable | Business rule violation | Blacklisted user, program capacity full |
| 500 Internal Server Error | Server-side error | Unhandled exceptions |

## 7.11 API Design Principles

The AlNady backend follows **RESTful API** design principles:

### 7.11.1 Resource-Based URLs

Endpoints are named after resources (nouns), not actions (verbs):

| Method | Endpoint | Action |
|---|---|---|
| `GET` | `/api/trainers` | List all verified trainers |
| `GET` | `/api/trainers/{id}` | Get trainer by ID |
| `POST` | `/api/trainers` | Create trainer profile |
| `PUT` | `/api/trainers/{id}` | Full update of trainer profile |
| `PATCH` | `/api/trainers/{id}` | Partial update of trainer profile |
| `DELETE` | `/api/trainers/{id}` | Delete trainer profile (Admin) |
| `GET` | `/api/programs` | List training programs |
| `POST` | `/api/enrollments` | Create enrollment request |
| `GET` | `/api/enrollments/{id}` | Get enrollment by ID |
| `POST` | `/api/auth/register` | Register new user |
| `POST` | `/api/auth/login` | Authenticate and get JWT |
| `POST` | `/api/auth/refresh-token` | Refresh JWT using refresh token |
| `POST` | `/api/auth/forgot-password` | Initiate password reset |
| `POST` | `/api/ratings` | Submit a rating |
| `GET` | `/api/admin/pending-profiles` | Get profiles pending approval |
| `POST` | `/api/admin/approve/{id}` | Approve a trainer/academy profile |

### 7.11.2 HTTP Methods

| Method | Semantics | Idempotent | Safe |
|---|---|---|---|
| `GET` | Retrieve resource(s) | Yes | Yes |
| `POST` | Create resource or trigger action | No | No |
| `PUT` | Replace resource entirely | Yes | No |
| `PATCH` | Partial resource update | No | No |
| `DELETE` | Remove resource | Yes | No |

### 7.11.3 Versioning

API versioning is handled via URL path prefix (`/api/v1/...`) to allow future breaking changes without disrupting existing clients.

## 7.12 Coding Standards and Best Practices

The AlNady codebase adheres to the following standards:

| Standard | Application |
|---|---|
| **SOLID Principles** | Single Responsibility (each class has one reason to change), Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion |
| **Clean Code** | Meaningful names, small functions, no magic numbers, self-documenting code, minimal comments explaining *why* not *what* |
| **Separation of Concerns** | Controllers handle HTTP, Application services handle business logic, Repositories handle data access — no mixing |
| **DRY (Don't Repeat Yourself)** | Shared logic extracted to base classes or utility methods; AutoMapper eliminates mapping duplication |
| **Async/Await** | All I/O-bound operations (database, HTTP, file) use `async`/`await` throughout the call stack to avoid thread blocking |
| **Nullable Reference Types** | Enabled project-wide (`<Nullable>enable</Nullable>`) to prevent null reference exceptions at compile time |
| **C# Naming Conventions** | PascalCase for classes, methods, and properties; camelCase for local variables; `I`-prefix for interfaces |
| **Unit Testing** | xUnit as the test framework, Moq for mocking, testing concentrated on Application layer use cases |

---

---

# 8. Database Design

## 8.1 Database Engine

AlNady uses **Microsoft SQL Server** (SQL Server 2022 or Azure SQL Database) as its relational database management system. The schema is managed entirely through **Entity Framework Core Code First Migrations**, ensuring the schema is always in sync with the C# entity model and version-controlled alongside application code.

**Connection string configuration** is stored in `appsettings.json` (without secrets) and overridden per-environment via environment variables or Azure Key Vault:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=AlNadyDb;..."
  }
}
```

## 8.2 Database Schema

The following tables are generated from EF Core entity definitions. Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`) are managed automatically by ASP.NET Identity.

### ApplicationUser (AspNetUsers extended)

| Column | Type | Constraints | Description |
|---|---|---|---|
| `Id` | `nvarchar(450)` | PK (GUID) | ASP.NET Identity default |
| `FullName` | `nvarchar(200)` | NOT NULL | User's full display name |
| `Email` | `nvarchar(256)` | UNIQUE, NOT NULL | Email address |
| `PhoneNumber` | `nvarchar(50)` | NULL | Phone number |
| `ProfileImage` | `nvarchar(500)` | NULL | URL to profile image in cloud storage |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | Account creation timestamp |
| *(ASP.NET Identity columns)* | — | — | Password hash, lockout, email confirmed, etc. |

### Trainer

| Column | Type | Constraints | Description |
|---|---|---|---|
| `TrainerId` | `int` | PK, IDENTITY | Surrogate primary key |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id, UNIQUE | One-to-one with ApplicationUser |
| `About` | `nvarchar(max)` | NULL | Trainer biography |
| `SpecializationSports` | `nvarchar(500)` | NOT NULL | Comma-separated or JSON sports list |
| `IsVerifiedByAdmin` | `bit` | NOT NULL, DEFAULT 0 | Admin approval status |
| `AverageRating` | `decimal(3,2)` | NOT NULL, DEFAULT 0 | Calculated average rating |

### Academy

| Column | Type | Constraints | Description |
|---|---|---|---|
| `AcademyId` | `int` | PK, IDENTITY | Surrogate primary key |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id, UNIQUE | One-to-one with ApplicationUser |
| `About` | `nvarchar(max)` | NULL | Academy description |
| `SpecializationSports` | `nvarchar(500)` | NOT NULL | Sports specializations |
| `IsVerified` | `bit` | NOT NULL, DEFAULT 0 | Admin verification flag |
| `AverageRating` | `decimal(3,2)` | NOT NULL, DEFAULT 0 | Calculated average rating |
| `Location` | `nvarchar(500)` | NULL | Physical address / coordinates |

### TrainingProgram

| Column | Type | Constraints | Description |
|---|---|---|---|
| `ProgramId` | `int` | PK, IDENTITY | Surrogate primary key |
| `TrainerId` | `int` | FK → Trainer.TrainerId, NULL | Offering trainer (nullable if academy program) |
| `AcademyId` | `int` | FK → Academy.AcademyId, NULL | Offering academy (nullable if trainer program) |
| `ProgramDate` | `date` | NOT NULL | Date of the training program |
| `ProgramTime` | `time` | NOT NULL | Start time of the program |
| `Price` | `decimal(10,2)` | NOT NULL | Enrollment price |
| `AvailableSlots` | `int` | NOT NULL | Remaining enrollment capacity |
| `Status` | `nvarchar(20)` | NOT NULL | Enum: `Available`, `Filled`, `Cancelled` |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | Record creation timestamp |

### EnrollmentRequest

| Column | Type | Constraints | Description |
|---|---|---|---|
| `RequestId` | `int` | PK, IDENTITY | Surrogate primary key |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id | Enrolling player |
| `ProgramId` | `int` | FK → TrainingProgram.ProgramId | Enrolled program |
| `Status` | `nvarchar(30)` | NOT NULL | Enum: `WaitingForPayment`, `Approved`, `Rejected` |
| `RequestDetails` | `nvarchar(max)` | NULL | Additional form data |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | Request timestamp |
| `ResponseDate` | `datetime2` | NULL | Date of approval/rejection |

### Rating

| Column | Type | Constraints | Description |
|---|---|---|---|
| `RatingId` | `int` | PK, IDENTITY | Surrogate primary key |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id | Rating author |
| `ProgramId` | `int` | FK → TrainingProgram.ProgramId | Rated program |
| `RatingValue` | `int` | NOT NULL, CHECK (1–5) | Star rating |
| `Comment` | `nvarchar(max)` | NULL | Optional text review |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | Submission timestamp |

### RatingAspect

| Column | Type | Constraints | Description |
|---|---|---|---|
| `RatingId` | `int` | PK (composite), FK → Rating.RatingId | Parent rating reference |
| `AspectName` | `nvarchar(100)` | PK (composite) | Aspect name (e.g., "Coaching Quality") |
| `RatingValue` | `int` | NOT NULL, CHECK (1–5) | Aspect star rating |

### Certificate

| Column | Type | Constraints | Description |
|---|---|---|---|
| `CertificateId` | `int` | PK, IDENTITY | Surrogate primary key |
| `TrainerId` | `int` | FK → Trainer.TrainerId, NULL | Owning trainer (nullable for academy certs) |
| `AcademyId` | `int` | FK → Academy.AcademyId, NULL | Owning academy (nullable for trainer certs) |
| `CertificateName` | `nvarchar(200)` | NOT NULL | Certificate title |
| `IsVerifiedByAdmin` | `bit` | NOT NULL, DEFAULT 0 | Admin verification status |
| `AddedDate` | `date` | NOT NULL | Date certificate was uploaded |
| `CertificateFileUrl` | `nvarchar(500)` | NOT NULL | URL to file in cloud storage |

## 8.3 Entity Relationships

The following Mermaid ERD describes the entity relationships as implemented in the SQL Server schema:

```
erDiagram

    USER {
        nvarchar(450) Id PK
        nvarchar(200) FullName
        nvarchar(256) Email
        nvarchar(50)  PhoneNumber
        nvarchar(500) ProfileImage
        datetime2     CreatedAt
    }

    TRAINER {
        int           TrainerId PK
        nvarchar(450) UserId    FK
        nvarchar(max) About
        nvarchar(500) SpecializationSports
        bit           IsVerifiedByAdmin
        decimal(3,2)  AverageRating
    }

    ACADEMY {
        int           AcademyId PK
        nvarchar(450) UserId    FK
        nvarchar(max) About
        nvarchar(500) SpecializationSports
        bit           IsVerified
        decimal(3,2)  AverageRating
        nvarchar(500) Location
    }

    TRAINING_PROGRAM {
        int           ProgramId  PK
        int           TrainerId  FK
        int           AcademyId  FK
        date          ProgramDate
        time          ProgramTime
        decimal(10,2) Price
        int           AvailableSlots
        nvarchar(20)  Status
        datetime2     CreatedAt
    }

    ENROLLMENT_REQUEST {
        int           RequestId      PK
        nvarchar(450) UserId         FK
        int           ProgramId      FK
        nvarchar(30)  Status
        nvarchar(max) RequestDetails
        datetime2     CreatedAt
        datetime2     ResponseDate
    }

    RATING {
        int           RatingId    PK
        nvarchar(450) UserId      FK
        int           ProgramId   FK
        int           RatingValue
        nvarchar(max) Comment
        datetime2     CreatedAt
    }

    RATING_ASPECT {
        int          RatingId    FK-PK
        nvarchar(100) AspectName  PK
        int          RatingValue
    }

    CERTIFICATE {
        int          CertificateId    PK
        int          TrainerId        FK
        int          AcademyId        FK
        nvarchar(200) CertificateName
        bit          IsVerifiedByAdmin
        date         AddedDate
        nvarchar(500) CertificateFileUrl
    }

    USER ||--o| TRAINER          : "has profile"
    USER ||--o| ACADEMY          : "has profile"
    USER ||--o{ ENROLLMENT_REQUEST : "submits"
    USER ||--o{ RATING           : "gives"
    TRAINER ||--o{ TRAINING_PROGRAM : "offers"
    ACADEMY  ||--o{ TRAINING_PROGRAM : "offers"
    TRAINING_PROGRAM ||--o{ ENROLLMENT_REQUEST : "receives"
    TRAINING_PROGRAM ||--o{ RATING : "receives"
    RATING ||--o{ RATING_ASPECT  : "has aspects"
    TRAINER ||--o{ CERTIFICATE   : "holds"
    ACADEMY  ||--o{ CERTIFICATE  : "holds"
```

---

---

# 9. Frontend Architecture

## 9.1 Technology Stack

The AlNady frontend is a **Single-Page Application (SPA)** built with the following technologies:

| Technology | Version | Purpose |
|---|---|---|
| **React** | 18 | Component-based UI framework |
| **TypeScript** | 5.x | Static typing for JavaScript |
| **Vite** | 5.x | Ultra-fast build tool and development server |
| **Tailwind CSS** | 3.x | Utility-first CSS framework for styling |
| **React Router** | 6.x | Client-side routing and navigation |
| **Axios** | 1.x | Promise-based HTTP client for API calls |

## 9.2 Application Structure

The frontend communicates exclusively with the AlNady ASP.NET Core API via Axios. Key design decisions:

- **Axios Interceptors**: Automatically attach `Authorization: Bearer <JWT>` header to all protected requests; intercept 401 responses to trigger token refresh flow.
- **React Router**: Protected routes use a `PrivateRoute` guard component that checks JWT validity and user role before rendering.
- **TypeScript Interfaces**: All API request and response shapes are typed using TypeScript interfaces that mirror the backend's DTO definitions, ensuring compile-time safety.
- **State Management**: React Context API or lightweight state management (Zustand) for authentication state (current user, JWT). Component-level `useState`/`useReducer` for local UI state.
- **Environment Variables**: API base URL and other configuration are stored in `.env` files and injected by Vite at build time (`import.meta.env.VITE_API_URL`).
- **Arabic/RTL Support**: Tailwind CSS's `dir` attribute and RTL utilities provide full right-to-left layout support for Arabic language mode.

---

---

# 10. Deployment

## 10.1 Target Environments

| Component | Environment | Platform |
|---|---|---|
| **Backend API** | Production | Azure App Service (or Render) |
| **Database** | Production | Azure SQL Database |
| **Frontend** | Production | Vercel |
| **File Storage** | Production | Azure Blob Storage |
| **Development** | Local | `dotnet run` + `npm run dev` |

## 10.2 Backend Deployment

### 10.2.1 Azure App Service

The ASP.NET Core API is deployed to **Azure App Service**:

1. The project is published as a **self-contained .NET 9 deployment** or a **framework-dependent deployment** (depending on the App Service runtime configuration).
2. The **CI/CD pipeline** (GitHub Actions or Azure DevOps) builds, tests, and deploys the application automatically on push to the `main` branch.
3. **Connection strings**, JWT secret keys, email API keys, and payment gateway keys are stored in **Azure Key Vault** and referenced via **Azure App Service managed identity** — never stored in `appsettings.json` in production.
4. **Application settings** in Azure App Service override `appsettings.json` values at runtime via environment variables.
5. **Health check endpoint** (`/health`) is configured as the App Service health check URL.

### 10.2.2 Render Alternative

If deployed on **Render**:
- The ASP.NET Core app is containerized using a **Dockerfile** (multi-stage build: build image with .NET SDK, runtime image with .NET Runtime).
- Environment variables are configured in the Render dashboard.
- Database connection string points to the Azure SQL Database endpoint.

### 10.2.3 EF Core Migrations in Production

Database migrations are applied automatically on application startup using:
```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync(); // Applies any pending migrations
```

Alternatively, migrations are applied via the CI/CD pipeline before deployment using `dotnet ef database update`.

## 10.3 Database Deployment

**Azure SQL Database** is used for production data storage:

- **Service Tier**: General Purpose or Business Critical depending on load requirements
- **Geo-Redundancy**: Enabled for disaster recovery
- **Automated Backups**: Azure SQL provides automatic backups (point-in-time restore up to 35 days)
- **Firewall Rules**: Only Azure App Service's outbound IPs are allowed; no public access from arbitrary IPs
- **Encryption**: Transparent Data Encryption (TDE) is enabled by default on Azure SQL Database
- **Connection Pooling**: Managed by EF Core's internal connection pool; `DbContext` is registered as `Scoped` to ensure one instance per HTTP request

## 10.4 Frontend Deployment

The React application is deployed to **Vercel**:

1. Vercel automatically detects the Vite project and configures the build command (`npm run build`) and output directory (`dist`).
2. The **API base URL** is configured as a Vercel environment variable (`VITE_API_URL=https://api.alnady.com`).
3. React Router's client-side routing requires Vercel's `vercel.json` rewrite rules to serve `index.html` for all non-asset routes.
4. **CORS** must be configured in the ASP.NET Core backend to allow requests from the Vercel deployment domain.

## 10.5 Production Considerations

| Concern | Approach |
|---|---|
| **HTTPS Enforcement** | ASP.NET Core `UseHttpsRedirection()` middleware; Azure App Service enforces HTTPS by default |
| **Environment-Based Configuration** | `appsettings.Production.json` + Azure App Service environment variables override development defaults |
| **Secrets Management** | Azure Key Vault for all secrets; no secrets in source code or `appsettings.json` |
| **Logging** | Serilog writes to Azure Application Insights sink in production for centralized log aggregation |
| **Monitoring** | Azure Application Insights for request tracking, performance monitoring, and error alerting |
| **Scaling** | Azure App Service auto-scaling based on CPU/memory thresholds; stateless JWT-based auth enables scaling without session affinity |
| **Rate Limiting** | Configured via ASP.NET Core rate limiting middleware; aggressive limits on auth endpoints |
| **CORS** | Configured to allow only the Vercel frontend domain and any admin client origins |
| **Docker Support** | A `Dockerfile` is maintained for local development parity and alternative deployment targets |

---

---

# 11. Other Requirements

| ID | Requirement |
|---|---|
| REQ-1201 | The system SHALL support both Arabic (RTL layout) and English languages; the language preference shall be stored in user profile settings. |
| REQ-1202 | The system SHALL comply with Egyptian data protection and privacy regulations applicable to web platforms. |
| REQ-1203 | The system SHALL provide accessibility features compliant with WCAG 2.1 AA guidelines (keyboard navigation, ARIA labels, sufficient color contrast). |
| REQ-1204 | The system SHALL maintain audit logs and financial transaction records for a minimum of 7 years to support audit requirements. |
| REQ-1205 | The system SHALL provide comprehensive API documentation via Swagger/OpenAPI accessible at the `/swagger` endpoint in non-production environments. |
| REQ-1206 | The system SHALL implement structured logging via Serilog for all application events, errors, and security-relevant activities. |
| REQ-1207 | The system SHALL support containerized deployment via Docker for development environment parity and CI/CD pipelines. |
| REQ-1208 | The system SHALL expose health check endpoints (`/health`, `/health/ready`, `/health/live`) for integration with deployment platform health monitoring. |
| REQ-1209 | The system SHALL maintain zero plaintext secrets in source code; all credentials, API keys, and connection strings SHALL be managed via Azure Key Vault or equivalent secrets management. |

---

---

# Appendix A: Glossary

| Term | Definition |
|---|---|
| **AlNady** | The sports training platform described in this document |
| **Player / User** | A sports enthusiast who uses the platform to discover and enroll in training programs |
| **Trainer** | An individual certified sports professional who offers training programs through the platform |
| **Academy** | A sports institution (club, gym, or academy) that offers training programs, potentially with multiple trainers |
| **Admin** | A system administrator with full oversight, moderation, and governance privileges |
| **Guest** | An unauthenticated visitor with read-only access to search and public profiles |
| **Program** | A training session or course offering created by a trainer or academy |
| **Enrollment** | The process of a Player registering and paying for a specific training program |
| **JWT (JSON Web Token)** | A signed, self-contained token used for stateless authentication between the React frontend and ASP.NET Core backend |
| **Refresh Token** | A long-lived token stored securely that allows a client to obtain a new JWT without re-entering credentials |
| **ASP.NET Core** | Microsoft's open-source, cross-platform framework for building web APIs, used as the backend of AlNady |
| **Clean Architecture** | A layered architectural pattern (Domain / Application / Infrastructure / API) enforcing strict dependency rules |
| **Entity Framework Core (EF Core)** | Microsoft's object-relational mapper (ORM) for .NET, used to interact with SQL Server using C# LINQ queries |
| **Code First** | An EF Core approach where the database schema is derived from C# entity class definitions |
| **Migration** | An EF Core incremental schema change file that tracks database evolution alongside application code |
| **Repository Pattern** | A data access abstraction that decouples Application logic from EF Core and SQL Server specifics |
| **Unit of Work** | A pattern that coordinates multiple repository operations within a single atomic database transaction |
| **ASP.NET Identity** | A membership system built into ASP.NET Core providing user management, password hashing, and role management |
| **FluentValidation** | A .NET library for defining strongly-typed validation rules for request DTOs |
| **AutoMapper** | A .NET library for automating object-to-object mapping between Domain entities and DTOs |
| **Serilog** | A structured logging library for .NET used throughout the AlNady backend |
| **Swagger / OpenAPI** | A specification and toolset for documenting and interacting with the AlNady RESTful API |
| **QR Code** | Quick Response code embedded in Egyptian National IDs, scanned during enrollment for identity verification |
| **Blacklist** | A list of users restricted from enrollment due to policy violations (e.g., excessive cancellations) |
| **Rating Aspect** | A specific dimension of a program's rating (e.g., "Coaching Quality", "Facilities", "Value for Money") |
| **Verification Code** | A time-limited alphanumeric code delivered by email for account verification or password reset |
| **Capacity / Available Slots** | The maximum number of participants a program can accommodate; decremented atomically on successful enrollment |
| **Cancellation Policy** | The refund rules applied when a player or trainer/academy cancels an enrolled program |
| **DI (Dependency Injection)** | An inversion-of-control technique where object dependencies are provided by an external container (ASP.NET Core's built-in DI) |
| **LINQ** | Language Integrated Query — C# syntax for querying data sources, translated by EF Core to SQL |
| **DTO (Data Transfer Object)** | A plain object used to transfer data between the API layer and client, decoupled from Domain entities |
| **CORS** | Cross-Origin Resource Sharing — HTTP mechanism allowing the React frontend (Vercel domain) to call the ASP.NET Core API |
| **RBAC** | Role-Based Access Control — authorization model where access is determined by the user's assigned role |
| **TDE** | Transparent Data Encryption — SQL Server feature encrypting the database file at rest |
| **RPO / RTO** | Recovery Point Objective / Recovery Time Objective — disaster recovery metrics defining maximum data loss and downtime tolerance |

---

---

# Appendix B: Analysis Models

The following analysis models accompany this SRS document:

### B.1 Entity Relationship Diagram (ERD)

The complete ERD is provided in `ERD.txt` using Mermaid diagram syntax. It depicts the following entities and their SQL Server implementation relationships:

- **USER** (extends ASP.NET Identity `AspNetUsers`) — central identity entity
- **TRAINER** — one-to-one with USER; holds trainer-specific profile attributes
- **ACADEMY** — one-to-one with USER; holds academy-specific profile attributes
- **TRAINING_PROGRAM** — many-to-one with TRAINER or ACADEMY; represents program offerings
- **ENROLLMENT_REQUEST** — many-to-one with USER and TRAINING_PROGRAM; tracks enrollment lifecycle
- **RATING** — many-to-one with USER and TRAINING_PROGRAM; holds review data
- **RATING_ASPECT** — many-to-one with RATING; composite primary key (RatingId + AspectName)
- **CERTIFICATE** — many-to-one with TRAINER or ACADEMY; stores credential documents

All foreign key relationships are enforced at the SQL Server level via EF Core's `HasForeignKey()` and `OnDelete()` Fluent API configurations. Referential integrity is maintained by the relational database engine.

### B.2 Data Flow Diagrams (DFD)

The complete DFD is provided in `Last DFD.txt` using Mermaid graph syntax. It is a Level-0 DFD showing:

**External Entities**: Player/User, Trainer, Academy, Admin, Email Service, Payment Gateway, Location Service

**System Processes**:
- P1: User Registration (→ ASP.NET Identity `UserManager`, Serilog event logging)
- P2: User Login (→ JWT generation via `JwtTokenService`)
- P3: Forgot Password (→ ASP.NET Identity token, email service)
- P4: Search Training Options (→ EF Core LINQ query on `TrainingPrograms`, `Trainers`, `Academies`)
- P6: Profile Management (→ EF Core update via `UnitOfWork`, cloud file storage)
- P7: Enrollment Request Handling (→ Payment gateway, EF Core transaction)
- P8: Rating & Feedback (→ EF Core `Rating` and `RatingAspect` insert, average recalculation)
- P9: Session Cancellation (→ Payment gateway refund, EF Core status update)
- P10: Authentication & Authorization (→ JWT middleware, ASP.NET Core authorization policies)
- P11: Admin Validation (→ Profile status update via `UnitOfWork`, email notification)
- P12: Recommendation Engine (→ EF Core query on enrollment history, ratings)

**Data Stores** (all backed by SQL Server tables managed by EF Core):
- D1: Users (`AspNetUsers`)
- D2: Profiles (`Trainers`, `Academies`)
- D3: Bookings/Sessions (`TrainingPrograms`)
- D4: Enrollments (`EnrollmentRequests`)
- D5: Ratings (`Ratings`, `RatingAspects`)
- D6: User Sessions (Refresh token table in SQL Server)
- D7: Blacklist (Blacklist table in SQL Server)
- D8: Documents (`Certificates`, file URLs pointing to Azure Blob Storage)
- D9: Logs (Serilog structured log files / Application Insights)
- D10: Recommendations (in-memory or cached recommendations table)
- D11: Training Sessions (subset view of `TrainingPrograms`)
- D12: Training Programs (`TrainingPrograms`)

### B.3 Use Case Diagrams

Detailed use cases for all major system functionalities are documented in `usecases_markdown.md`. The use cases cover:

1. User Registration
2. User Login
3. Forgot Password
4. Search Training Options
5. Trainer/Academy Profile Management
6. Program Enrollment
7. Rating and Feedback
8. Program Cancellation
9. Authorization

All use cases remain unchanged from their original specification; the underlying implementation mechanisms now use ASP.NET Core controllers, ASP.NET Identity, EF Core repositories, and the Unit of Work pattern instead of the previous Node.js/Express/MongoDB stack.

### B.4 Functional Decomposition

The 11 core system functions with input/processing/output specifications are documented in `Functional Requirements.md`. The processing mechanisms for each function now map to ASP.NET Core equivalents:

| Old Technology Reference | ASP.NET Core Equivalent |
|---|---|
| Express.js route handler | ASP.NET Core `[ApiController]` action method |
| Mongoose model / schema | EF Core entity class + `DbSet<T>` |
| MongoDB query | EF Core LINQ query via `ApplicationDbContext` |
| bcrypt password hash | ASP.NET Identity `PasswordHasher<TUser>` |
| JWT library (jsonwebtoken) | `System.IdentityModel.Tokens.Jwt` + `JwtSecurityTokenHandler` |
| Express middleware | ASP.NET Core middleware (`IMiddleware`) |
| Mongoose session/transaction | EF Core `UnitOfWork.SaveChangesAsync()` |

---

---

# Appendix C: To Be Determined List

| ID | Item | Notes |
|---|---|---|
| TBD-001 | Specific payment gateway provider | Tap vs. alternative; affects `IPaymentService` implementation only (clean architecture isolates this change) |
| TBD-002 | Email service provider | SendGrid vs. SMTP vs. alternative; affects `IEmailService` implementation only |
| TBD-003 | Cloud file storage provider | Azure Blob Storage vs. AWS S3; affects `IFileStorageService` implementation only |
| TBD-004 | Arabic translation / localization approach | Static JSON translation files vs. i18next integration |
| TBD-005 | Third-party security audit / penetration testing provider | For pre-production certification |
| TBD-006 | Specific monitoring and alerting tooling | Azure Application Insights (preferred) vs. Datadog vs. self-hosted Grafana/Prometheus |
| TBD-007 | Load testing tool selection | k6 vs. Apache JMeter for performance benchmarking |
| TBD-008 | CDN provider for static frontend assets | Vercel Edge Network (built-in) vs. Azure CDN |
| TBD-009 | Legal / compliance counsel for PDPL verification | Egyptian Personal Data Protection Law compliance |
| TBD-010 | SMS gateway provider | For optional two-factor authentication enhancement |
| TBD-011 | Recommendation engine algorithm | Collaborative filtering vs. content-based filtering vs. hybrid approach |
| TBD-012 | Real-time notification mechanism | SignalR (ASP.NET Core WebSocket) vs. polling vs. push notifications |

---

*End of Document*

---

**Document Control**

| Field | Value |
|---|---|
| Document Title | Software Requirements Specification – AlNady Sports Training Platform |
| Version | 2.0 |
| Date | June 2026 |
| Prepared By | AlNady Development Team |
| Architecture | ASP.NET Core 9 / Clean Architecture / SQL Server / React + TypeScript |
| Status | Approved for Graduation Project Submission |
