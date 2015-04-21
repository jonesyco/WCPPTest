namespace WCPPTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ErrorLog")]
    public partial class ErrorLog
    {
        [Key]
        public int AppErrorID { get; set; }

        public DateTime ErrorDateTime { get; set; }

        [StringLength(250)]
        public string UserIdentity { get; set; }

        public string ErrorText { get; set; }
    }
}
