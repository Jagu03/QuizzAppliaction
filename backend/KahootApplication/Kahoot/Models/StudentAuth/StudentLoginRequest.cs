using System.ComponentModel.DataAnnotations;

namespace Kahoot.Models.StudentAuth
{
    public class StudentLoginRequest
    {
        public string RollNo { get; set; }
        public string StudPassword { get; set; }

        [RegularExpression(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[012])/(19|20)\d\d$",
            ErrorMessage = "Date must be in DD/MM/YYYY format")]
        public string Dob { get; set; }

        [Range(0, 1, ErrorMessage = "LoginAs must be 0 (parent) or 1 (student)")]
        public byte LoginAs { get; set; } = 1; // Changed to byte

        public string IPAddr { get; set; }
        public string SessId { get; set; }
        public string Browser { get; set; }
        public string BVersion { get; set; }
        public long OtherLoginId { get; set; } // Changed to long
        public string DeviceId { get; set; }
    }
}
