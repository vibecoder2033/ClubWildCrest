using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class TablesForBooking
    {
        public int ID { get; set; }
        public string TableNo { get; set; }

        public string Table_Status { get; set; }
        public string Bar_Status { get; set; }
        public string Wine_Status { get; set; }
        public string OrderReceivedBy { get; set; }
    }
}