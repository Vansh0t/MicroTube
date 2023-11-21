CREATE PROCEDURE [dbo].[AppUserSession_Create]
	@UserID int,
	@Token nvarchar(50),
	@IssuedDateTime datetime,
    @ExpirationDateTime datetime
AS
BEGIN
	INSERT INTO dbo.AppUserSession(UserId, Token, PreviousToken, IssuedDateTime, ExpirationDateTime)
	VALUES (@UserID, @Token, NULL, @IssuedDateTime, @ExpirationDateTime);
END