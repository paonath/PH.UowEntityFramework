using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace PH.UowEntityFramework.EntityFramework.Audit
{

    /// <summary>
    /// Entity's audit data
    /// </summary>
    /// <seealso cref="PH.UowEntityFramework.EntityFramework.Audit.AuditInfoBase" />
    public abstract class 
        AuditInfo : AuditInfoBase
    {
        
        /// <summary>Gets or sets the new values.</summary>
        /// <value>The new values.</value>
        public string JsonStringValues { get; set; }

        /// <summary>Gets the new values as Dictionary.</summary>
        /// <value>The new values.</value>
        public Dictionary<string, object> Values => GetValues();

        private Dictionary<string, object> GetValues()
        {
            if (string.IsNullOrEmpty(JsonStringValues))
            {
                return new Dictionary<string, object>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(JsonStringValues);

            //return JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonStringValues);
        }
    }


    internal class AuditInfoResult : AuditInfo
    {
        public int Version { get; set; }
    }

    
}