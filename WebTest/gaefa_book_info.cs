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
    
    public partial class gaefa_book_info
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public gaefa_book_info()
        {
            this.gaefa_transfer_info = new HashSet<gaefa_transfer_info>();
            this.gaefa_paypal_info = new HashSet<gaefa_paypal_info>();
        }
    
        public string bookCode { get; set; }
        public string email { get; set; }
        public System.DateTime dateToGo { get; set; }
        public string orderReference { get; set; }
        public System.DateTime dateOrder { get; set; }
        public int tourID { get; set; }
        public string paymentMethod { get; set; }
        public string status { get; set; }
        public string passenger { get; set; }
        public decimal totalPrice { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<gaefa_transfer_info> gaefa_transfer_info { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<gaefa_paypal_info> gaefa_paypal_info { get; set; }
    }
}
