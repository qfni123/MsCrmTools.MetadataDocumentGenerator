using System;
using Type = System.Type;

namespace Xrmwise.Xrm.Framework
{
    /// <summary>
    /// Type conversion utility methods
    /// </summary>
    public static class TypeConverter
    {
        /// <summary>
        /// The convert to.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// T is a type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// typed value
        /// </returns>
        /// <exception cref="System.Exception">
        /// throw exception if cannot be converted to the type specified
        /// </exception>
        public static T ConvertTo<T>(object value)
        {
            return ConvertTo(value, default(T));
        }

        /// <summary>
        /// The convert to.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="nullValue">
        /// The null value.
        /// </param>
        /// <typeparam name="T">
        /// T is value return when null is passed in
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// throw exception if cannot be converted to the type specified
        /// </exception>
        public static T ConvertTo<T>(object value, T nullValue)
        {
            try
            {
                if (value == null)
                {
                    return nullValue;
                }

                var valueType = value.GetType();
                var retType = typeof(T);

                var isValueNullable = valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>);
                var isReturnValueNullable = retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Nullable<>);

                if (isReturnValueNullable)
                {
                    // return type is nullable, change from nullable of property value ot the nullable of return
                    var nullableConverter = new NullableConverter();
                    return (T)nullableConverter.ToNullable(value, retType);
                }

                if (isValueNullable)
                {
                    var nullablePropertyInfo = value.GetType().GetProperty("HasValue");
                    if ((bool)nullablePropertyInfo.GetValue(value, null))
                    {
                        var val = nullablePropertyInfo.GetValue("Value", null);
                        return (T)Convert.ChangeType(val, typeof(T));
                    }

                    return nullValue;
                }

                // either property value type or return type is nullable
                if (typeof(T) == typeof(object))
                {
                    return (T)value;
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to convert a value of {0} to a value of {1}. The source value is: {2}.", value == null ? string.Empty : value.GetType().FullName, typeof(T).FullName, value), e);
            }
        }

        /// <summary>
        /// The convert to.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="toType">
        /// The to type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// throw exception if cannot be converted to the type specified
        /// </exception>
        public static object ConvertTo(object value, Type toType)
        {
            if (toType == null)
            {
                return value;
            }

            if (value == null)
            {
                return null;
            }

            try
            {
                var valueType = value.GetType();

                var isValueNullable = valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>);
                var isReturnValueNullable = toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>);

                if (isReturnValueNullable)
                {
                    // return type is nullable, change from nullable of property value ot the nullable of return
                    var nullableConverter = new NullableConverter();
                    return nullableConverter.ToNullable(value, toType);
                }

                if (isValueNullable)
                {
                    var nullablePropertyInfo = value.GetType().GetProperty("HasValue");
                    if ((bool)nullablePropertyInfo.GetValue(value, null))
                    {
                        var val = nullablePropertyInfo.GetValue("Value", null);
                        return ChangeTo(val, toType);
                    }

                    return Activator.CreateInstance(toType);
                }

                // either property value type or return type is nullable
                if (toType == typeof(object))
                {
                    return value;
                }

                return ChangeTo(value, toType);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to convert a value of {0} to a value of {1}. The source value is: {2}.", value.GetType().FullName, toType.FullName, value), e);
            }            
        }

        /// <summary>
        /// The change to.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// object value
        /// </returns>
        /// <exception cref="System.Exception">
        /// Exception if cannot be converted
        /// </exception>
        private static object ChangeTo(object value, Type type)
        {
            try
            {
                return Convert.ChangeType(value, type);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to convert a value of {0} to a value of {1}. The source value is: {2}.", value.GetType().FullName, type.FullName, value), e);
            }
        }
    }
}
