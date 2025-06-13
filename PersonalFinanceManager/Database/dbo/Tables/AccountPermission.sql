CREATE TABLE [dbo].[AccountPermission] (
    [AccountPermissionId] INT NOT NULL IDENTITY,
    [AccountId] INT NOT NULL,
    [AppUserId] INT NOT NULL,
    [PermissionType] INT NOT NULL,
    CONSTRAINT [PK_AccountPermission] PRIMARY KEY ([AccountPermissionId]),
    CONSTRAINT [UN_AccountPermission_AccountId_AppUserId] UNIQUE ([AccountId], [AppUserId]),
    CONSTRAINT [FK_AccountPermission_Account] FOREIGN KEY ([AccountId])
        REFERENCES [Account]([AccountId]),
    CONSTRAINT [FK_AccountPermission_AppUser] FOREIGN KEY ([AppUserId])
        REFERENCES [AppUser]([Id])
);
GO
