using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events
{
    public class ProductInfromationNotification: INotification
    {
        public string? Type { get; set; }
        public ProductItem? SerialNumber { get; set; }
    }
    public class ProductItem
    {
        public string? Material { get; set; }
        public string? Serial { get; set; }
    }
}
