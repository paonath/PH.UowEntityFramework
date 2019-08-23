namespace PH.UowEntityFramework.EntityFramework.Infrastructure
{
    /// <summary>
    /// Tenant Context
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>Gets or sets the tenant identifier.</summary>
        /// <value>The tenant identifier.</value>
        string TenantName { get; set; }
    }
}