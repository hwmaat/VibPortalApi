using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibPortalApi.Models
{
    public class VibImport
    {
        [Key]
        public int Id { get; set; }
        public string? SupplierNr { get; set; }
        public string? Dimset { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? RevisionDate { get; set; }
        public string? Cas_Number { get; set; }
        public string? Cas_Percentages { get; set; }
        public string? H_Number { get; set; }
        public string? H_Cat { get; set; }
        public string? Adr_Un_Nr { get; set; }
        public string? Adr_CargoName { get; set; }
        public string? Adr_TransportHazardClass { get; set; }
        public string? Adr_PackagingGroup { get; set; }
        public string? Adr_EnvironmentHazards { get; set; }
        public string? Adr_ExtraInfo { get; set; }
        public string? Imdg_UnNumber { get; set; }
        public string? Imdg_CargoName { get; set; }
        public string? Imdg_TransportHazardClass { get; set; }
        public string? Imdg_PackagingGroup { get; set; }
        public string? Imdg_EnvironmentHazards { get; set; }
        public string? Imdg_ExtraInfo { get; set; }
        public string? ExtraInfoTunnelCode { get; set; }
        public string? FlashPoint { get; set; }
        public string? Ems_Fire { get; set; }
        public string? Ems_Spillage { get; set; }
        public string? UserName { get; set; }
        public string? EgNumber { get; set; }
        public string? Status { get; set; }
    }
}
