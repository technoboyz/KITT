using System.ComponentModel.DataAnnotations;

namespace KITT.Web.Models.Streamings;

/// <summary>
/// Definisce le informazioni per la pianificazione di una nuova live
/// </summary>
public class ScheduleStreamingModel
{
    /// <summary>
    /// Il titolo della live
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Uno slug per la live
    /// </summary>
    [Required]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Una data di pianificazione
    /// </summary>
    [Required]
    public DateTime ScheduleDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Un orario di inizio della diretta
    /// </summary>
    [Required]
    public DateTime StartingTime { get; set; }

    /// <summary>
    /// Un orario di fine della diretta
    /// </summary>
    [Required]
    public DateTime EndingTime { get; set; }

    /// <summary>
    /// Il canale in cui si svolgerà la diretta
    /// </summary>
    [Required]
    public string HostingChannelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Un eventuale abstract per la diretta
    /// </summary>
    public string? StreamingAbstract { get; set; }
}
