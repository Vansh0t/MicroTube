CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_UpdatePassword]
	@UserId int,
	@PasswordHash nvarchar(100)
AS
BEGIN
	UPDATE dbo.EmailPasswordAuthentication
	SET 
	PasswordHash = @PasswordHash
	WHERE UserId = @UserId;
END
