using System.Collections.Generic;
using PH.UowEntityFramework.EntityFramework.Abstractions.Models;

namespace PH.UowEntityFramework.TestCtx.Models
{
    public class NodeDebug : Entity<string>
    {
        
       
        public string NodeName { get; set; }

        public string ParentId { get; set; }
        public virtual  NodeDebug Parent { get; set; }

        public string DataId { get; set; }
        public virtual DataDebug Data { get; set; }

        public virtual ICollection<NodeDebug> Children { get; set; }

        public NodeDebug()
        {
            Children = new HashSet<NodeDebug>();
        }
    }
}