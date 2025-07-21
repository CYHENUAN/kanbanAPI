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
        Task<string> UpdateProductQuantityAsync(ProductUpdateRequest product);
        Task<List<ProductYieldRate>> GetProductYieldRateByStationAsync(DateTime? startOfDay, DateTime? endOfDay);
        Task<List<EquimentOEE>> GetEquimentOeeAsync(double workingHours, double planStopTime);
        Task<string> UploadAndonAsync(AndonInformation andon);
    }
}
