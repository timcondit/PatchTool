@echo OFF
sqlcmd -S .\SQLEXPRESS -i DropPatchInstallerTables.sql
@echo ON
