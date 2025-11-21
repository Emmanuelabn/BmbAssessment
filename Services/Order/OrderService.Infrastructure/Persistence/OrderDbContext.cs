using System;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(builder =>
            {
                builder.ToTable("Orders");

                builder.HasKey(o => o.Id);

                builder.Property(o => o.ProductId)
                       .IsRequired();

                builder.Property(o => o.ClientId)
                       .IsRequired();

                builder.Property(o => o.Quantity)
                       .IsRequired();

                builder.Property(o => o.Total)
                       .HasColumnType("decimal(18,2)")
                       .IsRequired();

                builder.Property(o => o.OrderDate)
                       .IsRequired();

                builder.Property(o => o.LoggedInEmployeeId)
                       .IsRequired();
            });
        }
    }
}
