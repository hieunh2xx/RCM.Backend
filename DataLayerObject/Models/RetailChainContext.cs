using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataLayerObject.Models
{
    public partial class RetailChainContext : DbContext
    {
        public RetailChainContext()
        {
        }

        public RetailChainContext(DbContextOptions<RetailChainContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<AttendanceHi> AttendanceHis { get; set; } = null!;
        public virtual DbSet<Batch> Batches { get; set; } = null!;
        public virtual DbSet<BatchDetail> BatchDetails { get; set; } = null!;
        public virtual DbSet<Cash> Cashes { get; set; } = null!;
        public virtual DbSet<Employee> Employees { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<PenaltyPayment> PenaltyPayments { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductPrice> ProductPrices { get; set; } = null!;
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
        public virtual DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } = null!;
        public virtual DbSet<Refund> Refunds { get; set; } = null!;
        public virtual DbSet<RefundDetail> RefundDetails { get; set; } = null!;
        public virtual DbSet<Salary> Salaries { get; set; } = null!;
        public virtual DbSet<SalaryPaymentHistory> SalaryPaymentHistories { get; set; } = null!;
        public virtual DbSet<StockAdjustment> StockAdjustments { get; set; } = null!;
        public virtual DbSet<StockAdjustmentDetail> StockAdjustmentDetails { get; set; } = null!;
        public virtual DbSet<StockAuditDetail> StockAuditDetails { get; set; } = null!;
        public virtual DbSet<StockAuditRecord> StockAuditRecords { get; set; } = null!;
        public virtual DbSet<StockLevel> StockLevels { get; set; } = null!;
        public virtual DbSet<Supplier> Suppliers { get; set; } = null!;
        public virtual DbSet<Warehouse> Warehouses { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=MSI;Database=RetailChain;User=sa;Password=123;TrustServerCertificate=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.HasIndex(e => e.Username, "UQ__Account__536C85E489920169")
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Role).HasColumnName("role");

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__Account__Employe__7E37BEF6");
            });

            modelBuilder.Entity<AttendanceHi>(entity =>
            {
                entity.Property(e => e.AttendanceDate).HasColumnType("date");

                entity.Property(e => e.CheckInTime).HasColumnType("datetime");

                entity.Property(e => e.CheckOutTime).HasColumnType("datetime");

                entity.Property(e => e.Shift).HasMaxLength(50);

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.AttendanceHis)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__Attendanc__Emplo__01142BA1");
            });

            modelBuilder.Entity<Batch>(entity =>
            {
                entity.ToTable("batch");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ReceivedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("received_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.Batches)
                    .HasForeignKey(d => d.WarehouseId)
                    .HasConstraintName("FK__batch__warehouse__5070F446");
            });

            modelBuilder.Entity<BatchDetail>(entity =>
            {
                entity.ToTable("batch_details");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BatchId).HasColumnName("batch_id");

                entity.Property(e => e.ExpirationDate)
                    .HasColumnType("date")
                    .HasColumnName("expiration_date");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.HasOne(d => d.Batch)
                    .WithMany(p => p.BatchDetails)
                    .HasForeignKey(d => d.BatchId)
                    .HasConstraintName("FK__batch_det__batch__534D60F1");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.BatchDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__batch_det__produ__5441852A");
            });

            modelBuilder.Entity<Cash>(entity =>
            {
                entity.ToTable("CASH");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Note)
                    .HasMaxLength(255)
                    .HasColumnName("note");

                entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.Cashes)
                    .HasForeignKey(d => d.BranchId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__CASH__BranchId__07C12930");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Cashes)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__CASH__EmployeeId__06CD04F7");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");

                entity.HasIndex(e => e.IdentityNumber, "UQ__Employee__6354A73F8ECA0308")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneNumber, "UQ__Employee__85FB4E3880D8DC7D")
                    .IsUnique();

                entity.Property(e => e.BirthDate).HasColumnType("date");

                entity.Property(e => e.CurrentAddress).HasMaxLength(255);

                entity.Property(e => e.FullName).HasMaxLength(100);

                entity.Property(e => e.Gender).HasMaxLength(10);

                entity.Property(e => e.Hometown).HasMaxLength(100);

                entity.Property(e => e.IdentityNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Image).HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.HasOne(d => d.Branch)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.BranchId)
                    .HasConstraintName("FK__Employee__Branch__3C69FB99");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");

                entity.Property(e => e.Discount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("discount");

                entity.Property(e => e.FinalAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("final_amount");

                entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");

                entity.Property(e => e.ShopId).HasColumnName("shop_id");

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total_amount");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__Order__EmployeeI__6FE99F9F");

                entity.HasOne(d => d.Shop)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ShopId)
                    .HasConstraintName("FK__Order__shop_id__6EF57B66");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("quantity");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total_price");

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("unit_price");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderDeta__order__72C60C4A");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__OrderDeta__produ__73BA3083");
            });

            modelBuilder.Entity<PenaltyPayment>(entity =>
            {
                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.PaymentDate).HasColumnType("date");

                entity.Property(e => e.Reason).HasMaxLength(255);

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.PenaltyPayments)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__PenaltyPa__Emplo__10566F31");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");

                entity.HasIndex(e => e.Barcode, "UQ__products__C16E36F89AEE5D7D")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Barcode)
                    .HasMaxLength(50)
                    .HasColumnName("barcode");

                entity.Property(e => e.BaseUnit)
                    .HasMaxLength(50)
                    .HasColumnName("base_unit");

                entity.Property(e => e.Category)
                    .HasMaxLength(50)
                    .HasColumnName("category");

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500)
                    .HasColumnName("image_url");

                entity.Property(e => e.IsEnabled)
                    .HasColumnName("is_enabled")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.QuantityPerUnit).HasColumnName("quantity_per_unit");

                entity.Property(e => e.Unit)
                    .HasMaxLength(50)
                    .HasColumnName("unit");

                entity.Property(e => e.Volume)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("volume");

                entity.Property(e => e.Weight)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("weight");
            });

            modelBuilder.Entity<ProductPrice>(entity =>
            {
                entity.ToTable("product_prices");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.EffectiveDate)
                    .HasColumnType("datetime")
                    .HasColumnName("effective_date")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductPrices)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__product_p__produ__46E78A0C");
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.ToTable("purchase_orders");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ExpectedArrival)
                    .HasColumnType("datetime")
                    .HasColumnName("expected_arrival");

                entity.Property(e => e.OrderDate)
                    .HasColumnType("datetime")
                    .HasColumnName("order_date");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasColumnName("status");

                entity.Property(e => e.SupplierId).HasColumnName("supplier_id");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.PurchaseOrders)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__purchase___suppl__5812160E");
            });

            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.ToTable("purchase_order_items");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BatchId).HasColumnName("batch_id");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");

                entity.Property(e => e.QuantityOrdered).HasColumnName("quantity_ordered");

                entity.Property(e => e.QuantityReceived)
                    .HasColumnName("quantity_received")
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Batch)
                    .WithMany(p => p.PurchaseOrderItems)
                    .HasForeignKey(d => d.BatchId)
                    .HasConstraintName("FK__purchase___batch__5DCAEF64");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PurchaseOrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__purchase___produ__5CD6CB2B");

                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.PurchaseOrderItems)
                    .HasForeignKey(d => d.PurchaseOrderId)
                    .HasConstraintName("FK__purchase___purch__5BE2A6F2");
            });

            modelBuilder.Entity<Refund>(entity =>
            {
                entity.ToTable("Refund");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.RefundAmount)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("refund_amount");

                entity.Property(e => e.RefundDate)
                    .HasColumnType("date")
                    .HasColumnName("refund_date");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__Refund__order_id__76969D2E");
            });

            modelBuilder.Entity<RefundDetail>(entity =>
            {
                entity.ToTable("RefundDetail");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("quantity");

                entity.Property(e => e.RefundId).HasColumnName("refund_id");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("total_price");

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("unit_price");

                entity.HasOne(d => d.Refund)
                    .WithMany(p => p.RefundDetails)
                    .HasForeignKey(d => d.RefundId)
                    .HasConstraintName("FK__RefundDet__refun__797309D9");
            });

            modelBuilder.Entity<Salary>(entity =>
            {
                entity.ToTable("Salary");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Salaries)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__Salary__Employee__03F0984C");
            });

            modelBuilder.Entity<SalaryPaymentHistory>(entity =>
            {
                entity.ToTable("SalaryPaymentHistory");

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.Note).HasMaxLength(255);

                entity.Property(e => e.PaymentDate).HasColumnType("date");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.SalaryPaymentHistories)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK__SalaryPay__Emplo__0B91BA14");

                entity.HasOne(d => d.Salary)
                    .WithMany(p => p.SalaryPaymentHistories)
                    .HasForeignKey(d => d.SalaryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SalaryPay__Salar__0C85DE4D");
            });

            modelBuilder.Entity<StockAdjustment>(entity =>
            {
                entity.ToTable("stock_adjustments");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AdjustmentDate)
                    .HasColumnType("datetime")
                    .HasColumnName("adjustment_date");

                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.StockAdjustments)
                    .HasForeignKey(d => d.WarehouseId)
                    .HasConstraintName("FK__stock_adj__wareh__6754599E");
            });

            modelBuilder.Entity<StockAdjustmentDetail>(entity =>
            {
                entity.ToTable("stock_adjustment_details");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AdjustedQuantity).HasColumnName("adjusted_quantity");

                entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.HasOne(d => d.Adjustment)
                    .WithMany(p => p.StockAdjustmentDetails)
                    .HasForeignKey(d => d.AdjustmentId)
                    .HasConstraintName("FK__stock_adj__adjus__6A30C649");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StockAdjustmentDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__stock_adj__produ__6B24EA82");
            });

            modelBuilder.Entity<StockAuditDetail>(entity =>
            {
                entity.ToTable("stock_audit_details");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AuditId).HasColumnName("audit_id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.RecordedQuantity).HasColumnName("recorded_quantity");

                entity.HasOne(d => d.Audit)
                    .WithMany(p => p.StockAuditDetails)
                    .HasForeignKey(d => d.AuditId)
                    .HasConstraintName("FK__stock_aud__audit__6383C8BA");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StockAuditDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__stock_aud__produ__6477ECF3");
            });

            modelBuilder.Entity<StockAuditRecord>(entity =>
            {
                entity.ToTable("stock_audit_records");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AuditDate)
                    .HasColumnType("datetime")
                    .HasColumnName("audit_date");

                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.StockAuditRecords)
                    .HasForeignKey(d => d.WarehouseId)
                    .HasConstraintName("FK__stock_aud__wareh__60A75C0F");
            });

            modelBuilder.Entity<StockLevel>(entity =>
            {
                entity.ToTable("stock_levels");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MinQuantity)
                    .HasColumnName("min_quantity")
                    .HasDefaultValueSql("((20))");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StockLevels)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__stock_lev__produ__4BAC3F29");

                entity.HasOne(d => d.Warehouse)
                    .WithMany(p => p.StockLevels)
                    .HasForeignKey(d => d.WarehouseId)
                    .HasConstraintName("FK__stock_lev__wareh__4CA06362");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("suppliers");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.ContactPerson)
                    .HasMaxLength(255)
                    .HasColumnName("contact_person");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .HasColumnName("phone");
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("warehouses");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.Capacity).HasColumnName("capacity");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
