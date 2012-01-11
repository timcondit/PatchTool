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
            CONSTRAINT [PK_branch_id] PRIMARY KEY CLUSTERED ([branch_id])
        )
END

-- ***************************************************************************
-- Insert branches into branch table
--
-- This may not be an appropriate use of INSERT, but it gets the job done.
-- ***************************************************************************
IF NOT EXISTS (
    SELECT [branch_id]
    FROM [dbo].[branches]
    -- seven branches: 9.10/base, 10.0/dev, int, base, 10.1/dev, int, base
    WHERE [branch_id] IN (1,2,3,4,5,6,7))
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
            CONSTRAINT [PK_build_id] PRIMARY KEY CLUSTERED ([build_id]),
            CONSTRAINT [FK_branch_id__build_id] FOREIGN KEY([branch_id]) REFERENCES [dbo].[build] ([build_id]),
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
            CONSTRAINT [FK_binary_instances__binary_id] FOREIGN KEY([binary_id]) REFERENCES [dbo].[binaries] ([binary_id]),
            CONSTRAINT [FK_binary_instances__build_id] FOREIGN KEY([build_id]) REFERENCES [dbo].[build] ([build_id]),
        )
  END


----********************************************************************************
----
---- Insert Volume Names into VolumeType table
---- Messages
----      VolumeTypeID 0 - Audio
----      VolumeTypeID 1 - TemporaryDownload
----      VolumeTypeID 2 - Video
----      VolumeTypeID 3 - LoggedAudio
----      VolumeTypeID 4 - LoggedVideo
----
----********************************************************************************
--IF NOT EXISTS (SELECT [VolumeName] FROM [dbo].[VolumeType] WHERE [VolumeName] IN ('Audio','TemporaryDownload','Video','LoggedAudio','LoggedVideo'))
--    BEGIN
--        INSERT [dbo].[VolumeType] ([VolumeTypeID],[VolumeName])
--        VALUES (0,'Audio')
--        INSERT [dbo].[VolumeType] ([VolumeTypeID],[VolumeName])
--        VALUES (1,'TemporaryDownload')
--        INSERT [dbo].[VolumeType] ([VolumeTypeID],[VolumeName])
--        VALUES (2,'Video')
--        INSERT [dbo].[VolumeType] ([VolumeTypeID],[VolumeName])
--        VALUES (3,'LoggedAudio')
--        INSERT [dbo].[VolumeType] ([VolumeTypeID],[VolumeName])
--        VALUES (4,'LoggedVideo')
--    END
--
----EncoderStatus table creation
--IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EncoderStatus]') AND type in (N'U'))
--    BEGIN
--        CREATE TABLE [dbo].[EncoderStatus]
--        (
--            [EncoderStatusID] int NOT NULL, --Forcing values, so this will not be an incremental Identity seed
--            [Status] nvarchar(255) NOT NULL,
--            CONSTRAINT [PK_EncoderStatus] PRIMARY KEY CLUSTERED ([EncoderStatusID])
--        )
--    END
--
--
----********************************************************************************
----
---- Insert Status Messages into EncoderStatus table
---- Messages
----      EncoderStatusID 0 - Not_Encoded
----      EncoderStatusID 1 - Pending
----      EncoderStatusID 2 - In_Progress
----      EncoderStatusID 3 - Complete
----      EncoderStatusID 99 - Error
----
----********************************************************************************
--IF NOT EXISTS (SELECT [Status] FROM [dbo].[EncoderStatus] WHERE [Status] IN ('Not_Encoded','Pending','In_Progress','Complete','Error'))
--    BEGIN
--        INSERT [dbo].[EncoderStatus] ([EncoderStatusID],[Status])
--        VALUES (0,'Not_Encoded')
--        INSERT [dbo].[EncoderStatus] ([EncoderStatusID],[Status])
--        VALUES (1,'Pending')
--        INSERT [dbo].[EncoderStatus] ([EncoderStatusID],[Status])
--        VALUES (2,'In_Progress')
--        INSERT [dbo].[EncoderStatus] ([EncoderStatusID],[Status])
--        VALUES (3,'Complete')
--        INSERT [dbo].[EncoderStatus] ([EncoderStatusID],[Status])
--        VALUES (99,'Error')
--    END
--
----EncoderMedia table creation
--IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EncoderMedia]') AND type in (N'U'))
--    BEGIN
--        CREATE TABLE [dbo].[EncoderMedia]
--        (
--            [EncoderMediaID] int NOT NULL IDENTITY(1,1),
--            [RecordingID] int NOT NULL,
--            [MediaID] int NOT NULL, -- NEWLY ADDED
--            [ServerID] int NOT NULL, -- NEWLY ADDED
--            [isMainMedia] bit NOT NULL,
--            [LengthInSec] int NOT NULL,
--            [MediaPath] nvarchar(1024) NULL,
--            [MediaType] nvarchar(10) NOT NULL,
--            [AudioSampleRate] int NOT NULL,
--            [AudioChannels] int NOT NULL,
--            [EncoderStatus] int DEFAULT 0,
--            [RecordType] int NOT NULL,
--            [OutputFile] nvarchar(1024) NOT NULL,
--            [Order] int NOT NULL,
--            [DateCreated] datetime NOT NULL,
--            [DateModified] datetime NULL,
--            CONSTRAINT [PK_EncoderMedia] PRIMARY KEY CLUSTERED ([EncoderMediaID])
--        )
--        CREATE NONCLUSTERED INDEX [IX_RecordingID] ON [dbo].[EncoderMedia] ([RecordingID] ASC)
--        ALTER TABLE [dbo].[EncoderMedia]  WITH CHECK ADD  CONSTRAINT [FK_EncoderMedia_EncoderStatus] FOREIGN KEY([EncoderStatus])
--        REFERENCES [dbo].[EncoderStatus] ([EncoderStatusID])
--    END
--
----EncodedRecording table creation
--IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EncodedRecording]') AND type in (N'U'))
--    BEGIN
--        CREATE   TABLE [dbo].EncodedRecording
--        (
--            [EncodedRecordingID] int NOT NULL IDENTITY(1,1),
--            [RecordingID] int NOT NULL,
--            [ServerID] int NOT NULL, -- NEWLY ADDED
--            [EncoderStatus] int NOT NULL,
--            [OutputFile] nvarchar(1024) NOT NULL,
--            [DateCreated] datetime NOT NULL,
--            [DateModified] datetime NULL,
--            CONSTRAINT [PK_EncodedRecordingID] PRIMARY KEY CLUSTERED ([EncodedRecordingID])
--        )
--        ALTER TABLE [dbo].[EncodedRecording]  WITH CHECK ADD  CONSTRAINT [FK_EncodedRecording_EncoderStatus] FOREIGN KEY([EncoderStatus])
--        REFERENCES [dbo].[EncoderStatus] ([EncoderStatusID])
--        ALTER TABLE [dbo].[EncodedRecording]  WITH NOCHECK ADD  CONSTRAINT [FK_EncodedRecording_Recording] FOREIGN KEY([RecordingID])
--        REFERENCES [dbo].[Recording] ([RecordingID]) --WITH NOCHECK because these tables can contain millions of records
--        ALTER TABLE [dbo].[EncodedRecording]  WITH CHECK ADD  CONSTRAINT [FK_EncodedRecording_etblServer] FOREIGN KEY([ServerID])
--        REFERENCES [dbo].[etblServer] ([nServerID])
--    END
--
----EncodedRecordingSet table creation
--IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EncodedRecordingSet]') AND type in (N'U'))
--    BEGIN
--        CREATE   TABLE [dbo].EncodedRecordingSet
--        (
--            [EncodedRecordingSetID] int NOT NULL IDENTITY(1,1),
--            [EncodedRecordingID] int NOT NULL,
--            [RecordingID] int NOT NULL,
--            [MediaID] int NOT NULL, -- NEWLY ADDED
--            [isMainMedia] bit NOT NULL,
--            [MediaPath] nvarchar(1024) NULL,
--            [LengthInSec] int NOT NULL,
--            [MediaType] nvarchar(10) NOT NULL,
--            [AudioSampleRate] int NOT NULL,
--            [AudioChannels] int NOT NULL,
--            [RecordType] int NOT NULL,
--            CONSTRAINT [PK_RecordingID] PRIMARY KEY CLUSTERED ([EncodedRecordingSetID])
--        )
--        CREATE NONCLUSTERED INDEX [IX_EncodedRecordingID] ON [dbo].[EncodedRecordingSet] ([EncodedRecordingID] ASC)
--        ALTER TABLE [dbo].[EncodedRecordingSet]  WITH CHECK ADD  CONSTRAINT [FK_EncodedRecordingSet_EncodedRecording] FOREIGN KEY([EncodedRecordingID])
--        REFERENCES [dbo].[EncodedRecording] ([EncodedRecordingID])
--        ALTER TABLE [dbo].[EncodedRecordingSet]  WITH NOCHECK ADD  CONSTRAINT [FK_EncodedRecordingSet_Recording] FOREIGN KEY([RecordingID])
--        REFERENCES [dbo].[Recording] ([RecordingID]) --WITH NOCHECK because these tables can contain millions of records
--        ALTER TABLE [dbo].[EncodedRecordingSet]  WITH NOCHECK ADD  CONSTRAINT [FK_EncodedRecordingSet_Media] FOREIGN KEY([MediaID])
--        REFERENCES [dbo].[Media] ([MediaID]) --WITH NOCHECK because these tables can contain millions of records
--        ALTER TABLE [dbo].[EncodedRecordingSet]  WITH CHECK ADD  CONSTRAINT [FK_EncodedRecordingSet_TypeRecording] FOREIGN KEY([RecordType])
--        REFERENCES [dbo].[TypeRecording] ([TypeRecordingID])
--    END
--
--GO
