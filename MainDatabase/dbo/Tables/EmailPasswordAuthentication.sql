CREATE TABLE [dbo].[EmailPasswordAuthentication]
(
	[UserId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES dbo.AppUser(Id) ON DELETE CASCADE,
	[PasswordHash] NVARCHAR(100) NOT NULL,
	[IsEmailConfirmed] BIT DEFAULT 0 NOT NULL,
	[EmailConfirmationString] NVARCHAR(100),
	[EmailConfirmationStringExpiration] DATETIME,
	[PasswordResetString] NVARCHAR(100),
	[PasswordResetStringExpiration] DATETIME, 
    [PendingEmail] NVARCHAR(50) NULL
)
