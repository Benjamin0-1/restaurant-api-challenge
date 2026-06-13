# Restaurant API

API REST para la gestión de productos de restaurante, construida con **ASP.NET Core 10** y **Entity Framework Core** sobre **SQLite**.

Repositorio: https://github.com/Benjamin0-1/restaurant-api-challenge

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (solo si se necesita re-generar migraciones)

```bash
dotnet tool install --global dotnet-ef
```

---

## 1. Clonar el repositorio

```bash
git clone https://github.com/Benjamin0-1/restaurant-api-challenge.git
cd restaurant-api-challenge
```

---

## 2. Ejecutar el proyecto

```bash
dotnet run --project Restaurant
```

La API quedará disponible en:

- `http://localhost:5000`
- `https://localhost:7000` (HTTPS)

La documentación interactiva (Swagger UI) se encuentra en:

```
https://localhost:7000/swagger
```

> **Nota:** Las migraciones y los seeders se ejecutan **automáticamente** al iniciar la aplicación. No es necesario correr comandos adicionales.

---

## 3. Correr migraciones manualmente

Si necesitas aplicar las migraciones sin iniciar la API:

```bash
dotnet ef database update \
  --project Restaurant.Shared \
  --startup-project Restaurant
```

Para crear una nueva migración:

```bash
dotnet ef migrations add <NombreMigracion> \
  --project Restaurant.Shared \
  --startup-project Restaurant
```

---

## 4. Ejecutar los seeders manualmente

Los seeders se ejecutan solos en cada inicio (ver `Program.cs`). Son idempotentes: si los datos ya existen, no vuelven a insertarse.

Si quisieras forzar un re-seed, basta con eliminar el archivo `Restaurant/restaurant.db` y reiniciar la aplicación:

```bash
rm Restaurant/restaurant.db
dotnet run --project Restaurant
```

---

## 5. Datos de prueba (seed)

Al iniciar, se insertan automáticamente los siguientes datos:

### Usuario administrador

| Campo    | Valor                   |
|----------|-------------------------|
| Email    | `admin@restaurant.com`  |
| Password | `12345678A`             |

### Categorías

| ID | Nombre      | Descripción              |
|----|-------------|--------------------------|
| 1  | Beverages   | Drinks and beverages     |
| 2  | Appetizers  | Starters and snacks      |
| 3  | Main Course | Main dishes              |
| 4  | Desserts    | Sweet treats             |

### Productos

| SKU     | Nombre          | Categoría   | Precio  |
|---------|-----------------|-------------|---------|
| BEV-001 | Lemonade        | Beverages   | $3.50   |
| BEV-002 | Iced Coffee     | Beverages   | $4.00   |
| APP-001 | Spring Rolls    | Appetizers  | $6.00   |
| APP-002 | Garlic Bread    | Appetizers  | $4.50   |
| MNC-001 | Grilled Chicken | Main Course | $14.99  |
| MNC-002 | Beef Burger     | Main Course | $12.99  |
| DES-001 | Chocolate Cake  | Desserts    | $5.50   |
| DES-002 | Cheesecake      | Desserts    | $6.00   |

---

## 6. Endpoints principales

### Autenticación

| Método | Ruta                  | Descripción              | Auth requerida |
|--------|-----------------------|--------------------------|----------------|
| POST   | `/api/v1/auth/login`  | Obtener JWT              | No             |
| POST   | `/api/v1/auth/signup` | Registrar nuevo usuario  | No             |

### Productos

| Método | Ruta                    | Descripción                              | Auth requerida |
|--------|-------------------------|------------------------------------------|----------------|
| GET    | `/api/v1/products`      | Listar productos del usuario (paginado)  | Si             |
| POST   | `/api/v1/products`      | Crear producto                           | Si             |
| PATCH  | `/api/v1/products/{id}` | Actualizar producto parcialmente         | Si             |
| DELETE | `/api/v1/products/{id}` | Eliminar producto (soft delete)          | Si             |

#### Filtros disponibles en GET `/api/v1/products`

| Parámetro    | Tipo    | Descripción                        |
|--------------|---------|------------------------------------|
| `name`       | string  | Filtrar por nombre (contiene)      |
| `minPrice`   | decimal | Precio mínimo                      |
| `maxPrice`   | decimal | Precio máximo                      |
| `isAvailable`| bool    | Disponibilidad                     |
| `categoryId` | int     | ID de categoría                    |
| `pageNumber` | int     | Número de página (default: 1)      |
| `pageSize`   | int     | Elementos por página (default: 10) |

---

## 7. Decisiones técnicas principales

- **SQLite**: base de datos embebida para facilitar la ejecución local sin infraestructura externa. En producción se reemplazaría por PostgreSQL o SQL Server.
- **Entity Framework Core + Code First**: permite versionar el esquema con migraciones tipadas y mantener el modelo en C#.
- **JWT Bearer**: autenticación stateless. El token contiene el `userId` como claim, lo que permite aislar los datos de cada usuario sin consultas adicionales.
- **FluentValidation**: separación de las reglas de validación del modelo de dominio, con mensajes de error estructurados vía `ValidationExceptionHandler`.
- **Soft delete (`IsDeleted`)**: los registros eliminados no se borran físicamente, lo que facilita auditorías y recuperación de datos.
- **Seeders automáticos**: se ejecutan al arrancar la app y son idempotentes. Garantizan que el entorno de desarrollo esté listo desde el primer `dotnet run`.
- **Paginación centralizada**: `PaginationFilters` es heredado por `ProductFilter`, lo que permite reutilizar la lógica de skip/take en cualquier endpoint futuro.
- **`AsNoTracking` en consultas de solo lectura**: evita que EF Core rastree las entidades en los GET, mejorando el rendimiento.

---

## 8. Respuestas a preguntas técnicas

### ¿Por qué modelaste la relación entre `Category` y `Product` de esa manera?

Se eligió una relación **uno a muchos**: una categoría puede tener muchos productos, pero cada producto pertenece exactamente a una categoría (`CategoryId` como FK obligatoria).

Esta decisión refleja la realidad del dominio: un producto de menú siempre pertenece a una sección (bebidas, postres, platos principales), y no tiene sentido que un mismo ítem pertenezca a múltiples categorías. Modelar una relación muchos a muchos agregaría complejidad sin beneficio real para este caso de uso.

El `ON DELETE RESTRICT` en la FK evita borrar una categoría que tenga productos asociados, protegiendo la integridad referencial.

---

### ¿Qué mejorarías si tuvieras más tiempo?

- **Roles y permisos**: distinguir entre usuarios admin (que pueden gestionar categorías) y usuarios regulares.
- **Refresh tokens**: el JWT actual expira en 60 minutos sin posibilidad de renovación sin re-login.
- **Categorías como recurso expuesto**: actualmente las categorías solo se usan internamente; agregaría endpoints CRUD para administrarlas.
- **Logging estructurado**: integrar Serilog con salida a archivo o a un agregador de logs.
- **Tests de integración**: los tests actuales son unitarios; agregaría tests de integración con una base de datos en memoria para cubrir los flujos completos.

---

### Si esta solución pasara a producción, ¿qué cambiarías primero?

Lo primero sería **reemplazar SQLite por una base de datos robusta** (PostgreSQL o SQL Server) con soporte real para concurrencia, transacciones y backups. SQLite no está diseñado para cargas concurrentes en producción.

Inmediatamente después:

1. Sacar el **JWT secret y la cadena de conexión de `appsettings.json`** y moverlos a variables de entorno o a un gestor de secretos (Azure Key Vault, AWS Secrets Manager).
2. Configurar **HTTPS obligatorio** y cabeceras de seguridad (HSTS, CSP).
3. Agregar **rate limiting** en los endpoints de autenticación para prevenir ataques de fuerza bruta.

---

### Si necesitaras notificar cambios en productos en tiempo real, ¿cómo lo resolverías?

Dependiendo del contexto:

- **WebSockets / SignalR**: para notificaciones push hacia clientes web o móviles conectados. SignalR abstrae los WebSockets y tiene soporte nativo en ASP.NET Core. Al crear, actualizar o eliminar un producto, el servicio emitiría un evento al hub correspondiente.
- **Message broker (RabbitMQ / Azure Service Bus)**: si los cambios deben propagarse a otros servicios backend (por ejemplo, un sistema de inventario), publicaría un evento `ProductUpdated` en una cola. Cada consumidor reacciona de forma independiente.
- **Server-Sent Events (SSE)**: alternativa más ligera que WebSockets para notificaciones unidireccionales (servidor → cliente) sin necesidad de librerías adicionales.

Para este tamaño de proyecto, **SignalR** sería la elección más pragmática: se integra fácilmente en el pipeline de ASP.NET Core y permite notificar a los clientes conectados con pocas líneas de código.
