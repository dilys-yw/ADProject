using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class RequisitionChartModel
    {
        
        public IList<SelectListItem> YearSelection { get; set; }
        public IList<SelectListItem> Items { get; set; }

        public string SelectedItem { get; set; }

        public string SelectedYear { get; set; }

        public string SelectedDept { get; set; }

        public IList<SelectListItem> Months { get; set; }
        
        public IList<SelectListItem> Departments { get; set; }

        
    }
   
}