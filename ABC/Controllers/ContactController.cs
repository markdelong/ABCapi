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
    [Route("api/Contact")]
    public class ContactController : Controller
    {
        private readonly string _dbconn = ConnectionString.Credentials;
        private readonly string _vsmconn = ConnectionString.VSMCredentials;

        [HttpGet]
        [Route("{ContactId}")]
        public Contact GetContactById(int ContactId)
        {
            return ContactManager.GetContactById(ContactId, _dbconn);
        }

        [HttpGet]
        [Route("GetByMemberCompanyId/{MemberCompanyId}")]
        public List<Contact> GetContactsByMemberCompanyId(int MemberCompanyId)
        {
            return ContactManager.GetContactsByMemberCompanyId(MemberCompanyId, _dbconn);
        }

        [Authorize]
        [HttpPut]
        public int SaveContact(Contact item)
        {
            item = (Contact)Utilities.CleanProperties<Contact>(item);

            if (item.MemberCompanyId == 0
                || item.Name.Length == 0)
            {
                HttpContext.Response.StatusCode = 400;
                return 0;
            }

            //if user is not a VSM admin, validate that this contact belongs to this user
            if(User.Claims.FirstOrDefault(c => c.Type == "VSMadmin") == null)
            {
                int userId = 0;
                //get user ID
                if(User.Claims.FirstOrDefault(c => c.Type == "userid") != null)
                {
                    userId = Efinity.General.tryInt(User.Claims.FirstOrDefault(c => c.Type == "userid").Value);
                }

                if(userId <= 0)
                {
                    //there is no logged in user
                    HttpContext.Response.StatusCode = 400;
                    return -1;
                }

                if (!ContactManager.DoesContactBelongToUser(item, userId, _dbconn, _vsmconn))
                {
                    //this contact does not belong to this user/company
                    HttpContext.Response.StatusCode = 403;
                    return -2;
                }
            }

            return ContactManager.SaveContact(item, _dbconn);
        }

        [Authorize]
        [HttpDelete]
        public void DeleteContact(int ContactId)
        {
            ContactManager.DeleteContactById(ContactId, _dbconn);
        }
    }
}