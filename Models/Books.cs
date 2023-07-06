﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models
{
    public class Books
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ISBN { get; set; }
        public string Author { get; set; }
        public string ImgUrl { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Qty { get; set; }
        public string CurStatus { get; set; } = SD.Active;
    }
}
