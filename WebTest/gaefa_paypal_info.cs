//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebTest
{
    using System;
    using System.Collections.Generic;
    
    public partial class gaefa_paypal_info
    {
        public string paypalTransactionID { get; set; }
        public string bookCode { get; set; }
        public string paypalDateAndTime { get; set; }
        public string paypalPayerID { get; set; }
        public string paypalName { get; set; }
        public string paypalAddress { get; set; }
        public decimal paypalAmount { get; set; }
        public string paypalNote { get; set; }
        public bool postedToGaefa { get; set; }
    
        public virtual gaefa_book_info gaefa_book_info { get; set; }
    }
}
