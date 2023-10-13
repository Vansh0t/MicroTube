CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_UpdateEmailConfirmation]
	@UserId int,
	@EmailConfirmationString nvarchar(100),
	@EmailConfirmationStringExpiration datetime
AS
BEGIN
	UPDATE dbo.EmailPasswordAuthentication
	SET 
	EmailConfirmationString = @EmailConfirmationString,
	EmailConfirmationStringExpiration = @EmailConfirmationStringExpiration
	WHERE UserId = @UserId;
END
