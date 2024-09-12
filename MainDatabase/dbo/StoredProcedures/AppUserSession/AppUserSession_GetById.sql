CREATE PROCEDURE [dbo].[AppUserSession_GetById]
	@Id uniqueidentifier
AS
BEGIN
	SELECT usedToken.*, userSession.*
	FROM dbo.UsedRefreshToken usedToken
	RIGHT JOIN dbo.AppUserSession userSession
	ON usedToken.SessionId  =  userSession.Id
	WHERE userSession.Id = @Id
END
