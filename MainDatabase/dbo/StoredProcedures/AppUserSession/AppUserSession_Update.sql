CREATE PROCEDURE [dbo].[AppUserSession_Update]
	@Id uniqueidentifier,
	@Token nvarchar(50),
	@IssuedDateTime datetime,
    @ExpirationDateTime datetime,
	@IsInvalidated bit
AS
BEGIN
	UPDATE dbo.AppUserSession 
	SET 
	Token = @Token,
	IssuedDateTime = @IssuedDateTime,
	ExpirationDateTime = @ExpirationDateTime,
	IsInvalidated = @IsInvalidated
	WHERE Id = @Id;
END