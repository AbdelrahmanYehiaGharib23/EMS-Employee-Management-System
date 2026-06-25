namespace EMS.DAL.Entities.DepartmentEntity
{
    public class Department:BaseEntity
    {
        public string Name { get; set; }= null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }

        public DateOnly? DateOfCreation { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();

    }
}
