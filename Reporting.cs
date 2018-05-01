using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HDB_CGI
{
    class reporting
    {
        /// <summary>
        /// Builds a custom text output using queried data
        /// </summary>
        /// <param name="sdiInfo"></param>
        /// <param name="downloadTable"></param>
        /// <param name="srchStr"></param>
        /// <param name="outFormat"></param>
        /// <returns></returns>
        public static List<string> buildOutputText(DataTable sdiInfo, DataTable downloadTable,
            string srchStr, string outFormat)
        {
            var outText = new List<string>();

            // Generate header
            var txt = new List<string>();
            txt.Add("USBR Hydrologic Database (HDB) System Data Access");
            txt.Add(" ");
            txt.Add("The Bureau of Reclamation makes efforts to maintain the accuracy of data found ");
            txt.Add("in the HDB system databases but the data is largely unverified and should be ");
            txt.Add("considered preliminary and subject to change.  Data and services are provided ");
            txt.Add("with the express understanding that the United States Government makes no ");
            txt.Add("warranties, expressed or implied, concerning the accuracy, completeness, ");
            txt.Add("usability, or suitability for any particular purpose of the information or data ");
            txt.Add("obtained by access to this computer system. The United States shall be under no ");
            txt.Add("liability whatsoever to any individual or group entity by reason of any use made ");
            txt.Add("thereof. ");
            txt.Add(" ");
            for (int i = 0; i < sdiInfo.Rows.Count; i++)
            {
                txt.Add("SDI " + sdiInfo.Rows[i][0] + ": " + sdiInfo.Rows[i][1].ToString().ToUpper() + " - " +
                    sdiInfo.Rows[i][2].ToString().ToUpper() + " in " + sdiInfo.Rows[i][3].ToString().ToUpper());
            }
            txt.Add("BEGIN DATA");
            string headLine = "";
            for (int i = 0; i < downloadTable.Columns.Count; i++)
            {
                if (i == 0)
                { headLine = headLine + "DATETIME".PadLeft(16) + ", "; }
                else
                { headLine = headLine + downloadTable.Columns[i].ColumnName.PadLeft(12) + ", "; }
            }
            txt.Add(headLine.Remove(headLine.Length - 2));
            // Generate body
            foreach (DataRow row in downloadTable.Rows)
            {
                string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                string newRow = "";
                for (int i = 0; i < fields.Count(); i++)
                {
                    if (i == 0)
                    { newRow = newRow + DateTime.Parse(fields[i]).ToString("MM/dd/yyyy HH:mm") + ", "; }
                    else
                    {
                        string fieldVal = fields[i];
                        if (fieldVal == "")
                        { fieldVal = "NaN"; }
                        newRow = newRow + fieldVal.PadLeft(12) + ", ";
                    }
                }
                txt.Add(newRow.Remove(newRow.Length - 2));
                //txt.Add(string.Join(",", fields));
            }
            txt.Add("END DATA");
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Return output
            string[] txtArray = txt.ToArray();
            List<string> outFile = writeHTML(txtArray, outFormat, srchStr);
            return outFile;
        }


        /// <summary>
        /// Builds a JSON array using queried data
        /// </summary>
        /// <param name="sdiInfo"></param>
        /// <param name="downloadTable"></param>
        /// <param name="srchStr"></param>
        /// <param name="outFormat"></param>
        /// <returns></returns>
        public static HDB_CGI_JSON.HdbCgiJson buildOutputJson(DataTable sdiInfo, DataTable downloadTable,
             DateTime t1, DateTime t2, string sourceTstep, string sourceTable, string mridString)
        {
            var outText = new List<string>();

            // Generate header
            var txt = new List<string>();

            // Populate top level JSON
            var jsonOut = new HDB_CGI_JSON.HdbCgiJson();
            jsonOut.QueryDate = DateTime.Now.ToString("G");
            jsonOut.StartDate = t1.ToString("G");
            jsonOut.EndDate = t2.ToString("G");
            jsonOut.TimeStep = sourceTstep;
            if (sourceTable.ToLower() == "m") { jsonOut.DataSource = "Modeled"; }
            else { jsonOut.DataSource = "Observed"; }

            // Resolve MRIDs
            var mridList = new List<string>();
            if (mridString == "" || mridString == null) { mridList.Add(null); }
            else { mridList.AddRange(mridString.Split(',').ToArray<string>()); }

            // Initialize JSON SDI/Site container
            var jsonSites = new List<HDB_CGI_JSON.Sites>();

            // Build JSON SDI/Site Objects
            foreach (DataRow series in sdiInfo.Rows)//loop through each SDI
            {
                foreach (var mrid in mridList)//loop through each mrid
                {
                    // Populate SDI/Site level metadata
                    var jsonSite = new HDB_CGI_JSON.Sites();
                    jsonSite.SDI = series[0].ToString();
                    jsonSite.MRID = mrid;
                    jsonSite.SiteName = series[1].ToString();
                    jsonSite.DataTypeName = series[2].ToString();
                    jsonSite.DataTypeUnit = series[3].ToString();
                    jsonSite.Latitude = series[4].ToString();
                    jsonSite.Longitude = series[5].ToString();
                    jsonSite.Elevation = series[6].ToString();
                    jsonSite.DB = series[7].ToString();

                    // Select TS data for the SDI - MRID
                    DataView view = new DataView(downloadTable);
                    DataTable dtQueryTable;
                    DataRow[] rows;
                    if (mrid == null)
                    {
                        dtQueryTable = view.ToTable(false, new string[] { "HDB_DATETIME", "SDI_" + jsonSite.SDI });
                        rows = dtQueryTable.Select();
                    }
                    else
                    {
                        dtQueryTable = view.ToTable(false, new string[] { "HDB_DATETIME", "MODEL_RUN_ID", "SDI_" + jsonSite.SDI });
                        rows = dtQueryTable.Select("MODEL_RUN_ID = '" + mrid + "'");
                    }

                    // Build the TS Data JSON container
                    var jsonSiteData = new List<HDB_CGI_JSON.Data>();
                    foreach (DataRow row in rows)
                    {
                        var jsonSiteDataPoint = new HDB_CGI_JSON.Data();
                        jsonSiteDataPoint.t = DateTime.Parse(row["HDB_DATETIME"].ToString()).ToString("G");
                        jsonSiteDataPoint.v  = row["SDI_" + jsonSite.SDI].ToString();
                        jsonSiteData.Add(jsonSiteDataPoint);
                    }

                    // Add Site Data to JSON Site Object
                    jsonSite.Data = jsonSiteData;

                    // Add JSON Site Object to JSON Sites Container
                    jsonSites.Add(jsonSite);
                }
            }

            // Add JSON Site Container Array to JSON out
            jsonOut.Series = jsonSites;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Return output
            return jsonOut;
        }


        /// <summary>
        /// Writes a HTML tagged C# List for output
        /// </summary>
        /// <param name="outFile"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static List<string> writeHTML(string[] outFile, string outFormat, string srchStr)
        {
            /* FORMAT
                 * 1 - CSV with preamble and headers
                 * 2 - Table with preamble and headers
                 * 3 - CSV no preamble with headers
                 * 4 - Table no preamble with headers
                 * 5 - 
                 * 6 - 
                 * 7 - 
                 * 8 - Pisces HdbWebSeries Query
                 * 9 - DyGraphs in Graphing.cs 
                 * 88 - Pure CSV for DyGraph generation
                 * 99 - AM Chart in Graphing.cs
                 * */

            var htmlOut = new List<string>();
            //string format = "3";
            bool isCSV = false, hasPreamble = false, isAmChart = false, isDyGraph = false, isHdbWebSeriesQuery = false, isJson = false;
            if (outFormat == "1" || outFormat == "3" || outFormat == "5")
            { isCSV = true; }
            if (outFormat == "1" || outFormat == "2")
            { hasPreamble = true; }
            if (outFormat == "8" || outFormat == "88")
            { isHdbWebSeriesQuery = true; }
            if (outFormat == "9" || outFormat.ToLower() == "graph")
            { isDyGraph = true; }
            if (outFormat == "99")
            { isAmChart = true; }
            if (outFormat.ToLower() == "csv")
            {
                hasPreamble = false;
                isCSV = true;
            }
            if (outFormat.ToLower() == "html")
            {
                hasPreamble = false;
            }
            if (outFormat.ToLower() == "json")
            {
                isJson = true;
            }

            int startOfDataRow = Array.IndexOf(outFile, "BEGIN DATA");

            // format == 99 or format == graph
            if (isAmChart || isDyGraph)
            { htmlOut = HDB_CGI.graphing.writeHTML_dyGraphs(outFile, srchStr); }
            // format == 8
            else if (isHdbWebSeriesQuery)
            {
                if (outFormat == "8")
                { 
                    htmlOut.Add("<PRE>");
                    for (int i = startOfDataRow + 2; i < outFile.Count() - 1; i++)
                    { htmlOut.Add(outFile[i] + "\r\n"); }
                }
                else
                {
                    var headerString = "Date,";
                    for (int i = 12; i < Array.IndexOf(outFile, "BEGIN DATA"); i++)
                    { headerString = headerString + outFile[i].Replace(",", " ") + ","; }
                    htmlOut.Add(headerString.Remove(headerString.Length - 1) + "\n");
                    for (int i = startOfDataRow + 2; i < outFile.Count() - 1; i++)
                    { htmlOut.Add(outFile[i] + "\n"); }
                }
                return htmlOut;
            }
            // format == json
            else if (isJson)
            {
                //[JR] build JSON output
            }
            // format == 1, 2, 3, 4
            else
            {
                htmlOut.Add("<HTML>");
                htmlOut.Add("<HEAD>");
                htmlOut.Add("<TITLE>Bureau of Reclamation HDB Data</TITLE>");
                htmlOut.Add("</HEAD>");
                htmlOut.Add("<BODY>");

                // Add preamble
                if (hasPreamble)
                {
                    htmlOut.Add("<PRE>");

                    htmlOut.Add("<B>" + outFile[0] + "</B>");
                    for (int i = 1; i <= startOfDataRow - 1; i++)
                    {
                        if (!HDB_CGI.cgi.jrDebug)
                        { htmlOut.Add("<BR>" + outFile[i]); }
                        else
                        { htmlOut.Add(outFile[i]); }
                    }
                    htmlOut.Add("</PRE>");
                    htmlOut.Add("<p>");
                }
                // Add data
                for (int i = startOfDataRow; i < outFile.Count(); i++)
                {
                    if (isCSV && hasPreamble)
                    {
                        if (i == startOfDataRow)
                        { htmlOut.Add("<PRE>"); }
                        htmlOut.Add("<BR>" + outFile[i]);
                    }
                    else if (isCSV && !hasPreamble)
                    {
                        if (i == startOfDataRow)
                        {
                            htmlOut.Add("<PRE>");
                            i++;
                            htmlOut.Add(outFile[i]);
                        }
                        else if (i == outFile.Count() - 1)
                        { }
                        else
                        { htmlOut.Add("<BR>" + outFile[i]); }
                    }
                    else
                    {
                        if (i == startOfDataRow)
                        {
                            htmlOut.Add("<TABLE BORDER=1>");
                            i++;
                            htmlOut.Add("<TR><TH>" + outFile[i].Replace(",", "</TH><TH>") + "</TH></TR>");
                        }
                        else if (i == outFile.Count() - 1)
                        { }
                        else
                        { htmlOut.Add("<TR><TD>" + outFile[i].Replace(",", "</TD><TD>") + "</TD></TR>"); }
                    }
                }
                // Add final lines
                if (isCSV)
                { htmlOut.Add("</PRE>"); }
                else
                { htmlOut.Add("</TABLE>"); }
                htmlOut.Add("</BODY></HTML>");
            }
            // Output
            return htmlOut;
        }

    }
}

namespace HDB_CGI_JSON
{

    public class HdbCgiJson
    {
        public string QueryDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TimeStep { get; set; }
        public string DataSource { get; set; }
        public List<Sites> Series { get; set; }
    }

    public class Sites
    {
        public string SDI { get; set; }
        public string SiteName { get; set; }
        public string DataTypeName { get; set; }
        public string DataTypeUnit { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Elevation { get; set; }
        public string DB { get; set; }
        public string MRID { get; set; }
        public List<Data> Data { get; set; }
    }

    public class Data
    {
        public string t { get; set; }
        public string v { get; set; }
    }

}