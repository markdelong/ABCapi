using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABC.Managers
{
    public class Codes
    {
        int _codeid = 0;
        int _codetypeid = 0;
        string _codename = "";
        string _code = "";

        public int CodeId {
            get { return _codeid; }
            set { _codeid = value; }
        }
        public int CodeTypeId {
            get { return _codetypeid; }
            set { _codetypeid = value; }
        }
        public string CodeName {
            get { return _codename; }
            set { _codename = value; }
        }
        public string Code {
            get { return _code; }
            set { _code = value; }
        }
    }
    //End genCode
}
