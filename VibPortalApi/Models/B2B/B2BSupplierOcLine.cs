using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibPortalApi.Models.B2B
{
    [Table("B2B_SupplierOc_Lines")]
    public class B2BSupplierOcLine
    {
        [Key]
        public int Id { get; set; }

        public int? Oc_Id { get; set; }

        public int? Line { get; set; }

        [StringLength(35)]
        public string? Dimset { get; set; }

        [StringLength(25)]
        public string? SupplierPartNr { get; set; }

        public decimal? Quantity_Kg { get; set; }

        public decimal? Price_t { get; set; }

        [StringLength(5)]
        public string? Currency { get; set; }

        [StringLength(50)]
        public string? Specification { get; set; }
    }
}
