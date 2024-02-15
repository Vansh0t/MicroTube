CREATE PROCEDURE [dbo].[EmailPasswordAuthentication_Get]
	@UserID uniqueidentifier
AS
BEGIN
	SELECT * 
	FROM dbo.EmailPasswordAuthentication
	WHERE UserId = @UserID;
END
