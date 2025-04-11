using System.ComponentModel.DataAnnotations.Schema;

namespace VibPortalApi.Models.DB2Models
{
    public class EuravibImport
    {
        [NotMapped] 
        public int? RowNum { get; set; }
        public string? Suppl_Nr { get; set; }
        public string? Dimset { get; set; }
        public DateTime? Entry_Date { get; set; }
        public DateTime? Rev_Date { get; set; }
        public string? Cas_Nr { get; set; }
        public string? Cas_Perc { get; set; }
        public string? H_Nr { get; set; }
        public string? H_Cat { get; set; }
        public string? Adr_Un_Nr { get; set; }
        public string? Adr_Cargo_Name { get; set; }
        public string? Adr_TransportHazard_Class { get; set; }
        public string? Adr_Packing_Group { get; set; }
        public string? Adr_Environment_Hazards { get; set; }
        public string? Adr_ExtraInfo { get; set; }
        public string? Imdg_Un_Nr { get; set; }
        public string? Imdg_Cargo_Name { get; set; }
        public string? Imdg_TransportHazard_Class { get; set; }
        public string? Imdg_Packing_Group { get; set; }
        public string? Imdg_Environment_Hazards { get; set; }
        public string? Imdg_ExtraInfo { get; set; }
        public string? ExtraInfo_TunnelCode { get; set; }
        public string? FlashPoint { get; set; }
        public string? Ems_Fire { get; set; }
        public string? Ems_Spillage { get; set; }
        public string? User { get; set; }
        public string? Eg_Nr { get; set; }
    }

    public class EuravibImportDto
    {
        public long? RowNum { get; set; }
        public string? Suppl_Nr { get; set; }
        public string? Dimset { get; set; }
        public DateTime? Entry_Date { get; set; }
        public DateTime? Rev_Date { get; set; }
        public string? Cas_Nr { get; set; }
        public string? Cas_Perc { get; set; }
        public string? H_Nr { get; set; }
        public string? H_Cat { get; set; }
        public string? Adr_Un_Nr { get; set; }
        public string? Adr_Cargo_Name { get; set; }
        public string? Adr_TransportHazard_Class { get; set; }
        public string? Adr_Packing_Group { get; set; }
        public string? Adr_Environment_Hazards { get; set; }
        public string? Adr_ExtraInfo { get; set; }
        public string? Imdg_Un_Nr { get; set; }
        public string? Imdg_Cargo_Name { get; set; }
        public string? Imdg_TransportHazard_Class { get; set; }
        public string? Imdg_Packing_Group { get; set; }
        public string? Imdg_Environment_Hazards { get; set; }
        public string? Imdg_ExtraInfo { get; set; }
        public string? ExtraInfo_TunnelCode { get; set; }
        public string? FlashPoint { get; set; }
        public string? Ems_Fire { get; set; }
        public string? Ems_Spillage { get; set; }
        public string? User { get; set; }
        public string? Eg_Nr { get; set; }
    }
}
