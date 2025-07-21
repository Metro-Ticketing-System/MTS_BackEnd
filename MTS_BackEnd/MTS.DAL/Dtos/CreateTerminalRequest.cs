namespace MTS.DAL.Dtos
{
    public class CreateTerminalRequest
    {
        public Guid UserId { get; set; } // FK to User
        public string Name { get; set; }
        public string Location { get; set; }
    }
}
