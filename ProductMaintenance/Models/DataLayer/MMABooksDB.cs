using System.Configuration;

namespace ProductMaintenance.Models.DataLayer
{
    /// <summary>
    /// gets connection string from the app config file 
    /// </summary>
    public static class MMABooksDB  
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["MMABooks"]
            .ConnectionString;
    }
}