namespace WCPPTest
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SEMInspection : DbContext
    {
        public SEMInspection()
            : base("name=SEMInspection")
        {
        }

        public virtual DbSet<Condition> Conditions { get; set; }
        public virtual DbSet<Equipment> Equipments { get; set; }
        public virtual DbSet<EquipmentRepairHistory> EquipmentRepairHistories { get; set; }
        public virtual DbSet<EquipmentType> EquipmentTypes { get; set; }
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<Inspection> Inspections { get; set; }
        public virtual DbSet<InspectionCriteria> InspectionCriterias { get; set; }
        public virtual DbSet<InspectionResult> InspectionResults { get; set; }
        public virtual DbSet<Utility> Utilities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Condition>()
                .Property(e => e.ConditionDescription)
                .IsUnicode(false);

            modelBuilder.Entity<Condition>()
                .HasMany(e => e.Equipments)
                .WithRequired(e => e.Condition)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Equipment>()
                .Property(e => e.EquipmentDescription)
                .IsUnicode(false);

            modelBuilder.Entity<Equipment>()
                .HasMany(e => e.Equipment1)
                .WithOptional(e => e.Equipment2)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<Equipment>()
                .HasMany(e => e.EquipmentRepairHistories)
                .WithRequired(e => e.Equipment)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Equipment>()
                .HasMany(e => e.Inspections)
                .WithRequired(e => e.Equipment)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EquipmentRepairHistory>()
                .Property(e => e.MoveType)
                .IsUnicode(false);

            modelBuilder.Entity<EquipmentRepairHistory>()
                .Property(e => e.Inspector)
                .IsUnicode(false);

            modelBuilder.Entity<EquipmentRepairHistory>()
                .Property(e => e.Comments)
                .IsUnicode(false);

            modelBuilder.Entity<EquipmentType>()
                .HasMany(e => e.Equipments)
                .WithRequired(e => e.EquipmentType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Inspection>()
                .HasMany(e => e.InspectionResults)
                .WithRequired(e => e.Inspection)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InspectionCriteria>()
                .HasMany(e => e.InspectionResults)
                .WithRequired(e => e.InspectionCriteria)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Utility>()
                .Property(e => e.LastUtilityCodePrinted)
                .IsFixedLength();
        }
    }
}
