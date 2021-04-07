using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class RentalOrder
    {
        public Rental RentalContent { get; set; }
        public CreditCardExtend PayCard { get; set; }
        public double Amount { get; set; }
        public bool IsSave { get; set; }
    }
}
