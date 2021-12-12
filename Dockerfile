FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY bin/Debug/net5.0/ App/
COPY Content/ App/Content
WORKDIR /App
EXPOSE 1337
ENTRYPOINT ["dotnet", "QuixPhysics.dll"]