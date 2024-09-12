CREATE PROCEDURE [dbo].[AppUser_CreatePasswordEmail]
	@Username nvarchar(50),
	@Email nvarchar(50),
	@PasswordHash nvarchar(100),
	@EmailConfirmationString nvarchar(100),
	@EmailConfirmationStringExpiration datetime,
	@CreatedUserId uniqueidentifier OUTPUT
AS
BEGIN
		DECLARE @OutputTable TABLE(id uniqueidentifier);
		INSERT INTO dbo.AppUser (Username, Email, PublicUsername, IsEmailConfirmed)
		OUTPUT INSERTED.Id INTO @OutputTable(id)
		VALUES (@Username, @Email, @Username, 0);
		
		SET @CreatedUserId = (SELECT id FROM @OutputTable);

		INSERT INTO dbo.EmailPasswordAuthentication(UserId, PasswordHash, EmailConfirmationString, EmailConfirmationStringExpiration)
		VALUES (@CreatedUserId, @PasswordHash, @EmailConfirmationString, @EmailConfirmationStringExpiration);
		
END
