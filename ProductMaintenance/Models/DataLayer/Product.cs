using System;
using System.Collections.Generic;

namespace ProductMaintenance.Models.DataLayer
{

    /// <summary>
    /// public constructors, variables will be used globally
    /// </summary>
    public class Product
    {
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int OnHandQuantity { get; set; }
    }
}
