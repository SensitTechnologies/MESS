namespace MESS.Services.LineOperator;
using Data.Models;

public interface ILineOperatorService
{
    //<summary>
    // Retrieves a list of all LineOperators currently registered
    //</summary>
    //<returns>List of LineOperator objects</returns>
    public List<LineOperator> GetLineOperators();
    
    //<summary>
    // Retrieves a LineOperator by id number
    //</summary>
    //<returns>LineOperator object</returns>
    public LineOperator? GetLineOperatorById(int id);
    
    //<summary>
    // Retrieves a LineOperator by last name
    //</summary>
    //<returns>LineOperator object</returns>
    public LineOperator? GetLineOperatorByLastName(string lastName);
    
    //<summary>
    // Creates a LineOperator object and saves it to the database
    //</summary>
    //<returns>LineOperator object</returns>
    public Task<bool> AddLineOperator(LineOperator lineOperator);
    
    //<summary>
    // Updates a LineOperator currently in the database
    //</summary>
    //<returns>Updated LineOperator object</returns>
    public Task<bool> UpdateLineOperator(LineOperator lineOperator);
    
    //<summary>
    // Deletes a LineOperator currently in the database
    //</summary>
    //<returns>Deleted LineOperator boolean</returns>
    public Task<bool> DeleteLineOperator(int id);
    
}