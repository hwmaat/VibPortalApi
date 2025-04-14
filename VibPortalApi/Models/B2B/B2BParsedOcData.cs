using System;
using System.Collections.Generic;

namespace VibPortalApi.Models.B2B
{
    public class B2BParsedOcData
    {
        public string? Supplier_Nr { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? CustomerNr { get; set; }
        public string? OrderNr { get; set; }
        public string? EuramaxPo_Nr { get; set; }

        public List<B2BParsedOcLine> Lines { get; set; } = new();
    }

    public class B2BParsedOcLine
    {
        public int? Line { get; set; }
        public string? Dimset { get; set; }
        public string? SupplierPartNr { get; set; }
        public decimal? Quantity_Kg { get; set; }
        public decimal? Price_t { get; set; }
        public string? Currency { get; set; }
        public string? Specification { get; set; }
        public string? EuramaxPo_Nr { get; set; }
    }
}
