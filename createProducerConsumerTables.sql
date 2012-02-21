use sfpi
go

-- drop and create tables
if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[consumer]') and type in (N'U'))
drop table [dbo].[consumer]
go

if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[producer]') and type in (N'U'))
drop table [dbo].[producer]
go

-- 900 chars max for producer path allows consumer_path to be unique
-- http://stackoverflow.com/questions/2863993/is-of-a-type-that-is-invalid-for-use-as-a-key-column-in-an-index
create table [dbo].[producer](
	[Id] int identity(1,1) primary key,
	[path] varchar (900) unique not null
	)
go

-- consumer_path is unique because two files are not allowed to exist in the same location
create table [dbo].[consumer](
	[p_id] int identity(1,1) not null,
	[consumer_path] varchar(900) unique not null
)
GO

-- this allows to add p_id to dbo.consumer
set IDENTITY_INSERT dbo.consumer on

-- foreign key
alter table [dbo].[consumer] with check add constraint [FK_consumer_producer] foreign key([p_id])
references [dbo].[producer] ([Id])
go
alter table [dbo].[consumer] check constraint [FK_consumer_producer]
go

-- load data into producer
bulk insert dbo.producer
from 'C:\Source\PatchTool\collector\collections\C-Source-PatchTool.txt'
with (fieldterminator = ',', rowterminator = '\n')
go

-- test insert
-- use case: add new file
--	1: UI shows full list of producer files
--	2: user finds producer_path, maybe by filtering or sorting
--	3: user provides consumer_path
--	4: query producer_path to collect producer_ID [consumer FK]
--	4.1: insert consumer (producer_ID, consumer_path)

-- Q: what's the goal?
-- A: enter new file record into consumer table [ID, producer_ID, consumer_path]
insert dbo.consumer (p_id, consumer_path) values (
(select Id from producer where Id = 4), 'workdir\miscCheck.ui');

-- test query
select * from producer
select * FROM consumer;
