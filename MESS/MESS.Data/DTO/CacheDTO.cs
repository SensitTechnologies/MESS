namespace MESS.Data.DTO;

public class CacheDTO<T>
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public T? Value { get; set; }
}