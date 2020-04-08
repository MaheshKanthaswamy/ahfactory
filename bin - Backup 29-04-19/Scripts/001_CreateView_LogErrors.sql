CREATE VIEW [dbo].[LogErrors]
AS
SELECT      TOP (100) PERCENT Id, Author, Message, Detail, CreationDate, [Assembly], Source
FROM          dbo.Logs
WHERE      ([Level] = N'ERROR')