using System.ComponentModel.DataAnnotations;

namespace Core.Persistence;

public class DatabaseOptions
{
    [Required] public DatabaseProvider Provider { get; set; }

    /// <summary>
    /// See https://www.connectionstrings.com/
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = "";
}
