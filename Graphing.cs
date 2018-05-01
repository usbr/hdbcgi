using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HDB_CGI
{
    class graphing
    {
        //private static bool dygraphsUrlData = true;

        /// <summary>
        /// Writes a HTML tagged C# List for dyGraphs output
        /// </summary>
        /// <param name="outFile"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static List<string> writeHTML_dyGraphs(string[] outFile, string query, bool dygraphsUrlData = true)
        {
            // The data in the outFile has to be preceded by a line that says "BEGIN DATA"
            //      and followed by a line that says "END DATA"
            // The series info has to be in outFile[12]

            List<string> htmlOut = new List<string>();

            // Populate chart HTML requirements
            #region
            htmlOut.Add(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">");
            htmlOut.Add("<html>");
            htmlOut.Add("<head>");
            htmlOut.Add(@"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">");
            htmlOut.Add("<title>HDB CGI Data Query Graph</title>");
            htmlOut.Add("<!-- Call DyGraphs JavaScript Reference -->");
            htmlOut.Add(@"<script type=""text/javascript""  src=""dygraph.min.js""></script>");
            htmlOut.Add(@"<link rel=""stylesheet"" href=""dygraph.css"">");
            htmlOut.Add(@"<style type=""text/css"">");
            htmlOut.Add("#graphdiv {position: absolute; left: 50px; right: 50px; top: 75px; bottom: 50px;}");
            htmlOut.Add("#graphdiv .dygraph-legend {width: 300px !important; background-color: transparent !important; left: 75px !important;}");
            htmlOut.Add("</style></head>");
            htmlOut.Add("<body>");
            htmlOut.Add("<!-- Place DyGraphs Chart -->");
            htmlOut.Add(@"<div id=""status"" style=""width:1000px; font-size:0.8em; padding-top:5px;""></div>");
            htmlOut.Add("<br>");
            htmlOut.Add(@"<div id=""graphdiv""></div>");
            htmlOut.Add("");
            htmlOut.Add("<!-- Build DyGraphs Chart -->");
            htmlOut.Add(@"<script type=""text/javascript"">");
            htmlOut.Add("g = new Dygraph(");
            htmlOut.Add(@"document.getElementById(""graphdiv""),");
            htmlOut.Add("");
            #endregion

            // Populate html data
            #region
            int startOfDataRow = Array.IndexOf(outFile, "BEGIN DATA") + 2;
            int endOfDataRow = Array.IndexOf(outFile, "END DATA") - 1;
            string headerString = @"""Date,";
            var units = new List<string>();
            for (int i = 12; i < Array.IndexOf(outFile, "BEGIN DATA"); i++)
            {
                headerString = headerString + outFile[i].Replace(",", " ") + ",";
                var unit = Regex.Matches(outFile[i], @"(in ).*");
                units.Add(unit[0].ToString().Replace("in ", ""));
            }
            if (!dygraphsUrlData)
            {
                htmlOut.Add(headerString.Remove(headerString.Length - 1) + @"\n "" +");
                // POPULATE DATA
                for (int i = startOfDataRow; i < endOfDataRow; i++)
                {
                    var val = outFile[i].Split(',');
                    var t = DateTime.Parse(outFile[i].Split(',')[0].ToString()).ToString("yyyy-MM-dd HH:mm");
                    string dataRow = "$" + t + ", ";
                    for (int j = 1; j < val.Count(); j++)
                    {
                        var jthVal = val[j].ToString().Trim();
                        if (jthVal == double.NaN.ToString())
                        { jthVal = "NaN"; }
                        dataRow = dataRow + jthVal + ", ";
                    }
                    if (i + 1 == endOfDataRow)
                    { htmlOut.Add((dataRow.Remove(dataRow.Length - 2) + @"\n$").Replace('$', '"')); }
                    else
                    { htmlOut.Add((dataRow.Remove(dataRow.Length - 2) + @"\n$ +").Replace('$', '"')); }
                }
            }
            else
            {
                //string query = @"http://ibr3lcrxcn01.bor.doi.net:8080/HDB_CGI.com?sdi=1930,1863&tstp=HR&syer=2015&smon=1&sday=1&eyer=2015&emon=1&eday=10&format=88";
                var tempQuery = query;
                tempQuery = tempQuery.ToLower().Replace("format=9", "format=88");
                tempQuery = tempQuery.ToLower().Replace("format=graph", "format=88");
                htmlOut.Add("'" + tempQuery + "'");
            }
            #endregion

            // Populate chart HTML requirements
            #region
            htmlOut.Add(", {fillGraph: true, showRangeSelector: true, legend: 'always'");
            //htmlOut.Add(", rangeSelectorPlotStrokeColor: '#0000ff', rangeSelectorPlotFillColor: '#0000ff'");
            htmlOut.Add(", xlabel: 'Date', ylabel: '" + units[0] + "', labelsSeparateLines: true");
            htmlOut.Add(", labelsDiv: document.getElementById('status'), axisLabelWidth: 75");
            htmlOut.Add(", highlightCircleSize: 5, pointSize: 1.5, strokeWidth: 1.5");
            if (units.Distinct().Count() > 1)
            { htmlOut.Add(", y2label: '" + units[1] + "', '" + headerString.Split(',')[2] + "' : { axis : { } } }"); }
            else
            { htmlOut.Add("}"); }
            htmlOut.Add(");");
            htmlOut.Add("");
            htmlOut.Add("</script>");
            htmlOut.Add("</body>");
            htmlOut.Add("</html>");
            #endregion

            return htmlOut;
        }


        /// <summary>
        /// Chart views of USBR LC Reservoirs for the last 7 days
        /// </summary>
        /// <returns></returns>
        public static List<string> dashboard_LcReservoirs()
        {
            var htmlOut = new List<string>();
            DateTime t2 = DateTime.Now.AddDays(0);
            DateTime t1 = t2.AddDays(-7);
            var t1Year = t1.Year;
            var t1Mon = t1.Month;
            var t1Day = t1.Day;
            var t2Year = t2.Year;
            var t2Mon = t2.Month;
            var t2Day = t2.Day;


            // Populate chart HTML requirements
            #region
            htmlOut.Add(@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01//EN"" ""http://www.w3.org/TR/html4/strict.dtd"">");
            htmlOut.Add("<html>");
            htmlOut.Add("<head>");
            htmlOut.Add(@"<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">");
            htmlOut.Add("<title>HDB CGI Data Query Graph</title>");
            htmlOut.Add("<!-- Call DyGraphs JavaScript Reference -->");
            htmlOut.Add(@"<script type=""text/javascript""  src=""dygraphs/dygraph-combined.js""></script>");
            htmlOut.Add("</head>");
            htmlOut.Add("<body>");
            htmlOut.Add(@"<style>");
            htmlOut.Add(@".chart { width: 700px; height: 400px; }");
            htmlOut.Add(@".chart-container { overflow: hidden; }");
            htmlOut.Add(@"#hooverDiv { float: left; }");
            htmlOut.Add(@"#davisDiv { float: left; }");
            htmlOut.Add(@"#parkerDiv { float: left; clear: left; }");
            htmlOut.Add(@"</style>");
            htmlOut.Add("<!-- Place DyGraphs Chart -->");
            htmlOut.Add(@"<p><h2>Hourly Data for the last 7 days are shown below</h2></p>");
            htmlOut.Add(@"<h3><font color=""#3CB371"">Water Level Elevation (Left Axis) </font><font color=""#B0C4DE""> Outflow (Right Axis)</font></h3>");
            htmlOut.Add(@"<br>");
            htmlOut.Add(@"<div class=""chart-container"">");
            htmlOut.Add(@"<div id=""hooverDiv"" class=""chart"">Hoover Dam - Lake Mead</div>");
            htmlOut.Add(@"<div id=""davisDiv"" class=""chart"">Davis Dam - Lake Mohave</div>");
            htmlOut.Add(@"<div id=""parkerDiv"" class=""chart"">Parker Dam - Lake Havasu</div>");
            htmlOut.Add(@"");
            htmlOut.Add("");
            #endregion

            htmlOut.Add("<!-- Build DyGraphs Chart -->");
            htmlOut.Add(@"<script type=""text/javascript"">");
            
            // HOOVER
            htmlOut.Add("g1 = new Dygraph(");
            htmlOut.Add(@"document.getElementById(""hooverDiv""),");
            htmlOut.Add("");
            string query = @"http://ibr3lcrsrv01.bor.doi.net:8080/HDB_CGI.com?svr=lchdb2&sdi=1930,1863&tstp=HR" +
                "&syer=" + t1Year + "&smon=" + t1Mon + "&sday=" + t1Day +
                "&eyer=" + t2Year + "&emon=" + t2Mon + "&eday=" + t2Day + "&format=88";
            htmlOut.Add("'" + query + "'");
            htmlOut.Add(", {fillGraph: true, showRangeSelector: true, legend: 'never'");
            htmlOut.Add(", xlabel: 'Date', ylabel: 'FEET', labelsSeparateLines: true");
            htmlOut.Add(", labelsDiv: document.getElementById('status'), axisLabelWidth: 75");
            htmlOut.Add(", y2label: 'CFS', 'SDI 1863: LAKE MEAD - AVERAGE POWER RELEASE in CFS' : { axis : { } } ");
            htmlOut.Add(", title: 'Hoover Dam - Lake Mead'});");
            
            // DAVIS
            htmlOut.Add("g2 = new Dygraph(");
            htmlOut.Add(@"document.getElementById(""davisDiv""),");
            htmlOut.Add("");
            query = @"http://ibr3lcrsrv01.bor.doi.net:8080/HDB_CGI.com?svr=lchdb2&sdi=2100,2166&tstp=HR" +
                "&syer=" + t1Year + "&smon=" + t1Mon + "&sday=" + t1Day +
                "&eyer=" + t2Year + "&emon=" + t2Mon + "&eday=" + t2Day + "&format=88";
            htmlOut.Add("'" + query + "'");
            htmlOut.Add(", {fillGraph: true, showRangeSelector: true, legend: 'never'");
            htmlOut.Add(", xlabel: 'Date', ylabel: 'FEET', labelsSeparateLines: true");
            htmlOut.Add(", labelsDiv: document.getElementById('status'), axisLabelWidth: 75");
            htmlOut.Add(", y2label: 'CFS', 'SDI 2166: LAKE MOHAVE - AVERAGE POWER RELEASE in CFS' : { axis : { } } ");
            htmlOut.Add(", title: 'Davis Dam - Lake Mohave'});");

            // Parker
            htmlOut.Add("g3 = new Dygraph(");
            htmlOut.Add(@"document.getElementById(""parkerDiv""),");
            htmlOut.Add("");
            query = @"http://ibr3lcrsrv01.bor.doi.net:8080/HDB_CGI.com?svr=lchdb2&sdi=2101,2146&tstp=HR" +
                "&syer=" + t1Year + "&smon=" + t1Mon + "&sday=" + t1Day +
                "&eyer=" + t2Year + "&emon=" + t2Mon + "&eday=" + t2Day + "&format=88";
            htmlOut.Add("'" + query + "'");
            htmlOut.Add(", {fillGraph: true, showRangeSelector: true, legend: 'never'");
            htmlOut.Add(", xlabel: 'Date', ylabel: 'FEET', labelsSeparateLines: true");
            htmlOut.Add(", labelsDiv: document.getElementById('status'), axisLabelWidth: 75");
            htmlOut.Add(", y2label: 'CFS', 'SDI 2146: LAKE HAVASU - AVERAGE POWER RELEASE in CFS' : { axis : { } } ");
            htmlOut.Add(", title: 'Parker Dam - Lake Havasu'});");

            htmlOut.Add("");
            htmlOut.Add("</script>");
            htmlOut.Add("</body>");
            htmlOut.Add("</html>");

            return htmlOut;
        }
    }
}
