namespace ASM_SIMS.Models
{
    public class AssignStudentsViewModel
    {
        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public List<StudentViewModel> Students { get; set; } = new List<StudentViewModel>();
    }
}