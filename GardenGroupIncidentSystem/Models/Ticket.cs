namespace GardenGroupIncidentSystem.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Collections.Generic;
    public enum Status
    {
        Open,
        Closed,
        Resolved,
    }
    public class Ticket
    {
        // Example: "T0101"
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        // Example: "E0080"
        [BsonElement("EmployeeID")]
        public string EmployeeID { get; set; }
        [BsonElement("Subject")]
        public string Subject { get; set; }
        [BsonElement("Description")]
        public string Description { get; set; }

        // Example: "Access Request", "Hardware Request", etc.
        [BsonElement("type")]
        public string Type { get; set; }

        // Example: "High", "Medium", "Low"
        [BsonElement("priority")]
        public string Priority { get; set; }

        // ISO timestamp string or DateTime
        [BsonElement("DateTimeReport")]
        public DateTime DateTimeReport { get; set; }
        [BsonElement("deadline")]
        public DateTime Deadline { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [BsonElement("Status")]
        public Status TicketStatus { get; set; }

        // History of who worked on this ticket
        [BsonElement("workedBy")]
        public List<WorkedBy> WorkedBy { get; set; } = new();
    }

    // Represents one person or handover in the workflow
    public class WorkedBy
    {
        [BsonElement("employee")]
        public Employee Employee { get; set; }
        [BsonElement("handover")]
        public Handover Handover { get; set; }
        [BsonElement("timestamps")]
        public Timestamps Timestamps { get; set; }
        [BsonElement("active")]
        public bool Active { get; set; }
    }

    // Represents the employee reference

    // Represents the handover info
    public class Handover
    {
        [BsonElement("toEmployee")]
        public string ToEmployee { get; set; }
        [BsonElement("by")]
        public string By { get; set; }
        [BsonElement("why")]
        public string Why { get; set; }
    }

    // Represents time-related data for each work log
    public class Timestamps
    {
        [BsonElement("assignedAt")]
        public DateTime? AssignedAt { get; set; }
        [BsonElement("handedAt")]
        public DateTime? HandedAt { get; set; }
        [BsonElement("finishedAt")]
        public DateTime? FinishedAt { get; set; }
    }

}