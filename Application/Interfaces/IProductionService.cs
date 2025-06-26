using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductionService
    {
        Task<List<ProductInformation>> GetAllProductNumbersInformationAsync();
        Task<OutPutQuantity> GetOutputQuantityAsync();
        Task<object> UpdateProductQuantityAsync(string serialnumber);
    }
}
