using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;
using ADProjGp2_LogicUStationeryStore.BusinessLogic;

namespace ADProjGp2_LogicUStationeryStore.Controllers
{
    public class ReorderReportController : Controller
    {
        // GET: ReorderReport
        ReorderReportBusinessLogic roBL = new ReorderReportBusinessLogic();
        ReportBusinessLogic rBL = new ReportBusinessLogic();

        string[] color = new string[12] { "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60", "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", };
        string[] border = new string[12] { "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60", "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080" };
        string[] months = new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public ActionResult AnnualPurchaseReport()
        {

            ReorderChartModel rm = new ReorderChartModel()
            {
                YearSelection = rBL.DisplayYears(),
                SelectedYear = "All",                         //Add this
            };

            return View(rm);
        }

        public JsonResult AnnualReorderData(ReorderChartModel rm)
        {
            Chartjs _chart = new Chartjs();

            _chart.labels = new string[] { };

            _chart.datasets = new List<Datasets>();
            List<Datasets> _dataSet = new List<Datasets>();

            if (rm.SelectedYear == "All")
            {
                List<string> years = new List<string>();
                for (int i = 3; i >= 1; i--)
                {
                    years.Add(rm.YearSelection[i].Value);
                }
                _chart.labels = years.ToArray();


              

                _dataSet.Add(new Datasets()
                {
                    label = "Stationery Reorder Quantity",
                    data = roBL.GetItems(),
                    backgroundColor = new string[] { "#6B7ABB", "#6B7ABB", "#6B7ABB" },
                    borderColor = new string[] { "#6B7ABB", "#6B7ABB", "#6B7ABB" },
                    borderWidth = "1"
                });

                _chart.datasets = _dataSet;
            }
            else
            {
                _chart.labels = months;
               
                List<string> colors = new List<string>();
                List<string> borders = new List<string>();

                for (int i = 0; i < 13; i++)
                {
                    colors.Add("#1A5276");
                    borders.Add("#1A5276");
                }

                _dataSet.Add(new Datasets()
                {
                    label = "Stationery Reorder Quantity",
                    data = roBL.GetItemsMonthly(rm.SelectedYear),
                    backgroundColor = colors.ToArray(),
                    borderColor = borders.ToArray(),
                    borderWidth = "1"
                });

                _chart.datasets = _dataSet;


            }

            return Json(_chart, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReorderReportBySupplier()
        {


            ReorderChartModel rm = new ReorderChartModel()
            {
                Items = rBL.DisplayItems(),
                YearSelection = rBL.DisplayYears(),
                Months = rBL.DisplayMonths(),
                Suppliers = roBL.DisplaySuppliers(),
                SelectedItem = "All",

            };

            return View(rm);
        }

        public JsonResult ReorderDataBySup(ReorderChartModel rm)
        {
            Chartjs _chart = new Chartjs();
            _chart.datasets = new List<Datasets>();
            List<Datasets> _dataSet = new List<Datasets>();

            _chart.labels = new string[] { };
            List<string> labels = new List<string>();



            List<string> selectedSupplier = new List<string>();
            for (int i = 0; i < rm.Suppliers.Count; i++)
            {
                if (rm.Suppliers[i].Selected)
                {
                    selectedSupplier.Add(rm.Suppliers[i].Value);
                }
            }

            rm.YearSelection = rBL.DisplayYears();
            List<string> years = new List<string>();
            for (int i = 3; i >= 1; i--)
            {
                years.Add(rm.YearSelection[i].Value);
            }

            if (rm.Months[0].Selected)
            {

                _chart.labels = years.ToArray();

                if (rm.SelectedItem == "All")
                {
                    for (int i = 0; i < selectedSupplier.Count; i++)
                    {
                        List<string> colors = new List<string>();
                        List<string> borders = new List<string>();
                        Dictionary<string, int> supplierOrder = roBL.GetTotalBySupplier(selectedSupplier[i], years);
                        List<int> listData = new List<int>();

                        foreach (KeyValuePair<string, int> pair in supplierOrder)
                        {
                            for (int j = 0; j < years.Count; j++)
                            {
                                if (pair.Key == years[j])
                                {
                                    listData.Add(pair.Value);
                                    colors.Add(color[i]);
                                    borders.Add(color[i]);
                                }
                            }

                        }

                        _dataSet.Add(new Datasets()
                        {
                            label = selectedSupplier[i],
                            data = listData.ToArray(),
                            backgroundColor = colors.ToArray(),
                            borderColor = borders.ToArray(),
                            borderWidth = "1"
                        });

                    }
                }
                else
                {

                    for (int i = 0; i < selectedSupplier.Count; i++)
                    {
                        Dictionary<string, int> supplierOrder = roBL.GetTotalBySupplier(selectedSupplier[i], rm.SelectedItem, years);
                        List<int> listData = new List<int>();
                        List<string> colors = new List<string>();
                        List<string> borders = new List<string>();

                        foreach (KeyValuePair<string, int> pair in supplierOrder)
                        {
                            for (int j = 0; j < years.Count; j++)
                            {
                                if (pair.Key == years[j])
                                {
                                    listData.Add(pair.Value);
                                    colors.Add(color[i]);
                                    borders.Add(border[i]);
                                }

                            }
                        }
                        _dataSet.Add(new Datasets()
                        {
                            label = selectedSupplier[i],
                            data = listData.ToArray(),
                            backgroundColor = colors.ToArray(),
                            borderColor = borders.ToArray(),
                            borderWidth = "1"
                        });

                    }

                }
            }

            else
            {
                List<int> currentYear = new List<int>();
                List<int> lastYear = new List<int>();
                List<int> previousYear = new List<int>();

                for (int i = 25; i <= 36; i++)
                {
                    if (rm.Months[i].Selected)
                    {
                        previousYear.Add(i - 24);
                        labels.Add(months[i - 25] + " " + years[0]);
                    }
                }
                for (int i = 13; i <= 24; i++)
                {
                    if (rm.Months[i].Selected)
                    {

                        lastYear.Add(i - 12);
                        labels.Add(months[i - 13] + " " + years[1]);
                    }
                }
                for (int i = 1; i <= 12; i++)
                {
                    if (rm.Months[i].Selected)
                    {
                        currentYear.Add(i);
                        labels.Add(months[i - 1] + " " + years[2]);
                    }
                }



                if (rm.SelectedItem == "All")
                {
                    for (int i = 0; i < selectedSupplier.Count; i++)
                    {
                        List<int> listData = new List<int>();
                        List<string> colors = new List<string>();
                        List<string> borders = new List<string>();

                        Dictionary<int, int> orderPreviousYear = roBL.GetTotalBySuppAndYear(selectedSupplier[i], years[0], previousYear);
                        foreach (KeyValuePair<int, int> pair in orderPreviousYear)
                        {
                            listData.Add(pair.Value);
                            colors.Add(color[i]);
                            borders.Add(border[i]);
                        }

                        Dictionary<int, int> orderLastYear = roBL.GetTotalBySuppAndYear(selectedSupplier[i], years[1], lastYear);
                        foreach (KeyValuePair<int, int> pair in orderLastYear)
                        {

                            listData.Add(pair.Value);
                            colors.Add(color[i]);
                            borders.Add(border[i]);
                        }

                        Dictionary<int, int> orderCurrentYear = roBL.GetTotalBySuppAndYear(selectedSupplier[i], years[2], currentYear);
                        foreach (KeyValuePair<int, int> pair in orderCurrentYear)
                        {

                            listData.Add(pair.Value);
                            colors.Add(color[i]);
                            borders.Add(border[i]);
                        }


                        _chart.labels = labels.ToArray();

                        _dataSet.Add(new Datasets()
                        {
                            label = selectedSupplier[i],
                            data = listData.ToArray(),
                            backgroundColor = colors.ToArray(),
                            borderColor = borders.ToArray(),
                            borderWidth = "1"
                        });
                    }
                }

                else
                {


                    for (int i = 0; i < selectedSupplier.Count; i++)
                    {
                        List<int> listData = new List<int>();
                        List<string> colors = new List<string>();
                        List<string> borders = new List<string>();

                        Dictionary<int, int> orderPreviousYear = roBL.GetTotalBySuppAndYear(selectedSupplier[i], years[0], previousYear, rm.SelectedItem);
                        foreach (KeyValuePair<int, int> pair in orderPreviousYear)
                        {

                            listData.Add(pair.Value);
                            colors.Add(color[i]);
                            borders.Add(border[i]);
                        }

                        Dictionary<int, int> orderLastYear = roBL.GetTotalBySuppAndYear(selectedSupplier[i], years[1], lastYear, rm.SelectedItem);
                        foreach (KeyValuePair<int, int> pair in orderLastYear)
                        {

                            listData.Add(pair.Value);
                            colors.Add(color[i]);
                            borders.Add(border[i]);
                        }

                        Dictionary<int, int> orderCurrentYear = roBL.GetTotalBySuppAndYear(selectedSupplier[i], years[2], currentYear, rm.SelectedItem);
                        foreach (KeyValuePair<int, int> pair in orderCurrentYear)
                        {

                            listData.Add(pair.Value);
                            colors.Add(color[i]);
                            borders.Add(border[i]);
                        }


                        _chart.labels = labels.ToArray();

                        _dataSet.Add(new Datasets()
                        {
                            label = selectedSupplier[i],
                            data = listData.ToArray(),
                            backgroundColor = colors.ToArray(),
                            borderColor = borders.ToArray(),
                            borderWidth = "1"
                        });

                    }
                }

            }
            _chart.datasets = _dataSet;
            return Json(_chart, JsonRequestBehavior.AllowGet);
        }
    }


}