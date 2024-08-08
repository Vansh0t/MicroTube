﻿CREATE TABLE [dbo].[Video]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[Title] NVARCHAR(200) NOT NULL,
	[Description] NVARCHAR(1000) DEFAULT NULL,
	[UploaderId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.AppUser(Id),
	[Urls] NVARCHAR(2000) NOT NULL,
	[ThumbnailUrls] VARCHAR(MAX) DEFAULT NULL, 
    [SnapshotUrls] VARCHAR(MAX) NULL DEFAULT NULL,
	[UploadTime] DATETIME NOT NULL, 
    [LengthSeconds] INT NOT NULL DEFAULT 1, 
    [SearchIndexId] NVARCHAR(50) NULL, 
    [Views] INT NOT NULL DEFAULT 0, 
    [Likes] INT NOT NULL DEFAULT 0 
)
