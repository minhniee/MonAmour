-- Script to fix Order status constraint to include 'cancelled' status
-- This script removes the existing constraint and adds a new one with 'cancelled' status

DECLARE @sql NVARCHAR(MAX) = N'';

-- Find and drop existing check constraints on Order.status column
;WITH C AS (
    SELECT c.name AS ck_name
    FROM sys.check_constraints c
    JOIN sys.objects o ON o.object_id = c.parent_object_id
    JOIN sys.columns col ON col.object_id = o.object_id AND col.column_id = c.parent_column_id
    WHERE o.name = 'Order' AND col.name = 'status'
)
SELECT @sql = STRING_AGG(N'ALTER TABLE [dbo].[Order] DROP CONSTRAINT [' + ck_name + N'];', CHAR(10))
FROM C;

-- Execute the DROP CONSTRAINT statements
IF @sql IS NOT NULL AND LEN(@sql) > 0
    EXEC sp_executesql @sql;

-- Add new constraint with 'cancelled' status included
ALTER TABLE [dbo].[Order]
ADD CONSTRAINT [CK_Order_Status]
CHECK ([status] IN ('cart','pending','confirmed','shipping','completed','cancelled'));

PRINT 'Order status constraint updated successfully to include cancelled status';
GO
