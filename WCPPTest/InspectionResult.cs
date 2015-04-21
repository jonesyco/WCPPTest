namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("InspectionResult")]
    public partial class InspectionResult
    {
        public int InspectionResultID { get; set; }

        public int InspectionId { get; set; }

        public int InspectionCriteriaID { get; set; }

        [StringLength(50)]
        public string InspectionCriteriaValue { get; set; }

        public virtual Inspection Inspection { get; set; }

        public virtual InspectionCriteria InspectionCriteria { get; set; }
    }
}
