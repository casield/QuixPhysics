FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY bin/Release/net5.0/publish/ App/
COPY Content/ App/Content
WORKDIR /App
EXPOSE 1337
ENTRYPOINT ["dotnet", "QuixPhysics.dll"]