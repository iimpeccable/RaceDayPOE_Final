using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RacingDays.API.Models
{
    // Authorization roles (Organiser / Participant)
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Application user domain entity
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Contact { get; set; } = string.Empty;

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }

    // Race category classification (Distance or Age)
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    // Race event entity
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Distance { get; set; }

        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }

        public int OrganizerId { get; set; }
        [ForeignKey("OrganizerId")]
        public User? Organizer { get; set; }

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }

    // Participant enrolment mapping
    public class Registration
    {
        [Key]
        public int RegistrationId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public string Status { get; set; } = "Confirmed";

        public ICollection<Result> Results { get; set; } = new List<Result>();
    }

    // Official race result record linked directly to participant registration
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        public int RegistrationId { get; set; }
        [ForeignKey("RegistrationId")]
        public Registration? Registration { get; set; }

        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }
}