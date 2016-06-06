using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using WxPayAPI;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        JsApiPay jsApiPay = new JsApiPay();
        // GET: Home
        public ActionResult Index()
        {
            if (Session["openid"] == null)
            {
                try
                {
                    //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                    GetOpenidAndAccessToken();

                }
                catch (Exception ex)
                {
                    //Response.Write(ex.ToString());
                    //throw;
                }
            }
            return View();
        }


        /**
        * 
        * 网页授权获取用户基本信息的全部过程
        * 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * 第一步：利用url跳转获取code
        * 第二步：利用code去获取openid和access_token
        * 
        */
        public void GetOpenidAndAccessToken()
        {
            if (Session["code"] != null)
            {
                //获取code码，以获取openid和access_token
                string code = Session["code"].ToString();
                Log.Debug(this.GetType().ToString(), "Get code : " + code);
                jsApiPay.GetOpenidAndAccessTokenFromCode(code);
            }
            else
            {
                //构造网页授权获取code的URL
                string host = Request.Url.Host;
                string path = Request.Path;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
                //string redirect_uri = HttpUtility.UrlEncode("http://gzh.lmx.ren");
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);
                Session["url"] = url;               
            }
        }


        /// <summary>
        /// 获取code
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getCode()
        {
            object objResult = "";
            if (Session["url"] != null)
            {
                objResult = Session["url"].ToString();
            }
            else
            {
                objResult = "url为空。";
            }
            return Json(objResult);
        }

        /// <summary>
        /// 通过code换取网页授权access_token和openid的返回数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getWxInfo()
        {
            object objResult = "";
            string strCode = Request.Form["code"];
            if (Session["access_token"] == null || Session["openid"] == null)
            {
                jsApiPay.GetOpenidAndAccessTokenFromCode(strCode);
            }
            string strAccess_Token = Session["access_token"].ToString();
            string strOpenid = Session["openid"].ToString();
            objResult = new { openid = strOpenid, access_token = strAccess_Token };
            return Json(objResult);
        }




        /// <summary>
        /// 充值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MeterRecharge()
        {
            object objResult = "";
            string strTotal_fee = Request.Form["totalfee"];
            string strFee = (double.Parse(strTotal_fee) * 100).ToString();

            //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
            jsApiPay.openid = Session["openid"].ToString();
            jsApiPay.total_fee = int.Parse(strFee);

            //JSAPI支付预处理
            try
            {
                string strBody = "南宫萧尘微信支付";//商品描述
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult(strBody);
                WxPayData wxJsApiParam = jsApiPay.GetJsApiParameters();//获取H5调起JS API参数

                ModelForOrder aOrder = new ModelForOrder()
                {
                    appId = wxJsApiParam.GetValue("appId").ToString(),
                    nonceStr = wxJsApiParam.GetValue("nonceStr").ToString(),
                    packageValue = wxJsApiParam.GetValue("package").ToString(),
                    paySign = wxJsApiParam.GetValue("paySign").ToString(),
                    timeStamp = wxJsApiParam.GetValue("timeStamp").ToString(),
                    msg = "成功下单,正在接入微信支付."
                };
                objResult = aOrder;
            }
            catch (Exception ex)
            {
                ModelForOrder aOrder = new ModelForOrder()
                {
                    appId = "",
                    nonceStr = "",
                    packageValue = "",
                    paySign = "",
                    timeStamp = "",
                    msg = "下单失败，请重试,多次失败,请联系管理员."
                };
                objResult = aOrder;
            }
            return Json(objResult);
        }


    }
}