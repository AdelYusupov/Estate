using Microsoft.EntityFrameworkCore;
using Estate.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Estate.Models
{
    public class RealEstateContext : DbContext
    {
        public DbSet<District> Districts { get; set; }
        public DbSet<RealEstateType> RealEstateTypes { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<RealEstate> RealEstates { get; set; }
        public DbSet<ApartmentFilter> ApartmentFilters { get; set; }
        public DbSet<HouseFilter> HouseFilters { get; set; }
        public DbSet<LandFilter> LandFilters { get; set; }
        public DbSet<Demand> Demands { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<Deal> Deals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-HJMCFFD;Database=RealEstateDataBase;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite primary key for Deal
            modelBuilder.Entity<Deal>()
                .HasKey(d => new { d.Demand_ID, d.Supply_ID });

            // Configure Deal relationships with proper navigation properties
            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Demand)
                .WithMany(d => d.Deals)
                .HasForeignKey(d => d.Demand_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Supply)
                .WithMany(s => s.Deals)
                .HasForeignKey(d => d.Supply_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Demand relationships
            modelBuilder.Entity<Demand>()
                .HasOne(d => d.Agent)
                .WithMany(a => a.Demands)
                .HasForeignKey(d => d.Agent_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Demand>()
                .HasOne(d => d.Client)
                .WithMany(c => c.Demands)
                .HasForeignKey(d => d.Client_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Demand>()
                .HasOne(d => d.ApartmentFilter)
                .WithMany(af => af.Demands)
                .HasForeignKey(d => d.ApartmentFilter_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Demand>()
                .HasOne(d => d.HouseFilter)
                .WithMany(hf => hf.Demands)
                .HasForeignKey(d => d.HouseFilter_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Demand>()
                .HasOne(d => d.LandFilter)
                .WithMany(lf => lf.Demands)
                .HasForeignKey(d => d.LandFilter_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Supply relationships
            modelBuilder.Entity<Supply>()
                .HasOne(s => s.Agent)
                .WithMany(a => a.Supplies)
                .HasForeignKey(s => s.Agent_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supply>()
                .HasOne(s => s.Client)
                .WithMany(c => c.Supplies)
                .HasForeignKey(s => s.Client_ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supply>()
                .HasOne(s => s.RealEstate)
                .WithMany(re => re.Supplies)
                .HasForeignKey(s => s.RealEstate_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure RealEstate relationships
            modelBuilder.Entity<RealEstate>()
                .HasOne(re => re.District)
                .WithMany(d => d.RealEstates)
                .HasForeignKey(re => re.District_ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RealEstate>()
                .HasOne(re => re.RealEstateType)
                .WithMany(t => t.RealEstates)
                .HasForeignKey(re => re.Type_ID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure table names and other entity-specific settings if needed
            modelBuilder.Entity<ApartmentFilter>().ToTable("ApartmentFilters");
            modelBuilder.Entity<HouseFilter>().ToTable("HouseFilters");
            modelBuilder.Entity<LandFilter>().ToTable("LandFilters");
        }
    }
}