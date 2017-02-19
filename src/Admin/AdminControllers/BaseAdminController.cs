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
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;

namespace hrcore.AdminControllers {
	public partial class BaseAdminController : Controller {
		public ILogger _logger;
        IDataProtector _protector;
        public ISession Session { get { return HttpContext.Session; } }
		public HttpRequest Req { get { return Request; } }
		public HttpResponse Res { get { return Response; } }

		//public SysuserInfo LoginUser { get; private set; }
		public BaseAdminController(ILogger logger) { _logger = logger; }
        public BaseAdminController(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(incSys.skeyDefault);
        }
        
		public override void OnActionExecuting(ActionExecutingContext context) {

            string retMsg = string.Empty;
            requestCheck(out retMsg);

            #region 参数验证

            if (context.ModelState.IsValid == false)
				foreach(var value in context.ModelState.Values)
					if (value.Errors.Any()) {
						context.Result = APIReturn.参数错误.SetMessage($"参数错误：{value.Errors.First().ErrorMessage}");
						return;
					}
            #endregion

            #region 初始化当前登陆账号
            //string username = Session.GetString("login.username");
            //if (!string.IsNullOrEmpty(username)) LoginUser = Sysuser.GetItemByUsername(username);

            //var method = (context.ActionDescriptor as ControllerActionDescriptor).MethodInfo;
            //if (method.GetCustomAttribute<需要登陆Attribute>() != null && LoginUser == null)
            //	context.Result = new RedirectResult("/signin");
            //else if (method.GetCustomAttribute<匿名访问Attribute>() == null && LoginUser == null)
            //	context.Result = new RedirectResult("/signin");
            //ViewBag.user = LoginUser;
            #endregion
			base.OnActionExecuting(context);
		}
		public override void OnActionExecuted(ActionExecutedContext context) {
			if (context.Exception != null) {
                IDictionary extData = context.Exception.Data;
                string extDataStr = string.Empty;
                foreach (string perKey in extData)
                    extDataStr = $"{extDataStr}&&{perKey}-{extData[perKey]}";

                string errorNum = DateTime.Now.ToTimeStamp().ToString();
                _logger.LogError("捕获异常", "Name", context.ActionDescriptor.DisplayName, "Num", errorNum, "from", HttpContext.Connection.RemoteIpAddress.ToString(), "Url", context.HttpContext.Request.Path, "Msg", context.Exception.Message);
                context.Result = APIReturn.系统异常.SetMessage($"系统异常:{context.Exception.Message.Substring(0, incSys.exceptionShowLength)}").SetData("errorNum", errorNum);
                context.Exception = null;
			}
			base.OnActionExecuted(context);
		}

		public override ViewResult View() {
			return base.View($"/Views/Admin/{this.ControllerContext.ActionDescriptor.ControllerName}/{this.ControllerContext.ActionDescriptor.MethodInfo.Name}.cshtml");
		}

        /// <summary>
        /// 请求记录及控制
        /// </summary>
        /// <param name="degree">控制级别，0-仅记录，1-记录+白名单过滤</param>
        /// <param name="frequencyLimit">是否开启频率控制</param>
        /// <returns></returns>
		protected bool requestCheck(out string retMsg, int degree = 0, bool frequencyLimit = false)
        {
            //获取IP
            string srcIP = HttpContext.Connection.RemoteIpAddress.ToString();
            string realIP = HttpContext.Request.Headers["X-Real-IP"].ToString();
            realIP = string.IsNullOrWhiteSpace(realIP) ? HttpContext.Request.Headers["X-Forwarded-For"].ToString() : realIP;
            srcIP = string.IsNullOrWhiteSpace(realIP) ? srcIP : realIP;

            //参数
            IQueryCollection query = HttpContext.Request.Query;
            List<string> keys = query.Keys.ToList();
            string paramStr = string.Empty;
            keys.ForEach(o=> {
                paramStr = $"{paramStr}&{o}={query[o]}";
            });

            _logger.LogDebug("请求发起", "url", HttpContext.Request.Path, "from", srcIP, "params", paramStr);
            retMsg = "记录完毕";
            return true;
        }
	}
}

#region 需要登陆、匿名访问、IgnoreObsoleteControllers、FormDataOperationFilter
public partial class 需要登陆Attribute : Attribute { }
public partial class 匿名访问Attribute : Attribute { }
public static class Swashbuckle_SwaggerGen_Application_SwaggerGenOptions_ExtensionMethods
{
    public class IgnoreObsoleteControllersFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (ApiDescription apiDescription in context.ApiDescriptionsGroups.Items.SelectMany(e => e.Items))
            {
                if (apiDescription.ControllerAttributes().OfType<ObsoleteAttribute>().Any() == false) continue;
                var key = "/" + apiDescription.RelativePath.TrimEnd('/');
                if (swaggerDoc.Paths.ContainsKey(key))
                    swaggerDoc.Paths.Remove(key);
            }
        }
    }
    public static void IgnoreObsoleteControllers(this SwaggerGenOptions options)
    {
        options.DocumentFilter<IgnoreObsoleteControllersFilter>();
    }
    public static object Json(this Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper html, object obj)
    {
        string str = JsonConvert.SerializeObject(obj);
        if (!string.IsNullOrEmpty(str)) str = Regex.Replace(str, @"<(/?script[\s>])", "<\"+\"$1", RegexOptions.IgnoreCase);
        if (html == null) return str;
        return html.Raw(str);
    }
}
namespace Swashbuckle.Swagger.Model
{
    public class FormDataOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var actattrs = context.ApiDescription.ActionAttributes();
            if (actattrs.OfType<HttpPostAttribute>().Any() ||
                actattrs.OfType<HttpPutAttribute>().Any())
                operation.Consumes = new[] { "multipart/form-data" };
        }
    }
}
#endregion

#region APIReturn

public class returnCode
{
    /// <summary>
    /// 成功
    /// </summary>
    public static int success = 0;
    /// <summary>
    /// 失败
    /// </summary>
    public static int failed = 100;
    /// <summary>
    /// 无权限
    /// </summary>
    public static int noAuth = 99;
    /// <summary>
    /// 异常
    /// </summary>
    public static int exception = 98;
    /// <summary>
    /// 未找到实例
    /// </summary>
    public static int hasNoRecord = 97;
    /// <summary>
    /// 参数错误
    /// </summary>
    public static int paramError = 96;
}



[JsonObject(MemberSerialization.OptIn)]
public partial class APIReturn : ContentResult {
	[JsonProperty("code")] public int Code { get; protected set; }
	[JsonProperty("message")] public string Message { get; protected set; }
	[JsonProperty("data")] public Hashtable Data { get; protected set; } = new Hashtable();
	[JsonProperty("success")] public bool Success { get { return this.Code == 0; } }

	public APIReturn() { }
	public APIReturn(int code) { this.SetCode(code); }
	public APIReturn(string message) { this.SetMessage(message); }
	public APIReturn(int code, string message, params object[] data) { this.SetCode(code).SetMessage(message).AppendData(data); }

	public APIReturn SetCode(int value) { this.Code = value;  return this; }
	public APIReturn SetMessage(string value) { this.Message = value;  return this; }
	public APIReturn SetData(params object[] value) {
		this.Data.Clear();
		return this.AppendData(value);
	}
	public APIReturn AppendData(params object[] value) {
		if (value == null || value.Length < 2 || value[0] == null) return this;
		for (int a = 0; a < value.Length; a += 2) {
			if (value[a] == null) continue;
			this.Data[value[a]] = a + 1 < value.Length ? value[a + 1] : null;
		}
		return this;
	}
    #region form 表单 target=iframe 提交回调处理
    private void Jsonp(ActionContext context)
    {
        string __callback = context.HttpContext.Request.HasFormContentType ? context.HttpContext.Request.Form["__callback"].ToString() : null;
        if (string.IsNullOrEmpty(__callback))
        {
            this.ContentType = "text/json;charset=utf-8;";
            this.Content = JsonConvert.SerializeObject(this);
        }
        else
        {
            this.ContentType = "text/html;charset=utf-8";
            this.Content = $"<script>top.{__callback}({Swashbuckle_SwaggerGen_Application_SwaggerGenOptions_ExtensionMethods.Json(null, this)});</script>";
        }
    }
    public override void ExecuteResult(ActionContext context)
    {
        Jsonp(context);
        base.ExecuteResult(context);
    }
    public override Task ExecuteResultAsync(ActionContext context)
    {
        Jsonp(context);
        return base.ExecuteResultAsync(context);
    }
    #endregion

    public static APIReturn 成功 { get { return new APIReturn(returnCode.success, "成功"); } }
	public static APIReturn 失败 { get { return new APIReturn(returnCode.failed, "失败"); } }
	public static APIReturn 未找到实例 { get { return new APIReturn(returnCode.hasNoRecord, "未找到实例"); } }
	public static APIReturn 参数错误 { get { return new APIReturn(returnCode.paramError, "参数格式错误"); } }
    public static APIReturn 无权限 { get { return new APIReturn(returnCode.noAuth, "无权限"); } }
    public static APIReturn 系统异常 { get { return new APIReturn(returnCode.exception, "系统异常"); } }
    
}
#endregion
