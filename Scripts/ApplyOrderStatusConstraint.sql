-- Script to apply Order status constraint
-- This script ensures Order status can only be: 'cart', 'confirmed', 'shipping', 'completed'

GO

DECLARE @sql NVARCHAR(MAX) = N'';

;WITH C AS (
    SELECT c.name AS ck_name
    FROM sys.check_constraints c
    JOIN sys.objects o ON o.object_id = c.parent_object_id
    JOIN sys.columns col ON col.object_id = o.object_id AND col.column_id = c.parent_column_id
    WHERE o.name = 'Order' AND col.name = 'status'
)
SELECT @sql = STRING_AGG(N'ALTER TABLE [dbo].[Order] DROP CONSTRAINT [' + ck_name + N'];', CHAR(10))
FROM C;

IF @sql IS NOT NULL AND LEN(@sql) > 0
    EXEC sp_executesql @sql;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints 
    WHERE name = 'CK_Order_Status' AND parent_object_id = OBJECT_ID('[dbo].[Order]')
)
BEGIN
    ALTER TABLE [dbo].[Order]
    ADD CONSTRAINT [CK_Order_Status]
    CHECK ([status] IN ('cart','confirmed','shipping','completed'));
END
ELSE
BEGIN
    -- Nếu tên đã tồn tại nhưng (giả sử) definition cũ, thì drop và tạo lại
    ALTER TABLE [dbo].[Order] DROP CONSTRAINT [CK_Order_Status];
    ALTER TABLE [dbo].[Order]
    ADD CONSTRAINT [CK_Order_Status]
    CHECK ([status] IN ('cart','confirmed','shipping','completed'));
END
GO

-- Verify the constraint was created
SELECT 
    cc.name AS constraint_name,
    cc.definition,
    o.name AS table_name,
    c.name AS column_name
FROM sys.check_constraints cc
JOIN sys.objects o ON o.object_id = cc.parent_object_id
JOIN sys.columns c ON c.object_id = o.object_id AND c.column_id = cc.parent_column_id
WHERE o.name = 'Order' AND c.name = 'status';

PRINT 'Order status constraint applied successfully!';
PRINT 'Valid status values: cart, confirmed, shipping, completed';
