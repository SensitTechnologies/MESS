using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.Product;

using Data.Models;

public class ProductService : IProductService
{
    private readonly ApplicationContext _context;

    public ProductService(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task AddProductAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Product?> FindProductByIdAsync(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.WorkInstructions)
                .Include(p => p.WorkStations)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            return product;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            return await _context.Products
                .Include(p => p.WorkInstructions)
                .Include(p => p.WorkStations)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<Product>();
        }
    }

    public async Task ModifyProductAsync(Product product)
    {
        try
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
    }

    public async Task RemoveProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}