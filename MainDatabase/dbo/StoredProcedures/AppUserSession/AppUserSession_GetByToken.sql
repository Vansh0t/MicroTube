CREATE PROCEDURE [dbo].[AppUserSession_GetByToken]
	@Token nvarchar(50)
AS
BEGIN
	SELECT usedToken.*, userSession.*
	FROM dbo.UsedRefreshToken usedToken
	RIGHT JOIN dbo.AppUserSession userSession
	ON usedToken.SessionId  =  userSession.Id
	WHERE userSession.Token =@Token
END