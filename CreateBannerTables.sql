-- Create Banner Service table
CREATE TABLE [dbo].[BannerService](
    [banner_id] [int] IDENTITY(1,1) NOT NULL,
    [img_url] [nvarchar](500) NOT NULL,
    [is_primary] [bit] NOT NULL DEFAULT 0,
    [display_order] [int] NOT NULL DEFAULT 0,
    [description] [nvarchar](1000) NULL,
    [is_active] [bit] NOT NULL DEFAULT 1,
    [created_at] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [updated_at] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_BannerService] PRIMARY KEY CLUSTERED ([banner_id] ASC)
) ON [PRIMARY];

-- Create Banner Homepage table
CREATE TABLE [dbo].[BannerHomepage](
    [banner_id] [int] IDENTITY(1,1) NOT NULL,
    [img_url] [nvarchar](500) NOT NULL,
    [is_primary] [bit] NOT NULL DEFAULT 0,
    [display_order] [int] NOT NULL DEFAULT 0,
    [description] [nvarchar](1000) NULL,
    [is_active] [bit] NOT NULL DEFAULT 1,
    [created_at] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [updated_at] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_BannerHomepage] PRIMARY KEY CLUSTERED ([banner_id] ASC)
) ON [PRIMARY];

-- Create Banner Product table
CREATE TABLE [dbo].[BannerProduct](
    [banner_id] [int] IDENTITY(1,1) NOT NULL,
    [img_url] [nvarchar](500) NOT NULL,
    [is_primary] [bit] NOT NULL DEFAULT 0,
    [display_order] [int] NOT NULL DEFAULT 0,
    [description] [nvarchar](1000) NULL,
    [is_active] [bit] NOT NULL DEFAULT 1,
    [created_at] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [updated_at] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_BannerProduct] PRIMARY KEY CLUSTERED ([banner_id] ASC)
) ON [PRIMARY];

-- Insert sample data for BannerService
INSERT INTO [dbo].[BannerService] ([img_url], [is_primary], [display_order], [description], [is_active])
VALUES 
    ('/images/banners/service-1.jpg', 1, 1, 'Dịch vụ cưới hỏi chuyên nghiệp', 1),
    ('/images/banners/service-2.jpg', 0, 2, 'Trang trí tiệc cưới sang trọng', 1),
    ('/images/banners/service-3.jpg', 0, 3, 'Chụp ảnh cưới đẹp mắt', 1);

-- Insert sample data for BannerHomepage
INSERT INTO [dbo].[BannerHomepage] ([img_url], [is_primary], [display_order], [description], [is_active])
VALUES 
    ('/images/banners/homepage-1.jpg', 1, 1, 'Banner chính trang chủ - Dịch vụ cưới hỏi cao cấp', 1),
    ('/images/banners/homepage-2.jpg', 0, 2, 'Banner phụ trang chủ - Ưu đãi đặc biệt', 1),
    ('/images/banners/homepage-3.jpg', 0, 3, 'Banner phụ trang chủ - Sản phẩm mới', 1);

-- Insert sample data for BannerProduct
INSERT INTO [dbo].[BannerProduct] ([img_url], [is_primary], [display_order], [description], [is_active])
VALUES 
    ('/images/banners/product-1.jpg', 1, 1, 'Banner sản phẩm chính - Bộ sưu tập mới', 1),
    ('/images/banners/product-2.jpg', 0, 2, 'Banner sản phẩm phụ - Giảm giá sốc', 1),
    ('/images/banners/product-3.jpg', 0, 3, 'Banner sản phẩm phụ - Sản phẩm bán chạy', 1);

PRINT 'Banner tables created successfully with sample data!';
