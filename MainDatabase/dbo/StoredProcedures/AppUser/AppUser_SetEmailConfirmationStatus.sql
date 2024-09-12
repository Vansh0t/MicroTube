CREATE PROCEDURE [dbo].[AppUser_SetEmailConfirmationStatus]
	@Id uniqueidentifier,
	@IsEmailConfirmed bit
AS
BEGIN
	UPDATE dbo.AppUser
	SET IsEmailConfirmed = @IsEmailConfirmed
	WHERE Id = @Id;
END
