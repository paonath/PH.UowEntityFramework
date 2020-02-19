using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace PH.UowEntityFramework.EntityFramework.Audit
{

    internal class DateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
        }
    }


    internal class EntityToJsonConverterFactory : JsonConverterFactory
    {
        /// <summary>When overridden in a derived class, determines whether the converter instance can convert the specified object type.</summary>
        /// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
        /// <returns>
        /// <see langword="true" /> if the instance can convert the specified object type; otherwise, <see langword="false" />.</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IEntity).IsAssignableFrom(typeToConvert);

        }

        /// <summary>Creates a converter for a specified type.</summary>
        /// <param name="typeToConvert">The type handled by the converter.</param>
        /// <param name="options">The serialization options to use.</param>
        /// <returns>A converter for which <typeparamref name="T" /> is compatible with <paramref name="typeToConvert" />.</returns>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new EntityToJsonConverter();

            //throw new NotImplementedException();
        }
    }

    internal class EntityToJsonConverter : System.Text.Json.Serialization.JsonConverter<object> 
    
        
    {



        [NotNull]
        static string GetMd5Hash([NotNull] byte[] arrayData)
        {
            if (null == arrayData || arrayData.Length == 0)
            {
                return string.Empty;
            }

            using (var md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(arrayData);

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
           
        }

        private static bool CanRetrieve([NotNull] PropertyInfo info, object source, [NotNull] out string name,
                                        [CanBeNull] out string value)
        {
            name  = info.Name;
            value = null;

            if (info.PropertyType.IsEnum)
            {
                value = $"{info.GetValue(source)}";
                return true;

            }

            if (info.PropertyType == typeof(Enum[]))
            {
                Enum[] r =(Enum[]) info.GetValue(source);
                if (null != r)
                {
                    var s = r.Select(x => x.ToString()).ToArray();
                    var v = string.Join(",", s);
                    value = $"[{v}]";
                }

            }

            if (info.PropertyType.IsValueType)
            {
                value = $"{info.GetValue(source)}";
                return true;
            }

            if (info.PropertyType == typeof(byte[]))
            {
                if (name == "Timestamp")
                {
                    return false;
                }
                
                    var rvalue = (byte[])info.GetValue(source);
                    if (null != rvalue && rvalue.Length > 0)
                    {
                        value = $"MD5 hash: {GetMd5Hash(rvalue)}";
                    
                    }

                    return true;
                
                
            }

            

            if (info.PropertyType == typeof(string))
            {
                value = $"{info.GetValue(source)}";
                return true;
            }

            if (info.PropertyType == typeof(char[]))
            {
                value = $"[]";
                char[] r = (char[]) info.GetValue(source);
                if (null != r)
                {
                    var s = r.Select(x => x.ToString()).ToArray();
                    var v = string.Join(",", s);
                    value = $"[{v}]";
                }

                return true;
            }

            if (info.PropertyType.IsGenericType &&
                (info.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                 || info.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)
                 || info.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                ))
            {
                return false;
            }


            if (info.PropertyType.IsClass)
            {
                if (typeof(IEntity).IsAssignableFrom(info.PropertyType))
                {
                    var aEntity = info.GetValue(source);
                    if (null != aEntity)
                    {
                        var id  = info.PropertyType.GetProperty("Id");
                        var obj = id?.GetValue(aEntity);
                        value = $"{obj}";
                    }


                    name = $"{name}->Id";
                    return true;
                }
                else
                {
                    var c = JsonConvert.SerializeObject(info.GetValue(source));
                    value = c;
                    return true;
                }

            }


            return false;
        }


        /// <summary>Reads and converts the JSON to type <typeparamref name="T" />.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        [Obsolete("Read entities from JSON is not supported")]
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Read entities from JSON is not supported");
        }

        /// <summary>Writes a specified value as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (null != value && value is IEntity)
            {

                var entityType = value.GetType();

                

                writer.WriteStartObject();
                

                
                foreach (var propertyInfo in entityType.GetProperties().OrderBy(x => x.Name).ToArray())
                {
                    if (CanRetrieve(propertyInfo, value, out string n, out string v))
                    {
                        //d.Add( n, v);
                        writer.WritePropertyName(n);
                        writer.WriteStringValue(v);
                    }
                }
                
                writer.WriteEndObject();
            }
        }



    }
}
