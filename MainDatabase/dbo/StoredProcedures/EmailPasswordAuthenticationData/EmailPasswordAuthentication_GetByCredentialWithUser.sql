CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_GetByCredentialWithUser]
	@Credential nvarchar(50)
AS
BEGIN
	SELECT a.*, u.*
	FROM dbo.EmailPasswordAuthentication a
	INNER JOIN dbo.AppUser u 
	ON a.UserId = u.Id
	AND (u.Username = @Credential OR u.Email = @Credential);
END
