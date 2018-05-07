using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABC.Managers
{
    //Auto genCode 2/15/2018 5:41:48 PM
    public class MemberCompany
    {
        int _membercompanyid = 0;
        string _member = "";
        string _contact = "";
        string _address = "";
        string _address2 = "";
        string _city = "";
        string _state = "";
        string _zip = "";
        string _phone = "";
        string _fax = "";
        string _companyemail = "";
        string _website = "";
        string _contactemail = "";
        string _companyprofile = "";
        Boolean _status = false;
        int _listinglevel = 0;
        int _importid = 0;
        string _logoimage = "";
        string _keywordtags = "";
        string _latitude = "";
        string _longitude = "";
        string _serviceareas = "";
        Boolean _STEPAffil = false;
        Boolean _SafetyALlianceAffil = false;

        public int MemberCompanyId {
            get { return _membercompanyid; }
            set { _membercompanyid = value; }
        }
        public string Member {
            get { return _member; }
            set { _member = value; }
        }
        public string Contact {
            get { return _contact; }
            set { _contact = value; }
        }
        public string Address {
            get { return _address; }
            set { _address = value; }
        }
        public string Address2 {
            get { return _address2; }
            set { _address2 = value; }
        }
        public string City {
            get { return _city; }
            set { _city = value; }
        }
        public string State {
            get { return _state; }
            set { _state = value; }
        }
        public string Zip {
            get { return _zip; }
            set { _zip = value; }
        }
        public string Phone {
            get { return _phone; }
            set { _phone = value; }
        }
        public string Fax {
            get { return _fax; }
            set { _fax = value; }
        }
        public string CompanyEmail {
            get { return _companyemail; }
            set { _companyemail = value; }
        }
        public string Website {
            get { return _website; }
            set { _website = value; }
        }
        public string ContactEmail {
            get { return _contactemail; }
            set { _contactemail = value; }
        }
        public string CompanyProfile {
            get { return _companyprofile; }
            set { _companyprofile = value; }
        }
        public Boolean Status {
            get { return _status; }
            set { _status = value; }
        }
        public int ListingLevel {
            get { return _listinglevel; }
            set { _listinglevel = value; }
        }
        public int ImportId {
            get { return _importid; }
            set { _importid = value; }
        }
        public string LogoImage {
            get { return _logoimage; }
            set { _logoimage = value; }
        }
        public string KeywordTags {
            get { return _keywordtags; }
            set { _keywordtags = value; }
        }
        public string Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }
        public string Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
        public string ServiceAreas
        {
            get { return _serviceareas; }
            set { _serviceareas = value; }
        }
        public Boolean STEPAffil {
            get { return _STEPAffil; }
            set { _STEPAffil = value; }
        }
        public Boolean SafetyAllianceAffil {
            get { return _SafetyALlianceAffil; }
            set { _SafetyALlianceAffil = value; }
        }
    }
    //End genCode
}
