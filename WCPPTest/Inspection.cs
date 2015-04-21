namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Inspection")]
    public partial class Inspection
    {
        public Inspection()
        {
            InspectionResults = new HashSet<InspectionResult>();
        }

        public int InspectionId { get; set; }

        public int EquipmentId { get; set; }

        public string Inspector { get; set; }

        public DateTime? InspectionDate { get; set; }

        public virtual Equipment Equipment { get; set; }

        public virtual ICollection<InspectionResult> InspectionResults { get; set; }
    }
}
