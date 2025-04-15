﻿CREATE TABLE [dbo].[Account]
(
	[AccountId] INT NOT NULL,
	[Name] NVARCHAR(200) NOT NULL,
	[Type] SMALLINT,
	[Currency] NVARCHAR(20) NOT NULL,
	[Balance] DECIMAL(38,4) NOT NULL,
	[ShowInSummary] BIT,
	CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ([AccountId])
);
GO
