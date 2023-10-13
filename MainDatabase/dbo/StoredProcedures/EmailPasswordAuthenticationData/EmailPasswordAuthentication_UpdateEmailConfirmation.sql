CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_UpdateEmailConfirmation]
	@UserId int,
	@EmailConfirmationString nvarchar(100),
	@EmailConfirmationStringExpiration datetime,
	@PendingEmail nvarchar(50)
AS
BEGIN
	UPDATE dbo.EmailPasswordAuthentication
	SET 
	EmailConfirmationString = @EmailConfirmationString,
	EmailConfirmationStringExpiration = @EmailConfirmationStringExpiration,
	PendingEmail = @PendingEmail
	WHERE UserId = @UserId;
END
