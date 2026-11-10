using System;
using System.Collections.Generic;

namespace RacingDays.API.DTOs
{
    // Auth & User DTOs
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Contact { get; set; } = string.Empty;
        public int RoleId { get; set; } = 2; // 1 = Organiser, 2 = Participant
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Event DTOs
    public class EventDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public int TotalEnrolments { get; set; }
    }

    public class CreateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
    }

    public class OrganiserDashboardDto
    {
        public int TotalEventsCreated { get; set; }
        public int TotalEnrolments { get; set; }
        public List<EventSummaryDto> Events { get; set; } = new();
    }

    public class EventSummaryDto
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalEnrolments { get; set; }
    }

    // Registration & Results DTOs
    public class RegisterForEventDto
    {
        public int CategoryId { get; set; }
    }

    public class RegistrationResponseDto
    {
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = "Confirmed";
    }

    public class CreateResultDto
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public TimeSpan Time { get; set; }
        public int Position { get; set; }
    }
}