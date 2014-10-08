using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mytools.Web.Models;

namespace mytools.Web.Controllers
{
    public class TaskController : Controller
    {
        //执行计划任务的
        public ActionResult Index()
        {
            try
            {
                //艾宾浩斯提醒
                new Ebbinghaus().Notify();
                return Content("ok");
            }
            catch (Exception ex)
            {
                return Content(ex.StackTrace);
            }
        }
    }
}
