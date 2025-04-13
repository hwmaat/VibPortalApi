using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibPortalApi.Models.B2B
{
    [Table("B2B_SupplierOc")]
    public class B2BSupplierOc
    {
        [Key]
        public int Oc_Id { get; set; }

        [StringLength(10)]
        public string? Supplier_Nr { get; set; }

        public DateTime? OrderDate { get; set; }

        [StringLength(15)]
        public string? CustomerNr { get; set; }

        [StringLength(15)]
        public string? OrderNr { get; set; }

        [StringLength(15)]
        public string? EuramaxPo_Nr { get; set; }

        [StringLength(250)]
        public string? GmailId { get; set; }

        [StringLength(500)]
        public string? AttachtmentName { get; set; }

        [StringLength(15)]
        public string? Status { get; set; }
    }
}
