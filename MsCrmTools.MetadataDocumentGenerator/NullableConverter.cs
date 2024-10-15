using System;
using System.ComponentModel;

namespace Xrmwise.Xrm.Framework
{
    /// <summary>
    /// The null-able processor.
    /// </summary>
    public class NullableConverter
    {
        /// <summary>
        /// The convert to null-able.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="nullableType">
        /// The null-able type.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object ToNullable(object value, Type nullableType)
        {
            var underlyingType = Nullable.GetUnderlyingType(nullableType);
            var method = this.GetType().GetMethod("ToNullable", new[] { typeof(object) });
            var generic = method.MakeGenericMethod(underlyingType);
            return generic.Invoke(this, new[] { value });            
        }

        /// <summary>
        /// IMPORTANT: This method needs to be public. Also Don't change the method name.
        /// Don't call this method directly.
        /// It is referred via reflection
        /// </summary>
        /// <typeparam name="T">
        /// T is a type
        /// </typeparam>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// a instance of T
        /// </returns>
        public T? ToNullable<T>(object value) where T : struct
        {
            T? result = default(T);
            try
            {
                var td = TypeDescriptor.GetConverter(typeof(T));

                if (td.CanConvertFrom(value.GetType()))
                {
                    var convertFrom = td.ConvertFrom(value);
                    if (convertFrom != null)
                    {
                        result = (T)convertFrom;
                    }
                }
                else
                {
                    result = (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error occured while initiating a Nulllable instance of {typeof (T)}", e);
            }

            return result;
        }
    }
}
