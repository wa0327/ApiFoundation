using System;
using System.Reflection;

namespace ApiFoundation
{
    internal static class TypeExtensions
    {
        private static FieldInfo GetFieldInfo(this Type source, string name)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return source.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 取得 public/private 欄位值。
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="obj">The object.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal static TValue GetField<TValue>(this Type source, object obj, string name)
        {
            return (TValue)source.GetFieldInfo(name).GetValue(obj);
        }

        /// <summary>
        /// 設定 public/private 欄位值。
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="obj">The object.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        internal static void SetField<TValue>(this  Type source, object obj, string name, TValue value)
        {
            source.GetFieldInfo(name).SetValue(obj, value);
        }
    }
}