

namespace EMS.DAL.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; } //PK

       // ⭐⭐⭐ Soft Delete Columns ⭐⭐⭐
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public int CreatedBy { get; set; } //User Id
        public DateTime? CreatedOn { get; set; } 
        public int LastModifiedBy { get; set; } //User Id Modified
        public DateTime? LastModifiedOn { get; set; } // Automatic Calculated in (Configuration)
       
        
    }
}
