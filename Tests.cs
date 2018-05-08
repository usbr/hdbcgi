using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using HDB_CGI;

namespace HDB_CGI
{
    [TestFixture()]
    public class Tests
    {
        [Test()]
        public void GetHostList()
        {
            HDB_CGI.cgi.GetHostList();
            var hosts = cgi.hostList;
            Assert.Greater(hosts.Count, 0);
        }
        
        [Test()]
        public void ConnectLC()
        {
            var conx = cgi.ConnectHDB("lchdb2");
            Assert.AreEqual("Open", conx.State.ToString());
        }

        [Test()]
        public void ConnectUC()
        {
            var conx = cgi.ConnectHDB("uchdb2");
            Assert.AreEqual("Open", conx.State.ToString());
        }

        [Test()]
        public void ConnectYAO()
        {
            var conx = cgi.ConnectHDB("yaohdb");
            Assert.AreEqual("Open", conx.State.ToString());
        }

        [Test()]
        public void ConnectECAO()
        {
            var conx = cgi.ConnectHDB("ecohdb");
            Assert.AreEqual("Open", conx.State.ToString());
        }

        [Test()]
        public void ConnectLBAO()
        {
            var conx = cgi.ConnectHDB("lbohdb");
            Assert.AreEqual("Open", conx.State.ToString());
        }

        [Test()]
        public void QueryV0DateFormat()
        {
            var urlString = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1928,1930&tstp=DY&syer=2015&smon=1&sday=1&eyer=2015&emon=1&eday=10&format=1";
            var outFile = cgi.hdbWebQuery(urlString);
            Assert.AreEqual(@"<BR>01/10/2015 00:00,          NaN,      1088.48", outFile[34].ToString());
        }

        [Test()]
        public void QueryV1DateFormat()
        {
            var urlString = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1928,1930&tstp=DY&t1=1/1/2015&t2=1/10/2015&format=csv";
            var outFile = cgi.hdbWebQuery(urlString);
            Assert.AreEqual(@"<BR>01/10/2015 00:00,          NaN,      1088.48", outFile[16].ToString());
        }

        [Test()]
        public void QueryIsoDateFormat()
        {
            var urlString = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1928,1930&tstp=DY&t1=2015-01-01T00:00&t2=2015-01-10T00:00&format=1";
            var outFile = cgi.hdbWebQuery(urlString);
            Assert.AreEqual(@"<BR>01/10/2015 00:00,          NaN,      1088.48", outFile[34].ToString());
        }

        [Test()]
        public void QueryModeledData()
        {
            var urlString = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1930&tstp=MN&t1=2017-12-01T00:00&t2=2018-05-01T00:00&table=M&mrid=3012&format=88";
            var outFile = cgi.hdbWebQuery(urlString);
            Assert.AreEqual(@"05/01/2018 00:00,         3012, 1062.152106639250" + "\n", outFile[6].ToString());
        }

        [Test()]
        public void QueryInstantData()
        {
            var urlString = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=25401&tstp=IN&t1=2018-05-07T05:00&t2=2018-05-07T08:00&table=R&mrid=&format=table";
            var outFile = cgi.hdbWebQuery(urlString);
            Assert.AreEqual(13, outFile.Count);
        }

        [Test()]
        public void TestJSON()
        {
            var urlString = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1928,1930&tstp=DY&t1=2015-01-01T00:00&t2=2015-01-10T00:00&format=json";
            try
            {
                var outFile = JsonConvert.SerializeObject(cgi.hdbWebQuery(urlString));
                Assert.AreEqual(0,0);
            }
            catch
            {
                Assert.Fail("Output is not a valid JSON");
            }
        }

    }
}
