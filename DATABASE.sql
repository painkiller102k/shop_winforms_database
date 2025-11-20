create database tooded
use tooded

CREATE TABLE [dbo].[Kategooria] ( [Id] INT IDENTITY (1, 1) NOT NULL, [Kategooria_nimetus] NVARCHAR (50) NOT NULL, [Kirjendus] TEXT NULL, PRIMARY KEY CLUSTERED ([Id] ASC) );

CREATE TABLE [dbo].[Toodetabel] ( [Id] INT IDENTITY (1, 1) NOT NULL, [Toodenimetus] NCHAR (50) NOT NULL, [Kogus] INT NOT NULL, [Hind] INT NOT NULL, [Pilt] TEXT NULL, [Kategooriad] INT NOT NULL, [Bpilt] VARBINARY (MAX) NULL, PRIMARY KEY CLUSTERED ([Id] ASC) );