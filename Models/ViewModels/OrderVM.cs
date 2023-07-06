using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class OrderVM
    {
       public IEnumerable< Books> BooksList { get; set; }
       public IEnumerable<OrderDetails> OrderDetailList { get; set; }
       public IEnumerable<Orders> OrderList { get; set; }
       public Books Books { get; set; }
       public Orders Orders{ get; set; }
       public OrderDetails OrderDetails { get; set; }
       public ApplicationUser User { get; set; }


    }
}
