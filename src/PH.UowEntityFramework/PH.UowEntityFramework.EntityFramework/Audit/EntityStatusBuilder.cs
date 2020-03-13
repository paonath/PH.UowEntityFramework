using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;

namespace PH.UowEntityFramework.EntityFramework.Audit
{
    public class EntityStatus
    {
        public List<Entry> Entries { get; set; }

        public EntityStatus()
        {
            this.Entries = new List<Entry>();
        }

        public void AddEntry(Entry e)
        {
            this.Entries.Add(e);
        }

        public void AddRange([NotNull] IEnumerable<Entry> l)
        {
            this.Entries.AddRange(l.OrderBy(x => x.Name).ToArray());
        }

        public static String Serialize(EntityStatus es)
        {
            
            return JsonConvert.SerializeObject(es, Formatting.None);
        }

        public static implicit operator string([CanBeNull] EntityStatus s)
        {
            if (null == s)
            {
                return "";
            }

            return JsonConvert.SerializeObject(s, Formatting.None);
        }

        public static EntityStatus Deserialize(String json)
        {
            EntityStatus es = JsonConvert.DeserializeObject(json, typeof(EntityStatus)) as EntityStatus;
            return es;
        }
    }

    public class Entry
    {
        public string Name { get; set; }
        public string EntryValue { get; set; }
        public bool IsRecursive { get; set; }

        public Entry()
        {
            this.IsRecursive = false;
        }
    }



    internal static class EntityStatusBuilder
    {
        [NotNull]
        static string GetMd5Hash([NotNull] byte[] arrayData)
        {
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

        private static bool CanRetrieve([NotNull] PropertyInfo info, object source, bool retrieveFromReferenceId, [NotNull] out string name,
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
                var rvalue = (byte[])info.GetValue(source);
                
                value = $"MD5 hash: {GetMd5Hash(rvalue)}";
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
                if (typeof(Entity<>).IsAssignableFrom(info.PropertyType))
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

        [CanBeNull]
        public static EntityStatus Parse([CanBeNull] object e, bool retrieveFromReferenceId = true)
            => Parse(e,new List<Entry>(), new Dictionary<string, string>() {{"__UTC serialize time", $"{DateTime.UtcNow:O}"}},
                     retrieveFromReferenceId);


        [CanBeNull]
        public static EntityStatus Parse([CanBeNull] object e, List<Entry> additionalEntries, Dictionary<string, string> additionalInfo,
                                         bool retrieveFromReferenceId = true)
        {
            if (null == e)
            {
                return null;
            }

            var myList = new List<Entry>();

            if (null != additionalEntries && additionalEntries.Count > 0)
            {
                myList.AddRange(additionalEntries);
            }


            var entityType = e.GetType();
            foreach (var propertyInfo in entityType.GetProperties().OrderBy(x => x.Name).ToArray())
            {
                if (CanRetrieve(propertyInfo, e, retrieveFromReferenceId, out string n, out string v))
                {
                    myList.Add(new Entry() {Name = n, EntryValue = v});
                }
            }

            if (myList.Count > 0)
            {
                
                var s = new EntityStatus();

                var id = myList.FirstOrDefault(x => x.Name == "ID" || x.Name == "id" || x.Name == "Id");
                s.AddEntry(id);

                var properties = myList
                                 .Where(x => x.Name != id.Name &&
                                             !x.Name.StartsWith("__", StringComparison.InvariantCultureIgnoreCase))
                                 .OrderBy(x => x.Name).ToArray();

                s.AddRange(properties);

                var additional = properties.Where(x => x.Name.StartsWith("__", StringComparison.InvariantCultureIgnoreCase))
                                           .OrderBy(x => x.Name).ToArray();

                s.AddRange(additional);

                if (!additionalInfo.ContainsKey("__UTC serialize time"))
                {
                    additionalInfo.Add("__UTC serialize time", $"{DateTime.UtcNow:O}");
                }


                foreach (var keyValuePair in additionalInfo)
                {
                    var key = keyValuePair.Key.StartsWith("__", StringComparison.InvariantCultureIgnoreCase)
                                  ? keyValuePair.Key
                                  : $"__{keyValuePair.Key}";
                    myList.Add(new Entry() {Name = key, EntryValue = keyValuePair.Value});
                }
                return s;

            }
            else
            {
                return new EntityStatus();
            }


            
        }
    }


}