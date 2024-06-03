FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install -g microsoft.sqlpackage
WORKDIR "src/"
COPY ["MainDatabase/publish/MainDatabase.dacpac", "MainDatabase/"]
ENTRYPOINT sqlpackage /a:Publish /tsn:"tcp:sql-server,1433" /tdn:"MicroTubeDb" /tu:"sa" /tp:$MSSQL_SA_PASSWORD /sf:"/src/MainDatabase/MainDatabase.dacpac" /ttsc:True /stsc:True /st:10 /tt:10 \
		   && echo "Database published"