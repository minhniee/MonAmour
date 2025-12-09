USE MonAmourDb;

-- Insert Reviews với ảnh từ thư mục wwwroot/Imagine/IMGProduct

-- ============================================
-- NHÓM NẾN THƠM (Scented Candles)
-- ============================================

-- Review 1: Coffee Caramel Candle (product_id = 4)
-- User: Trương Thái Anh (user_id = 1012)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1012, 'product', 4, 5, 
    N'Mùi thơm kiểu thoang thoảng thôi chứ không bị gắt đâu, dễ chịu lắm. Mua tặng mà bạn người yêu mình cứ khen suốt, thấy vui ghê! 😙',
    '/Imagine/IMGProduct/scentedCandles/nen1.jpg',
    GETDATE(), GETDATE());

-- Review 2: Coffee Caramel Candle (product_id = 4)
-- User: Đặng Quốc Hưng (user_id = 1011)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1011, 'product', 4, 5,
    N'Đi làm về mệt, đốt chút nến lên thấy cái phòng nó nhẹ nhõm hẳn luôn. Mê cái sự dịu dàng này quá đi.',
    '/Imagine/IMGProduct/scentedCandles/nen2.jpg',
    DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()));

-- Review 3: Sweet Tea Candle (product_id = 5)
-- User: Vũ Thị Thúy Anh (user_id = 1013)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1013, 'product', 5, 5,
    N'Shop gói hàng có tâm xỉu, nhận cái hộp mà tưởng đâu quà ai tặng không á. Nến xinh, hộp đẹp, mở ra là thấy vui rồi.',
    '/Imagine/IMGProduct/scentedCandles/nen3.jpg',
    DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE()));

-- Review 4: Silent Night Candle (product_id = 6)
-- User: Lê Quỳnh Chi (user_id = 1031)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1031, 'product', 6, 5,
    N'Ưng cái vibe nhẹ nhàng này ghê. Chắc chắn sẽ ghé shop ủng hộ dài dài nè.',
    '/Imagine/IMGProduct/scentedCandles/nen1.jpg',
    DATEADD(day, -7, GETDATE()), DATEADD(day, -7, GETDATE()));

-- Review 5: Coffee Caramel Candle (product_id = 4) - Review có ảnh
-- User: Phạm Bích Phương (user_id = 1032)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1032, 'product', 4, 5,
    N'Eo ơi cái mùi nó nịnh mũi dã man, kiểu ngọt nhẹ chứ không hề bị hắc hay đau đầu đâu nha. Duyệt! 🌸',
    '/Imagine/IMGProduct/scentedCandles/nen2.jpg',
    DATEADD(day, -10, GETDATE()), DATEADD(day, -10, GETDATE()));

-- Review 6: Sweet Tea Candle (product_id = 5) - Review có ảnh
-- User: Phạm Thế Trường Vũ (user_id = 1008)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1008, 'product', 5, 5,
    N'Để hũ nến ở góc bàn làm việc vừa thơm vừa xinh, nhìn chill chill có động lực cày deadline hẳn. ✨',
    '/Imagine/IMGProduct/scentedCandles/nen3.jpg',
    DATEADD(day, -12, GETDATE()), DATEADD(day, -12, GETDATE()));

-- ============================================
-- NHÓM VÒNG TAY & NHẪN ĐÔI
-- ============================================

-- Review 7: Cặp Nhẫn Tình Nhân (product_id = 1)
-- User: Nguyễn Văn Chiến (user_id = 1008) - Lưu ý: user này đã review product 5, nên dùng user khác
-- Thay bằng user khác để tránh trùng
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1027, 'product', 1, 5,
    N'Kỷ niệm ngày yêu mua set này đúng chuẩn bài luôn. Vòng xinh mà hộp cũng xịn, trọn vẹn cảm xúc lắm.',
    '/Imagine/IMGProduct/ring/nhan1.jpeg',
    DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE()));

-- Review 8: Cặp Nhẫn Tình Nhân (product_id = 1)
-- User: Nguyễn Hiệp (user_id = 1009)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1009, 'product', 1, 5,
    N'Đơn giản mà lên tay sang lắm nha. Tặng xong thấy người ấy cười tít mắt là biết ưng rồi 🥰',
    '/Imagine/IMGProduct/ring/nhan2.jpeg',
    DATEADD(day, -6, GETDATE()), DATEADD(day, -6, GETDATE()));

-- Review 9: Cặp Nhẫn Tình Nhân (product_id = 1)
-- User: Trương Thái Anh (user_id = 1012)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1012, 'product', 1, 5,
    N'Nhìn cách gói ghém là biết shop đặt cái tâm vào thế nào rồi. Cảm ơn shop vì món quà ý nghĩa này nha.',
    '/Imagine/IMGProduct/ring/nhan3.jpeg',
    DATEADD(day, -8, GETDATE()), DATEADD(day, -8, GETDATE()));

-- Review 10: Cặp Nhẫn Tình Nhân (product_id = 1)
-- User: Đặng Quốc Hưng (user_id = 1011)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1011, 'product', 1, 5,
    N'Bất ngờ vì độ tỉ mỉ của shop, từ cái thiệp viết tay đến chiếc vòng đều nét căng. Nhìn bên ngoài còn đẹp hơn ảnh.',
    '/Imagine/IMGProduct/ring/nhan4.jpeg',
    DATEADD(day, -15, GETDATE()), DATEADD(day, -15, GETDATE()));

-- Review 11: Vòng tay đôi Silver Line (product_id = 2)
-- User: Vũ Thị Thúy Anh (user_id = 1013)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1013, 'product', 2, 5,
    N'Tay mình siêu nhỏ luôn mà may quá vòng có nấc chỉnh, đeo lên vừa in xinh xỉu. Mấy bạn tay tấm cứ yên tâm nha.',
    '/Imagine/IMGProduct/bracelet/vong1.jpg',
    DATEADD(day, -4, GETDATE()), DATEADD(day, -4, GETDATE()));

-- Review 12: Vòng tay đôi Love Knot (product_id = 3)
-- User: Lê Quỳnh Chi (user_id = 1031)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1031, 'product', 3, 5,
    N'Cái form nhẫn này đỉnh nha, đeo vào cảm giác tay thon với gọn hẳn. Nhìn tay mình mà mình còn mê!',
    '/Imagine/IMGProduct/bracelet/vong2.jpg',
    DATEADD(day, -9, GETDATE()), DATEADD(day, -9, GETDATE()));

-- Review 13: Vòng tay đôi Silver Line (product_id = 2)
-- User: Phạm Bích Phương (user_id = 1032)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1032, 'product', 2, 5,
    N'Cảm ơn bạn nhân viên tư vấn nhiệt tình nha, mình đeo vừa in luôn, không bị lỏng tí nào. 10 điểm cho sự có tâm này!',
    '/Imagine/IMGProduct/bracelet/vong1.jpg',
    DATEADD(day, -11, GETDATE()), DATEADD(day, -11, GETDATE()));

-- Review 14: Cặp Nhẫn Tình Nhân (product_id = 1) - Thêm review với ảnh
-- User: Vũ Thuý Hạnh (user_id = 1028)
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1028, 'product', 1, 5,
    N'Kỷ niệm ngày yêu mua set này đúng chuẩn bài luôn. Vòng xinh mà hộp cũng xịn, trọn vẹn cảm xúc lắm.',
    '/Imagine/IMGProduct/ring/nhan5.jpeg',
    DATEADD(day, -13, GETDATE()), DATEADD(day, -13, GETDATE()));

-- Review 15: Vòng tay đôi Love Knot (product_id = 3) - Thêm review với ảnh
-- User: Nguyễn Đức Anh (user_id = 1029) - Thay user khác vì 1009 đã review product 1
INSERT INTO Review (user_id, target_type, target_id, rating, comment, image_url, created_at, updated_at)
VALUES (1029, 'product', 3, 5,
    N'Đơn giản mà lên tay sang lắm nha. Tặng xong thấy người ấy cười tít mắt là biết ưng rồi 🥰',
    '/Imagine/IMGProduct/ring/nhan6.jpeg',
    DATEADD(day, -14, GETDATE()), DATEADD(day, -14, GETDATE()));

PRINT 'Đã thêm thành công các review với ảnh!';

