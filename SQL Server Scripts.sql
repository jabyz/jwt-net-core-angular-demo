USE [master]
GO

/****** Object:  Database [demo-jwt]    Script Date: 20/03/2021 08:12:49 p. m. ******/
CREATE DATABASE [demo-jwt]
GO

USE [demo-jwt]
GO

/****** Object:  Table [dbo].[Usr]    Script Date: 20/03/2021 08:12:33 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Usr](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](max) NOT NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NOT NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PasswordHash] [binary](64) NOT NULL,
	[Salt] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

USE [demo-jwt]
GO
/****** Object:  StoredProcedure [dbo].[spUsrLogin]    Script Date: 20/03/2021 08:12:02 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[spUsrLogin]
    @pUserName VARCHAR(254),
    @pPassword VARCHAR(50)
AS
BEGIN

    SET NOCOUNT ON

	DECLARE @responseMessage VARCHAR(MAX) = ''
	DECLARE @responseStatus VARCHAR(MAX) = ''
	

    DECLARE @userID INT

    IF EXISTS (SELECT TOP 1 Id FROM [dbo].[Usr] WHERE UserName=@pUserName)
    BEGIN
        SET @userID=(SELECT TOP 1 Id FROM [dbo].[Usr] WHERE UserName=@pUserName AND PasswordHash=HASHBYTES('SHA2_512', @pPassword+CAST(Salt AS NVARCHAR(36))))

       IF(@userID IS NULL)
           SELECT @responseMessage = 'Incorrect password',@responseStatus = 0, @UserID = 0
       ELSE 
           SELECT @responseMessage = 'User successfully logged in',@responseStatus = 1, @UserID = @userID
    END
    ELSE
       SELECT @responseMessage = 'Invalid login',@responseStatus = 0, @UserID = 0
	
	IF EXISTS ((SELECT TOP 1 Id FROM [dbo].[Usr] WHERE EmailConfirmed = 0 AND UserName=@pUserName AND PasswordHash=HASHBYTES('SHA2_512', @pPassword+CAST(Salt AS NVARCHAR(36)))))
		SELECT @responseMessage = 'Please Confirm you eMail',@responseStatus = 0, @UserID = 0
		
	SELECT @responseMessage responseMessage,@responseStatus responseStatus, @UserID UserID
END
GO

USE [demo-jwt]
GO
/****** Object:  StoredProcedure [dbo].[spUsrRegister]    Script Date: 20/03/2021 08:12:13 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[spUsrRegister]
    @pUserName NVARCHAR(MAX), 
    @pPassword NVARCHAR(MAX),
    @pFirstName NVARCHAR(MAX) = NULL, 
    @pLastName NVARCHAR(MAX) = NULL,
	@pEmail NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON

    DECLARE @salt UNIQUEIDENTIFIER=NEWID()
    BEGIN TRY

        INSERT INTO dbo.[Usr] (UserName, PasswordHash, Salt, FirstName, LastName, Email, EmailConfirmed)
        VALUES(@pUserName, HASHBYTES('SHA2_512', @pPassword+CAST(@salt AS NVARCHAR(36))), @salt, @pFirstName, @pLastName, @pEmail, 0)

	   SELECT responseMessage = 'User successfully registered!',responseStatus = 1

    END TRY
    BEGIN CATCH
        SELECT responseMessage=ERROR_MESSAGE(),responseStatus = 1
    END CATCH

END


GO




