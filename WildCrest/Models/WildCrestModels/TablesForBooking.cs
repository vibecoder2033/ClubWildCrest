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

        // TableNo is a varchar like "Table 10", so a plain string sort would place
        // "Table 10" before "Table 2". Sort on the numeric part instead.
        public static List<TablesForBooking> SortByTableNo(IEnumerable<TablesForBooking> tables)
        {
            return tables.OrderBy(t => TableNoValue(t.TableNo))
                         .ThenBy(t => t.ID)
                         .ToList();
        }

        static int TableNoValue(string tableNo)
        {
            if (string.IsNullOrEmpty(tableNo))
            {
                return int.MaxValue;
            }
            string digits = new string(tableNo.Where(char.IsDigit).ToArray());
            int number;
            return int.TryParse(digits, out number) ? number : int.MaxValue;
        }
    }
}