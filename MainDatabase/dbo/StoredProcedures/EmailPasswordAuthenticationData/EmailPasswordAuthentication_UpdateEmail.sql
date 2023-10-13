CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_UpdateEmail]
	@UserId int,
	@Email nvarchar(50)
AS
BEGIN
	UPDATE dbo.AppUser
	SET 
	Email = @Email
	WHERE Id = @UserId;
END
