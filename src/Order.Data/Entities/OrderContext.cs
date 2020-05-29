using Microsoft.EntityFrameworkCore;

namespace Order.Data.Entities
{
    public partial class OrderContext : DbContext
    {
        public OrderContext()
        {
        }

        public OrderContext(DbContextOptions<OrderContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }
        public virtual DbSet<OrderProduct> OrderProduct { get; set; }
        public virtual DbSet<OrderService> OrderService { get; set; }
        public virtual DbSet<OrderStatus> OrderStatus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("order");

                entity.HasIndex(e => e.CustomerId)
                    .HasName("CustomerId");

                entity.HasIndex(e => e.StatusId)
                    .HasName("StatusId");

                entity.Property(e => e.Id).HasColumnType("binary(16)");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.Property(e => e.ResellerId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.Property(e => e.StatusId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("order_ofk_1");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_item");

                entity.HasIndex(e => e.OrderId)
                    .HasName("OrderId");

                entity.HasIndex(e => e.ProductId)
                    .HasName("ProductId");

                entity.HasIndex(e => e.ServiceId)
                    .HasName("ServiceId");

                entity.Property(e => e.Id).HasColumnType("binary(16)");

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.Property(e => e.Quantity).HasColumnType("int(11)");

                entity.Property(e => e.ServiceId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("order_item_oifk_1");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("order_product_oifk_1");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.OrderItem)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("order_service_oifk_1");
            });

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.ToTable("order_product");

                entity.HasIndex(e => e.ServiceId)
                    .HasName("order_service_opfk_1");

                entity.Property(e => e.Id).HasColumnType("binary(16)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceId)
                    .IsRequired()
                    .HasColumnType("binary(16)");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.OrderProduct)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("order_service_opfk_1");
            });

            modelBuilder.Entity<OrderService>(entity =>
            {
                entity.ToTable("order_service");

                entity.Property(e => e.Id).HasColumnType("binary(16)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("order_status");

                entity.HasIndex(e => e.Name)
                    .HasName("Status");

                entity.Property(e => e.Id).HasColumnType("binary(16)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
