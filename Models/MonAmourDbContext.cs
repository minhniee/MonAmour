using Microsoft.EntityFrameworkCore;

namespace MonAmour.Models;

public partial class MonAmourDbContext : DbContext
{
    public MonAmourDbContext()
    {
    }

    public MonAmourDbContext(DbContextOptions<MonAmourDbContext> options)
        : base(options)
    {
    }


    public virtual DbSet<CassoTransaction> CassoTransactions { get; set; }

    public virtual DbSet<Concept> Concepts { get; set; }

    public virtual DbSet<ConceptAmbience> ConceptAmbiences { get; set; }

    public virtual DbSet<ConceptCategory> ConceptCategories { get; set; }

    public virtual DbSet<ConceptColor> ConceptColors { get; set; }

    public virtual DbSet<ConceptImg> ConceptImgs { get; set; }

    public virtual DbSet<ConceptColorJunction> ConceptColorJunctions { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentDetail> PaymentDetails { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductImg> ProductImgs { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ShippingOption> ShippingOptions { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwPaymentDetail> VwPaymentDetails { get; set; }

    public virtual DbSet<WishList> WishLists { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<BannerService> BannerServices { get; set; }

    public virtual DbSet<BannerHomepage> BannerHomepages { get; set; }

    public virtual DbSet<BannerProduct> BannerProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(ConnectionString);
        }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<CassoTransaction>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_CassoTransactions_OrderId");

            entity.HasIndex(e => e.UserId, "IX_CassoTransactions_UserId");

            entity.HasIndex(e => e.When, "IX_CassoTransactions_When");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BankSubAccId).HasMaxLength(50);
            entity.Property(e => e.CorrespondingAccount).HasMaxLength(50);
            entity.Property(e => e.CorrespondingAccountName).HasMaxLength(255);
            entity.Property(e => e.CorrespondingBankId).HasMaxLength(50);
            entity.Property(e => e.CorrespondingBankName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProcessedAt).HasColumnType("datetime");
            entity.Property(e => e.Ref).HasMaxLength(255);
            entity.Property(e => e.Reference).HasMaxLength(255);
            entity.Property(e => e.SubAccId).HasMaxLength(50);
            entity.Property(e => e.VirtualAccount).HasMaxLength(50);
            entity.Property(e => e.When).HasColumnType("datetime");
        });

        modelBuilder.Entity<Concept>(entity =>
        {
            entity.HasKey(e => e.ConceptId).HasName("PK__Concept__7925FD2DE0435406");
            entity.HasKey(e => e.ConceptId).HasName("PK__Concept__7925FD2D23849D9A");

            entity.ToTable("Concept");

            entity.Property(e => e.ConceptId).HasColumnName("concept_id");
            entity.Property(e => e.AmbienceId).HasColumnName("ambience_id");
            entity.Property(e => e.AvailabilityStatus)
                .HasDefaultValue(true)
                .HasColumnName("availability_status");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PreparationTime).HasColumnName("preparation_time");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Ambience).WithMany(p => p.Concepts)
                .HasForeignKey(d => d.AmbienceId)
                .HasConstraintName("FK__Concept__ambienc__6E01572D");

            entity.HasOne(d => d.Category).WithMany(p => p.Concepts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Concept__categor__6D0D32F4");

            entity.HasOne(d => d.Location).WithMany(p => p.Concepts)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Concept__locatio__6B24EA82");
        });

        modelBuilder.Entity<ConceptAmbience>(entity =>
        {
            entity.HasKey(e => e.AmbienceId).HasName("PK__Concept___5D801B5814ACE9E4");
            entity.HasKey(e => e.AmbienceId).HasName("PK__Concept___5D801B5861E7DD5C");

            entity.ToTable("Concept_Ambience");

            entity.Property(e => e.AmbienceId).HasColumnName("ambience_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ConceptCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Concept___D54EE9B4744B5019");
            entity.HasKey(e => e.CategoryId).HasName("PK__Concept___D54EE9B476E3335C");

            entity.ToTable("Concept_Category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ConceptColor>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PK__Concept___1143CECBEC7F8C36");

            entity.ToTable("Concept_Color");

            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ConceptColorJunction>(entity =>
        {
            entity.HasKey(e => new { e.ConceptId, e.ColorId }).HasName("PK_Concept_Color_Junction");

            entity.ToTable("Concept_Color_Junction");

            entity.Property(e => e.ConceptId).HasColumnName("concept_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");

            entity.HasOne(d => d.Concept).WithMany(p => p.ConceptColorJunctions)
                .HasForeignKey(d => d.ConceptId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Concept_Color_Junction_Concept");

            entity.HasOne(d => d.Color).WithMany(p => p.ConceptColorJunctions)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Concept_Color_Junction_Color");
        });

        modelBuilder.Entity<ConceptImg>(entity =>
        {
            entity.HasKey(e => e.ImgId).HasName("PK__Concept___6F16A71CDF71B7F8");

            entity.ToTable("Concept_img");

            entity.Property(e => e.ImgId).HasColumnName("img_id");
            entity.Property(e => e.AltText)
                .HasMaxLength(255)
                .HasColumnName("alt_text");
            entity.Property(e => e.ConceptId).HasColumnName("concept_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.ImgName)
                .HasMaxLength(255)
                .HasColumnName("img_name");
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("img_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");

            entity.HasOne(d => d.Concept).WithMany(p => p.ConceptImgs)
                .HasForeignKey(d => d.ConceptId)
                .HasConstraintName("FK__Concept_i__conce__73BA3083");
        });

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.ContentId).HasName("PK__Content__655FE510DC9A9FD9");
            entity.HasKey(e => e.ContentId).HasName("PK__Content__655FE5107D65012E");

            entity.ToTable("Content");

            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Tags)
                .HasMaxLength(255)
                .HasColumnName("tags");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Author).WithMany(p => p.Contents)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__Content__author___339FAB6E");
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Email_Te__BE44E079234D1F03");
            entity.HasKey(e => e.TemplateId).HasName("PK__Email_Te__BE44E0792149D512");

            entity.ToTable("Email_Template");

            entity.HasIndex(e => e.Name, "UQ__Email_Te__72E12F1B3FDC91E4").IsUnique();
            entity.HasIndex(e => e.Name, "UQ__Email_Te__72E12F1B063E9FC9").IsUnique();

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");
            entity.Property(e => e.TemplateType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("template_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Variables)
                .HasMaxLength(500)
                .HasColumnName("variables");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.EmailTemplates)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Email_Tem__creat__52593CB8");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EA6822AA9F");
            entity.HasKey(e => e.LocationId).HasName("PK__Location__771831EAA72D3B62");

            entity.ToTable("Location");

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.GgmapLink)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ggmap_link");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PartnerId).HasColumnName("partner_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Partner).WithMany(p => p.Locations)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__Location__partne__5FB337D6");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842FB5BD5E60");
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F5E9C768D");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ActionUrl)
                .HasMaxLength(255)
                .HasColumnName("action_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("notification_type");
            entity.Property(e => e.ReadAt)
                .HasColumnType("datetime")
                .HasColumnName("read_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__user___3864608B");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__46596229A3D0FEA1");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveredAt)
                .HasColumnType("datetime")
                .HasColumnName("delivered_at");
            entity.Property(e => e.EstimatedDelivery).HasColumnName("estimated_delivery");
            entity.Property(e => e.ShippingCost)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("shipping_cost");
            entity.Property(e => e.ShippingOptionId).HasColumnName("shipping_option_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_price");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(100)
                .HasColumnName("tracking_number");
            entity.Property(e => e.ShippingAddress)
                .HasMaxLength(4000)
                .HasColumnName("shipping_address");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ShippingOption).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShippingOptionId)
                .HasConstraintName("FK__Order__shipping___18EBB532");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Order__user_id__160F4887");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__3764B6BCD4612A63");
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__3764B6BCAA8EB486");

            entity.ToTable("OrderItem");

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__order__1DB06A4F");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderItem__produ__1EA48E88");
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.PartnerId).HasName("PK__Partner__576F1B2760143582");
            entity.HasKey(e => e.PartnerId).HasName("PK__Partner__576F1B271BAFCF5F");

            entity.ToTable("Partner");

            entity.Property(e => e.PartnerId).HasColumnName("partner_id");
            entity.Property(e => e.ContactInfo)
                .HasMaxLength(255)
                .HasColumnName("contact_info");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Partners)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Partner__user_id__571DF1D5");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__ED1FC9EA0F051E89");
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__ED1FC9EA7BD182D3");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentReference).HasMaxLength(255);
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK__Payment__payment__245D67DE");

            entity.HasOne(d => d.User).WithMany(p => p.Payments).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<PaymentDetail>(entity =>
        {
            entity.HasKey(e => e.PaymentDetailId).HasName("PK__PaymentD__C66E6E36960CA82A");
            entity.HasKey(e => e.PaymentDetailId).HasName("PK__PaymentD__C66E6E36678D2E3A");

            entity.ToTable("PaymentDetail");

            entity.Property(e => e.PaymentDetailId).HasColumnName("payment_detail_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__PaymentDe__order__29221CFB");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentDetails)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK__PaymentDe__payme__282DF8C2");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__8A3EA9EB1F4AA5BD");
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__8A3EA9EB329C70EB");

            entity.ToTable("PaymentMethod");

            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.ToTable("PaymentStatus");

            entity.HasIndex(e => e.PaymentId, "IX_PaymentStatus_PaymentId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentStatuses)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__47027DF5214502C9");
            entity.HasKey(e => e.ProductId).HasName("PK__Product__47027DF5CA9C12F9");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Material)
                .HasMaxLength(100)
                .HasColumnName("material");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.TargetAudience)
                .HasMaxLength(100)
                .HasColumnName("target_audience");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__categor__02FC7413");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Product___D54EE9B42E010B9D");
            entity.HasKey(e => e.CategoryId).HasName("PK__Product___D54EE9B469629BC5");

            entity.ToTable("Product_Category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProductImg>(entity =>
        {
            entity.HasKey(e => e.ImgId).HasName("PK__Product___6F16A71C2327A88B");
            entity.HasKey(e => e.ImgId).HasName("PK__Product___6F16A71CE7A99999");

            entity.ToTable("Product_img");

            entity.Property(e => e.ImgId).HasColumnName("img_id");
            entity.Property(e => e.AltText)
                .HasMaxLength(255)
                .HasColumnName("alt_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.ImgName)
                .HasMaxLength(255)
                .HasColumnName("img_name");
            entity.Property(e => e.ImgUrl)
                .HasMaxLength(255)
                .HasColumnName("img_url");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImgs)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Product_i__produ__08B54D69");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__60883D90BCD570AF");
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__60883D90544C4A16");

            entity.ToTable("Review");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("target_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Review__user_id__2CF2ADDF");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC401337EF");
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CCEA25C033");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__783254B1CC21FE4C").IsUnique();
            entity.HasIndex(e => e.RoleName, "UQ__Role__783254B1CCD27224").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<ShippingOption>(entity =>
        {
            entity.HasKey(e => e.ShippingOptionId).HasName("PK__Shipping__6B1300C824455E88");
            entity.HasKey(e => e.ShippingOptionId).HasName("PK__Shipping__6B1300C8B41876E6");

            entity.ToTable("ShippingOption");

            entity.Property(e => e.ShippingOptionId).HasColumnName("shipping_option_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Token__CB3C9E17CD394AA9");
            entity.HasKey(e => e.TokenId).HasName("PK__Token__CB3C9E174111CDFD");

            entity.ToTable("Token");

            entity.HasIndex(e => new { e.ExpiresAt, e.IsActive }, "IX_Token_ExpiresActive");

            entity.HasIndex(e => new { e.UserId, e.TokenType }, "IX_Token_UserTokenType");

            entity.HasIndex(e => e.TokenValue, "IX_Token_Value");

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.TokenType)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("token_type");
            entity.Property(e => e.TokenValue)
                .HasMaxLength(500)
                .HasColumnName("token_value");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UsedAt)
                .HasColumnType("datetime")
                .HasColumnName("used_at");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Token__user_id__48CFD27E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370FEA846F54");
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370FBBE4393B");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__AB6E6164DE0E25C7").IsUnique();
            entity.HasIndex(e => e.Email, "UQ__User__AB6E6164BDEAD157").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__User__B43B145FBDADCA42").IsUnique();
            entity.HasIndex(e => e.Phone, "UQ__User__B43B145F8C22A302").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .HasColumnName("avatar");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Verified)
                .HasDefaultValue(false)
                .HasColumnName("verified");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__User_Rol__6EDEA15328A69503");
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__User_Rol__6EDEA153D1F4F13E");

            entity.ToTable("User_Role");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("assigned_at");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Role__role___44FF419A");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Role__user___440B1D61");
        });

        modelBuilder.Entity<VwPaymentDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PaymentDetails");

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PaymentDetailAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PaymentMethodName).HasMaxLength(50);
            entity.Property(e => e.PaymentReference).HasMaxLength(255);
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_price");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<WishList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Wish_lis__3213E83F805E3562");
            entity.HasKey(e => e.Id).HasName("PK__Wish_lis__3213E83F9142F152");

            entity.ToTable("Wish_list");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConceptId).HasColumnName("concept_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Concept).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.ConceptId)
                .HasConstraintName("FK__Wish_list__conce__10566F31");

            entity.HasOne(d => d.Product).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Wish_list__produ__0F624AF8");

            entity.HasOne(d => d.User).WithMany(p => p.WishLists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Wish_list__user___0E6E26BF");
        });




        // Configure Blog models explicitly
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId);
            entity.ToTable("Blog");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Excerpt).HasColumnName("excerpt");
            entity.Property(e => e.FeaturedImage).HasColumnName("featured_image");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Tags).HasColumnName("tags");
            entity.Property(e => e.PublishedDate).HasColumnName("published_date");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured");
            entity.Property(e => e.IsPublished).HasColumnName("is_published");
            entity.Property(e => e.ReadTime).HasColumnName("read_time");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasOne(d => d.Author)
                .WithMany(p => p.Blogs)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_Blog_User");
                
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Blog_Category");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.ToTable("Blog_Category");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Slug).HasColumnName("slug");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.ToTable("Blog_Comment");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AuthorName).HasColumnName("author_name");
            entity.Property(e => e.AuthorEmail).HasColumnName("author_email");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsApproved).HasColumnName("is_approved");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasOne(d => d.Blog)
                .WithMany(p => p.Comments)
                .HasForeignKey(d => d.BlogId)
                .HasConstraintName("FK_BlogComment_Blog");
                
            entity.HasOne(d => d.User)
                .WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_BlogComment_User");
        });

        // Configure BannerService
        modelBuilder.Entity<BannerService>(entity =>
        {
            entity.HasKey(e => e.BannerId);
            entity.ToTable("BannerService");
            entity.Property(e => e.BannerId).HasColumnName("banner_id");
            entity.Property(e => e.ImgUrl).HasColumnName("img_url").HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Configure BannerHomepage
        modelBuilder.Entity<BannerHomepage>(entity =>
        {
            entity.HasKey(e => e.BannerId);
            entity.ToTable("BannerHomepage");
            entity.Property(e => e.BannerId).HasColumnName("banner_id");
            entity.Property(e => e.ImgUrl).HasColumnName("img_url").HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Configure BannerProduct
        modelBuilder.Entity<BannerProduct>(entity =>
        {
            entity.HasKey(e => e.BannerId);
            entity.ToTable("BannerProduct");
            entity.Property(e => e.BannerId).HasColumnName("banner_id");
            entity.Property(e => e.ImgUrl).HasColumnName("img_url").HasMaxLength(500);
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
