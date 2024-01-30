CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_GetWithUser]
	@UserID uniqueidentifier
AS
BEGIN
	SELECT a.*, u.*
	FROM dbo.EmailPasswordAuthentication a
	INNER JOIN dbo.AppUser u 
	ON a.UserId = u.Id
	AND a.UserId = @UserID;
END
