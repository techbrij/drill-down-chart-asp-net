using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;


/// <summary>
/// For more details, visit techbrij.com
/// </summary>
public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //GenerateData();
        if (!IsPostBack)
        {
            BindYearChart();
        }
    }

    void BindYearChart()
    {
        ChartArea chArea = Chart1.ChartAreas[0];
        DataPointCollection dpc = Chart1.Series[0].Points;
        chArea.AxisX.Title = "Year";
        chArea.AxisY.Title = "Sale Amt";
        chArea.AxisY.LabelStyle.Format = "##,###";
        using (DBSampleEntities context = new DBSampleEntities())
        {

            var data = (from item in context.SaleInfoes
                        group item by item.SaleDate.Value.Year into g
                        select new { Year = g.Key, SaleAmt = g.Sum(x => x.SaleAmt) });

            foreach (var item in data)
            {
                dpc.AddXY(item.Year, item.SaleAmt);
            }
        }

        Series series = Chart1.Series[0];
        series.IsValueShownAsLabel = true;
        series.PostBackValue = "#VALX";
        series.LabelFormat = "##,###";
        series.LabelBackColor = Color.White;
        ViewState["ChartMode"] = "Year";
    }

    void BindQuarterChart(int year)
    {

        Dictionary<int, int> data = new Dictionary<int, int>();

        //Set X axis data
        for (int i = 1; i <= 4; i++)
        {
            data.Add(i, 0);
        }

        ChartArea chArea = Chart1.ChartAreas[0];
        DataPointCollection dpc = Chart1.Series[0].Points;
        chArea.AxisX.Title = "Quarter";
        chArea.AxisY.Title = "Sale Amt";
        chArea.AxisY.LabelStyle.Format = "##,###";
        
        using (DBSampleEntities context = new DBSampleEntities())
        {

            var amts = (from item in context.SaleInfoes
                        where item.SaleDate.Value.Year == year
                        group item by ((item.SaleDate.Value.Month - 1) / 3) + 1 into g
                        select new { Quarter = g.Key, SaleAmt = g.Sum(x => x.SaleAmt) });

            //Set Y axis values
            foreach (var item in amts)
            {
                data[item.Quarter] = Convert.ToInt32(item.SaleAmt);

            }

            //Add points in chart
            foreach (int str in data.Keys)
            {
                dpc.AddXY("Q" + str, data[str]);
            }


        }
        Series series = Chart1.Series[0];
        series.IsValueShownAsLabel = true;
        series.PostBackValue = "#VALX";
        series.LabelFormat = "##,###";
        series.LabelBackColor = Color.White;
        ViewState["ChartMode"] = "Quarter";
    }

    void BindMonthChart(int quarter, int year)
    {
        Dictionary<int, int> data = new Dictionary<int, int>();
        //Set X axis data
        for (int i = 1; i <= 3; i++)
        {
            data.Add((quarter-1) * 3 + i, 0);
        }
        ChartArea chArea = Chart1.ChartAreas[0];
        DataPointCollection dpc = Chart1.Series[0].Points;
        chArea.AxisX.Title = "Month";
        chArea.AxisY.Title = "Sale Amt";
        chArea.AxisY.LabelStyle.Format = "##,###";
        using (DBSampleEntities context = new DBSampleEntities())
        {

            var amts = (from item in context.SaleInfoes
                        where ((item.SaleDate.Value.Month - 1) / 3) + 1 == quarter && item.SaleDate.Value.Year == year
                        group item by item.SaleDate.Value.Month into g
                        select new { Month = g.Key, SaleAmt = g.Sum(x => x.SaleAmt) });
            //Set Y axis values
            foreach (var item in amts)
            {
                data[item.Month] =Convert.ToInt32(item.SaleAmt);                
            }

            //Add points in chart
            foreach (int str in data.Keys)
            {
                dpc.AddXY( DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(str), data[str]);
            }
        }
        Series series = Chart1.Series[0];
        series.IsValueShownAsLabel = true;
        series.LabelFormat = "##,###";
        series.LabelBackColor = Color.White;
        ViewState["ChartMode"] = "Month";
    }

    void GenerateData()
    {
        Random rand = new Random();
        using (DBSampleEntities context = new DBSampleEntities())
        {

            for (int i = 0; i < 1250; i++)
            {
                context.SaleInfoes.Add(new SaleInfo() { SaleDate = DateTime.Now.AddDays(-1 * i), SaleAmt = rand.Next(100, 1000) });
            }
            context.SaveChanges();

        }

    }

    protected void Chart1_Click(object sender, ImageMapEventArgs e)
    {
        switch (ViewState["ChartMode"].ToString())
        {
            case "Year":
                int year = Convert.ToInt32(e.PostBackValue);
                BindQuarterChart(year);
                ViewState["SelectedYear"] = year;
                break;
            case "Quarter":
                BindMonthChart(Convert.ToInt32(e.PostBackValue.ToString().Replace("Q", "")), Convert.ToInt32( ViewState["SelectedYear"]));
                break;
            default:
                break;
        }
    }
}

