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
    
    public partial class tour_info
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tour_info()
        {
            this.book_info = new HashSet<book_info>();
        }
    
        public int id { get; set; }
        public string destination { get; set; }
        public System.DateTime dateFrom { get; set; }
        public System.DateTime dateUntil { get; set; }
        public string flight { get; set; }
        public string inn { get; set; }
        public string custom { get; set; }
        public decimal price { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<book_info> book_info { get; set; }
    }
}
