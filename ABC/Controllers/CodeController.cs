using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ABC.Managers;
using Microsoft.AspNetCore.Authorization;

namespace ABC.Controllers
{
    [Produces("application/json")]
    [Route("api/Code")]
    public class CodeController : Controller
    {
        private readonly string _dbconn = ConnectionString.Credentials;
        private readonly string _vsmconn = ConnectionString.VSMCredentials;


        [HttpGet]
        [Route("GetCodesByMemberCompanyId/{MemberCompanyId}")]
        public List<Codes> GetCodesByMemberCompanyId(int MemberCompanyId)
        {
            return CodeManager.GetCodesByMemberCompanyId(MemberCompanyId, _dbconn);
        }

        [HttpGet]
        [Route("GetCSIChildren/{csiCode}")]
        public List<Codes> GetCSIChildren(string csiCode)
        {
            return CodeManager.CSIChildren(csiCode, _dbconn);
        }

        [HttpGet]
        [Route("GetNAICSChildren/{naicsCode}")]
        public List<Codes> GetNAICSChildren(string naicsCode)
        {
            return CodeManager.NAICSChildren(naicsCode, _dbconn);
        }

        [HttpGet]
        [Route("GetTopLevelCSI")]
        public List<Codes> GetTopLevelCSI()
        {
            return CodeManager.TopLevelCSI(_dbconn);
        }

        [HttpGet]
        [Route("GetTopLevelNAICS")]
        public List<Codes> GetTopLevelNAICS()
        {
            return CodeManager.TopLevelNAICS(_dbconn);
        }

        [HttpGet]
        [Route("CreateNewCode/{naicsCode}")]
        public List<Codes> AddCSIChildren(string naicsCode)
        {
            return CodeManager.NAICSChildren(naicsCode, _dbconn);
        }

        [Authorize]
        [HttpPost]
        [Route("SaveCodeIdsToCompany")]
        public bool SaveCodeIdsToCompany()
        {

            int memberCompanyId = Efinity.General.tryInt
                (
                Request.Form["memberCompanyId"].ToString()
                );

            if (User.Claims.FirstOrDefault(c => c.Type == "VSMadmin") == null)
            {

                // get member-company id
                if (!int.TryParse(User.Claims.First(c => c.Type == "userid").Value, out int memberId))
                {
                    HttpContext.Response.StatusCode = 400;
                    return false;
                }

                // if ids do not match, return false
                if (!CompanyMemberManager.DoesUserCompanyIdMatch(memberCompanyId, memberId, _vsmconn))
                {
                    HttpContext.Response.StatusCode = 403;
                    return false;
                }
            }

            string[] codeIdArr = Request.Form["codeIds"].ToString().Split(',');

            Code2MemberCompanyManager.DeleteCode2MemberCompanyByMemberCompanyId(memberCompanyId, _dbconn);

            foreach (string codeId in codeIdArr)
            {
                Codes c = CodeManager.GetCodeByCodeId(codeId, _dbconn);

                Code2MemberCompanyManager.SaveCode2MemberCompany(new Code2MemberCompany()
                {
                    Code2MemberCompanyId = 0,
                    MemberCompanyId = memberCompanyId,
                    CodeId = c.CodeId
                }, _dbconn);
            }

            return true;
        }

        [Authorize]
        [HttpPost]
        [Route("SaveCodesToCompany")]
        public bool SaveCodesToCompany()
        {
            int memberCompanyId = Efinity.General.tryInt(
                Request.Form["memberCompanyId"].ToString()
                );

            if (User.Claims.FirstOrDefault(c => c.Type == "VSMadmin") == null)
            {

                // get member-company id
                if (!int.TryParse(User.Claims.First(c => c.Type == "userid").Value, out int memberId))
                {
                    HttpContext.Response.StatusCode = 400;
                    return false;
                }

                // if ids do not match, return false
                if (!CompanyMemberManager.DoesUserCompanyIdMatch(memberCompanyId, memberId, _vsmconn))
                {
                    HttpContext.Response.StatusCode = 403;
                    return false;
                }
            }

            string[] codeArr = Request.Form["codes"].ToString().Split(',');

            Code2MemberCompanyManager.DeleteCode2MemberCompanyByMemberCompanyId(memberCompanyId, _dbconn);

            foreach (string code in codeArr)
            {
                Codes c = CodeManager.GetCodeByCode(code, _dbconn);

                Code2MemberCompanyManager.SaveCode2MemberCompany(new Code2MemberCompany()
                {
                    Code2MemberCompanyId = 0,
                    MemberCompanyId = memberCompanyId,
                    CodeId = c.CodeId
                }, _dbconn);
            }

            return true;
        }
    }
}