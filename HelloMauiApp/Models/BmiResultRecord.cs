using SQLite;

namespace HelloMauiApp.Models;

[Table("bmi_results")]
public class BmiResultRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Column("bmi_value")]
    public double Bmi { get; set; }

    [Column("classification")]
    public string Classification { get; set; }

    [Column("calculation_date")]
    public DateTime Date { get; set; }
}