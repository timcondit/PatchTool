USE [sfpi]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_patch_source]') AND parent_object_id = OBJECT_ID(N'[dbo].[patch]'))
ALTER TABLE [dbo].[patch] DROP CONSTRAINT [FK_patch_source]
GO

USE [sfpi]
GO

/****** Object:  Table [dbo].[patch]    Script Date: 02/13/2012 17:05:05 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[patch]') AND type in (N'U'))
DROP TABLE [dbo].[patch]
GO

USE [sfpi]
GO

/****** Object:  Table [dbo].[patch]    Script Date: 02/13/2012 17:05:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[patch](
    [Id] [uniqueidentifier] NOT NULL,
    [s_Id] [uniqueidentifier] NULL,
    [t_Id] [uniqueidentifier] NULL,
    [is_active] [bit] NULL,
    CONSTRAINT [PK_patch] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[patch]  WITH CHECK ADD  CONSTRAINT [FK_patch_source] FOREIGN KEY([s_Id])
REFERENCES [dbo].[source] ([Id])
GO

ALTER TABLE [dbo].[patch] CHECK CONSTRAINT [FK_patch_source]
GO


