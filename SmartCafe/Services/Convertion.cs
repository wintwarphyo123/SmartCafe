using SmartCafe.Interfaces;
using System.Data;

namespace SmartCafe.Services
{
    public class Convertion : IConvertion
    {
        public string GetFileExtension(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return ".bin";

            try
            {
                // Take the first few characters to check the file header safely
                string data = base64String.Length > 20 ? base64String.Substring(0, 20) : base64String;

                if (data.StartsWith("iVBORw")) return ".png";
                if (data.StartsWith("/9j/")) return ".jpg";
                if (data.StartsWith("R0lGOD")) return ".gif";
                if (data.StartsWith("UklGR")) return ".webp";

                return ".jpg"; // Default fallback
            }
            catch
            {
                return ".jpg";
            }
        }

    }
}
