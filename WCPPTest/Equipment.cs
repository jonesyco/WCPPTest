namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Equipment")]
    public partial class Equipment
    {
        public Equipment()
        {
            Equipment1 = new HashSet<Equipment>();
            EquipmentRepairHistories = new HashSet<EquipmentRepairHistory>();
            Inspections = new HashSet<Inspection>();
        }

        public int EquipmentId { get; set; }

        public int EquipmentTypeId { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string EquipmentDescription { get; set; }

        [StringLength(50)]
        public string EquipmentLength { get; set; }

        [StringLength(50)]
        public string EquipmentSize { get; set; }

        [StringLength(50)]
        public string Manufacturer { get; set; }

        [StringLength(50)]
        public string Serial { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime InitialInvDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NextInspectionDue { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? RemoveFromServiceDate { get; set; }

        public int? ParentId { get; set; }

        [Required]
        public string UtilityId { get; set; }

        public int Level { get; set; }

        public int Status { get; set; }

        public int ConditionId { get; set; }

        public virtual Condition Condition { get; set; }

        public virtual ICollection<Equipment> Equipment1 { get; set; }

        public virtual Equipment Equipment2 { get; set; }

        public virtual EquipmentType EquipmentType { get; set; }

        public virtual ICollection<EquipmentRepairHistory> EquipmentRepairHistories { get; set; }

        public virtual ICollection<Inspection> Inspections { get; set; }
    }
}
