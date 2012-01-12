USE SFPI_v2
GO


IF EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[binary_instances]') AND TYPE IN ( N'U' ))
    DROP TABLE dbo.binary_instances

IF EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[branches]') AND TYPE IN ( N'U' ))
    DROP TABLE [dbo].[branches]

IF EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[build]') AND TYPE IN ( N'U' ))
    DROP TABLE [dbo].[build]

IF EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = Object_id(N'[dbo].[binaries]') AND TYPE IN ( N'U' ))
    DROP TABLE [dbo].[binaries]
GO

