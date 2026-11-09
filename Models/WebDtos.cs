using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RacingDays.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(10, 120, ErrorMessage = "Please enter a valid age")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        public string Contact { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account type is required")]
        public int RoleId { get; set; } // 1 for Organiser, 2 for Participant
    }

    public class EventViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public int TotalEnrolments { get; set; }
    }

    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event title is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be greater than 0")]
        public decimal Distance { get; set; } = 10.0m;

        public string EventType { get; set; } = "Road Running";
        public int CategoryId { get; set; } = 1;
        public string? BannerImageUrl { get; set; }
    }

    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class OrganiserDashboardViewModel
    {
        public int TotalEventsCreated { get; set; }
        public int TotalEnrolments { get; set; }
        public List<EventViewModel> Events { get; set; } = new();
    }

    public class ParticipantRosterViewModel
    {
        public int RegistrationId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int ParticipantAge { get; set; }
        public string ParticipantContact { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class EventResultViewModel
    {
        public int ResultId { get; set; }
        public int RegistrationId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Position { get; set; }
        public TimeSpan FinishTime { get; set; }
    }

    public class SubmitResultViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int RegistrationId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required")]
        [Range(1, 10000, ErrorMessage = "Position must be 1 or greater")]
        public int Position { get; set; }

        [Required(ErrorMessage = "Finish time is required")]
        public string FinishTimeInput { get; set; } = "00:45:00";
    }

    public class RegistrationViewModel
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ParticipantResultViewModel
    {
        public int ResultId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Position { get; set; }
        public TimeSpan FinishTime { get; set; }
    }

    public class UserProfileViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Contact { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}