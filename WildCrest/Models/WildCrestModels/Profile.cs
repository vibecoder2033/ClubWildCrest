using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WildCrest.Models.WildCrestModels
{
    public class Profile
    {
        public int ID { get; set; }
        public string F_Name { get; set; }
        public string L_Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string DOB { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> EmailNotifications { get; set; }
        public Nullable<int> UserType { get; set; }
        public string Password { get; set; }
        public string AddedBy { get; set; }
        public int UserMembershipCount { get; set; }
        public int? MembershipPlanIDForUser { get; set; }
        public string MembershipPlanNameForUser { get; set; }
        public int PlanForYear { get; set; }

        public string MembershipJoiningDate { get; set; }
        public string MembershipExpiryDate { get; set; }

        public string AadharNo { get; set; }
        public string PAN_Card { get; set; }

        public Nullable<bool> NewUsrBySuperApprv { get; set; }
        public Nullable<bool> DelUsrBySuperApprv { get; set; }

        public Nullable<bool> Walk_In_Member { get; set; }
        public string Reference_Of_Walk_In { get; set; }

        public int pageListTotal { get; set; }

        public string DeletedBy { get; set; }

        public BillingDetails billingDet { get; set; }

        public string billingDetailsInJson { get; set; }

        //public List<MembershipPlans> MembershipWithYear = new List<MembershipPlans>();
        public string MembershipWithYear { get; set; }

        public string CustomMemberID { get; set; }

        public string FatherName { get; set; }

        public string Additional_Notes { get; set; }
        public string Addon_Member { get; set; }
        public string Relationship { get; set; }
        public string Staff_JoiningDate { get; set; }

        public HttpPostedFileBase MemberPhoto { get; set; }
        public List<HttpPostedFileBase> MemberDocuments { get; set; }

        public string Cash_Payment { get; set; }
        public string Paytm_Payment { get; set; }
        public string Card_Payment { get; set; }
        public string Cheque_Payment { get; set; }
    }
}