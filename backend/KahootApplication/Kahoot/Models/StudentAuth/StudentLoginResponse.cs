namespace Kahoot.Models.StudentAuth
{
    public class StudentLoginResponse
    {
        public long LoginId { get; set; }
        public string? LastLoginTime { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? RollNo { get; set; }
        public string? ImageName { get; set; }
        public string? RegisterNumber { get; set; }
        public string? ImpresCode { get; set; }
        public string? Quota { get; set; }
        public string? Community { get; set; }
        public string? StudType { get; set; }
        public string? FirstGrad { get; set; }
        public int HostelerId { get; set; }
        public string? Gender { get; set; }
        public string? ShortGender { get; set; }
        public byte StudStat { get; set; }
        public byte ReqHostel { get; set; }
        public byte ReqTransport { get; set; }
        public int InstId { get; set; }
        public string? SType { get; set; }
        public string? AdmnTypeShort { get; set; }
        public string? AdmnType { get; set; }
        public byte Icr { get; set; }
        public byte Rsl { get; set; }
        public byte LoginAs { get; set; }
        public string? Pic { get; set; }
        public int Result { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
    }
}
