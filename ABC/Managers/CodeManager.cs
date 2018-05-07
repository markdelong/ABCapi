using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ABC.Managers;

namespace ABC.Managers
{
    public class CodeManager
    {
        //Auto genCode 2/6/2018 3:56:18 PM

        public static Codes GetCodesById(int CodeId, string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Codes WHERE CodeId = @CodeId");
            command.Parameters.Add(new SqlParameter("@CodeId", CodeId));
            return ORM.DbReader<Codes>(command, true)[0];
        }


        public static List<Codes> GetCodesByMemberCompanyId(int MemberCompanyId, string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), @"SELECT * FROM Codes c 
                                                                                        INNER JOIN Code2MemberCompany c2m ON c.CodeId = c2m.CodeId
                                                                                        WHERE c2m.MemberCompanyId = @MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@MemberCompanyId", MemberCompanyId));
            return ORM.DbReader<Codes>(command);
        }

        public static List<Codes> CSIChildren(string parentCode, string connection)
        {
            string queryCode = "";
            parentCode = parentCode.Trim();
            string[] parentSplit = parentCode.Split(' ');
            if (parentSplit.Length == 3)
            {
                if (parentSplit[1] == "00")
                {
                    queryCode = parentSplit[0] + "%";
                }
                else if (parentSplit[2] == "00")
                {
                    queryCode = parentSplit[0] + " " + parentSplit[1] + "%";
                }
            }
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Codes WHERE CodeTypeId = 2 AND  Code LIKE @QueryCode");
            command.Parameters.Add(new SqlParameter("@QueryCode", queryCode));
            List<Codes> returnCodes = ORM.DbReader<Codes>(command);
            return returnCodes;
        }

        public static List<Codes> NAICSChildren(string parentCode, string connection)
        {
            string queryCode = "";
            parentCode = parentCode.Trim();
            string[] parentSplit = {parentCode.Substring(0, 2), parentCode.Substring(2, 2), parentCode.Substring(4, 2) };
            if (parentSplit.Length == 3)
            {
                if (parentSplit[1] == "00")
                {
                    queryCode = parentSplit[0] + "%";
                }
                else if (parentSplit[2] == "00")
                {
                    queryCode = parentSplit[0] + parentSplit[1] + "%";
                }
            }

            string strSQL = "SELECT * FROM Codes WHERE CodeTypeId = 1 AND Code LIKE @QueryCode";

            
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), strSQL);
            command.Parameters.Add(new SqlParameter("@QueryCode", queryCode));
            List<Codes> returnCodes = ORM.DbReader<Codes>(command);
            return returnCodes;
        }

        public static List<Codes> TopLevelNAICS(string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Codes WHERE CodeTypeId = 1 AND Code LIKE '%0000'");
            List<Codes> returnCodes = ORM.DbReader<Codes>(command);
            return returnCodes;
        }

        public static List<Codes> TopLevelCSI(string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Codes WHERE CodeTypeId = 2 AND Code LIKE '%00 00'");
            List<Codes> returnCodes = ORM.DbReader<Codes>(command);
            return returnCodes;
        }

        public static Codes GetCodeByCode(string Code, string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Codes WHERE Code = @Code");
            command.Parameters.Add(new SqlParameter("@Code", Code));
            return ORM.DbReader<Codes>(command, true)[0];
        }
        public static Codes GetCodeByCodeId(string CodeId, string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM Codes WHERE CodeId = @CodeId");
            command.Parameters.Add(new SqlParameter("@CodeId", CodeId));
            return ORM.DbReader<Codes>(command, true)[0];
        }

        public static void DeleteCodesById(int CodeId, string connection)
        {
            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), "DELETE FROM Codes WHERE CodeId = @CodeId");
            command.Parameters.Add(new SqlParameter("@CodeId", CodeId));
            ORM.ExecuteNoReturn(command);
        }

        public static int SaveCodes(Codes item, string connection)
        {
            bool update = (item.CodeId > 0) ? true : false;
            return ORM.SaveRecord(item, update, "", "", connection);
        }
        //End genCode
    }
}
