CREATE DATABASE [DbLight]
 ON PRIMARY
  (NAME = DbLight_Data,
   FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\DbLight_Data.mdf',
   SIZE = 5MB,
   MAXSIZE = UNLIMITED,
   FILEGROWTH = 10%)
 LOG ON
  (NAME = DbLight_Log,
   FILENAME =N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\DbLight_log.ldf',
   SIZE = 1MB,
   MAXSIZE = UNLIMITED,
   FILEGROWTH = 10%)
COLLATE SQL_Latin1_General_CP1_CI_AS
GO

USE [master]
GO
CREATE LOGIN [test] WITH PASSWORD=N'test', DEFAULT_DATABASE=[DbLight], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
USE [DbLight]
GO
CREATE USER [test] FOR LOGIN [test]
GO
USE [DbLight]
GO
ALTER ROLE [db_owner] ADD MEMBER [test]
GO

--TABLE START Role
CREATE TABLE [dbo].[Role](
    [RoleId] [INT] NOT NULL,
    [RoleName] [NVARCHAR](128) NULL,
CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED(
    [RoleId] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
--TABLE END

--TABLE START RoleUser
CREATE TABLE [dbo].[RoleUser](
    [RoleId] [INT] NOT NULL,
    [UserId] [INT] NOT NULL,
CONSTRAINT [PK_RoleUser] PRIMARY KEY CLUSTERED(
    [RoleId] ASC,
    [UserId] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
--TABLE END

--TABLE START Sex
CREATE TABLE [dbo].[Sex](
    [SexId] [INT] NOT NULL,
    [SexName] [NVARCHAR](128) NOT NULL,
CONSTRAINT [PK_Sex] PRIMARY KEY CLUSTERED(
    [SexId] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
--TABLE END

--TABLE START User
CREATE TABLE [dbo].[User](
    [UserId] [INT] NOT NULL,
    [UserName] [NVARCHAR](128) NULL,
    [WeChatCode] [NVARCHAR](64) NULL,
    [Phone] [NVARCHAR](16) NULL,
    [Birthday] [DATETIME] NULL,
    [Income] [MONEY] NULL,
    [Height] [FLOAT] NULL,
    [SexId] [INT] NULL,
    [Married] [BIT] NULL,
    [Remark] [NVARCHAR](MAX) NULL,
    [Photo] [IMAGE] NULL,
    [RegisterTime] [DATETIME] NULL,
CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED(
    [UserId] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]
--TABLE END
