CREATE TABLE [dbo].[Transaction] (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Address NVARCHAR(500) NULL, -- Opcjonalne pole, mo�e by� NULL
    Description NVARCHAR(1000) NULL, -- Opcjonalne pole, mo�e by� NULL
    Amount DECIMAL(18, 4) NOT NULL, -- Kwota transakcji w walucie konta
    Date DATETIME2 NOT NULL DEFAULT GETUTCDATE(), -- Data transakcji, domy�lnie UTC obecny czas
    
    AccountId INT NOT NULL,
    CategoryId INT NULL, -- Opcjonalna kategoria, mo�e by� NULL
    OwnerId INT NOT NULL, -- W�a�ciciel transakcji
    
    OriginalAmount DECIMAL(18, 4) NULL, -- Oryginalna warto�� przed przewalutowaniem (mo�e by� NULL)
    OriginalCurrencyCode NVARCHAR(3) NULL, -- Kod oryginalnej waluty (np. "USD", mo�e by� NULL)
    ExchangeRate DECIMAL(18, 6) NULL, -- Kurs wymiany (np. 1.234567, mo�e by� NULL)
    
    CONSTRAINT PK_Transaction PRIMARY KEY (Id),
    
    CONSTRAINT FK_Transaction_Accounts_AccountId FOREIGN KEY (AccountId) REFERENCES Account ([AccountId]) ON DELETE CASCADE,
    CONSTRAINT FK_Transaction_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Category (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Transaction_AppUsers_OwnerId FOREIGN KEY (OwnerId) REFERENCES AppUser (Id) ON DELETE NO ACTION
);