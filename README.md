# 🏡 Convivia

**Facilitar el reparto justo y organizado de tareas domésticas.**

Convivia es una app móvil diseñada para mejorar la convivencia en pisos compartidos o residencias, ayudando a distribuir equitativamente las tareas del hogar. A través de una interfaz intuitiva y un sistema de niveles llamado **Karma**, los usuarios pueden organizar quién hace qué y cuándo, fomentando un entorno más justo y colaborativo.

## 📱 Tecnologías utilizadas
- Frontend: **React**
- Backend: **.NET**
- Diseño de interfaz: **Figma**
- Base de datos: **CosmosDB**

## ✨ Funcionalidades principales
- Crear y gestionar **residencias compartidas**
- Añadir y asignar **tareas domésticas**
- Visualizar actividades en un **calendario integrado**
- Recibir **recordatorios** automáticos
- Sistema de **niveles “Karma”** que recompensa a quienes más contribuyen

## 👥 Usuarios objetivo
- Personas que comparten piso
- Residencias colectivas con organización doméstica

## 🔐 Registro
Para usar Convivia, los usuarios deben crear una cuenta. El registro permite acceder a funcionalidades personalizadas, sincronización de datos y seguimiento del Karma.

## 🚀 Instalación
Disponible en plataformas móviles como **Play Store** y **App Store**. Solo tienes que buscar “Convivia” e instalarla.

## 📸 Ejemplos de uso
- _"Marta crea la residencia, asigna tareas semanales y consulta el calendario para ver quién está al día."_  
- _"Luis revisa su Karma y ve que necesita colaborar más para subir de nivel."_

## 📄 Licencia
Este proyecto está licenciado bajo la **Apache License 2.0**.  
Consulta el archivo `LICENSE` para más detalles.

## 📬 Contacto
¿Tienes preguntas, sugerencias o ganas de colaborar?  
Escríbenos a: **contacto@conviviaapp.com**

```mermaid
flowchart TB
  A["Program.cs: arranque de la aplicación"] --> B["Inicializa Firebase (FirebaseConfig.InitializeFirebase)"]
  A --> C["Contenedor DI: registra servicios y FirestoreDb"]
  B --> D["FirestoreDb (FirestoreDb.Create)"]
  C --> E["Servicios registrados: IFirebaseService, UserService, TareaService, PlantillaTareaService, EspacioService, ..."]
  C --> F["Mapeo de endpoints: MapControllers(), MapEspacioEndpoints(), MapPeticionEndpoints(), MapInvitacionEndpoints(), MapSalaEndpoints(), MapPlantillaTareaEndpoints()"]
  F --> G["Cliente HTTP -> Enrutador (Routing)"]
  G --> H["Endpoint '/api/tareas' (PlantillaTareaEndpoints)"]
  H --> I["Validación DTO (fechas)"]
  I --> J["Si dto.Fechas.Count > 1 -> PlantillaTareaService.CreateFromTareaDtoAsync()"]
  J --> K["Plantilla creada -> TareaService.CreateFromPlantillaAsync(plantilla, fechas) -> crea múltiples tareas"]
  I --> L["Si dto.Fechas.Count == 1 -> TareaService.AddAsync(dto) -> crea tarea única"]
  K --> M["IFirebaseService (FirebaseService) -> operaciones CRUD sobre colecciones 'tareas' y 'plantillatareas'"]
  L --> M
  M --> D
  F --> N["POST /api/usuarios/importar-datos -> UserService.ProbarConexionAsync()"]
  N --> O["UserService -> usa FirestoreDb directamente para operaciones de espacio/usuarios"]
  O --> D
  D --> P["Google Firestore (persistencia)"]
```
