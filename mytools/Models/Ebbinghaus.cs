using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using mytools.Web.Ext;

namespace mytools.Web.Models
{
    public class Ebbinghaus
    {
        private Mongo mongo = new Mongo();
        private string[] shell = new[] { "ls", "set", "xx", "rm","info" };
        //复习间隔(天)
        internal static int[] span = new[] { 1, 3, 7, 15, 30 };
        public static string help = @"
用法：
回复 ls  #查看自己的学习进度列表
回复 set qq yourvalle  #设置自己的QQ号(发送学习提醒到qq邮箱，微信不让主动给用户发微信所以改发邮件)
回复 set alarm yourvalle  #设置提醒时间(0-23数字,不设置alarm默认20点提醒)
回复 info  #查看自己的设置
回复 xx lesson1  #学习新内容lesson1
回复 xx no 1  #完成复习编号(命令ls可查到学习内容的编号)为1的内容)
回复 rm 1  #删除编号为1的学习内容

";
        public string Exec(string cmd,string openid)
        {
            if (IsShell(cmd))
            {
                var parms = cmd.Split(' ');
                var sl = parms[0].ToLower().Trim();
                switch (sl)
                {
                    case "ls":
                        return ls(openid);
                    case "set":
                        return set(openid, parms);
                    case "xx":
                        return xx(openid, parms);
                    case "rm":
                        return rm(openid, parms);
                    case "info":
                        return info(openid, parms);
                }
            }
            return null;
        }
        public bool IsShell(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return false;
            var arr = cmd.Split(' ');
            return shell.Contains(arr[0].ToLower());
        }
        private string ls(string openid)
        {
            var tasks = mongo.DataBase().GetCollection<tasks>("tasks");
            var query = Query.EQ("openid", BsonValue.Create(openid));
            var list = tasks.Find(query).OrderBy(a => a.times == 5 ? 1 : 0).OrderBy(a => a.addtime);

            if (list != null && list.Count() > 0)
            {
                return list.Select(a => a.ToString()).Aggregate((a, b) => a + Environment.NewLine + b);
            }
            return "你还啥也没学呢。。";
        }
        private string set(string openid, string[] parms)
        {
            var how = string.Format("how to set : {0} eg:set qq 10000 (设置qq号为10000) {0} eg:set alarm 20 (设置提醒时间为20:00点){0}PS:不设置alarm默认20：00提醒", Environment.NewLine);

            if (parms.Length > 2) {

                var users = mongo.DataBase().GetCollection<users>("users");
                var query = Query.EQ("openid", BsonValue.Create(openid));
                var user = users.FindOne(query);
                bool insert = false;
                if (user == null)
                {
                    insert = true;
                    user = new users();
                    user.openid = openid;
                    user.alarmtime = 20;
                }

                var type = parms[1];
                var val = parms[2];
                if (type.Equals("qq", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Regex.IsMatch(val, @"^\d{5,15}$"))
                    {
                        user.qq = val;
                        if (insert)
                        {
                            users.Insert<users>(user);
                        }
                        else
                        {
                            var update = new UpdateDocument { { "$set", new QueryDocument { { "qq", val } } } };
                            users.Update(query, update);
                        }
                        return string.Format("DONE! {0}{1}", Environment.NewLine, user);
                    }
                    return "qq号格式不对";
                }
                if (type.Equals("alarm", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Regex.IsMatch(val, @"^\d{1,2}$"))
                    {
                        var hour = Convert.ToInt32(val);
                        if (hour >= 0 && hour < 24) {
                            if (insert)
                            {
                                user.alarmtime = hour;
                                users.Insert<users>(user);
                            }
                            else
                            {
                                var update = new UpdateDocument { { "$set", new QueryDocument { { "alarmtime", hour } } } };
                                users.Update(query, update);
                            }
                            return string.Format("DONE! {0}{1}", Environment.NewLine, user);
                        }
                        return "提醒时间格式不对(0-23数字)";
                    }
                    return "提醒时间格式不对(0-23数字)";
                }
                return how;
            }
            return how;
        }
        private string xx(string openid, string[] parms)
        {
            var how = string.Format("完成学习内容 : {0}eg:xx lesson1 学习新内容lesson1 {0}eg:xx no 1 (完成复习编号(命令ls可查到学习内容的编号)为1的内容)", Environment.NewLine);
            var tasks = mongo.DataBase().GetCollection<tasks>("tasks");

            var query = Query.EQ("openid", BsonValue.Create(openid));
            var list = tasks.Find(query);
            if (parms.Length > 2 && parms[1].Equals("no",StringComparison.CurrentCultureIgnoreCase)) {
                var id = 0;
                int.TryParse(parms[2], out id);
                if (id > 0) {
                    var entity = list.FirstOrDefault(a => a.MYID == id);
                    if (entity != null) {
                        entity.times = entity.times + 1;

                        var updatequery = Query.And(
                            Query.EQ("openid", BsonValue.Create(openid)),
                            Query.EQ("MYID", BsonValue.Create(id)));
                        var update = new UpdateDocument { { "$set", new QueryDocument { { "times", entity.times } } } };
                        tasks.Update(updatequery, update);
                        return entity.ToString();
                    }
                }
                return string.Format("木有找到编号{0}的需要复习的任务", parms[2]);
            }
            if (parms.Length == 2)
            {
                var maxid = 0;
                if (list.Count() > 0)
                {
                    maxid = list.Max(a => a.MYID);
                }

                var task = new tasks();
                task.addtime = DateTime.UtcNow;
                task.openid = openid;
                task.taskname = parms[1];
                task.times = 0;
                task.notifytimes = 0;
                task.MYID = maxid + 1;
                tasks.Insert<tasks>(task);
                return task.ToString();
            }
            return how;
        }
        private string rm(string openid, string[] parms)
        {
            var how = string.Format("删除学习内容 : {0}eg:rm 1 删除编号为1的学习内容 ", Environment.NewLine);
            var tasks = mongo.DataBase().GetCollection<tasks>("tasks");

            if (parms.Length > 1)
            {
                var id = 0;
                int.TryParse(parms[1], out id);
                if (id > 0)
                {
                    var rmquery = Query.And(
                            Query.EQ("openid", BsonValue.Create(openid)),
                            Query.EQ("MYID", BsonValue.Create(id)));

                    tasks.Remove(rmquery);
                    return "删除完成";
                }
                return string.Format("木有找到编号{0}的需要删除的任务", parms[2]);
            }
            return how;
        }
        private string info(string openid, string[] parms)
        {
            var users = mongo.DataBase().GetCollection<users>("users");
            var query = Query.EQ("openid", BsonValue.Create(openid));
            var user = users.FindOne(query);

            if (user != null)
            {
                return user.ToString();
            }
            return "还没有设置qq和alarm，无法邮件通知你学习进度，回复 set 查看如何设置";
        }
        public void Notify()
        {
            var tasks = mongo.DataBase().GetCollection<tasks>("tasks");
            var users = mongo.DataBase().GetCollection<users>("users");
            var query = Query.LT("times", BsonValue.Create(5));

            var list = tasks.Find(query);
            users user = null;
            if (list != null && list.Count() > 0)
            {
                foreach (var item in list.GroupBy(a => a.openid))
                {
                    user = users.FindOne(Query.EQ("openid", BsonValue.Create(item.Key)));
                    if (user != null && !string.IsNullOrWhiteSpace(user.qq) && DateTime.UtcNow.AddHours(8).Hour >= user.alarmtime)
                    {
                        var neednofity = item.Where(a => a.addtime.AddDays(a.times).Date == DateTime.UtcNow.Date)
                            .Where(a => a.times > a.notifytimes);

                        foreach (var info in neednofity)
                        {
                            //发邮件，更新notifytimes
                            if (Email.Mail(string.Format("{0}@qq.com", user.qq), string.Format("是时候学习{0}啦", info.taskname), string.Format("是时候学习{0}啦", info.taskname)))
                            {
                                var updatequery = Query.And(
                                                           Query.EQ("openid", BsonValue.Create(info.openid)),
                                                           Query.EQ("MYID", BsonValue.Create(info.MYID)));
                                var update = new UpdateDocument { { "$set", new QueryDocument { { "notifytimes", info.notifytimes + 1 } } } };
                                tasks.Update(updatequery, update);
                            }
                        }
                    }
                }
            }
        }
    }
    public class users
    {
        public ObjectId  Id { get; set; }
        public string openid { get; set; }
        public string qq { get; set; }
        public int alarmtime { get; set; }
        public override string ToString()
        {
            return string.Format("qq:{0}{1} alarm:{2}点", qq, Environment.NewLine, alarmtime);
        }
    }
    public class tasks
    {
        public ObjectId Id { get; set; }
        public int MYID { get; set; }
        public string openid { get; set; }
        public string taskname { get; set; }
        public int times { get; set; }
        public int notifytimes { get; set; }
        public DateTime addtime { get; set; }
        public override string ToString()
        {
            if (times > 4)
            {
                return string.Format("{0}.{1} 已按照遗忘曲线全部搞定，牛逼！这辈子估计是忘不了了", MYID, taskname);
            }
            else
            {
                return string.Format("{0}.{1} 已按照遗忘曲线复习{2}/5次，下次复习时间:{3}!努力吧!", MYID, taskname, times, addtime.AddDays(Ebbinghaus.span[times]).AddHours(8).ToShortDateString());
            }
        }
    }
}