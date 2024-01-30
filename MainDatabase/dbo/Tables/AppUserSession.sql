﻿CREATE TABLE [dbo].[AppUserSession]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(), 
    [UserId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.AppUser(Id) ON DELETE CASCADE,
	[Token] NVARCHAR(50) NOT NULL UNIQUE,
    [IssuedDateTime] DATETIME NOT NULL, 
    [ExpirationDateTime] DATETIME NOT NULL,
	[IsInvalidated] bit NOT NULL DEFAULT 0
)