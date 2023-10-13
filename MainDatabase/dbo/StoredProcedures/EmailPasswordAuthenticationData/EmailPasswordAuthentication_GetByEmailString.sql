CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_GetByEmailString]
	@EmailConfirmationString nvarchar(100)
AS
BEGIN
	SELECT * 
	FROM dbo.EmailPasswordAuthentication
	WHERE EmailConfirmationString = @EmailConfirmationString;
END
