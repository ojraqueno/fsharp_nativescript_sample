CREATE TABLE [dbo].[Users]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Email] NVARCHAR(256) NULL, 
    [Username] NVARCHAR(256) NULL, 
    [PasswordSalt] INT NULL, 
    [PasswordHash] NVARCHAR(128) NULL
)
