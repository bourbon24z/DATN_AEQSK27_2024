namespace DATN.Dto
{
    public class UserListDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
        public string PatientName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; }
    }
}