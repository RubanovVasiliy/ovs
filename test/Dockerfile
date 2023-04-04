# Используем образ SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Устанавливаем рабочую директорию внутри контейнера
WORKDIR /app

# Копируем файлы проекта в контейнер
COPY . ./

# Восстанавливаем зависимости и собираем приложение
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Используем образ Runtime для запуска приложения
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "app.dll"]
