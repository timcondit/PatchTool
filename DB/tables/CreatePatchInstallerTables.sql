--  table:
--      branches
--  fields:
--      branch_id int
--      repository_root text            // svn://svn.click2coach.net/EPS
--      repository_path text            // $repository_root + /bla/bla/bla
--      friendly_name text allow_nulls  // 10.1dev
IF NOT EXISTS (SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[branches]')
    AND TYPE IN ( N'U' ))
    BEGIN
        CREATE TABLE [dbo].[branches]
        (
            [branch_id] INT NOT NULL,
            [repository_path] NVARCHAR(255) NOT NULL,
            [friendly_name] NVARCHAR(255),
            CONSTRAINT [PK_branch_id] PRIMARY KEY CLUSTERED ([branch_id])
        )
END

--  table:
--      build
--  fields:
--      build_id int (PK?)
--      branch_id int (FK into branches.branch_id)
--      build_date date allow_nulls
--      build_machine text allow_nulls          // (host?)
--      build_version text (M.m.R)              // marketing
--      build_number int
--      wc_root text allow_nulls                // OS path
--      checked_out_revision int allow_nulls    // build_revision or whatever
IF NOT EXISTS (SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[build]')
    AND TYPE IN ( N'U' ))
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

--  table:
--      binaries                // manually curated
--  fields:
--      binary_id int (PK?)
--      name text allow_nulls   // e.g., ChanMgrSvc.exe
--      build_path text         // SOME_ROOT/workdir/ChannelManager/
IF NOT EXISTS (SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[binaries]')
    AND TYPE IN ( N'U' ))
    BEGIN
        CREATE TABLE [dbo].[binaries]
        (
            [binary_id] INT NOT NULL,
            [name] NVARCHAR(255),
            [build_path] NVARCHAR(255) NOT NULL,
            CONSTRAINT [PK_binary_id] PRIMARY KEY CLUSTERED ([binary_id])
        )
  END

--  table:
--      binary_instances                // automatically generated
--  fields:
--      binary_id int (FK into binaries.binary_id)
--      build_id int (FK into build.build_id)
--      md5sum text
--      binary_version text (M.m.R.b)   // explorer file version
IF NOT EXISTS (SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[binary_instances]')
    AND TYPE IN ( N'U' ))
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
