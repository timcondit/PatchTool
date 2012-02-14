-- annotated SQL files

USE [sfpi]
GO

-- search the sys.objects table for an object of type U (a table) called
-- dbo.source, and drop it if present
IF  EXISTS (SELECT * FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[source]')
    AND type in (N'U'))
    DROP TABLE [dbo].[source]
    GO

-- [from MSDN] When SET ANSI_NULLS is ON, a SELECT statement that uses WHERE
-- column_name = NULL returns zero rows even if there are null values in
-- column_name.  A SELECT statement that uses WHERE column_name <> NULL
-- returns zero rows even if there are nonnull values in column_name.
SET ANSI_NULLS ON
GO

-- [from MSDN] When SET QUOTED_IDENTIFIER is ON, identifiers can be delimited
-- by double quotation marks, and literals must be delimited by single
-- quotation marks.  When SET QUOTED_IDENTIFIER is OFF, identifiers cannot be
-- quoted and must follow all Transact-SQL rules for identifiers.
SET QUOTED_IDENTIFIER ON
GO

-- [from MSDN] Columns defined with char, varchar, binary, and varbinary data
-- types have a defined size.
--
-- SET ANSI_PADDING must be ON when you are creating or changing indexes on
-- computed columns or indexed views.  The default for SET ANSI_PADDING is ON.
SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[source](
    [Id] [uniqueidentifier] NOT NULL,
    [path] [varchar](900) NULL,
    -- TODO explain PRIMARY KEY CLUSTERED
    CONSTRAINT [PK_source] PRIMARY KEY CLUSTERED ([Id] ASC) WITH
    (
        PAD_INDEX = OFF,
        STATISTICS_NORECOMPUTE = OFF,
        IGNORE_DUP_KEY = OFF,
        ALLOW_ROW_LOCKS = ON,
        ALLOW_PAGE_LOCKS = ON
    )
    ON [PRIMARY]
) ON [PRIMARY]
GO

-- [from MSDN] Columns defined with char, varchar, binary, and varbinary data
-- types have a defined size.
--
-- SET ANSI_PADDING must be ON when you are creating or changing indexes on
-- computed columns or indexed views.  The default for SET ANSI_PADDING is ON.
SET ANSI_PADDING OFF
GO

