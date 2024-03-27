﻿CREATE TABLE [dbo].[VideoUploadProgress]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
	[LastReportedTimestamp] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [Status] INT NOT NULL DEFAULT 0, 
    [UploaderId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES dbo.AppUser(Id) ON DELETE CASCADE,
	[Title] NVARCHAR(200) NOT NULL,
	[Description] NVARCHAR(1000) DEFAULT NULL, 
    [RemoteCacheLocation] NVARCHAR(50) NOT NULL, 
    [RemoteCacheFileName] NVARCHAR(50) NOT NULL, 
    [Message] NVARCHAR(100) NULL DEFAULT NULL, 
    [LengthSeconds] INT NULL DEFAULT NULL, 
    [Format] NVARCHAR(20) NULL DEFAULT NULL, 
    [FrameSize] NVARCHAR(50) NULL DEFAULT NULL, 
    [Fps] INT NULL DEFAULT NULL, 
    [Timestamp] DATETIME NOT NULL DEFAULT GETUTCDATE()
)
