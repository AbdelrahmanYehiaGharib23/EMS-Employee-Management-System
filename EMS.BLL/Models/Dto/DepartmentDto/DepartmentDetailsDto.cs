namespace EMS.BLL.Models.Dto.DepartmentDto;

public record DepartmentDetailsDto
{
    public int Id { get; init; }
    public int CreatedBy { get; init; }
    public DateTime? CreatedOn { get; init; }
    public DateOnly? DateOfCreation { get; init; }
    public string? Name { get; init; }
    public string? Code { get; init; }
    public string? Description { get; init; }
    public int LastModifiedBy { get; init; }
    public DateTime? LastModifiedOn { get; init; }
}
