CREATE TABLE [dbo].[Account]
(
    [AccountId] INT NOT NULL IDENTITY,
    [Name] NVARCHAR(200) NOT NULL,
    [Type] SMALLINT,
    [CurrencyCode] NVARCHAR(20) NOT NULL,
    [Balance] DECIMAL(38,4) NOT NULL,
    [ShowInSummary] BIT,
    [OwnerId] INT NOT NULL,

    CONSTRAINT FK_Account_AppUser_OwnerId FOREIGN KEY ([OwnerId])
        REFERENCES [AppUser]([Id])
        ON DELETE CASCADE,

    CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ([AccountId])
);
GO
