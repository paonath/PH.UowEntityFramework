namespace PH.UowEntityFramework.TestCtx.Models
{
    public class DataDebug : PH.UowEntityFramework.EntityFramework.Abstractions.Models.Entity<string>
    {
        public string AuthorId { get; set; }
        public virtual UserDebug Author { get; set; }

        public string Title { get; set; }
    }
}