CREATE TABLE [dbo].[Transaction] (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Address NVARCHAR(500) NULL, -- Opcjonalne pole, mo¿e byæ NULL
    Description NVARCHAR(1000) NULL, -- Opcjonalne pole, mo¿e byæ NULL
    Amount DECIMAL(18, 4) NOT NULL, -- Kwota transakcji w walucie konta
    Date DATETIME2 NOT NULL DEFAULT GETUTCDATE(), -- Data transakcji, domyœlnie UTC obecny czas
    
    AccountId INT NOT NULL,
    CategoryId INT NULL, -- Opcjonalna kategoria, mo¿e byæ NULL
    OwnerId INT NOT NULL, -- W³aœciciel transakcji
    
    OriginalAmount DECIMAL(18, 4) NULL, -- Oryginalna wartoœæ przed przewalutowaniem (mo¿e byæ NULL)
    OriginalCurrencyCode NVARCHAR(3) NULL, -- Kod oryginalnej waluty (np. "USD", mo¿e byæ NULL)
    ExchangeRate DECIMAL(18, 6) NULL, -- Kurs wymiany (np. 1.234567, mo¿e byæ NULL)
    
    CONSTRAINT PK_Transaction PRIMARY KEY (Id),
    
    CONSTRAINT FK_Transaction_Accounts_AccountId FOREIGN KEY (AccountId) REFERENCES Account ([AccountId]) ON DELETE CASCADE,
    CONSTRAINT FK_Transaction_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Category (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Transaction_AppUsers_OwnerId FOREIGN KEY (OwnerId) REFERENCES AppUser (Id) ON DELETE NO ACTION
);