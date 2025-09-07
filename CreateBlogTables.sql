-- Script to create Blog related tables for MonAmour database

USE MonAmourDb;
GO

-- Create Blog_Category table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Blog_Category' AND xtype='U')
BEGIN
    CREATE TABLE Blog_Category (
        category_id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(255),
        slug NVARCHAR(50),
        is_active BIT DEFAULT 1,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );
    
    PRINT 'Blog_Category table created successfully';
END
ELSE
BEGIN
    PRINT 'Blog_Category table already exists';
END
GO

-- Create Blog table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Blog' AND xtype='U')
BEGIN
    CREATE TABLE Blog (
        blog_id INT IDENTITY(1,1) PRIMARY KEY,
        title NVARCHAR(255) NOT NULL,
        content NTEXT NOT NULL,
        excerpt NVARCHAR(500),
        featured_image NVARCHAR(255),
        author_id INT,
        category_id INT,
        tags NVARCHAR(255),
        published_date DATETIME,
        is_featured BIT DEFAULT 0,
        is_published BIT DEFAULT 0,
        read_time INT DEFAULT 0,
        view_count INT DEFAULT 0,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_Blog_Author FOREIGN KEY (author_id) REFERENCES [User](user_id),
        CONSTRAINT FK_Blog_Category FOREIGN KEY (category_id) REFERENCES Blog_Category(category_id)
    );
    
    PRINT 'Blog table created successfully';
END
ELSE
BEGIN
    PRINT 'Blog table already exists';
END
GO

-- Create Blog_Comment table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Blog_Comment' AND xtype='U')
BEGIN
    CREATE TABLE Blog_Comment (
        comment_id INT IDENTITY(1,1) PRIMARY KEY,
        blog_id INT NOT NULL,
        user_id INT,
        author_name NVARCHAR(100),
        author_email NVARCHAR(255),
        content NTEXT NOT NULL,
        is_approved BIT DEFAULT 0,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_BlogComment_Blog FOREIGN KEY (blog_id) REFERENCES Blog(blog_id) ON DELETE CASCADE,
        CONSTRAINT FK_BlogComment_User FOREIGN KEY (user_id) REFERENCES [User](user_id)
    );
    
    PRINT 'Blog_Comment table created successfully';
END
ELSE
BEGIN
    PRINT 'Blog_Comment table already exists';
END
GO

-- Insert sample blog categories
IF NOT EXISTS (SELECT * FROM Blog_Category)
BEGIN
    INSERT INTO Blog_Category (name, description, slug, is_active) VALUES
    ('Tin tức', 'Tin tức mới nhất', 'tin-tuc', 1),
    ('Sự kiện', 'Các sự kiện đặc biệt', 'su-kien', 1),
    ('Lời khuyên', 'Lời khuyên hữu ích', 'loi-khuyen', 1),
    ('Câu chuyện tình yêu', 'Những câu chuyện tình yêu đẹp', 'cau-chuyen-tinh-yeu', 1);
    
    PRINT 'Sample blog categories inserted successfully';
END
GO

-- Insert sample blogs
IF NOT EXISTS (SELECT * FROM Blog)
BEGIN
    DECLARE @CategoryId1 INT = (SELECT category_id FROM Blog_Category WHERE slug = 'su-kien');
    DECLARE @CategoryId2 INT = (SELECT category_id FROM Blog_Category WHERE slug = 'tin-tuc');
    DECLARE @CategoryId3 INT = (SELECT category_id FROM Blog_Category WHERE slug = 'loi-khuyen');
    
    INSERT INTO Blog (title, content, excerpt, featured_image, category_id, tags, published_date, is_featured, is_published, read_time) VALUES
    (
        'EMPOWERING AT-RISK AND DISADVANTAGED YOUTH IN VIETNAM',
        '<p>Khám phá những trải nghiệm lãng mạn độc đáo được thiết kế đặc biệt cho các cặp đôi. Từ bữa tối dưới ánh nến đến những chuyến picnic riêng tư trong không gian tuyệt đẹp.</p><p>Chúng tôi mang đến cho bạn những khoảnh khắc đáng nhớ nhất với người thương. Mỗi concept được thiết kế tỉ mỉ để tạo nên những kỷ niệm không thể quên.</p>',
        'Khám phá những trải nghiệm lãng mạn độc đáo được thiết kế đặc biệt cho các cặp đôi. Từ bữa tối dưới ánh nến đến những chuyến picnic riêng tư...',
        '/romantic-dinner-setup.jpg',
        @CategoryId1,
        'sự kiện,lãng mạn,cặp đôi',
        '2025-08-15',
        1,
        1,
        5
    ),
    (
        'THE HISTORY OF KOTO',
        '<p>Jimmy Pham has been in Vietnam in 1996 and moved to Australia as a young child with his brother and mother, spending his formative years in Sydney...</p><p>This is the inspiring story of how KOTO began and the impact it has made on countless young lives in Vietnam.</p>',
        'Jimmy Pham has been in Vietnam in 1996 and moved to Australia as a young child with his brother and mother, spending his formative years in Sydney...',
        '/romantic-picnic-setup.jpg',
        @CategoryId2,
        'lịch sử,KOTO,câu chuyện',
        '2025-08-10',
        0,
        1,
        8
    ),
    (
        'FOUNDER''S MESSAGE',
        '<p>I am humbled that my dreams to provide a mixed medium with training, a stable income and just workplace in a social business such as I hope has...</p><p>Our mission continues to evolve as we empower more young people to build better futures for themselves and their communities.</p>',
        'I am humbled that my dreams to provide a mixed medium with training, a stable income and just workplace in a social business such as I hope has...',
        '/luxury-anniversary-gift-box.jpg',
        @CategoryId3,
        'thông điệp,nhà sáng lập,tầm nhìn',
        '2025-08-05',
        0,
        1,
        6
    ),
    (
        'NEWS',
        '<p>KOTO staff and volunteers are privileged to visit the business initiatives from Hanoi, the foundation''s key young...</p><p>Stay updated with our latest developments and community initiatives that are making a real difference.</p>',
        'KOTO staff and volunteers are privileged to visit the business initiatives from Hanoi, the foundation''s key young...',
        '/rooftop-cinema-romantic-setup.jpg',
        @CategoryId2,
        'tin tức,cộng đồng,hoạt động',
        '2025-08-01',
        0,
        1,
        4
    );
    
    -- Insert additional WE ARE KOTO posts
    INSERT INTO Blog (title, content, excerpt, category_id, tags, published_date, is_featured, is_published, read_time) VALUES
    ('WE ARE KOTO', '<p>Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aenean lobortis elementum tellus...</p>', 'Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aen...', @CategoryId2, 'KOTO,giới thiệu', '2025-07-28', 0, 1, 3),
    ('WE ARE KOTO', '<p>Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aenean lobortis elementum tellus...</p>', 'Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aen...', @CategoryId2, 'KOTO,giới thiệu', '2025-07-25', 0, 1, 3),
    ('WE ARE KOTO', '<p>Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aenean lobortis elementum tellus...</p>', 'Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aen...', @CategoryId2, 'KOTO,giới thiệu', '2025-07-22', 0, 1, 3),
    ('WE ARE KOTO', '<p>Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aenean lobortis elementum tellus...</p>', 'Lorem ipsum dolor sit amet consectetur. Ac hac sociis arcu aen...', @CategoryId2, 'KOTO,giới thiệu', '2025-07-20', 0, 1, 3);
    
    PRINT 'Sample blogs inserted successfully';
END
GO

PRINT 'Blog tables and sample data setup completed successfully!';
