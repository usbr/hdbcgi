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
        /// Writes a HTML tagged C# List for amCharts output
        /// </summary>
        /// <param name="outFile"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static List<string> writeHTML_amCharts(string[] outFile)
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
            htmlOut.Add(@"<link rel=""stylesheet"" href=""style.css"" type=""text/css"">");
            htmlOut.Add(@"<script src=""amcharts/amcharts.js"" type=""text/javascript""></script>");
            htmlOut.Add(@"<script src=""amcharts/serial.js"" type=""text/javascript""></script>");
            htmlOut.Add(@"<script type=""text/javascript"">");
            htmlOut.Add("    ");
            htmlOut.Add("<!-- INITIALIZE AM CHART VARIABLES -->");
            htmlOut.Add("var chart;");
            htmlOut.Add("var chartData = [];");
            htmlOut.Add("var chartCursor;");
            htmlOut.Add("    ");
            htmlOut.Add("<!-- MAIN ENTRY POINT TO BUILD AM CHART -->");
            htmlOut.Add("AmCharts.ready(function () {");
            htmlOut.Add("    <!-- GENERATES CHART DATA FROM HDB-CGI -->");
            htmlOut.Add("    generateChartData();");
            htmlOut.Add("    ");
            htmlOut.Add("    <!-- INITIALIZE AM CHART SERIAL CHART -->");
            htmlOut.Add("    chart = new AmCharts.AmSerialChart();");
            htmlOut.Add(@"    chart.pathToImages = ""amcharts/images/"";");
            htmlOut.Add("    chart.dataProvider = chartData;");
            htmlOut.Add(@"    chart.categoryField = ""hdbDateTime"";");
            htmlOut.Add("    chart.balloon.bulletSize = 5;");
            htmlOut.Add("    ");
            htmlOut.Add("    <!-- LISTEN FOR dataUpdated EVENT AND CALL zoomChart -->");
            htmlOut.Add(@"    chart.addListener(""dataUpdated"", zoomChart);");
            htmlOut.Add("    ");
            // Define axis parameters
            // X-Axis
            htmlOut.Add("    <!-- DEFINE X-AXIS PARAMETERS -->");
            htmlOut.Add("    var categoryAxis = chart.categoryAxis;");
            htmlOut.Add("    categoryAxis.parseDates = true; ");// as our data is date-based, we set parseDates to true");
            htmlOut.Add(@"    categoryAxis.minPeriod = ""hh""; ");// our data is daily, so we set minPeriod to DD");
            htmlOut.Add(@"    chart.dataDateFormat = ""YYYY-MM-DD JJ"";");
            htmlOut.Add("    categoryAxis.dashLength = 1;");
            htmlOut.Add("    categoryAxis.minorGridEnabled = true;");
            htmlOut.Add("    categoryAxis.twoLineMode = true;");
            htmlOut.Add("    categoryAxis.dateFormats =");
            htmlOut.Add("    [{period:'fff',format:'JJ:NN:SS'},");
            htmlOut.Add("        {period:'ss',format:'JJ:NN:SS'},");
            htmlOut.Add("        {period:'mm',format:'JJ:NN'},");
            htmlOut.Add("        {period:'hh',format:'JJ:NN'},");
            htmlOut.Add("        {period:'DD',format:'MMM DD'},");
            htmlOut.Add("        {period:'WW',format:'MMM DD'},");
            htmlOut.Add("        {period:'MM',format:'MMM'},");
            htmlOut.Add("        {period:'YYYY',format:'YYYY'}];");
            htmlOut.Add(@"    categoryAxis.axisColor = ""#DADADA"";");
            htmlOut.Add("    ");
            // Y-Axis
            htmlOut.Add("    <!-- DEFINE Y-AXIS PARAMETERS -->");
            htmlOut.Add("    var valueAxis = new AmCharts.ValueAxis();");
            htmlOut.Add("    valueAxis.axisAlpha = 0;");
            htmlOut.Add("    valueAxis.dashLength = 1;");
            htmlOut.Add("    chart.addValueAxis(valueAxis);");
            htmlOut.Add("    ");
            // Define graph parameters
            htmlOut.Add("    <!-- DEFINE GRAPH PARAMETERS -->");
            htmlOut.Add("    var graph = new AmCharts.AmGraph();");
            htmlOut.Add(@"    graph.title = ""red line"";");
            htmlOut.Add(@"    graph.valueField = ""hdbData"";");
            htmlOut.Add(@"    graph.bullet = ""round"";");
            htmlOut.Add(@"    graph.bulletBorderColor = ""#FFFFFF"";");
            htmlOut.Add("    graph.bulletBorderThickness = 2;");
            htmlOut.Add("    graph.bulletBorderAlpha = 1;");
            htmlOut.Add("    graph.lineThickness = 2;");
            htmlOut.Add(@"    graph.lineColor = ""#0000ff"";");
            htmlOut.Add(@"    graph.negativeLineColor = ""#0000ff"";");
            htmlOut.Add("    graph.hideBulletsCount = 50; ");// this makes the chart to hide bullets when there are more than 50 series in selection");
            htmlOut.Add("    chart.addGraph(graph);");
            htmlOut.Add("    ");
            // Defines cursor parameters
            htmlOut.Add("    <!-- DEFINE CURSOR PARAMETERS -->");
            htmlOut.Add("    chartCursor = new AmCharts.ChartCursor();");
            htmlOut.Add(@"    chartCursor.cursorPosition = ""mouse"";");
            htmlOut.Add(@"    chartCursor.cursorColor = ""#daa520"";");
            htmlOut.Add(@"    chartCursor.pan = true; ");
            //htmlOut.Add("chartCursor.valueLineEnabled = true;");
            //htmlOut.Add("chartCursor.valueLineBalloonEnabled = true;");
            htmlOut.Add("    chart.addChartCursor(chartCursor);");
            htmlOut.Add("    ");
            // Defines scrollbar parameters
            htmlOut.Add("    <!-- DEFINE SCROLLBAR PARAMETERS -->");
            htmlOut.Add("    var chartScrollbar = new AmCharts.ChartScrollbar();");
            htmlOut.Add("    chart.addChartScrollbar(chartScrollbar);");
            htmlOut.Add("    chartScrollbar.Graph = graph;");
            htmlOut.Add("    chartScrollbar.scrollbarHeight = 30;");
            htmlOut.Add("    ");
            htmlOut.Add("    <!-- WRITE AM CHART -->");
            htmlOut.Add(@"    chart.creditsPosition = ""bottom-right"";");//delete this line once we have a license...
            htmlOut.Add(@"    chart.write(""chartdiv"");");
            htmlOut.Add("});");
            htmlOut.Add("");
            // Zoom chart function
            htmlOut.Add("<!-- THIS FUNCTION CALLED DURING CHART INITIATION -->");
            htmlOut.Add("function zoomChart() {");
            // different zoom methods can be used - zoomToIndexes, zoomToDates, zoomToCategoryValues");
            htmlOut.Add("    chart.zoomToIndexes(chartData.length - 40, chartData.length - 1);");
            htmlOut.Add("}");
            htmlOut.Add("");
            htmlOut.Add("<!-- CHANGES CURSOR FROM PAN TO SELECT -->");
            htmlOut.Add("function setPanSelect() {");
            htmlOut.Add(@"    if (document.getElementById(""rb1"").checked) {");
            htmlOut.Add("        chartCursor.pan = false;");
            htmlOut.Add("        chartCursor.zoomable = true;");
            htmlOut.Add("    } else {");
            htmlOut.Add("        chartCursor.pan = true;");
            htmlOut.Add("    }");
            htmlOut.Add("    chart.validateNow();");
            htmlOut.Add("}");
            htmlOut.Add("");
            htmlOut.Add("<!-- IMPORTS AND FORMATS DATA FROM THE HDB-CGI FOR PLOTTING -->");
            htmlOut.Add("function generateChartData() {");
            htmlOut.Add("    chartData = [");
            htmlOut.Add("        <!-- #DataStart -->");
            #endregion

            int startOfDataRow = Array.IndexOf(outFile, "BEGIN DATA") + 2;
            int endOfDataRow = Array.IndexOf(outFile, "END DATA") - 1;
            // POPULATE DATA
            for (int i = startOfDataRow; i < endOfDataRow; i++)
            {
                var val = outFile[i].Split(',')[1].Trim().ToString();
                if (val == double.NaN.ToString())
                { val = "NaN"; }
                htmlOut.Add(("        { hdbDateTime: AmCharts.stringToDate($" +
                    DateTime.Parse(outFile[i].Split(',')[0].ToString()).ToString("yyyy-MM-dd HH") +
                    "$, $YYYY-MM-DD JJ$), hdbData: " + val + " },").Replace('$', '"'));
            }

            // Populate more chart HTML requirements
            #region
            htmlOut.Add("        <!-- #DataEnd -->");
            htmlOut.Add("    ];");
            htmlOut.Add("}");
            htmlOut.Add("");
            htmlOut.Add("</script>");
            htmlOut.Add("</head>");
            htmlOut.Add("");
            htmlOut.Add("<body>");
            htmlOut.Add(@"<div id=""chartdiv"" style=""width: 100%; height: 400px;""></div>");
            htmlOut.Add(@"<div style=""margin-left:35px;"">");
            htmlOut.Add(@"<input type=""radio"" name=""group"" id=""rb1"" onclick=""setPanSelect()"">Select");
            htmlOut.Add(@"<input type=""radio"" checked=""true"" name=""group"" id=""rb2"" onclick=""setPanSelect()"">Pan");
            htmlOut.Add("</div>");
            htmlOut.Add("<br>");
            htmlOut.Add("<br>");
            htmlOut.Add("<!-- #InfoStart -->");
            htmlOut.Add(outFile[12]);
            htmlOut.Add("<!-- #InfoEnd -->");
            htmlOut.Add("<br>");
            htmlOut.Add("</body>	");
            htmlOut.Add("</html>");
            #endregion

            return htmlOut;
        }
        

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
