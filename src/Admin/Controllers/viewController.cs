using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using hrcore.BLL;
using hrcore.Model;
using Microsoft.AspNetCore.DataProtection;

namespace hrcore.AdminControllers
{
    [Route("mapi")]
    public class viewController:BaseAdminController
    {
        public viewController(ILogger<viewController> logger) : base(logger) { }

        [RequireHttps]
        [Route(@"[action]/")]
        public APIReturn test()
        {
            return APIReturn.成功;
        }

        [Route(@"[action]/")]
        public APIReturn test2()
        {
            return APIReturn.成功.SetMessage("成功2");
        }
    }
}
