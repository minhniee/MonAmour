using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonAmour.Migrations
{
    /// <inheritdoc />
    public partial class CreateBlogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blog_Category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    slug = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog_Category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "CassoTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Tid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    When = table.Column<DateTime>(type: "datetime", nullable: false),
                    BankSubAccId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubAccId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VirtualAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrespondingAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrespondingAccountName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CorrespondingBankId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrespondingBankName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ref = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CassoTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Concept_Ambience",
                columns: table => new
                {
                    ambience_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Concept___5D801B5861E7DD5C", x => x.ambience_id);
                });

            migrationBuilder.CreateTable(
                name: "Concept_Category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Concept___D54EE9B476E3335C", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "Concept_Color",
                columns: table => new
                {
                    color_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Concept___1143CECBEC7F8C36", x => x.color_id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    payment_method_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentM__8A3EA9EB329C70EB", x => x.payment_method_id);
                });

            migrationBuilder.CreateTable(
                name: "Product_Category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Product___D54EE9B469629BC5", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__760965CCEA25C033", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingOption",
                columns: table => new
                {
                    shipping_option_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Shipping__6B1300C8B41876E6", x => x.shipping_option_id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    verified = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    gender = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__B9BE370FBBE4393B", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    material = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    target_audience = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    stock_quantity = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Product__47027DF5CA9C12F9", x => x.product_id);
                    table.ForeignKey(
                        name: "FK__Product__categor__02FC7413",
                        column: x => x.category_id,
                        principalTable: "Product_Category",
                        principalColumn: "category_id");
                });

            migrationBuilder.CreateTable(
                name: "Blog",
                columns: table => new
                {
                    blog_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    featured_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    author_id = table.Column<int>(type: "int", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    tags = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    published_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: true),
                    ReadTime = table.Column<int>(type: "int", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlogCategoryCategoryId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog", x => x.blog_id);
                    table.ForeignKey(
                        name: "FK_Blog_Blog_Category_BlogCategoryCategoryId",
                        column: x => x.BlogCategoryCategoryId,
                        principalTable: "Blog_Category",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_Blog_Category",
                        column: x => x.category_id,
                        principalTable: "Blog_Category",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_Blog_User",
                        column: x => x.author_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Blog_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Content",
                columns: table => new
                {
                    content_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    tags = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    author_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Content__655FE5107D65012E", x => x.content_id);
                    table.ForeignKey(
                        name: "FK__Content__author___339FAB6E",
                        column: x => x.author_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Email_Template",
                columns: table => new
                {
                    template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    template_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    variables = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Email_Te__BE44E0792149D512", x => x.template_id);
                    table.ForeignKey(
                        name: "FK__Email_Tem__creat__52593CB8",
                        column: x => x.created_by,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    notification_type = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    action_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    read_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__E059842F5E9C768D", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK__Notificat__user___3864608B",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    shipping_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    shipping_option_id = table.Column<int>(type: "int", nullable: true),
                    tracking_number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    estimated_delivery = table.Column<DateOnly>(type: "date", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    shipping_address = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order__46596229A3D0FEA1", x => x.order_id);
                    table.ForeignKey(
                        name: "FK__Order__shipping___18EBB532",
                        column: x => x.shipping_option_id,
                        principalTable: "ShippingOption",
                        principalColumn: "shipping_option_id");
                    table.ForeignKey(
                        name: "FK__Order__user_id__160F4887",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Partner",
                columns: table => new
                {
                    partner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    contact_info = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Partner__576F1B271BAFCF5F", x => x.partner_id);
                    table.ForeignKey(
                        name: "FK__Partner__user_id__571DF1D5",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    payment_method_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    processed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payment__ED1FC9EA7BD182D3", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_Payment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK__Payment__payment__245D67DE",
                        column: x => x.payment_method_id,
                        principalTable: "PaymentMethod",
                        principalColumn: "payment_method_id");
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    target_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    target_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Review__60883D90544C4A16", x => x.review_id);
                    table.ForeignKey(
                        name: "FK__Review__user_id__2CF2ADDF",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    token_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    used_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Token__CB3C9E174111CDFD", x => x.token_id);
                    table.ForeignKey(
                        name: "FK__Token__user_id__48CFD27E",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "User_Role",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    assigned_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User_Rol__6EDEA153D1F4F13E", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK__User_Role__role___44FF419A",
                        column: x => x.role_id,
                        principalTable: "Role",
                        principalColumn: "role_id");
                    table.ForeignKey(
                        name: "FK__User_Role__user___440B1D61",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Product_img",
                columns: table => new
                {
                    img_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: true),
                    img_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    img_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    alt_text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_primary = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    display_order = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Product___6F16A71CE7A99999", x => x.img_id);
                    table.ForeignKey(
                        name: "FK__Product_i__produ__08B54D69",
                        column: x => x.product_id,
                        principalTable: "Product",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateTable(
                name: "Blog_Comment",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    blog_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    author_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    author_email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_approved = table.Column<bool>(type: "bit", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blog_Comment", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_BlogComment_Blog",
                        column: x => x.blog_id,
                        principalTable: "Blog",
                        principalColumn: "blog_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogComment_User",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_Blog_Comment_User_UserId1",
                        column: x => x.UserId1,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    order_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    product_id = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderIte__3764B6BCAA8EB486", x => x.order_item_id);
                    table.ForeignKey(
                        name: "FK__OrderItem__order__1DB06A4F",
                        column: x => x.order_id,
                        principalTable: "Order",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "FK__OrderItem__produ__1EA48E88",
                        column: x => x.product_id,
                        principalTable: "Product",
                        principalColumn: "product_id");
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    district = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "active"),
                    partner_id = table.Column<int>(type: "int", nullable: true),
                    ggmap_link = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Location__771831EAA72D3B62", x => x.location_id);
                    table.ForeignKey(
                        name: "FK__Location__partne__5FB337D6",
                        column: x => x.partner_id,
                        principalTable: "Partner",
                        principalColumn: "partner_id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatus",
                columns: table => new
                {
                    PaymentStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatus", x => x.PaymentStatusId);
                    table.ForeignKey(
                        name: "FK_PaymentStatus_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payment",
                        principalColumn: "payment_id");
                });

            migrationBuilder.CreateTable(
                name: "Concept",
                columns: table => new
                {
                    concept_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    location_id = table.Column<int>(type: "int", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    ambience_id = table.Column<int>(type: "int", nullable: true),
                    preparation_time = table.Column<int>(type: "int", nullable: true),
                    availability_status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Concept__7925FD2D23849D9A", x => x.concept_id);
                    table.ForeignKey(
                        name: "FK__Concept__ambienc__6E01572D",
                        column: x => x.ambience_id,
                        principalTable: "Concept_Ambience",
                        principalColumn: "ambience_id");
                    table.ForeignKey(
                        name: "FK__Concept__categor__6D0D32F4",
                        column: x => x.category_id,
                        principalTable: "Concept_Category",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK__Concept__locatio__6B24EA82",
                        column: x => x.location_id,
                        principalTable: "Location",
                        principalColumn: "location_id");
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    concept_id = table.Column<int>(type: "int", nullable: true),
                    booking_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    booking_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    payment_status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending"),
                    confirmed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Booking__5DE3A5B18F1D4649", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK__Booking__concept__7A672E12",
                        column: x => x.concept_id,
                        principalTable: "Concept",
                        principalColumn: "concept_id");
                    table.ForeignKey(
                        name: "FK__Booking__user_id__797309D9",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Concept_Color_Junction",
                columns: table => new
                {
                    concept_id = table.Column<int>(type: "int", nullable: false),
                    color_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concept_Color_Junction", x => new { x.concept_id, x.color_id });
                    table.ForeignKey(
                        name: "FK_Concept_Color_Junction_Color",
                        column: x => x.color_id,
                        principalTable: "Concept_Color",
                        principalColumn: "color_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Concept_Color_Junction_Concept",
                        column: x => x.concept_id,
                        principalTable: "Concept",
                        principalColumn: "concept_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Concept_img",
                columns: table => new
                {
                    img_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    concept_id = table.Column<int>(type: "int", nullable: true),
                    img_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    img_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    alt_text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_primary = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    display_order = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Concept___6F16A71CDF71B7F8", x => x.img_id);
                    table.ForeignKey(
                        name: "FK__Concept_i__conce__73BA3083",
                        column: x => x.concept_id,
                        principalTable: "Concept",
                        principalColumn: "concept_id");
                });

            migrationBuilder.CreateTable(
                name: "Wish_list",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: true),
                    concept_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Wish_lis__3213E83F9142F152", x => x.id);
                    table.ForeignKey(
                        name: "FK__Wish_list__conce__10566F31",
                        column: x => x.concept_id,
                        principalTable: "Concept",
                        principalColumn: "concept_id");
                    table.ForeignKey(
                        name: "FK__Wish_list__produ__0F624AF8",
                        column: x => x.product_id,
                        principalTable: "Product",
                        principalColumn: "product_id");
                    table.ForeignKey(
                        name: "FK__Wish_list__user___0E6E26BF",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetail",
                columns: table => new
                {
                    payment_detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payment_id = table.Column<int>(type: "int", nullable: true),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    booking_id = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentD__C66E6E36678D2E3A", x => x.payment_detail_id);
                    table.ForeignKey(
                        name: "FK__PaymentDe__booki__2A164134",
                        column: x => x.booking_id,
                        principalTable: "Booking",
                        principalColumn: "booking_id");
                    table.ForeignKey(
                        name: "FK__PaymentDe__order__29221CFB",
                        column: x => x.order_id,
                        principalTable: "Order",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "FK__PaymentDe__payme__282DF8C2",
                        column: x => x.payment_id,
                        principalTable: "Payment",
                        principalColumn: "payment_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blog_author_id",
                table: "Blog",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_BlogCategoryCategoryId",
                table: "Blog",
                column: "BlogCategoryCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_category_id",
                table: "Blog",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_UserId",
                table: "Blog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Comment_blog_id",
                table: "Blog_Comment",
                column: "blog_id");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Comment_user_id",
                table: "Blog_Comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_Comment_UserId1",
                table: "Blog_Comment",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_concept_id",
                table: "Booking",
                column: "concept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_user_id",
                table: "Booking",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CassoTransactions_OrderId",
                table: "CassoTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CassoTransactions_UserId",
                table: "CassoTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CassoTransactions_When",
                table: "CassoTransactions",
                column: "When");

            migrationBuilder.CreateIndex(
                name: "IX_Concept_ambience_id",
                table: "Concept",
                column: "ambience_id");

            migrationBuilder.CreateIndex(
                name: "IX_Concept_category_id",
                table: "Concept",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Concept_location_id",
                table: "Concept",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_Concept_Color_Junction_color_id",
                table: "Concept_Color_Junction",
                column: "color_id");

            migrationBuilder.CreateIndex(
                name: "IX_Concept_img_concept_id",
                table: "Concept_img",
                column: "concept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Content_author_id",
                table: "Content",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_Email_Template_created_by",
                table: "Email_Template",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "UQ__Email_Te__72E12F1B063E9FC9",
                table: "Email_Template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Email_Te__72E12F1B3FDC91E4",
                table: "Email_Template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_partner_id",
                table: "Location",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_user_id",
                table: "Notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_shipping_option_id",
                table: "Order",
                column: "shipping_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_user_id",
                table: "Order",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_order_id",
                table: "OrderItem",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_product_id",
                table: "OrderItem",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Partner_user_id",
                table: "Partner",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_payment_method_id",
                table: "Payment",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_UserId",
                table: "Payment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_booking_id",
                table: "PaymentDetail",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_order_id",
                table: "PaymentDetail",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_payment_id",
                table: "PaymentDetail",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatus_PaymentId",
                table: "PaymentStatus",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_category_id",
                table: "Product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_img_product_id",
                table: "Product_img",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Review_user_id",
                table: "Review",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Role__783254B1CC21FE4C",
                table: "Role",
                column: "role_name",
                unique: true,
                filter: "[role_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Role__783254B1CCD27224",
                table: "Role",
                column: "role_name",
                unique: true,
                filter: "[role_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Token_ExpiresActive",
                table: "Token",
                columns: new[] { "expires_at", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_Token_UserTokenType",
                table: "Token",
                columns: new[] { "user_id", "token_type" });

            migrationBuilder.CreateIndex(
                name: "IX_Token_Value",
                table: "Token",
                column: "token_value");

            migrationBuilder.CreateIndex(
                name: "UQ__User__AB6E6164BDEAD157",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__User__AB6E6164DE0E25C7",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__User__B43B145F8C22A302",
                table: "User",
                column: "phone",
                unique: true,
                filter: "[phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__User__B43B145FBDADCA42",
                table: "User",
                column: "phone",
                unique: true,
                filter: "[phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_Role_role_id",
                table: "User_Role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Wish_list_concept_id",
                table: "Wish_list",
                column: "concept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Wish_list_product_id",
                table: "Wish_list",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Wish_list_user_id",
                table: "Wish_list",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blog_Comment");

            migrationBuilder.DropTable(
                name: "CassoTransactions");

            migrationBuilder.DropTable(
                name: "Concept_Color_Junction");

            migrationBuilder.DropTable(
                name: "Concept_img");

            migrationBuilder.DropTable(
                name: "Content");

            migrationBuilder.DropTable(
                name: "Email_Template");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "PaymentDetail");

            migrationBuilder.DropTable(
                name: "PaymentStatus");

            migrationBuilder.DropTable(
                name: "Product_img");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "User_Role");

            migrationBuilder.DropTable(
                name: "Wish_list");

            migrationBuilder.DropTable(
                name: "Blog");

            migrationBuilder.DropTable(
                name: "Concept_Color");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Blog_Category");

            migrationBuilder.DropTable(
                name: "Concept");

            migrationBuilder.DropTable(
                name: "ShippingOption");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Product_Category");

            migrationBuilder.DropTable(
                name: "Concept_Ambience");

            migrationBuilder.DropTable(
                name: "Concept_Category");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Partner");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
