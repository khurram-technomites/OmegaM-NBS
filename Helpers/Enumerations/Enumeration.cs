
namespace NowBuySell.Web.Helpers.Enumerations
{
    public class Enumeration
    {
        public enum EmailTemplate
        {
            ForgotPassword = 1,
            Feedback = 2,
            Payment = 3,
        }

        public enum EmailLang
        {
            English = 1,
            Arabic = 2,
        }

        public enum UserRoles
        {
            Admin = 1,
            Operator = 2,
            Editor = 3,
            Viewer = 4,
        }

        public enum CarApprovalStatus
        {
            Pending = 1,
            Processing = 2,
            Approved = 3,
            Rejected = 4,
        }

        public enum InvoiceStatus
        {
            Sent = 1,
            Paid = 2,

            Void = 3,
            WriteOff = 4,
            Draft = 5,
            UnPaid = 6,
            PaidViaCash = 7,
        }
    }
}