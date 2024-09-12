CREATE PROCEDURE [dbo].[UsedRefreshToken_Create]
	@SessionId uniqueidentifier,
	@Token nvarchar(50)
AS
BEGIN
	INSERT INTO dbo.UsedRefreshToken (SessionId, Token)
	VALUES (@SessionId, @Token); 
END
