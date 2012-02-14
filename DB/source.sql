USE [sfpi]
GO

/****** Object:  Table [dbo].[source]    Script Date: 02/13/2012 17:05:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[source]') AND type in (N'U'))
DROP TABLE [dbo].[source]
GO

USE [sfpi]
GO

/****** Object:  Table [dbo].[source]    Script Date: 02/13/2012 17:05:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[source](
    [Id] [uniqueidentifier] NOT NULL,
    [path] [varchar](900) NULL,
    CONSTRAINT [PK_source] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


