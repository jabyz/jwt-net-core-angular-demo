using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jwt_net_core_angular_demo_api.Models
{
  public class Msg
  {
    public string ResponseStatus { get; set; }
    //STATUSES
    //0 : ERROR
    //1 : SUCCESS
    //2 : INFO
    //3 : WARNING


    public string ResponseMessage { get; set; }
  }
}
