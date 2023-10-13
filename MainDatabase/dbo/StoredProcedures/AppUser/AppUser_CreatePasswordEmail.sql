CREATE PROCEDURE [dbo].[AppUser_CreatePasswordEmail]
	@Username nvarchar(50),
	@Email nvarchar(50),
	@PasswordHash nvarchar(100),
	@EmailConfirmationString nvarchar(100),
	@EmailConfirmationStringExpiration datetime,
	@CreatedUserId int OUTPUT
AS
BEGIN
		INSERT INTO dbo.AppUser (Username, Email, PublicUsername)
		VALUES (@Username, @Email, @Username);

		SET @CreatedUserId = SCOPE_IDENTITY();

		INSERT INTO dbo.EmailPasswordAuthentication(UserId, PasswordHash, EmailConfirmationString, EmailConfirmationStringExpiration)
		VALUES (@CreatedUserId, @PasswordHash, @EmailConfirmationString, @EmailConfirmationStringExpiration);
		
END
