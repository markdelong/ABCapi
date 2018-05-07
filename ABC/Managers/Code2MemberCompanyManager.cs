using Efinity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ABC.Managers;

namespace ABC.Managers
{
    public class Code2MemberCompanyManager
    {
        //Auto genCode 2/27/2018 5:25:09 PM
        
        public static Code2MemberCompany GetCode2MemberCompanyById(int Code2MemberCompanyId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Code2MemberCompany WHERE Code2MemberCompanyId = @Code2MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@Code2MemberCompanyId", Code2MemberCompanyId));
            return ORM.DbReader<Code2MemberCompany>(command, true)[0];
        }

        public static void DeleteCode2MemberCompanyById(int Code2MemberCompanyId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "DELETE FROM Code2MemberCompany WHERE Code2MemberCompanyId = @Code2MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@Code2MemberCompanyId", Code2MemberCompanyId));
            Efinity.ORM.ExecuteNoReturn(command);
        }

        public static void DeleteCode2MemberCompanyByMemberCompanyId(int MemberCompanyId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "DELETE FROM Code2MemberCompany WHERE MemberCompanyId = @MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@MemberCompanyId", MemberCompanyId));
            Efinity.ORM.ExecuteNoReturn(command);
        }

        public static int SaveCode2MemberCompany(Code2MemberCompany item, string connection)
        {
            bool update = (item.Code2MemberCompanyId > 0) ? true : false;
            return Efinity.ORM.SaveRecord(item, update, "Code2MemberCompanyId", "Code2MemberCompany", connection);
        }
        //End genCode
    }
}
