CREATE PROCEDURE [dbo].[AppUser_Get]
	@Id uniqueidentifier
AS
BEGIN
	SELECT Id, Username, Email, PublicUsername, IsEmailConfirmed
	FROM dbo.AppUser
	WHERE Id = @Id;
END
