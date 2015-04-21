namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EquipmentType")]
    public partial class EquipmentType
    {
        public EquipmentType()
        {
            Equipments = new HashSet<Equipment>();
            InspectionCriterias = new HashSet<InspectionCriteria>();
        }

        public int EquipmentTypeId { get; set; }

        public string EquipmentTypeDesc { get; set; }

        public short EquipmentMaintIntervalDays { get; set; }

        public int EquipmentTypeParentId { get; set; }

        public virtual ICollection<Equipment> Equipments { get; set; }

        public virtual ICollection<InspectionCriteria> InspectionCriterias { get; set; }
    }
}
