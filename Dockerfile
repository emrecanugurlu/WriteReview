# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce sadece csproj'ları kopyala ki restore katmanı cache'lensin
COPY ["Presentation/WriteReview.API/WriteReview.API.csproj", "Presentation/WriteReview.API/"]
COPY ["Core/WriteReview.Application/WriteReview.Application.csproj", "Core/WriteReview.Application/"]
COPY ["Core/WriteReview.Domain/WriteReview.Domain.csproj", "Core/WriteReview.Domain/"]
COPY ["Infrastructure/WriteReview.Persistence/WriteReview.Persistence.csproj", "Infrastructure/WriteReview.Persistence/"]
RUN dotnet restore "Presentation/WriteReview.API/WriteReview.API.csproj"

# Geri kalan kaynağı kopyala ve publish et
COPY . .
RUN dotnet publish "Presentation/WriteReview.API/WriteReview.API.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cloud Run PORT=8080 verir; Kestrel bunu dinlesin
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "WriteReview.API.dll"]
