CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_GetByEmailString]
	@EmailConfirmationString nvarchar(100)
AS
BEGIN
	SELECT a.*, u.*
	FROM dbo.EmailPasswordAuthentication a
	INNER JOIN dbo.AppUser u 
	ON a.UserId = u.Id
	AND a.EmailConfirmationString = @EmailConfirmationString;
END
