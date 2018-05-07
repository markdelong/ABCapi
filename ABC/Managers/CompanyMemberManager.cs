using Efinity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ABC.Managers
{
    public class CompanyMemberManager
    {
        //Auto genCode 2/15/2018 5:42:48 PM

        public static MemberCompany GetMemberCompanyById(int MemberCompanyId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM MemberCompany WHERE MemberCompanyId = @MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@MemberCompanyId", MemberCompanyId));
            return ORM.DbReader<MemberCompany>(command, true)[0];
        }

        public static List<MemberCompany> GetMemberCompany(string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "SELECT * FROM MemberCompany ORDER BY Member");
            return ORM.DbReader<MemberCompany>(command);
        }

        public static List<MemberCompany> GetMemberInfoByGenericSearch(string searchTerm, int currentPage, int itemsPerPage, bool badAddressReport, bool archivedOnly, bool unArchivedOnly, string searchSQL, string connection)
        {
            return ORM.DbReader<MemberCompany>(
                GetMemberInfoGenericSearchCommand(searchTerm, currentPage, itemsPerPage, badAddressReport, archivedOnly, unArchivedOnly, searchSQL, connection)
                );
        }

        public static int GetMemberInfoCountByGenericSearch(string searchTerm, int currentPage, int itemsPerPage, bool badAddressReport, bool archivedOnly, bool unArchivedOnly, string searchSQL, string connection)
        {
            return ORM.ExecuteSingleReturn<int>(
                GetMemberInfoGenericSearchCommand(searchTerm, currentPage, itemsPerPage, badAddressReport, archivedOnly, unArchivedOnly, connection, searchSQL, true)
                );
        }

        public static SqlCommand GetMemberInfoGenericSearchCommand(string searchTerm, int currentPage, int itemsPerPage, bool badAddressReport, bool archivedOnly, bool unArchivedOnly, string searchSQL, string connection, bool getCount = false)
        {
            //string select = "p.*";
            string select = "*";
            if (getCount)
            {
                select = "distinct count(*) over() as totalrows"; //no idea, don't ask
            }

            string sql = @"SELECT " + select + @" FROM MemberCompany m 
                    WHERE (m.MEMBER LIKE '%' + @search + '%'
                    OR m.CONTACT LIKE '%' + @search + '%'
                    OR m.[ADDRESS] LIKE '%' + @search + '%'
                    OR m.CITY LIKE '%' + @search + '%'
                    OR m.[STATE] LIKE '%' + @search + '%'
                    OR m.ZIP LIKE '%' + @search + '%'
                    OR m.PHONE LIKE '%' + @search + '%'
                    OR m.COMPANYEMAIL LIKE '%' + @search + '%'
                    OR m.WEBSITE LIKE '%' + @search + '%'
                    OR m.KEYWORDTAGS LIKE '%' + @search + '%')";

            if(badAddressReport)
            {
                sql += " AND ((m.Latitude = '0' AND m.Longitude = '0') OR (m.Latitude = '' AND m.Longitude = '') OR (m.Latitude IS NULL OR m.Longitude IS NULL))";
            }
            else if (archivedOnly)
            {
                sql += " AND m.Status = 1";
            }
            else if (unArchivedOnly)
            {
                sql += " AND m.Status = 0";
            }
            if (searchSQL.Length > 0)
            {
                sql += searchSQL;
            }
            if (!getCount)
            {
                if (searchSQL.Length <= 0)
                {
                    sql += " ORDER BY m.Member";
                }
                sql += " OFFSET " + ((currentPage - 1) * itemsPerPage) + " ROWS FETCH NEXT " + itemsPerPage + " ROWS ONLY";
            }

            SqlCommand command = ORM.DbCommand(new SqlConnection(connection), sql);
            command.Parameters.Add(new SqlParameter("search", searchTerm));

            return command;
        }

        public static List<MemberCompany> SearchMembers(double NELat, double NELong, double SWLat, double SWLong, string keywordSearchTerm, string connection)
        {
            string sql = "";
            string error = "";

            SqlCommand command = new SqlCommand(); 
            if (keywordSearchTerm.Trim().Length > 0) //keyword search
            {
                //user is performing a keyword search rather than a map search
                sql = @"SELECT m.MemberCompanyId, m.Member, m.Contact, m.[Address], m.Address2, m.City, m.[State], m.Zip, m.Phone, m.Fax, m.CompanyEmail, m.Website, m.ContactEmail, m.CompanyProfile, m.ListingLevel, m.LogoImage, m.KeywordTags, m.Latitude, m.Longitude, m.ServiceAreas
                    FROM MemberCompany m 
                LEFT JOIN Code2MemberCompany c2m ON m.MemberCompanyId = c2m.MemberCompanyId
                LEFT JOIN Codes c ON c.CodeId = c2m.CodeId
                WHERE m.Status = 0"; //Status = 1 means archived--kinda backwards

                string[] keywords = keywordSearchTerm.Trim().Split(',');
                int keywordCount = 0;

                Dictionary<string, string> keywordParams = new Dictionary<string, string>();
                foreach(string keyword in keywords)
                {
                    sql += @" AND (m.Member LIKE '%' + @keyword" + keywordCount + @" + '%'
                    OR m.ServiceAreas LIKE '%' + @keyword" + keywordCount + @" + '%'
                    OR m.KeywordTags LIKE '%' + @keyword" + keywordCount + @" + '%'
                    OR m.City LIKE '%' + @keyword" + keywordCount + @" + '%'
                    OR c.Code LIKE '%' + @keyword" + keywordCount + @" + '%'
                    OR c.CodeName LIKE '%' + @keyword" + keywordCount + @" + '%')";

                    keywordParams.Add("@keyword" + keywordCount, keyword.Trim());
                    keywordCount++;
                }
                sql += @" GROUP BY m.MemberCompanyId, m.Member, m.Contact, m.[Address], m.Address2, m.City, m.[State], m.Zip, m.Phone, m.Fax, m.CompanyEmail, m.Website, m.ContactEmail, m.CompanyProfile, m.ListingLevel, m.LogoImage, m.KeywordTags, m.Latitude, m.Longitude, m.ServiceAreas 
                        ORDER BY m.ListingLevel DESC, m.Member";
                command = Efinity.ORM.DbCommand(new SqlConnection(connection), sql);
                //throw new ApplicationException(sql);

                //have to add parameters after command is created
                foreach (KeyValuePair<string, string> param in keywordParams)
                {
                    command.Parameters.Add(new SqlParameter(param.Key, param.Value));
                }
                
            }
            else //map search
            {
                sql = @"SELECT * FROM MemberCompany m 
                WHERE (m.Latitude <= @latNE AND m.Latitude >= @latSW)
                AND (m.Longitude <= @longNE AND m.Longitude >= @longSW)
                AND (m.Latitude != '' AND m.Longitude != '') 
                ORDER BY m.ListingLevel DESC, m.Member";

                command = Efinity.ORM.DbCommand(new SqlConnection(connection), sql);

                command.Parameters.Add(new SqlParameter("@latNE", NELat));
                command.Parameters.Add(new SqlParameter("@latSW", SWLat));
                command.Parameters.Add(new SqlParameter("@longNE", NELong));
                command.Parameters.Add(new SqlParameter("@longSW", SWLong));
            }


           // return error;

            return ORM.DbReader<MemberCompany>(command);
        }

        public static void DeleteMemberCompanyById(int MemberCompanyId, string connection)
        {
            SqlCommand command = Efinity.ORM.DbCommand(new SqlConnection(connection), "DELETE FROM MemberCompany WHERE MemberCompanyId = @MemberCompanyId");
            command.Parameters.Add(new SqlParameter("@MemberCompanyId", MemberCompanyId));
            Efinity.ORM.ExecuteNoReturn(command);
        }

        public static int SaveMemberCompany(MemberCompany item, string connection)
        {
            bool update = (item.MemberCompanyId > 0) ? true : false;
            return Efinity.ORM.SaveRecord(item, update, "MemberCompanyId", "MemberCompany", connection);
        }
        //End genCode
    }
}
