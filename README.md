RaceDay - Event Management System

System Overview
RaceDay is a web-based event management platform designed for South Africa's road running, walking, and cycling communities. It modernizes paper-based registration by enabling organizers to create and manage events, categories, and results, while participants can browse events, register, and track their performance history.

The system is built progressively across three parts:
Part 1: Planning documents (ERD, API endpoint plan, SQL database script). 
Part 2: RESTful Web API implementation (ASP.NET Core, EF Core, JWT Auth, xUnit tests, and CI/CD).
Part 3: Final containerised, cloud-aware deployment.

User Roles & System Permissions
Organiser
Create, update, and delete events across sports (Run, Walk, Cycle).
Define age and distance categories.
View all participant registrations for managed events.
Record and publish race results (finish times and positions).

Participant
Register for an account and log in via JWT authentication.
View and update personal profile details.
Browse upcoming events and register under specific categories.
View personal and public event results.


Part 1 Refinements & Architecture Improvements
Based on initial feedback, the system schema was updated prior to API implementation:
1.	Multi-Sport Event Support: Expanded event domain entities beyond running to support cycling, walking, and multi-discipline race events. 
2.	Unified User Entity: Consolidated separate user types into a single User entity referencing a Role entity via foreign key (RoleId), cleanly separating Organiser and Participant permissions. 
3.	Normalized Primary Keys: Assigned dedicated primary keys (PaymentId) across entities for database normalization. 
Part 2
•	Framework: ASP.NET Core 9.0 / .NET 10 Web API 
•	ORM: Entity Framework Core (Code-First Approach) 
•	Database: Microsoft SQL Server 
•	Authentication: JWT (JSON Web Tokens) with BCrypt Password Hashing 
•	Documentation: Swagger / OpenAPI UI 
•	Testing: xUnit Framework with EF Core InMemory Database 
•	CI/CD: GitHub Actions Automation Pipeline 
Unit Testing
The solution includes an automated xUnit test suite (RaceDay.Tests) covering:
•	User registration logic and data validation. 
•	Duplicate email conflict handling. 
•	Event retrieval and query verification. 
•	Role-based access restriction checks. 
All unit tests execute against an in-memory database instance to ensure rapid, isolated validation. 
Screenshot Of Successful Tests
 


CI/CD Workflow
Configured with GitHub Actions in .github/workflows/ci.yml. On every push and pull request, the workflow: 
1.	Restores project dependencies. 
2.	Compiles the ASP.NET Core solution in Release mode. 
3.	Executes all unit tests in RaceDay.Tests. 
Screenshot of successful build:
 
Video Walkthrough
•	Unlisted YouTube Link Part 1: https://youtu.be/9dEouBo3yCO
•	Unlisted YouTube Link Part 2: https://youtu.be/LwcqEs_oa0o 
•	Unlisted YouTube Link Part 3: https://youtu.be/W8M-1BxqCqc 
Walkthrough Topics Covered:
•	ERD architecture and design decisions.
•	ASP.NET Core project structure, Controllers, and DTOs.
•	Live endpoint demonstration and JWT authorization in Swagger UI. 
•	Unit test execution in Visual Studio Test Explorer.
•	GitHub Actions CI/CD pipeline validation.

