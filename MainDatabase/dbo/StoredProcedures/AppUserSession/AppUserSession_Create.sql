CREATE PROCEDURE [dbo].[AppUserSession_Create]
	@UserID uniqueidentifier,
	@Token nvarchar(50),
	@IssuedDateTime datetime,
    @ExpirationDateTime datetime
AS
BEGIN
	INSERT INTO dbo.AppUserSession(UserId, Token, IssuedDateTime, ExpirationDateTime)
	VALUES (@UserID, @Token, @IssuedDateTime, @ExpirationDateTime);
END