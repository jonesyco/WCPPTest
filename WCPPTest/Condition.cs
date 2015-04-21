namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Condition")]
    public partial class Condition
    {
        public Condition()
        {
            Equipments = new HashSet<Equipment>();
        }

        public int ConditionId { get; set; }

        [StringLength(50)]
        public string ConditionName { get; set; }

        [Column(TypeName = "text")]
        public string ConditionDescription { get; set; }

        public virtual ICollection<Equipment> Equipments { get; set; }
    }
}
