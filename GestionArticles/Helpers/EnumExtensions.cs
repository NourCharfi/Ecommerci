using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace GestionArticles.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (member != null)
            {
                var displayAttr = member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                if (displayAttr != null && !string.IsNullOrWhiteSpace(displayAttr.Name))
                    return displayAttr.Name!;
            }
            return value.ToString();
        }
    }

    /// <summary>
    /// Helper pour formater les prix sans ,00 pour les nombres entiers
    /// </summary>
    public static class PriceFormatter
    {
        /// <summary>
        /// Formate un prix en DT sans ,00 pour les nombres entiers
        /// Exemple: 100.00 ? "100 DT", 100.50 ? "100,50 DT"
        /// </summary>
        public static string FormatPrice(decimal price)
        {
            // Si le prix est un nombre entier, afficher sans décimales
            if (price == Math.Floor(price))
            {
                return price.ToString("F0", CultureInfo.GetCultureInfo("fr-FR")) + " DT";
            }
            // Sinon afficher avec 2 décimales
            return price.ToString("F2", CultureInfo.GetCultureInfo("fr-FR")) + " DT";
        }

        /// <summary>
        /// Formate un prix en DT sans ,00 pour les nombres entiers (sans suffixe DT)
        /// </summary>
        public static string FormatPriceNoSuffix(decimal price)
        {
            if (price == Math.Floor(price))
            {
                return price.ToString("F0", CultureInfo.GetCultureInfo("fr-FR"));
            }
            return price.ToString("F2", CultureInfo.GetCultureInfo("fr-FR"));
        }

        /// <summary>
        /// Formate un prix avec conversion float ? decimal
        /// </summary>
        public static string FormatPrice(float price) => FormatPrice((decimal)price);

        /// <summary>
        /// Formate un prix avec conversion double ? decimal
        /// </summary>
        public static string FormatPrice(double price) => FormatPrice((decimal)price);
    }
}