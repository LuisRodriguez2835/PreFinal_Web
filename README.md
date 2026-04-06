# GymApiFinal

Proyecto nuevo desde cero para el examen final de Desarrollo de Servicios Web.

## Qué incluye
- API REST en .NET 8
- SQL Server en Docker
- Entity Framework Core
- JWT
- Roles: ADMIN, ENTRENADOR, SOCIO
- Rutas protegidas
- Validaciones con DataAnnotations
- Middleware de errores centralizado
- Swagger

## Extensiones / paquetes usados
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Design
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore
- BCrypt.Net-Next

## Requisitos
- Docker
- .NET 8 SDK
- SQL Server en Docker
- SSMS o Azure Data Studio
- Postman

## 1) Levantar SQL Server con Docker
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TuPassword123*" -p 1433:1433 --name sqlgym --hostname sqlgym -d mcr.microsoft.com/mssql/server:2022-latest
```

## 2) Ejecutar el script
Corrige esto en tu script:
```sql
USE GimnasioDB;
GO
```


## 3) Restaurar paquetes y correr
```bash
dotnet restore
dotnet run
```

## 4) Swagger
Abre:
- http://localhost:5099/swagger

## 5) Login demo
El proyecto reemplaza `**HASH**` automáticamente al arrancar:
- admin / Admin123*
- entrenador / Entrenador123*
- socio / Socio123*

## 6) Endpoints principales
### Auth
- POST `/api/auth/login`

### ADMIN
- CRUD `/api/socios`
- CRUD `/api/entrenadores`
- CRUD `/api/membresias`
- POST `/api/socios/{socioId}/membresias/{membresiaId}`
- GET `/api/dashboard/resumen-admin`

### ENTRENADOR
- GET `/api/entrenadores/mis-socios`
- POST `/api/asistencias/checkin`
- PUT `/api/asistencias/checkout`
- CRUD `/api/rutinas`

### SOCIO
- GET `/api/socios/me/membresia`
- GET `/api/socios/me/asistencias`
- GET `/api/socios/me/rutinas`
