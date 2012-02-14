USE [sfpi]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_target_patch]') AND parent_object_id = OBJECT_ID(N'[dbo].[target]'))
ALTER TABLE [dbo].[target] DROP CONSTRAINT [FK_target_patch]
GO

USE [sfpi]
GO

/****** Object:  Table [dbo].[target]    Script Date: 02/13/2012 17:06:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[target]') AND type in (N'U'))
DROP TABLE [dbo].[target]
GO

USE [sfpi]
GO

/****** Object:  Table [dbo].[target]    Script Date: 02/13/2012 17:06:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[target](
    [Id] [uniqueidentifier] NOT NULL,
    [path] [varchar](900) NULL,
    CONSTRAINT [PK_target_1] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[target]  WITH CHECK ADD  CONSTRAINT [FK_target_patch] FOREIGN KEY([Id])
REFERENCES [dbo].[patch] ([Id])
GO

ALTER TABLE [dbo].[target] CHECK CONSTRAINT [FK_target_patch]
GO


