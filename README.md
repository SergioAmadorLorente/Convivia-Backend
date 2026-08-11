**_### ESPAÑOL_**

# Convivia
![Logo](https://drive.google.com/uc?export=view&id=1so15OHqQq5aajitB4dvVLvT_QWf53RhJ)

**Facilitar el reparto justo y organizado de tareas domésticas.**

Convivia es una app móvil diseñada para mejorar la convivencia en pisos compartidos o residencias, ayudando a distribuir equitativamente las tareas del hogar. A través de una interfaz intuitiva y un sistema de niveles llamado **Karma**, los usuarios pueden organizar quién hace qué y cuándo, fomentando un entorno más justo y colaborativo.

##  Tecnologías utilizadas
- Frontend: **React (TypeScript)**
- Backend: **C#.NET**
- Diseño de interfaz: **Figma**
- Base de datos: **Firebase**

## DEMO Youtube

[Ver DEMO](https://youtu.be/hWP5XzAgejk)

##  Funcionalidades principales
- Crear y gestionar **Residencias Compartidas**
- Añadir y asignar **Tareas Domésticas y facturas**
- Sistema de **Niveles “Karma”** que recompensa a quienes más contribuyen
- Disponible idioma **Español, Íngles, Francés, Italiano , Alemán y Portugués**

## Usuarios objetivo
- Personas que comparten piso
- Residencias colectivas con organización doméstica

## Registro
Para usar Convivia, los usuarios deben crear una cuenta. El registro permite acceder a funcionalidades personalizadas, sincronización de datos y seguimiento del Karma.

##  Instalación GRATIS
Solo tienes que descargarla e instalarla(Es APK Dynamic, solo se instala 1 vez y se actualiza sola al abrir la app si existe alguna update)

[Descargar](https://drive.google.com/file/d/18DU81odgJFJyJUcyGd-qrx3Zk9GBTwXD/view)

##  Ejemplos de uso
- _"Marta crea la residencia, asigna tareas semanales y consulta el calendario para ver quién está al día."_  
- _"Luis revisa su Karma y ve que necesita colaborar más para subir de nivel."_


**### _ENGLISH_**

# Convivia
![Logo](https://drive.google.com/uc?export=view&id=1so15OHqQq5aajitB4dvVLvT_QWf53RhJ)

**Facilitating the fair and organized distribution of household chores.**

Convivia is a mobile app designed to improve cohabitation in shared apartments or residences by helping to distribute household tasks equitably. Through an intuitive interface and a leveling system called **Karma**, users can organize who does what and when, fostering a fairer and more collaborative environment.


**Convivia**
**Facilitating the fair and organized distribution of household chores.**
**Convivia**
****** ## Technologies Used
- Frontend: **React (TypeScript)**
- Backend: **C#.NET**
- Interface Design: **Figma**
- Database: **Firebase**

## DEMO Youtube
[Watch DEMO](https://youtu.be/hWP5XzAgejk)

## Main Features
- Create and manage **Shared Living Spaces**
- Add and assign **Household Chores and Invoices**
- **Karma Leveling** system that rewards top contributors
-  Available **English, Spanish, French, Italian, German and Portuguese** Language

## Target Users
- People who share apartments
- Shared living spaces with household management

## Registration
To use Convivia, users must create an account. Registration allows access to customized features, data synchronization, and Karma tracking.

## Installation FREE
Just download and install it (It's a Dynamic APK, it only needs to be installed once and it updates automatically when you open the app if there's an update).

[Download](https://drive.google.com/file/d/18DU81odgJFJyJUcyGd-qrx3Zk9GBTwXD/view)

## Usage Examples
- "Marta creates the residence, assigns weekly tasks, and checks the calendar to see who's on track."
- "Luis checks his Karma and sees that he needs to collaborate more to level up."

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
        Service["Application/Service"]
    end

    %% API
    subgraph API
        APIControllers["API/Controllers"]
    end

    %% Base de Datos
    DB[(Firebase)]

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

    Service --> AppDTO
    AppDTO --> Service

    APIControllers --> Service
    Service --> APIControllers
```

## License

This project is provided for educational, research, and portfolio purposes only.

Commercial use, redistribution, sublicensing, selling, hosting as a service, or creating commercial derivative works is prohibited without the prior written permission of the author.

See the LICENSE file for more information.
