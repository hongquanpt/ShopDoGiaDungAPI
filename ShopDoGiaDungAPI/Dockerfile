# Stage 1: Build ?ng d?ng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj v� restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy to�n b? m� ngu?n v� build ?ng d?ng
COPY . .
WORKDIR /src
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (cho ph�p Render ho?c d?ch v? kh�c k?t n?i)
EXPOSE 80
EXPOSE 443

# L?nh ch?y ?ng d?ng
ENTRYPOINT ["dotnet", "ShopDoGiaDungAPI.dll"]