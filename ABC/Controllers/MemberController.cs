using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ABC.Managers;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ABC.Controllers
{
    [Produces("application/json")]
    [Route("api/Member")]
    public class MemberController : Controller
    {
        string dbconn = ConnectionString.Credentials;
        string vsmconn = ConnectionString.VSMCredentials;
        private readonly IConfiguration _configuration;

        public MemberController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetMembers/{amount}")]
        public List<MemberCompany> GetMembers(int amount)
        {
            return CompanyMemberManager.GetMemberCompany(dbconn);
        }

        [HttpGet]
        [Route("GetMembersById/{id}")]
        public MemberCompany GetMembersById(int id)
        {
            return CompanyMemberManager.GetMemberCompanyById(id, dbconn);
        }

        [HttpPost]
        [Route("Search")]
        public List<MemberCompany> SearchMembers()
        {
            double NELat = 0;
            double NELong = 0;
            double SWLat = 0;
            double SWLong = 0;
            string keywordSearchTerm = "";

            try
            {   
                NELat = Efinity.General.tryDouble(Request.Form["NELat"].ToString());
                NELong = Efinity.General.tryDouble(Request.Form["NELong"].ToString());
                SWLat = Efinity.General.tryDouble(Request.Form["SWLat"].ToString());
                SWLong = Efinity.General.tryDouble(Request.Form["SWLong"].ToString());
            }
            catch { }

            try
            {
                keywordSearchTerm = Request.Form["keywordSearchTerm"].ToString();
            }
            catch { };

            return CompanyMemberManager.SearchMembers(NELat, NELong, SWLat, SWLong, keywordSearchTerm, dbconn);
        }

        [HttpPost]
        [Route("MemberSearch")]
        public dynamic GetMemberInfoSearch()
        {
            dynamic obj = new JObject();
            try
            {
                string searchTerm = "";
                if (Request.Form["searchTerm"].ToString() != null)
                {
                    searchTerm = Request.Form["searchTerm"].ToString();
                }
                int currentPage = 1;
                if (Request.Form["currentPage"].ToString() != null)
                {
                    currentPage = Efinity.General.tryInt(Request.Form["currentPage"].ToString());
                }
                int itemsPerPage = 0;
                if (Request.Form["itemsPerPage"].ToString() != null)
                {
                    itemsPerPage = Efinity.General.tryInt(Request.Form["itemsPerPage"].ToString());
                }

                bool badAddressReport = false;
                if (Request.Form["badAddressReport"].ToString() != null)
                {
                    if (Request.Form["badAddressReport"].ToString() == "true")
                    {
                        badAddressReport = true;
                    }
                }
                bool archivedOnly = false;
                if (Request.Form["archived"].ToString() != null)
                {
                    if (Request.Form["archived"].ToString() == "true")
                    {
                        archivedOnly = true;
                    }
                }
                bool unArchivedOnly = false;
                if (Request.Form["unArchived"].ToString() != null)
                {
                    if (Request.Form["unArchived"].ToString() == "true")
                    {
                        unArchivedOnly = true;
                    }
                }

                string sortSQL = "";
                if (Request.Form["sortOrder"].ToString() != null && Request.Form["sortOrder"].ToString().Length > 0 && Request.Form["sortOrder"].ToString().Split(",").Length == 2)
                {
                    if (Request.Form["SortOrder"].ToString().Split(",")[0] == "email")
                    {
                        sortSQL += " ORDER BY m.CompanyEmail ";
                    }
                    if (Request.Form["SortOrder"].ToString().Split(",")[1] == "1")
                    {
                        sortSQL += "DESC";
                    }
                }

                List<MemberCompany> mList = CompanyMemberManager.GetMemberInfoByGenericSearch(searchTerm, currentPage, itemsPerPage, badAddressReport, archivedOnly, unArchivedOnly, sortSQL, dbconn);

                int totalRecordCount = 0;
                try
                {
                    totalRecordCount = CompanyMemberManager.GetMemberInfoCountByGenericSearch(searchTerm, currentPage, itemsPerPage, badAddressReport, archivedOnly, unArchivedOnly, sortSQL, dbconn);
                }
                catch { }
                decimal totalPages = Math.Ceiling(decimal.Parse(totalRecordCount.ToString()) / itemsPerPage);

                dynamic paginationObj = new JObject();
                paginationObj.itemsPerPage = itemsPerPage;
                paginationObj.currentPage = currentPage;
                paginationObj.totalRecordCount = totalRecordCount;
                paginationObj.totalPages = totalPages;


                obj["Items"] = JToken.FromObject(mList);
                obj["Pagination"] = JToken.FromObject(paginationObj);
            }
            catch (Exception ex)
            {
                obj.Error = ex.Message;
                obj.StackTrace = ex.StackTrace;
            }

            return obj;
        }

        [HttpGet]
        [Route("NameValuePair")]
        public List<dynamic> GetMemberNameValuePairJSON()
        {
            List<MemberCompany> mList = CompanyMemberManager.GetMemberCompany(dbconn);

            dynamic dList = new List<dynamic>();
            foreach (MemberCompany m in mList)
            {
                dynamic item = new JObject();
                item["name"] = m.Member;
                item["value"] = m.MemberCompanyId;
                dList.Add(item);
            }

            return dList;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveMember")]
        public int SaveMember(MemberCompany mc)
        {
            mc = (MemberCompany)Utilities.CleanProperties<MemberCompany>(mc);

            // if user is not a VSM admin, validate requested and actual company IDs match
            if (User.Claims.FirstOrDefault(c => c.Type == "VSMadmin") == null)
            {

                // get member-company id
                if (!int.TryParse(User.Claims.First(c => c.Type == "userid").Value, out int memberId))
                {
                    HttpContext.Response.StatusCode = 400;
                    return -1;
                }

                // get company-to-update id
                int companyId = mc.MemberCompanyId;

                // if ids do not match, return -2
                if (!CompanyMemberManager.DoesUserCompanyIdMatch(companyId, memberId, vsmconn))
                {
                    HttpContext.Response.StatusCode = 403;
                    return -2;
                }
            }

            return CompanyMemberManager.SaveMemberCompany(mc, dbconn);
        }

        [Authorize(Policy = "IsVSMadmin")]
        [HttpDelete]
        [Route("DeleteMemberById/{id}")]
        public bool DeleteMemberById(int id)
        {
            CompanyMemberManager.DeleteMemberCompanyById(id, dbconn);
            return true;
        }
    }
}