CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_GetByPasswordString]
	@PasswordResetString nvarchar(100)
AS
BEGIN
	SELECT * 
	FROM dbo.EmailPasswordAuthentication
	WHERE PasswordResetString = @PasswordResetString;
END
