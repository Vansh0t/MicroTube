CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_Get]
	@UserID int
AS
BEGIN
	SELECT * 
	FROM dbo.EmailPasswordAuthentication
	WHERE UserId = @UserID;
END
