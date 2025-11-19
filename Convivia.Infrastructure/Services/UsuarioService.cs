using BCrypt.Net; // Para hashing de password (instala: dotnet add package BCrypt.Net-Next)
using Convivia.Aplicacion.DTOs;
using Convivia.Application.DTOs;
using Convivia.Application.Mappers;
using Convivia.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Services
{
    public class UsuarioService
    {
        public const string COLLECTION = "usuarios";
        private readonly IFirebaseService _firebase;

        public UsuarioService(IFirebaseService firebase)
        {
            _firebase = firebase;
        }

        // Obtener usuario por id (devuelve DTO)
        public async Task<UsuarioDto?> GetAsync(string id)
        {
            var usuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            return UsuarioMapper.ToDto(usuario);
        }

        // Obtener usuario por email (devuelve DTO)
        public async Task<UsuarioDto?> GetByEmailAsync(string email)
        {
            var usuarios = await _firebase.QueryAsync<Usuario>(COLLECTION, "Email", email);
            var usuario = usuarios.Count > 0 ? usuarios[0] : null;
            return UsuarioMapper.ToDto(usuario);
        }

        // Crear usuario (recibe CreateUsuarioDto, devuelve UsuarioDto)
        public async Task<UsuarioDto> AddAsync(CreateUsuarioDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Mapea DTO a dominio
            var usuario = UsuarioMapper.FromCreateDto(dto);
            if (usuario == null) throw new InvalidOperationException("Error al mapear el DTO.");

            // Validaciones básicas (delegadas a dominio si es posible; aquí por simplicidad)
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuario.Email))
                throw new ArgumentException("El email no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuario.Password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            // Verifica unicidad de email
            var existing = await _firebase.QueryAsync<Usuario>(COLLECTION, "Email", usuario.Email);
            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            // Hashea password (seguridad: nunca guardes plain text)
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);  // Corregido: Agrega .Net.BCrypt

            // Asegura FechaRegistro en UTC
            usuario.FechaRegistro = DateTime.UtcNow;

            // Guarda en Firestore
            await _firebase.AddAsync(COLLECTION, usuario.Id, usuario);

            // Devuelve DTO
            return UsuarioMapper.ToDto(usuario);
        }

        // Actualizar usuario completo (recibe UpdateUsuarioDto, devuelve UsuarioDto)
        public async Task<UsuarioDto?> UpdateAsync(string id, UpdateUsuarioDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var usuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            if (usuario == null) return null;

            // Usa mapper para updates parciales (solo campos no-null)
            UsuarioMapper.UpdateDomainFromDto(usuario, dto);

            // Hashea password si se actualizó
            if (!string.IsNullOrWhiteSpace(dto.Password))
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);  // Corregido

            // Guarda
            await _firebase.UpdateAsync(COLLECTION, id, usuario);

            // Devuelve DTO
            return UsuarioMapper.ToDto(usuario);
        }

        // PATCH: Actualización parcial de usuario (usa UpdateUsuarioDto)
        public async Task<UsuarioDto?> PatchAsync(string id, UpdateUsuarioDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var usuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            if (usuario == null) return null;

            // Usa mapper para updates parciales
            UsuarioMapper.UpdateDomainFromDto(usuario, dto);

            // Hashea password si se actualizó
            if (!string.IsNullOrWhiteSpace(dto.Password))
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);  // Corregido

            // Guarda
            await _firebase.UpdateAsync(COLLECTION, id, usuario);

            // Devuelve DTO
            return UsuarioMapper.ToDto(usuario);
        }

        // Eliminar usuario
        public async Task<bool> DeleteAsync(string id)
        {
            var usuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            if (usuario == null) return false;
            await _firebase.DeleteAsync(COLLECTION, id);
            return true;
        }

        // Contar usuarios
        public async Task<int> CountUsuariosAsync()
        {
            var usuarios = await _firebase.GetAllAsync<Usuario>(COLLECTION);
            return usuarios.Count;
        }
    }
}