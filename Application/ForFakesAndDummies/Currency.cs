using System.ComponentModel.DataAnnotations;

namespace Simple_Account_Service.Application.ForFakesAndDummies;

public class Currency
{
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(3)]
    public string Code { get; set; } = null!;
}
