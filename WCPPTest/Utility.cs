namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Utility")]
    public partial class Utility
    {
        public int Id { get; set; }

        [StringLength(10)]
        public string LastUtilityCodePrinted { get; set; }
    }
}
