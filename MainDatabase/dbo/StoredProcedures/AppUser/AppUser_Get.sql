﻿CREATE PROCEDURE [dbo].[AppUser_Get]
	@Id int
AS
BEGIN
	SELECT Id, Username, Email, PublicUsername, IsEmailConfirmed
	FROM dbo.AppUser
	WHERE Id = @Id;
END
