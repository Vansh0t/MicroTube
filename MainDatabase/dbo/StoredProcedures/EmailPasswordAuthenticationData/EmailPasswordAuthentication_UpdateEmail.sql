CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_UpdateEmail]
	@UserId uniqueidentifier,
	@Email nvarchar(50),
	@IsEmailConfirmed bit
AS
BEGIN
	UPDATE dbo.AppUser
	SET 
	Email = @Email,
	IsEmailConfirmed = @IsEmailConfirmed
	WHERE Id = @UserId;
END
