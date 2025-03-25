namespace MESS.Data.DTO;

public class ProductionLogFormDTO
{
    public int ProductionLogId { get; set; }

    public List<ProductionLogStepDTO> LogSteps{ get; set; } = [];
}