CREATE PROCEDURE [dbo].[AppUserSession_GetByToken]
	@Token nvarchar(50)
AS
BEGIN
	SELECT * FROM dbo.AppUserSession WHERE Token = @Token;
END