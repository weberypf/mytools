using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
namespace mytools.Web.Ext
{
    public class Mongo
    {
        private string conn = System.Configuration.ConfigurationManager.AppSettings["MONGOLAB_URI"];
        private MongoServer _server;
        public Mongo()
        {
            _server = MongoServer.Create(conn);
        }
        public MongoDatabase DataBase(string databaseName="")
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                databaseName = System.Configuration.ConfigurationManager.AppSettings["mongodatabase"];
            return _server.GetDatabase(databaseName);
        }
    }
}