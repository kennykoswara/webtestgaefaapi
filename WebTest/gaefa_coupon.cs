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
    
    public partial class gaefa_coupon
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public gaefa_coupon()
        {
            this.gaefa_book_new = new HashSet<gaefa_book_new>();
        }
    
        public string couponCode { get; set; }
        public Nullable<int> packMin { get; set; }
        public Nullable<decimal> priceMin { get; set; }
        public System.DateTime expiryDate { get; set; }
        public System.DateTime availableDate { get; set; }
        public bool status { get; set; }
        public Nullable<System.DateTime> usedDate { get; set; }
        public Nullable<int> discPercentage { get; set; }
        public Nullable<decimal> discPrice { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<gaefa_book_new> gaefa_book_new { get; set; }
    }
}
