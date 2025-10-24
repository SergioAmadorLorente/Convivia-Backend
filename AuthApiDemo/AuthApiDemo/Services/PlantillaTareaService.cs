
/// TO DO: Implementar el servicio para manejar plantillas de tareas en la aplicación AuthApiDemo.
/// 

using System;
using System.Threading.Tasks;
using AuthApiDemo.Models;
using AuthApiDemo.DTOs;

namespace AuthApiDemo.Services
{
    public class PlantillaTareaService
    {
        public const string COLLECTION = "plantillatareas";
        private readonly IFirebaseService _firebase;

        public PlantillaTareaService(IFirebaseService firebase)
        {
            _firebase = firebase;
        }

    }
}
