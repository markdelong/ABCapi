using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ABC.Managers;

namespace ABC.Controllers
{
    [Produces("application/json")]
    [Route("api/Code")]
    public class CodeController : Controller
    {
        string dbconn = ConnectionString.Credentials;

        [HttpGet]
        [Route("GetCodesByMemberCompanyId/{MemberCompanyId}")]
        public List<Codes> GetCodesByMemberCompanyId(int MemberCompanyId)
        {
            return CodeManager.GetCodesByMemberCompanyId(MemberCompanyId, dbconn);
        }

        [HttpGet]
        [Route("GetCSIChildren/{csiCode}")]
        public List<Codes> GetCSIChildren(string csiCode)
        {
            return CodeManager.CSIChildren(csiCode, dbconn);
        }
        [HttpGet]
        [Route("GetNAICSChildren/{naicsCode}")]
        public List<Codes> GetNAICSChildren(string naicsCode)
        {
            return CodeManager.NAICSChildren(naicsCode, dbconn);
        }

        [HttpGet]
        [Route("GetTopLevelCSI")]
        public List<Codes> GetTopLevelCSI()
        {
            return CodeManager.TopLevelCSI(dbconn);
        }
        [HttpGet]
        [Route("GetTopLevelNAICS")]
        public List<Codes> GetTopLevelNAICS()
        {
            return CodeManager.TopLevelNAICS(dbconn);
        }
        [HttpGet]
        [Route("CreateNewCode/{naicsCode}")]
        public List<Codes> AddCSIChildren(string naicsCode)
        {
            return CodeManager.NAICSChildren(naicsCode, dbconn);
        }

        [HttpPost]
        [Route("SaveCodeIdsToCompany")]
        public void SaveCodeIdsToCompany()
        {
            int memberCompanyId = Efinity.General.tryInt(
                Request.Form["memberCompanyId"].ToString()
                );

            string[] codeIdArr = Request.Form["codeIds"].ToString().Split(',');

            Code2MemberCompanyManager.DeleteCode2MemberCompanyByMemberCompanyId(memberCompanyId, dbconn);

            foreach (string codeId in codeIdArr)
            {
                Codes c = CodeManager.GetCodeByCodeId(codeId, dbconn);

                Code2MemberCompanyManager.SaveCode2MemberCompany(new Code2MemberCompany()
                {
                    Code2MemberCompanyId = 0,
                    MemberCompanyId = memberCompanyId,
                    CodeId = c.CodeId
                }, dbconn);
            }
        }

        [HttpPost]
        [Route("SaveCodesToCompany")]
        public void SaveCodesToCompany()
        {
            int memberCompanyId = Efinity.General.tryInt(
                Request.Form["memberCompanyId"].ToString()
                );

            string[] codeArr = Request.Form["codes"].ToString().Split(',');

            Code2MemberCompanyManager.DeleteCode2MemberCompanyByMemberCompanyId(memberCompanyId, dbconn);

            foreach(string code in codeArr)
            {
                Codes c = CodeManager.GetCodeByCode(code, dbconn);

                Code2MemberCompanyManager.SaveCode2MemberCompany(new Code2MemberCompany()
                {
                    Code2MemberCompanyId = 0,
                    MemberCompanyId = memberCompanyId,
                    CodeId = c.CodeId
                }, dbconn);
            }
        }
    }
}