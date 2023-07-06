using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CartDetails
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Books Books { get; set; }
        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public Carts Carts { get; set; }
        public int Qty { get; set; }
    }
}
