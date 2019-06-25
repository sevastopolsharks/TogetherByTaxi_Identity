using System;
using System.ComponentModel;

namespace SevSharks.Identity.BusinessLogic.Extensions
{
    public static class EnumExtentions
    {
        public static string GetDescription(this Enum @enum)
        {
            var attributes = (DescriptionAttribute[])Attribute.GetCustomAttributes(@enum.GetType().GetField(@enum.ToString()), typeof(DescriptionAttribute));
            return attributes.Length > 0 ? attributes[0].Description : @enum.ToString();
        }
    }
}