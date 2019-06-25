using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class RenewalMemberById
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string CustomMemberID { get; set; }
        public string MembershipJoiningDate { get; set; }
        public string MembershipExpiryDate { get; set; }
    }
}