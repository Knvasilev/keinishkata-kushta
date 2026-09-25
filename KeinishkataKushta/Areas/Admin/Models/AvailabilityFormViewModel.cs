using System.ComponentModel.DataAnnotations;
using KeinishkataKushta.Data.Entities;

namespace KeinishkataKushta.Web.Areas.Admin.Models;

public class AvailabilityFormViewModel
{
    public int Id { get; set; }
    [StringLength(500)]
    public string? RoomsUsed { get; set; }
    [Required, DataType(DataType.Date)]
    public DateOnly? Arrival { get; set; }
    [Required, DataType(DataType.Date)]
    public DateOnly? Departure { get; set; }
    [EnumDataType(typeof(AvailabilityBlockKind))]
    public AvailabilityBlockKind Kind { get; set; }
    [StringLength(1000)]
    public string? Notes { get; set; }
    public string? Version { get; set; }
}
