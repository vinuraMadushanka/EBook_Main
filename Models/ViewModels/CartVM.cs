using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class CartVM
    {
       public IEnumerable< Books> BooksList { get; set; }
       public IEnumerable<CartDetails> CartDetailList { get; set; }
       public IEnumerable<Comments> Comments { get; set; }
       public Books Books { get; set; }
       public Carts Carts{ get; set; }
       public CartDetails CartDetails { get; set; }


    }
}
