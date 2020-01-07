using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace PH.UowEntityFramework.EntityFramework.Audit
{

    /// <summary>
    /// Entity's audit data
    /// </summary>
    /// <seealso cref="PH.UowEntityFramework.EntityFramework.Audit.AuditInfoBase" />
    public class 
        AuditInfo : AuditInfoBase
    {
        /// <summary>Gets or sets the old values.</summary>
        /// <value>The old values.</value>
        public string JsonOldValues { get; set; }

        /// <summary>Gets the old values as Dictionary, if any.</summary>
        /// <value>The new values.</value>
        public Dictionary<string, object> OldValues => GetOldValues();

        private Dictionary<string, object> GetOldValues()
        {
            if (string.IsNullOrEmpty(JsonOldValues))
            {
                return new Dictionary<string, object>();
            }
            
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonOldValues);
        }

        /// <summary>Gets or sets the new values.</summary>
        /// <value>The new values.</value>
        public string JsonNewValues { get; set; }

        /// <summary>Gets the new values as Dictionary.</summary>
        /// <value>The new values.</value>
        public Dictionary<string, object> NewValues => GetNewValues();

        private Dictionary<string, object> GetNewValues()
        {
            if (string.IsNullOrEmpty(JsonNewValues))
            {
                return new Dictionary<string, object>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonNewValues);
        }
    }
}