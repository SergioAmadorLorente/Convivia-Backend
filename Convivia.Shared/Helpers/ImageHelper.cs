using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Convivia.Shared.Helpers
{
    /// <summary>
    /// Helper para procesamiento y optimización de imágenes
    /// </summary>
    public static class ImageHelper
    {
        private const long MaxSizeBytes = 524288; // 0.5 MiB

        /// <summary>
        /// Redimensiona y comprime una imagen si supera el tamaño máximo permitido.
        /// </summary>
        /// <param name="imageBytes">Bytes de la imagen original</param>
        /// <returns>Bytes de la imagen optimizada</returns>
        public static byte[] OptimizeImageIfNeeded(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return imageBytes;

            // Si la imagen ya cumple con el tamaño, no hacer nada
            if (imageBytes.Length <= MaxSizeBytes)
                return imageBytes;

            try
            {
                using var image = Image.Load(imageBytes);
                
                // Calculamos el ratio de reducción necesario
                double ratio = Math.Sqrt((double)MaxSizeBytes / imageBytes.Length);
                
                // Reducimos las dimensiones en función del ratio
                int newWidth = (int)(image.Width * ratio * 0.9); // 0.9 para tener margen
                int newHeight = (int)(image.Height * ratio * 0.9);

                // Aseguramos dimensiones mínimas razonables
                if (newWidth < 100) newWidth = 100;
                if (newHeight < 100) newHeight = 100;

                // Redimensionamos la imagen
                image.Mutate(x => x.Resize(newWidth, newHeight));

                // Guardamos con compresión
                using var outputStream = new MemoryStream();
                var encoder = new JpegEncoder
                {
                    Quality = 85 // Calidad del JPEG (1-100)
                };

                image.Save(outputStream, encoder);
                var optimizedBytes = outputStream.ToArray();

                // Si aún supera el tamaño, intentamos con menor calidad
                if (optimizedBytes.Length > MaxSizeBytes)
                {
                    outputStream.SetLength(0);
                    var encoder70 = new JpegEncoder { Quality = 70 };
                    image.Save(outputStream, encoder70);
                    optimizedBytes = outputStream.ToArray();
                }

                // Si aún supera el tamaño, reducimos más las dimensiones
                if (optimizedBytes.Length > MaxSizeBytes)
                {
                    newWidth = (int)(newWidth * 0.8);
                    newHeight = (int)(newHeight * 0.8);
                    image.Mutate(x => x.Resize(newWidth, newHeight));
                    
                    outputStream.SetLength(0);
                    var encoder75 = new JpegEncoder { Quality = 75 };
                    image.Save(outputStream, encoder75);
                    optimizedBytes = outputStream.ToArray();
                }

                return optimizedBytes;
            }
            catch
            {
                // Si falla el procesamiento, devolvemos la imagen original
                return imageBytes;
            }
        }

        /// <summary>
        /// Obtiene el tamaño máximo permitido en bytes.
        /// </summary>
        public static long GetMaxSizeBytes() => MaxSizeBytes;
    }
}
