using System.Data.SqlClient;

namespace SevSharks.Identity.BusinessLogic.Extensions
{
    public static class SqlDataReaderExtensions
    {
        public static string SafeGetString(this SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
            {
                return reader.GetString(colIndex);
            }
                
            return string.Empty;
        }

        public static int SafeGetInteger(this SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
            {
                return reader.GetInt32(colIndex);
            }

            return 0;
        }

        public static bool SafeGetBoolean(this SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
            {
                return reader.GetBoolean(colIndex);
            }

            return false;
        }
    }
}
