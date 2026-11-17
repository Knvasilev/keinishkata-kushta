using System.ComponentModel.DataAnnotations;

namespace KeinishkataKushta.Web.Areas.Admin.Models;

public class GalleryCaptionBatchViewModel
{
    public int? RoomId { get; set; }
    public List<GalleryCaptionRowViewModel> Rows { get; set; } = new();
}

public class GalleryCaptionRowViewModel
{
    public int Id { get; set; }
    [StringLength(500)]
    public string? Caption { get; set; }
    [StringLength(500)]
    public string? CaptionEn { get; set; }
    [StringLength(500)]
    public string? OriginalCaption { get; set; }
    [StringLength(500)]
    public string? OriginalCaptionEn { get; set; }
}
