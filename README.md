# ContactHUB

ContactHUB es una aplicación web ASP.NET Core MVC para la gestión de contactos personales y empresariales, diseñada con una arquitectura modular y buenas prácticas de desarrollo.

## Tabla de Contenidos
- [Características](#características)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Arquitectura](#arquitectura)
- [Módulos Principales](#módulos-principales)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Configuración](#configuración)
- [Créditos](#créditos)

---

## Características
- Gestión de contactos con CRUD completo
- Filtros por departamento y etiquetas
- Exportación de contactos a Excel y JSON
- Estadísticas de actividad (agregados/eliminados hoy, activos, total)
- Interfaz moderna con Bootstrap y Razor
- Validaciones y manejo de errores modularizados
- Autenticación y autorización de usuarios

## Estructura del Proyecto
```
ContactHUB/
├── Controllers/
│   ├── ContactosController.cs
│   └── HomeController.cs
├── Data/
│   └── DbContext.cs
├── Helpers/
│   ├── ContactosQueryHelper.cs
│   ├── ContactosCrudHelper.cs
│   ├── ContactosExportHelper.cs
│   ├── ContactosStatsHelper.cs
│   ├── ContactosValidationHelper.cs
│   └── ContactosActionHelper.cs
├── Models/
│   └── ErrorViewModel.cs
├── Views/
│   ├── Contactos/
│   │   ├── Index.cshtml
│   │   ├── _ContactosCardsPartial.cshtml
│   │   ├── _ContactosFilterPartial.cshtml
│   │   ├── _ContactosStatsPartial.cshtml
│   │   ├── _ContactoToastAndDeleteScripts.cshtml
│   │   └── _ContactoModalScripts.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   ├── Shared/
│   │   └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
├── Program.cs
├── ContactHUB.csproj
└── ContactHUB.sln
```

## Arquitectura
- **MVC (Model-View-Controller):** Separación clara entre lógica de negocio, presentación y acceso a datos.
- **Helpers:** Lógica modular para queries, CRUD, exportación, estadísticas y validaciones.
- **Vistas Parciales:** UI dividida en componentes reutilizables (tarjetas, filtros, stats, scripts).
- **Validación y Manejo de Errores:** Centralizados en helpers para mantener los controladores limpios.
- **Frontend:** Bootstrap, jQuery y Razor para una experiencia moderna y responsiva.

## Módulos Principales
- **ContactosController:** Orquesta la gestión de contactos, usando helpers para lógica y validaciones.
- **ContactosQueryHelper:** Consultas y filtrado de contactos.
- **ContactosCrudHelper:** Operaciones CRUD y validaciones de negocio.
- **ContactosExportHelper:** Exportación a Excel y JSON.
- **ContactosStatsHelper:** Estadísticas de actividad y estado.
- **ContactosValidationHelper:** Validaciones de entrada y usuario.
- **ContactosActionHelper:** Flujo y manejo de errores en acciones complejas.
- **Vistas Parciales:**
  - `_ContactosCardsPartial.cshtml`: Tarjetas de contactos
  - `_ContactosFilterPartial.cshtml`: Filtros de búsqueda
  - `_ContactosStatsPartial.cshtml`: Estadísticas
  - `_ContactoToastAndDeleteScripts.cshtml`: Scripts de UI
  - `_ContactoModalScripts.cshtml`: Modales de edición/creación

## Instalación y Ejecución
1. Clona el repositorio:
   ```bash
   git clone https://github.com/richcrd/ContactHUB.git
   ```
2. Restaura los paquetes NuGet:
   ```bash
   dotnet restore
   ```
3. Aplica las migraciones y configura la base de datos:
   ```bash
   dotnet ef database update
   ```
4. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```
5. Accede a `http://localhost:5000` en tu navegador.

## Configuración
- Edita `appsettings.json` para la cadena de conexión y parámetros de la base de datos.
- Personaliza estilos en `wwwroot/css/site.css` y scripts en `wwwroot/js/site.js`.

## Créditos
- Desarrollado por richcrd y colaboradores.
- Basado en ASP.NET Core MVC, Entity Framework Core, Docker, Bootstrap y jQuery.

---

Para dudas, sugerencias o contribuciones, abre un issue o pull request en el repositorio.
