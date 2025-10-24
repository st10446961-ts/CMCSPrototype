using System.ComponentModel.DataAnnotations;

namespace CMCSPrototype.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; }

        [Required]
        [Display(Name = "Claim Month")]
        public string ClaimMonth { get; set; }

        [Range(1, 300, ErrorMessage = "Hours must be between 1 and 300.")]
        [Display(Name = "Hours Worked")]
        public double HoursWorked { get; set; }

        [Range(1, 2000, ErrorMessage = "Hourly rate must be between 1 and 2000.")]
        [Display(Name = "Hourly Rate (R)")]
        public double HourlyRate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        // --- File info ---
        [Display(Name = "Supporting Document")]
        public string? SupportingDocument { get; set; }

        [Display(Name = "Document Path")]
        public string? SupportingDocumentPath { get; set; }

        [Display(Name = "Total Amount (R)")]
        public double TotalAmount => HoursWorked * HourlyRate;

        [Display(Name = "Supporting Document")]
        public IFormFile UploadedFile { get; set; }
    }
}
