namespace VibPortalApi.Models
{
    public class ZenyaDocument
    {
        public bool active { get; set; }
        public string document_id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string quick_code { get; set; } = string.Empty;
        public string summary { get; set; } = string.Empty;
        public int version { get; set; }
        public int revision { get; set; }
        public bool can_download_binary { get; set; }
        public bool can_download_content { get; set; }
        public string download_binary_extension { get; set; } = string.Empty;
        public string mime_type { get; set; } = string.Empty;
        public string last_modified_datetime { get; set; } = string.Empty;
        public bool download_as_pdf { get; set; }
        public string type { get; set; } = string.Empty;
        public bool show_in_office_online_viewer { get; set; }
        public bool is_editable_form { get; set; }
        public bool is_printable { get; set; }
        public DocumentType document_type { get; set; } = new();
        public DocumentType document_type_mini { get; set; } = new();
        public bool print_header_required { get; set; }
        public bool can_be_compared { get; set; }
        public bool can_be_edited { get; set; }
        public bool can_check_document { get; set; }
        public bool marked_as_favorite { get; set; }
        public string office_print_cover_page_mode { get; set; } = string.Empty;
        public FolderMini folder_mini { get; set; } = new();
        public string state { get; set; } = string.Empty;
        public bool has_delete_published_permission { get; set; }
        public string original_type { get; set; } = string.Empty;
    }

    public class DocumentType
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public class FolderMini
    {
        public int folder_id { get; set; }
        public string folder_name { get; set; } = string.Empty;
        public string full_path { get; set; } = string.Empty;
    }
}