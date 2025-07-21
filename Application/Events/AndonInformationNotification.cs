using Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Events
{
    public class AndonInformationNotification: INotification
    {
        public AndonInformation AndonData { get; set; } = new AndonInformation();
    }
}
