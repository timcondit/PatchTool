@echo OFF
sqlcmd -S .\SQLEXPRESS -i CreatePatchInstallerTables.sql
@echo ON
