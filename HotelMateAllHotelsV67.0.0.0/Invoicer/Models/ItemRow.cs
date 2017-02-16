using System;
using System.Collections.Generic;

namespace Invoicer.Models
{
    public class ItemRow
    {
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal VAT { get; set; }
        public decimal Price { get; set; }
        public string Discount { get; set; }
        public decimal Total { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public decimal Credit { get; set; }
        public decimal Debit { get; set; }

        
        public bool HasDiscount
        {
            get
            {
                return (!string.IsNullOrEmpty(Discount));
            }
        }

        

        public static ItemRow Make(string name, string description, decimal amount, decimal vat, decimal price, decimal total)
        {
            return Make(name, description, amount, vat, price, "", total);
        }

        public static ItemRow Make(string name, string description, decimal amount, decimal vat, decimal price, string discount, decimal total)
        {
            return new ItemRow()
            {
                Name = name,
                Description = description,
                Amount = amount,
                VAT = vat,
                Price = price,
                Discount = discount,
                Total = total,
            };
        }
    }
}