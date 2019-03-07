ALTER TABLE [Tests] DROP CONSTRAINT [PK_Tests];

EXEC sp_rename N'[Tests]', N'TestRuns';

CREATE TABLE [Tests] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(450) NULL,
    CONSTRAINT [PK_Tests] PRIMARY KEY ([Id])
);

insert into Tests(Name)
select distinct name from testruns;

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TestRuns]') AND [c].[name] = N'Name');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [TestRuns] DROP CONSTRAINT [' + @var0 + '];');

ALTER TABLE [TestRuns] ADD [TestId] int NOT NULL DEFAULT 0;

update testruns
set testid = (select id from tests t where t.Name = testruns.name);

ALTER TABLE [TestRuns] DROP COLUMN [Name];

ALTER TABLE [TestRuns] ADD CONSTRAINT [PK_TestRuns] PRIMARY KEY ([TestId], [PipelineJobId]);

CREATE INDEX [IX_TestRuns_PipelineJobId] ON [TestRuns] ([PipelineJobId]);


CREATE UNIQUE INDEX [IX_Tests_Name] ON [Tests] ([Name]) WHERE [Name] IS NOT NULL;

ALTER TABLE [TestRuns] ADD CONSTRAINT [FK_TestRuns_Tests_TestId] FOREIGN KEY ([TestId]) REFERENCES [Tests] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190226130237_CreateTestRunsTable', N'2.2.1-servicing-10028');