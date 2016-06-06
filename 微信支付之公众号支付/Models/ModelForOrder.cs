using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class ModelForOrder
    {
        public string appId { get; set; }
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }
        public string packageValue { get; set; }
        public string paySign { get; set; }

        public string msg { get; set; }
    }
}