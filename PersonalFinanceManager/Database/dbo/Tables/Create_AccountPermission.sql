CREATE TABLE [dbo].[AccountPermission] (
    [AccountId] INT NOT NULL,
    [AppUserId] INT NOT NULL,
    [PermissionType] INT NOT NULL,
    CONSTRAINT [PK_AccountPermission] PRIMARY KEY ([AccountId], [AppUserId]),
    CONSTRAINT [FK_AccountPermission_Account] FOREIGN KEY ([AccountId])
        REFERENCES [Account]([AccountId]),
    CONSTRAINT [FK_AccountPermission_AppUser] FOREIGN KEY ([AppUserId])
        REFERENCES [AppUser]([Id])
);
GO
