﻿CREATE TABLE [dbo].[Category] (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Color NVARCHAR(7) NOT NULL, -- #RRGGBB format
    Description NVARCHAR(max) NULL,
    OwnerId INT NOT NULL,
    CONSTRAINT PK_Category PRIMARY KEY (Id),
    CONSTRAINT FK_Category_AppUser_OwnerId FOREIGN KEY (OwnerId) REFERENCES AppUser (Id) ON DELETE NO ACTION
);
GO