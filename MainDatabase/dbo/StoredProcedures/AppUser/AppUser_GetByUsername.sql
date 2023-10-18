CREATE PROCEDURE [dbo].[AppUser_GetByUsername]
	@Username nvarchar(50)
AS
BEGIN
	SELECT Id, Username, Email, PublicUsername, IsEmailConfirmed
	FROM dbo.AppUser
	WHERE Username = @Username;
END
