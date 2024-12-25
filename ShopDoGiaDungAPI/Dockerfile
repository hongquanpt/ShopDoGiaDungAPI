# Stage 1: Build ?ng d?ng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy toàn b? mã ngu?n và build ?ng d?ng
COPY . .
WORKDIR /src
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (cho phép Render ho?c d?ch v? khác k?t n?i)
EXPOSE 80
EXPOSE 443

# L?nh ch?y ?ng d?ng
ENTRYPOINT ["dotnet", "ShopDoGiaDungAPI.dll"]