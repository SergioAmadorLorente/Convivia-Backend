# Configuración de Firebase - Solución al Error de Credenciales

## Problema Resuelto

**Error Original:** `System.InvalidOperationException: JSON data does not represent a valid service account credential.`

### Causa Raíz
Existían dos archivos `serviceAccount.json` en el proyecto:
1. `Convivia.API/Firebase/serviceAccount.json` - ? Contenía credenciales válidas
2. `Convivia.Interface/Firebase/serviceAccount.json` - ? Estaba vacío (plantilla)

La aplicación estaba leyendo el archivo vacío, causando el error de validación.

## Solución Implementada

### 1. Actualización de Credenciales
- Se actualizó `Convivia.Interface/Firebase/serviceAccount.json` con las credenciales válidas
- Ambos archivos ahora contienen las mismas credenciales completas

### 2. Uso de Implementación Robusta
- Se eliminó `Convivia.API/Infraestructure/Firebase.cs` (implementación simple)
- Se usa únicamente `Convivia.Interface/Infraestructure/FirebaseConfig` (implementación robusta con validación)
- Esta implementación incluye:
  - Validación de campos críticos (`private_key`, `client_email`)
  - Soporte para múltiples fuentes de credenciales
  - Logs detallados para debugging
  - Fallback a ADC (Application Default Credentials)

### 3. Actualización de Program.cs
Se cambió el namespace importado:
```csharp
// Antes:
using AuthApiDemo.Infraestructure;

// Ahora:
using Convivia.Interface.Infraestructure;
```

## Configuración de Credenciales

### Opción 1: Archivo Local (Recomendado para Desarrollo)
Asegúrate de tener un archivo válido en:
- `Convivia.API/Firebase/serviceAccount.json` o
- `Convivia.Interface/Firebase/serviceAccount.json`

El archivo debe contener:
```json
{
  "type": "service_account",
  "project_id": "tu-proyecto-id",
  "private_key_id": "...",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@tu-proyecto.iam.gserviceaccount.com",
  "client_id": "...",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "...",
  "universe_domain": "googleapis.com"
}
```

### Opción 2: Variable de Entorno con Ruta
```bash
set FIREBASE_CREDENTIALS_PATH=C:\ruta\completa\a\serviceAccount.json
```

### Opción 3: Variable de Entorno con JSON
```bash
set FIREBASE_CREDENTIALS_JSON={"type":"service_account",...}
```

### Opción 4: Application Default Credentials (Producción en GCP)
Si la aplicación corre en Google Cloud Platform (Cloud Run, App Engine, GKE), las credenciales se obtienen automáticamente.

## Logs de Inicialización

Al iniciar la aplicación, verás logs como:
```
[FirebaseConfig] Buscando credenciales en: C:\path\to\Firebase\serviceAccount.json
[FirebaseConfig] BaseDirectory: C:\path\to\
[FirebaseConfig] Archivo encontrado. Validando contenido...
[FirebaseConfig] Primeros 100 caracteres del archivo: {
  "type": "service_account",
  "project_id": "convivia-862f2",...
[FirebaseConfig] Credenciales válidas encontradas en archivo. GOOGLE_APPLICATION_CREDENTIALS establecido.
[FirebaseConfig] FirebaseApp inicializado correctamente.
```

## Seguridad

?? **IMPORTANTE**: 
- **NUNCA** commitees `serviceAccount.json` a Git si contiene credenciales reales
- Usa `.gitignore` para excluir archivos de credenciales
- En producción, usa variables de entorno o servicios de secretos (Azure Key Vault, GCP Secret Manager, etc.)
- Rota las credenciales periódicamente

## Verificación

Para verificar que Firebase está correctamente configurado:
1. Ejecuta la aplicación
2. Revisa los logs de consola
3. Prueba el endpoint: `POST /api/usuarios/importar-datos`
4. Si ves "FirebaseApp inicializado correctamente", la configuración es exitosa

## Troubleshooting

### Si sigues viendo el error:
1. Verifica que el archivo existe en la ruta indicada en los logs
2. Abre el archivo y confirma que `private_key` y `client_email` no estén vacíos
3. Verifica que el JSON sea válido (usa un validador JSON online)
4. Asegúrate de que el archivo se copie al directorio de salida (revisa el .csproj)
5. Intenta limpiar y reconstruir la solución

### Error "File not found":
```bash
# Verifica que el archivo esté en la ruta correcta
dir bin\Debug\net8.0\Firebase\serviceAccount.json
```

### Error de formato JSON:
- Asegúrate de que no haya caracteres especiales no escapados
- Verifica que `\n` esté presente en `private_key`
- Usa un editor que soporte UTF-8 sin BOM
