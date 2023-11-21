CREATE TABLE [dbo].[AppUserSession]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NOT NULL FOREIGN KEY REFERENCES dbo.AppUser(Id) ON DELETE CASCADE,
	[Token] nvarchar(50) NOT NULL UNIQUE,
	[PreviousToken] nvarchar(50), 
    [IssuedDateTime] DATETIME NOT NULL, 
    [ExpirationDateTime] DATETIME NOT NULL,
	[IsInvalidated] bit NOT NULL DEFAULT 0
)
