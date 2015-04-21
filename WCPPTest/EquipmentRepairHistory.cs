namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EquipmentRepairHistory")]
    public partial class EquipmentRepairHistory
    {
        [Key]
        public int EquipmentRepairId { get; set; }

        public int EquipmentId { get; set; }

        [Column(TypeName = "text")]
        public string MoveType { get; set; }

        [Column(TypeName = "text")]
        public string Inspector { get; set; }

        public DateTime MoveDate { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string Comments { get; set; }

        public virtual Equipment Equipment { get; set; }
    }
}
