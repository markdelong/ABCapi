using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ABC.Managers
{
    public class ContactManager
    {
        //Auto genCode 6/19/2018 1:01:38 PM

        public static Contact GetContactById(int ContactId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Contact WHERE ContactId = @ContactId");
            command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
            return ORM.DbReader<Contact>(command, true)[0];
        }

        public static bool DoesContactBelongToUser(Contact contact, int UserId, string abcconn, string vsmconn)
        {

            //first, get the company that this user belongs to
            MemberCompany m = CompanyMemberManager.GetMemberCompanyByUserId(UserId, vsmconn, abcconn);

            //user is creating new contact
            if (contact.ContactId == 0 && m.MemberCompanyId == contact.MemberCompanyId) return true;

            //next, grab Contact associated
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(abcconn), @"SELECT * FROM Contact WHERE ContactId = @ContactId
                                                                                        AND MemberCompanyId = @MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@ContactId", contact.ContactId));
            command.Parameters.Add(new SqlParameter("@MemberCompanyId", m.MemberCompanyId));

            Contact c = ORM.DbReader<Contact>(command, true)[0];

            return c.ContactId == contact.ContactId;
        }

        public static List<Contact> GetContactsByMemberCompanyId(int MemberCompanyId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Contact WHERE MemberCompanyId = @MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@MemberCompanyId", MemberCompanyId));
            return ORM.DbReader<Contact>(command);
        }

        public static void DeleteContactById(int ContactId, string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "DELETE FROM Contact WHERE ContactId = @ContactId");
            command.Parameters.Add(new SqlParameter("@ContactId", ContactId));
            ORM.ExecuteNoReturn(command);
        }

        public static int SaveContact(Contact item, string connection)
        {
            bool update = item.ContactId > 0;
            return Efinity.ORM.SaveRecord(item, update, "ContactId", "Contact", connection);
        }
        //End genCode
    }
}
