using System.Collections.Generic;
using JetBrains.Annotations;

namespace PH.UowEntityFramework.TestCtx.Models
{
    public class UserDebug : PH.UowEntityFramework.EntityFramework.Abstractions.Identity.Models.UserEntity
    {
        [CanBeNull] 
        public string Firstname { get; set; }
        [CanBeNull]
        public string LastName { get; set; }

        public virtual ICollection<DataDebug> GeneratedData { get; set; }

        public UserDebug()
        {
            GeneratedData = new HashSet<DataDebug>();
        }
    }
}