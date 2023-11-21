CREATE PROCEDURE [dbo].[AppUserSession_Update]
	@Id int,
	@Token nvarchar(50),
	@PreviousToken nvarchar(50),
	@IssuedDateTime datetime,
    @ExpirationDateTime datetime,
	@IsInvalidated bit
AS
BEGIN
	UPDATE dbo.AppUserSession 
	SET 
	Token = @Token,
	PreviousToken = @PreviousToken,
	IssuedDateTime = @IssuedDateTime,
	ExpirationDateTime = @ExpirationDateTime,
	IsInvalidated = @IsInvalidated
	WHERE Id = @Id;
END