// ReSharper disable CheckNamespace

namespace Library
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this byte value) where T : struct => ((int)value).ToEnum<T>();
        public static T ToEnum<T>(this int value) where T : struct => value.ToString().ToEnum<T>();
        /// <summary>
        /// Trys to convert the string to the Enum-Value. If not found throws exception   
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value) where T : struct => value.ToEnum<T>(true);
        /// <summary>
        /// Trys to convert the string to the Enum-Value. If not found returns defaultValueIfNotFound
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValueIfNotFound"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value, T defaultValueIfNotFound) where T : struct => Enum.TryParse<T>(value, true, out var result) ? result : defaultValueIfNotFound;
        public static T? ToEnumNullable<T>(this string value) where T : struct => value.IsTrimmedNullOrEmpty() ? null : value.ToEnum<T>();
        public static T ToEnum<T>(this string value, bool ignoreCase) where T : struct => Enum.Parse<T>(value.Trim(), ignoreCase);

        public static bool TryParse<T>(this string value, out T enumValue) where T : struct
        {
            bool result = Enum.TryParse(typeof(T), value, true, out object? obj);
            enumValue = (T?)obj ?? default;
            return result;
        }
    }
}
