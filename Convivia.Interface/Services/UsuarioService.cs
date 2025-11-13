using Convivia.Domain.Models;
using Convivia.Application.DTOs;
using Convivia.Application.Mappers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Convivia.Interface.Services
{
    public class UsuarioService
    {
        public const string COLLECTION = "usuarios";
        private readonly IFirebaseService _firebase;
        
        public UsuarioService(IFirebaseService firebase)
        {
            _firebase = firebase;
        }

        // Obtener usuario por id
        public async Task<Usuario?> GetAsync(string id)
        {
            var usuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            return usuario;
        }

        // Obtener usuario por email
        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            var usuarios = await _firebase.QueryAsync<Usuario>(COLLECTION, "Email", email);
            return usuarios.Count > 0 ? usuarios[0] : null;
        }

        // Crear usuario
        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuario.Email))
                throw new ArgumentException("El email no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuario.Password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            var existing = await _firebase.QueryAsync<Usuario>(COLLECTION, "Email", usuario.Email);
            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            
            usuario.FechaRegistro = usuario.FechaRegistro.ToUniversalTime();

            await _firebase.AddAsync(COLLECTION, usuario.Id, usuario);
            return usuario;
        }

        /*
        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuario.Email))
                throw new ArgumentException("El email no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuario.Password))
                throw new ArgumentException("La contraseña no puede estar vacía.");
            var existing = await _firebase.QueryAsync<Usuario>(COLLECTION, "Email", usuario.Email);
            if (existing.Count > 0)
                throw new InvalidOperationException("Ya existe un usuario con ese email.");
            await _firebase.AddAsync(COLLECTION, usuario.Id, usuario);
            return usuario;
        }
        */

        // Actualizar usuario
        public async Task<Usuario?> UpdateAsync(string id, Usuario updatedUsuario)
        {
            var existingUsuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            if (existingUsuario == null)
                return null;
            existingUsuario.Nombre = updatedUsuario.Nombre;
            existingUsuario.Email = updatedUsuario.Email;
            existingUsuario.Password = updatedUsuario.Password;
            existingUsuario.Telefono = updatedUsuario.Telefono;
            existingUsuario.Premium = updatedUsuario.Premium;
            await _firebase.UpdateAsync(COLLECTION, id, existingUsuario);
            return existingUsuario;
        }

        // PATCH: Actualización parcial de usuario
        public async Task<UsuarioDto?> PatchAsync(string id, UpdateUsuarioDto dto)
        {
            var persist = await _firebase.GetAsync<UsuarioPersist>(COLLECTION, id);
            if (persist == null)
                return null;

            if (dto.Nombre != null) persist.Nombre = dto.Nombre;
            if (dto.Email != null) persist.Email = dto.Email;
            if (dto.Telefono != null) persist.Telefono = dto.Telefono;

            await _firebase.UpdateAsync(COLLECTION, id, persist);
            return UsuarioMapper.ToDto(persist);
        }

        // Eliminar usuario
        public async Task<bool> DeleteAsync(string id)
        {
            var existingUsuario = await _firebase.GetAsync<Usuario>(COLLECTION, id);
            if (existingUsuario == null)
                return false;
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
