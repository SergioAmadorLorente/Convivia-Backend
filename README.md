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

# Flujo del programa desacoplado

Este diagrama representa la arquitectura desacoplada de una aplicación dividida en distintos dominios técnicos: Infrastructure, Domain, Application y API.  
Cada grupo contiene componentes que interactúan entre sí para mantener la responsabilidad bien separada:

- **Infrastructure** maneja la persistencia y el mapeo de datos entre la base de datos y la capa de dominio.
- **Domain** almacena las entidades del núcleo del negocio, independientes de detalles técnicos.
- **Application** transforma y gestiona los datos a DTOs que luego mostraremos pasando por mapeos.
- **API** expone los datos transformados por la capa de aplicación a los consumidores externos.
- **DB** es la fuente persistente final, conectada solo a la infraestructura.

Las flechas en el diagrama detallan el flujo de datos y dependencias entre las distintas capas, garantizando un diseño desacoplado y escalable, donde cada bloque tiene un rol claro.

```mermaid
flowchart TB
    %% Infraestructura
    subgraph Infrastructure
        InfraModels["Infrastructure/Models"]
        InfraMapp["Infrastructure/Mapp"]
    end

    %% Dominio
    subgraph Domain
        DomainEntities["Domain/Entities"]
    end

    %% Aplicación
    subgraph App
        AppMapp["Application/Mapp"]
        AppDTO["Application/DTO"]
    end

    %% API
    subgraph API
        APIControllers["API/Controllers"]
    end

    %% Base de Datos
    DB["DB"]

    %% Relaciones
    DB --> InfraModels
    InfraModels --> DB

    InfraModels --> InfraMapp
    InfraMapp --> InfraModels

    DomainEntities --> InfraMapp
    InfraMapp --> DomainEntities

    AppMapp --> DomainEntities
    DomainEntities --> AppMapp

    AppDTO --> AppMapp
    AppMapp --> AppDTO

    AppDTO --> APIControllers
    APIControllers --> AppDTO
```
