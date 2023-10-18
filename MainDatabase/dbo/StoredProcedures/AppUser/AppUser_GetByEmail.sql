CREATE PROCEDURE [dbo].[AppUser_GetByEmail]
	@Email nvarchar(50)
AS
BEGIN
	SELECT Id, Username, Email, PublicUsername, IsEmailConfirmed
	FROM dbo.AppUser
	WHERE Email = @Email;
END
