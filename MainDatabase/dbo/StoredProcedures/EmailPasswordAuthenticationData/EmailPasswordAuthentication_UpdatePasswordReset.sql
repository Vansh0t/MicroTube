CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_UpdatePasswordReset]
	@UserId int,
	@PasswordResetString nvarchar(100),
	@PasswordResetStringExpiration datetime
AS
BEGIN
	UPDATE dbo.EmailPasswordAuthentication
	SET 
	PasswordResetString = @PasswordResetString,
	PasswordResetStringExpiration = @PasswordResetStringExpiration
	WHERE UserId = @UserId;
END
