using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Devart.Data.Universal;
using Devart.Data.Oracle;

namespace HDB_CGI
{
    public class cgi
    {
        // Search for [JR] tag to find areas that could use some work
        public static bool jrDebug = false;

        /// <summary>
        /// Container for the available HDBs that the CGI can connect to -- must map 1:1 with hostlist.txt file
        /// </summary>
        private static List<string> hdbList = new List<string>
        {
            "lchdb2",
            "uchdb2",
            "yaohdb",
            "ecohdb",
            "lbohdb"
        };

        /// <summary>
        /// Container for the HDB host and log-on information -- populated by hostlist.txt file
        /// </summary>
        private static List<string[]> hostList = new List<string[]>
        {
            // new string[] {host, service, port, user, pass}
        };


        /// <summary>
        /// Main entry point for HDB queries
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            GetHostList();

            string query = "";
            if (!jrDebug)
            {
                try
                {
                    Console.Write("Content-Type: text/html\n\n");
                    query = GetQuery();
                    if (!ValidateQuery(query))
                    { Console.WriteLine("Error: Invalid query"); }

                    // Initialize output container
                    List<string> outFile = new List<string>();

                    // Check for predefined views and dashboards
                    string outFormat = Regex.Match(query, @"format=([aA-zZ\-]+)").Groups[1].Value.ToString();
                    if (outFormat.ToLower() == "lcreservoirs")
                    { outFile = HDB_CGI.graphing.dashboard_LcReservoirs(); }

                    else
                    { outFile = hdbWebQuery(query); }
                    foreach (var line in outFile)
                    {
                        if (outFormat == "9" || outFormat == "99" || outFormat.ToLower() == "graph")
                        { Console.WriteLine(line); }
                        else
                        { Console.Write(line); }
                    }
                }
                // [JR] Fix error logging so it goes to an actual log file...
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            else
            {
                #region
                // Test URLs
                //query = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1928,1930&tstp=DY&syer=2015&smon=1&sday=1&eyer=2015&emon=1&eday=10&format=1";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=?svr=yaohdb&sdi=21877&tstp=DY&t1=1/1/2016&t2=1/31/2016&format=2";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=uchdb2&sdi=1872,1981&tstp=dy&t1=-5&format=1";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=uchdb2&sdi=*&tstp=MN&t1=1/1/2015&t2=12/31/2016&table=M&mrid=2212&format=2";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=1345,1721,1776&tstp=MN&t1=-5&table=M&mrid=2212&format=2";
                //query = @"http://localhost:8080/HDB_CGI.com?format=lcreservoirs";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=2089&tstp=IN&t1=8/10/2017&t2=8/12/2017&format=graph";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=2089&tstp=IN&t1=8/10/2017&t2=8/12/2017&format=json";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=ecohdb&sdi=100488,100514&tstp=dy&t1=8/1/2017&t2=8/20/2017&format=table";
                //query = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=2100,2101&tstp=MN&t1=08-01-2016&t2=08-31-2018&table=R&mrid=&format=json";
                query = @"http://localhost:8080/HDB_CGI.com?svr=lchdb2&sdi=2100,2101&tstp=MN&t1=08-01-2017&t2=08-31-2018&table=M&mrid=3039,3035&format=1";
                //query = @"http://ibr3lcrsrv01.bor.doi.net:8080/HDB_CGI.com?svr=lchdb2&sdi=1863,1930,2166,2146&tstp=DY&t1=1/1/1980&t2=1/1/2016&format=json";
                //query = @"http://ibr3lcrsrv01.bor.doi.net:8080/HDB_CGI.com?svr=lbohdb&sdi=60064,60066&tstp=DY&t1=1/1/2018&t2=4/1/2018&format=1";

                // Initialize output container
                List<string> outFile = new List<string>();

                // Check for predefined views and dashboards
                string outFormat = Regex.Match(query, @"format=([aA-zZ\-]+)").Groups[1].Value.ToString();
                if (outFormat.ToLower() == "lcreservoirs")
                { outFile = HDB_CGI.graphing.dashboard_LcReservoirs(); }
                else
                { outFile = hdbWebQuery(query); }

                // Open HTML file
                string fOut = System.IO.Path.GetTempFileName() + ".html";
                System.IO.File.WriteAllLines(fOut, outFile.ToArray());
                System.Diagnostics.Process.Start(fOut);

                #endregion
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ACTIVE CODE
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region

        /// <summary>
        /// Get hostlist.txt file which contains the specifics for the HDBs that are available for active connections
        /// </summary>
        private static void GetHostList()
        {
            try
            {
                var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), "hostlist.txt");
                string[] hostText = File.ReadAllLines(path);
                foreach (var host in hostText)
                {
                    List<string> hostItems = new List<string>();
                    foreach (var item in host.Split(','))
                    {
                        hostItems.Add(item.Trim());
                    }
                    hostList.Add(hostItems.ToArray());
                }
            }
            catch
            {
                Console.WriteLine("Textfile containing HDB hosts information not found...");
                Console.WriteLine("\thostlist.txt has to be in the same folder as the executable ");
                Console.WriteLine("\ttext should contain:");
                Console.WriteLine("\thdb-server-name, hdb-service-name, hdb-port-number, hdb-user-name, hdb-user-password");
                Console.WriteLine("");
            }
        }


        /// <summary>
        /// Checks the query string
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static bool ValidateQuery(string query)
        { return Regex.IsMatch(query, "[^A-Za-z0-9=&%+-/_]"); }


        /// <summary>
        /// Reads the query (Get or Post) and generates a query string for the program
        /// </summary>
        /// <returns></returns>
        private static string GetQuery()
        {
            // Construct search string
            string srchString = "";
            var method = System.Environment.GetEnvironmentVariable("REQUEST_METHOD");
            if (method == null)
            { return ""; }
            if (method.Equals("POST")) //POST Method
            {
                string PostedData = "";
                int PostedDataLength = Convert.ToInt32(System.Environment.GetEnvironmentVariable("CONTENT_LENGTH"));
                if (PostedDataLength > 2048) PostedDataLength = 2048;   // Max length for POST data (security limit)
                for (int i = 0; i < PostedDataLength; i++)
                { PostedData += Convert.ToChar(Console.Read()).ToString(); }
                srchString = "?" + PostedData;
            }
            else //GET Method
            { srchString = "?" + System.Environment.GetEnvironmentVariable("QUERY_STRING"); }
            // Sanitize query string. [JR] Update code here to catch other dangerous characters
            srchString = srchString.Replace("&#39", ""); // Remove HTML '
            srchString = srchString.Replace("%27", ""); // Remove HEX '
            srchString = srchString.Replace("%&#44", ","); // Replace HTML ,
            srchString = srchString.Replace("%2C", ","); // Replace HEX ,
            return srchString;
        }


        /// <summary>
        /// Connects to HDB
        /// </summary>
        /// <returns></returns>
        private static UniConnection ConnectHDB(string hdb)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Oracle DB connection parameters
            string dbServer = hdb;
            var a = hostList.Contains(new string[] { "lchdb2" });
            if (hdbList.IndexOf(dbServer) < 0)
            { throw new Exception("HDB Database not recognized."); }

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // Open Oracle DB connections
            if (jrDebug)
            { Console.Write("Connecting to HDB... "); }

            Devart.Data.Oracle.OracleConnectionStringBuilder sb = new OracleConnectionStringBuilder();
            sb.Direct = true;
            sb.Server = hostList[hdbList.IndexOf(dbServer)][0];
            sb.ServiceName = hostList[hdbList.IndexOf(dbServer)][1];
            sb.Port = Convert.ToInt16(hostList[hdbList.IndexOf(dbServer)][2]);
            sb.UserId = hostList[hdbList.IndexOf(dbServer)][3];
            sb.Password = hostList[hdbList.IndexOf(dbServer)][4];
            UniConnection dbConx = new UniConnection();
            dbConx.ConnectionString = "Provider=Oracle;" + sb.ConnectionString;
            dbConx.Open();

            if (jrDebug)
            { Console.WriteLine("Success!"); }
            return dbConx;
        }


        /// <summary>
        /// Disconnects HDB
        /// </summary>
        /// <param name="conx"></param>
        private static void DisconnectHDB(UniConnection conx)
        { 
            conx.Dispose(); 
        }
        

        /// <summary>
        /// Entry point for data download and processing. 
        /// Goes to HDB, gets data and info, and generates simple text output
        /// </summary>
        /// <param name="srchStr"></param>
        /// <returns></returns>
        public static List<string> hdbWebQuery(string srchStr)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Connect to HDB Server
            UniConnection hDB;
            Match svrStrMatch = Regex.Match(srchStr, @"svr=([A-Za-z0-9]+)&");
            if (svrStrMatch.Success && hdbList.Contains(svrStrMatch.Groups[1].Value.ToString().ToLower()))
            {
                string svrStr = svrStrMatch.Groups[1].Value.ToString().ToLower();
                hDB = ConnectHDB(svrStr);
            }
            else
            {
                Console.WriteLine("Error: Invalid HDB name.");
                return new List<string> { };
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Build date ranges for series lookup

            // Define HDB table time-step.
            Match outTstep = Regex.Match(srchStr, @"&tstp=([A-Za-z]+)&");
            string sourceTstep = "";
            if (outTstep.Groups[1].Value.ToString().ToLower() == "in")
            { sourceTstep = "INSTANT"; }
            else if (outTstep.Groups[1].Value.ToString().ToLower() == "hr")
            { sourceTstep = "HOUR"; }
            else if (outTstep.Groups[1].Value.ToString().ToLower() == "dy")
            { sourceTstep = "DAY"; }
            else if (outTstep.Groups[1].Value.ToString().ToLower() == "mn")
            { sourceTstep = "MONTH"; }
            else if (outTstep.Groups[1].Value.ToString().ToLower() == "yr")
            { sourceTstep = "YEAR"; }
            else if (outTstep.Groups[1].Value.ToString().ToLower() == "wy")
            { sourceTstep = "WY"; }
            else
            {
                Console.WriteLine("Error: Invalid Query Time-Step.");
                return new List<string> { };
            }

            Match sYrStr = Regex.Match(srchStr, @"&syer=([0-9\-]+)&");
            Match sMnStr = Regex.Match(srchStr, @"&smon=([0-9\-]+)&");
            Match sDyStr = Regex.Match(srchStr, @"&sday=([0-9\-]+)&");
            Match eYrStr = Regex.Match(srchStr, @"&eyer=([0-9\-]+)&");
            Match eMnStr = Regex.Match(srchStr, @"&emon=([0-9\-]+)&");
            Match eDyStr = Regex.Match(srchStr, @"&eday=([0-9\-]+)&");
            Match t1Str = Regex.Match(srchStr, @"&t1=([0-9\0-9-]+)&");
            Match t2Str = Regex.Match(srchStr, @"&t2=([0-9\0-9-]+)&");
            
            DateTime t1 = new DateTime();
            DateTime t2 = new DateTime();
          
            // Search string has old DateTime patterns
            if (sYrStr.Success && sMnStr.Success && sDyStr.Success && eYrStr.Success && eMnStr.Success && eDyStr.Success)
            {
                t1 = new DateTime(Convert.ToInt16(sYrStr.Groups[1].Value), Convert.ToInt16(sMnStr.Groups[1].Value),
                    Convert.ToInt16(sDyStr.Groups[1].Value));
                t2 = new DateTime(Convert.ToInt16(eYrStr.Groups[1].Value), Convert.ToInt16(eMnStr.Groups[1].Value),
                        Convert.ToInt16(eDyStr.Groups[1].Value));
            }
            // Search string has new DateTime patterns
            else if (t1Str.Success)
            {
                if (t2Str.Success)
                {
                    t1 = DateTime.Parse(t1Str.Groups[1].Value);
                    t2 = DateTime.Parse(t2Str.Groups[1].Value);
                }
                else
                {
                    switch (sourceTstep)
                    {
                        case "DAY":
                            t1 = DateTime.Now.Date.AddDays(Convert.ToInt16(t1Str.Groups[1].Value));
                            t2 = DateTime.Now.Date;
                            break;
                        case "MONTH":
                            t1 = new DateTime(DateTime.Now.AddMonths(Convert.ToInt16(t1Str.Groups[1].Value)).Year,
                                DateTime.Now.AddMonths(Convert.ToInt16(t1Str.Groups[1].Value)).Month,
                                1);
                            t2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                            break;
                        case "YEAR":
                            t1 = new DateTime(DateTime.Now.AddYears(Convert.ToInt16(t1Str.Groups[1].Value)).Year, 1, 1);
                            t2 = new DateTime(DateTime.Now.Year, 1, 1);
                            break;
                        default:
                            //case "INSTANT":
                            //case "HOUR":
                            t1 = new DateTime(DateTime.Now.AddHours(Convert.ToInt16(t1Str.Groups[1].Value)).Year,
                                DateTime.Now.AddHours(Convert.ToInt16(t1Str.Groups[1].Value)).Month,
                                DateTime.Now.AddHours(Convert.ToInt16(t1Str.Groups[1].Value)).Day,
                                DateTime.Now.AddHours(Convert.ToInt16(t1Str.Groups[1].Value)).Hour, 0, 0);
                            t2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                            break;
                    }                    
                }
            }
            else
            {
                Console.WriteLine("Error: Invalid Query Dates.");
                return new List<string> { };
            }
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get SDIs and query information for HDB lookup
            string outFormat = Regex.Match(srchStr, @"&format=([A-Za-z0-9]+)").Groups[1].Value.ToString();
            // [OPTIONAL INPUT] Define HDB table source. Default to the R-tables if not defined in input search string
            string sourceTable = "";
            string mridString = "";
            Match outSourceTable = Regex.Match(srchStr, @"&table=([A-Za-z])");
            if (!outSourceTable.Success)
            { 
                sourceTable = "R";
                mridString = null;
            }
            else
            { 
                sourceTable = outSourceTable.Groups[1].Value.ToString();
                if (sourceTable == "M")
                { mridString = Regex.Match(srchStr, @"&mrid=([\s\S]*?)&").Groups[1].Value.ToString(); }
            }
            // Get SDIs and check for duplicates
            var sdiString = Regex.Match(srchStr, @"sdi=([\s\S]*?)&").Groups[1].Value.ToString();
            List<string> sdiList = new List<string>();
            sdiList.AddRange(sdiString.Split(','));
            sdiList = sdiList.Distinct().ToList();
            List<string> invalidSdiList = new List<string>();
            invalidSdiList = sdiList.Where(w => w.Any(c => !Char.IsDigit(c))).ToList();
            sdiString = "";
            if (invalidSdiList.Count() > 0 && sourceTable == "M") //no sdi passed in so get a list of sdis
            {
                sdiString = getUniqueSdisFromMTable(hDB, mridString, sourceTstep); 
            }
            else if (invalidSdiList.Count() > 0) //querying r tables with no sdi? nope!
            {
                Console.WriteLine("Error: Invalid SDI in query.");
                return new List<string> { };
            }
            else //build list of sdis
            {
                foreach (var item in sdiList)
                { sdiString = sdiString + item + ","; }                
            }
            sdiString = sdiString.Remove(sdiString.Count() - 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get HDB data
            // Main data query. Uses Stored HDB Procedure "GET_HDB_CGI_DATA" & "GET_HDB_CGI_INFO"
            var downloadTable = queryHdbDataUsingStoredProcedure(hDB, sdiString, sourceTstep,
                t1.Day.ToString() + "-" + t1.ToString("MMM") + "-" + t1.Year.ToString(),
                t2.Day.ToString() + "-" + t2.ToString("MMM") + "-" + t2.Year.ToString(),
                sourceTable, mridString);
            // SDI info query
            var sdiInfo = queryHdbInfo(hDB, sdiString);
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Disconnect from HDB
            DisconnectHDB(hDB);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Generate output
            var outFile = new List<string>();
            if (outFormat == "json")
            {
                var jsonOut = HDB_CGI.reporting.buildOutputJson(sdiInfo, downloadTable, t1, t2, sourceTstep, sourceTable, mridString);
                outFile.Add(JsonConvert.SerializeObject(jsonOut));
            }
            else
            {
                outFile = HDB_CGI.reporting.buildOutputText(sdiInfo, downloadTable, srchStr, outFormat);
            }
            return outFile;
        }


        /// <summary>
        /// Gets Oracle DB data using the GET_HDB_CGI_DATA stored procedure and returns a DataTable with a common date range and sdi columns
        /// </summary>
        /// <param name="conx"></param>
        /// <param name="sdiList"></param>
        /// <param name="runIDs"></param>
        /// <returns></returns>
        private static DataTable queryHdbDataUsingStoredProcedure(UniConnection conx, string sdiList, string tStep, 
            string startDate, string endDate, string sourceTable = "R", string modelRunIds = null)
        {
            // Initialize stuff...
            var dTab = new DataTable();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Connect to and get HDB data
            if (jrDebug)
            { Console.Write("Downloading data... "); }
            UniCommand cmd = new UniCommand("GET_HDB_CGI_DATA", conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("o_cursorOutput", UniDbType.Cursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("i_sdiList", UniDbType.VarChar).Value = sdiList;
            cmd.Parameters.Add("i_tStep", UniDbType.Char).Value = tStep;
            cmd.Parameters.Add("i_startDate", UniDbType.Char).Value = startDate;
            cmd.Parameters.Add("i_endDate", UniDbType.Char).Value = endDate;
            cmd.Parameters.Add("i_sourceTable", UniDbType.Char).Value = sourceTable;
            cmd.Parameters.Add("i_modelRunIds", UniDbType.Char).Value = modelRunIds;
            UniDataReader dr = cmd.ExecuteReader();
            var schemaTable = dr.GetSchemaTable();
            
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Put DB data into a .NET DataTable
            dTab.Load(dr);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Return output
            if (jrDebug)
            { Console.WriteLine("Success!"); }
            dr.Dispose();
            cmd.Dispose();
            return dTab;
        }


        /// <summary>
        /// Gets SDI info from HDB and returns a DataTable
        /// </summary>
        /// <param name="conx"></param>
        /// <param name="sdiString"></param>
        /// <returns></returns>
        private static DataTable queryHdbInfo(UniConnection conx, string sdiString)
        {
            // Initialize stuff...
            var dTab = new DataTable();
            List<string> sdiList = new List<string>(sdiString.Split(','));

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Connect to and get HDB data
            if (jrDebug)
            { Console.Write("Downloading sdi info... "); }
            UniCommand cmd = new UniCommand("GET_HDB_CGI_INFO", conx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("o_cursorOutput", UniDbType.Cursor).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("i_sdiList", UniDbType.Char).Value = sdiString;
            UniDataReader dr = cmd.ExecuteReader();
            var schemaTable = dr.GetSchemaTable();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Put DB data into a .NET DataTable
            dTab.Load(dr);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Return output
            if (jrDebug)
            { Console.WriteLine("Success!"); }
            dr.Dispose();
            cmd.Dispose();
            return dTab;
        }


        /// <summary>
        /// Gets unique SDIs given a particular MRID and M-Table interval
        /// </summary>
        /// <param name="conx"></param>
        /// <param name="mridString"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static string getUniqueSdisFromMTable(UniConnection conx, string mridString, string interval)
        {
            // Initialize stuff...
            string sdiString = "";
            string sql = "SELECT UNIQUE(SITE_DATATYPE_ID) FROM M_" + interval + " WHERE MODEL_RUN_ID IN (" + mridString + ")";

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Connect to and get HDB data
            if (jrDebug)
            { Console.Write("Getting Unique SDIs... "); }
            UniCommand cmd = new UniCommand(sql, conx);
            cmd.CommandType = System.Data.CommandType.Text;
            UniDataReader dr = cmd.ExecuteReader();
            var schemaTable = dr.GetSchemaTable();

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Build a string of SDIS with a comma delimiter
            while (dr.Read())
            { 
                sdiString = sdiString + dr[0].ToString() + ",";
            }
            dr.Dispose();
            cmd.Dispose();
            if (jrDebug)
            { Console.WriteLine("Success!"); }
            
            return sdiString;
        }

        
        /// <summary>
        /// Passes an sql query to a specified HDB
        /// </summary>
        /// <param name="hdbString"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        public static DataTable getDataUsingSQL(string hdbString, string sqlQuery)
        {
            var dTab = new DataTable();

            var conx = ConnectHDB(hdbString);
            UniCommand cmd = new UniCommand(sqlQuery, conx);
            cmd.CommandType = System.Data.CommandType.Text;
            UniDataReader dr = cmd.ExecuteReader();
            var schemaTable = dr.GetSchemaTable();            

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Put DB data into a .NET DataTable
            dTab.Load(dr);

            DisconnectHDB(conx);
            return dTab;
        }

        #endregion

    }  
} 

