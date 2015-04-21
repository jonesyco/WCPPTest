namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("InspectionCriteria")]
    public partial class InspectionCriteria
    {
        public InspectionCriteria()
        {
            InspectionResults = new HashSet<InspectionResult>();
        }

        public int InspectionCriteriaId { get; set; }

        public int EquipmentTypeId { get; set; }

        [Required]
        [StringLength(150)]
        public string InspectionCriteriaName { get; set; }

        public int InspectionCriteriaValueType { get; set; }

        public virtual EquipmentType EquipmentType { get; set; }

        public virtual ICollection<InspectionResult> InspectionResults { get; set; }
    }
}
