using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class Chartjs
    {
      
            public string[] labels { get; set; }
            public List<Datasets> datasets { get; set; }
        }
        public class Datasets
        {
            public string label { get; set; }
            public string[] backgroundColor { get; set; }
            public string[] borderColor { get; set; }
            public string borderWidth { get; set; }
            public int[] data { get; set; }
            
            //public bool fill { get; set; }           
            //public int lineTension { get; set; }

            //public string stack { get; set; }

           // public string type { get; set; }

        }
    }
