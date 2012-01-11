-- ***************************************************************************
-- table:
--      branches
-- description:
--      SVN branches
-- ***************************************************************************
IF NOT EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[branches]') AND TYPE IN ( N'U' ))
    BEGIN
        CREATE TABLE [dbo].[branches]
        (
            [branch_id] INT NOT NULL,
            [repository_path] NVARCHAR(255) NOT NULL,
            [friendly_name] NVARCHAR(255),

            CONSTRAINT [PK_branch_id]
            PRIMARY KEY CLUSTERED ([branch_id])
        )
END

-- ***************************************************************************
-- Insert branches into branch table
--
-- This may not be an appropriate use of INSERT, but it gets the job done.
-- ***************************************************************************
IF NOT EXISTS (
    SELECT [friendly_name]
    FROM [dbo].[branches]
    WHERE [friendly_name] IN ('9.10base','10.0dev','10.0int','10.0base','10.1dev','10.1int','10.1base'))
    BEGIN
        DECLARE @repo_root NVARCHAR(255)
        SET @repo_root = 'svn://svn.click2coach.net/EPS/branches'
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (1, @repo_root + '/9.10/maintenance/base', '9.10base')
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (2, @repo_root + '/10.0/maintenance/dev', '10.0dev')
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (3, @repo_root + '/10.0/maintenance/int', '10.0int')
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (4, @repo_root + '/10.0/maintenance/base', '10.0base')
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (5, @repo_root + '/10.1/maintenance/dev', '10.1dev')
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (6, @repo_root + '/10.1/maintenance/int', '10.1int')
        INSERT [dbo].[branches] ([branch_id],[repository_path],[friendly_name])
        VALUES (7, @repo_root + '/10.1/maintenance/base', '10.1base')
END

-- table:
--      build
IF NOT EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[build]') AND TYPE IN ( N'U' ))
    BEGIN
        CREATE TABLE [dbo].[build]
        (
            [build_id] INT NOT NULL,
            [branch_id] INT NOT NULL,
            [build_date] DATETIME,
            [build_machine] NVARCHAR(255),
            [build_version] NVARCHAR(255) NOT NULL,
            [build_number] INT NOT NULL,
            [wc_root] NVARCHAR(255),
            [revision] INT,

            CONSTRAINT [PK_build_id]
            PRIMARY KEY CLUSTERED ([build_id]),

            CONSTRAINT [FK_branch_id__build_id]
            FOREIGN KEY([branch_id])
            REFERENCES [dbo].[build] ([build_id]),
        )
END

-- table:
--      binaries
-- description:
--      Generic list of binaries that are eligible for patching.  Manually
--      edited, maybe thru an admin web page.
IF NOT EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[binaries]') AND TYPE IN ( N'U' ))
    BEGIN
        CREATE TABLE [dbo].[binaries]
        (
            [binary_id] INT NOT NULL,
            [name] NVARCHAR(255),
            [build_path] NVARCHAR(255) NOT NULL,

            CONSTRAINT [PK_binary_id] PRIMARY KEY CLUSTERED ([binary_id])
        )
END

-- table:
--      binary_instances
-- description:
--      The specific binaries (should probably be files, as they aren't
--      necessarily binaries) which may be added to a patch.
IF NOT EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[binary_instances]') AND TYPE IN ( N'U' ))
    BEGIN
        CREATE TABLE [dbo].[binary_instances]
        (
            [binary_id] INT NOT NULL,
            [build_id] INT NOT NULL,
            [md5sum] NVARCHAR(255) NOT NULL,
            [binary_version] NVARCHAR(255) NOT NULL,

            CONSTRAINT [FK_binary_instances__binary_id]
            FOREIGN KEY([binary_id])
            REFERENCES [dbo].[binaries] ([binary_id]),

            CONSTRAINT [FK_binary_instances__build_id]
            FOREIGN KEY([build_id])
            REFERENCES [dbo].[build] ([build_id]),
        )
END

GO

