using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ADProjGp2_LogicUStationeryStore.Models;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class ReqDetailsModel
    {
        public string ReqID { get; set; }
        public DateTime ApprovalDate { get; set; }
        public int ReqQty { get; set; }
        public string DeptID { get; set; }
        public string Category { get; set; }
    }

    public class ReportBusinessLogic
    {
        SSISEntities entity = new SSISEntities();
        public IList<SelectListItem> DisplayYears()
        {
            IList<SelectListItem> year = new List<SelectListItem>();

            year.Add(new SelectListItem() { Value = "All", Text = "All" });
            year.Add(new SelectListItem() { Value = DateTime.Today.Year.ToString(), Text = DateTime.Today.Year.ToString() });
            year.Add(new SelectListItem() { Value = (DateTime.Today.Year - 1).ToString(), Text = (DateTime.Today.Year - 1).ToString() });
            year.Add(new SelectListItem() { Value = (DateTime.Today.Year - 2).ToString(), Text = (DateTime.Today.Year - 2).ToString() });

            return year;
        }

        public IList<SelectListItem> DisplayMonths()
        {
            List<string> months = new List<string>() { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            IList<SelectListItem> Months = new List<SelectListItem>();

            Months.Add(new SelectListItem
            {
                Text = "Comparison for three years",
                Value = "Comparison for three years",
            });
            foreach (string m in months)
            {
                Months.Add(new SelectListItem
                {
                    Text = m,
                    Value = m
                });
            }           
            foreach (string m in months)
            {
                Months.Add(new SelectListItem
                {
                    Text = m,
                    Value = m
                });
            }          
            foreach (string m in months)
            {
                Months.Add(new SelectListItem
                {
                    Text = m,
                    Value = m
                });
            }

            return Months;
        }

        public IList<SelectListItem> DisplayDepartments()
        {
            List<string> depts = entity.DeptCollectionDetails.Select(c => c.departmentId).ToList();
            IList<SelectListItem> departments = new List<SelectListItem>();
            

            foreach (string dep in depts)
            {
                departments.Add(new SelectListItem
                {
                    Text = dep,
                    Value = dep,
                });
            }
            return departments;
        }

        public IList<SelectListItem> DisplayItems()
        {
            HashSet<string> categories = new HashSet<string>(entity.Catalogues.Select(c => c.category));
            IList<SelectListItem> cat = new List<SelectListItem>();
            cat.Add(new SelectListItem
            {
                Text = "All",
                Value = "All",
            });
            foreach (string catg in categories)
            {
                cat.Add(new SelectListItem
                {
                    Text = catg,
                    Value = catg,
                });
            }
            return cat;
        }

        public int[] GetItems()
        {
            var totalRequisition = (from req in entity.Requisitions
                                   join reqD in entity.RequisitionDetails
                                   on req.requisitionId equals reqD.requisitionId
                                   where req.approvalDate.Value.Year >= DateTime.Today.Year - 2
                                   select new ReqDetailsModel
                                   {
                                       ReqID = req.requisitionId,
                                       ApprovalDate = req.approvalDate.Value,
                                       ReqQty = reqD.requestQty,

                                   }).ToList();

            
            int currentYearQuantity = 0;
            int lastYearQuantity = 0;
            int previousYearQuantity = 0;

            foreach (var req in totalRequisition)
            {
                if (req.ApprovalDate.Year == DateTime.Today.Year - 2)
                {
                    previousYearQuantity += req.ReqQty;
                }
                else if (req.ApprovalDate.Year == DateTime.Today.Year - 1)
                {
                    lastYearQuantity += req.ReqQty;
                }
                else if (req.ApprovalDate.Year == DateTime.Today.Year)
                {
                    currentYearQuantity += req.ReqQty;
                }
            }

            int[] data = new int[3] { previousYearQuantity, lastYearQuantity, currentYearQuantity };

            return data;
        }

        public int[] GetItemsMonthly (string year)
        {
            var details = (from req in entity.Requisitions
                           join reqD in entity.RequisitionDetails
                           on req.requisitionId equals reqD.requisitionId
                           where req.approvalDate.Value.Year.ToString() == year
                           select new ReqDetailsModel
                           {
                               ReqID = req.requisitionId,
                               ApprovalDate = (DateTime)req.approvalDate.Value,
                               ReqQty = reqD.requestQty,
                               DeptID = req.departmentId,

                           }).ToList();

            
            int[] reqQuantity = new int[12];

            foreach (var req in details)
            {
                int month = req.ApprovalDate.Month;

                reqQuantity[month - 1] += req.ReqQty;

            }

            return reqQuantity;
        }

        public Dictionary<string, int> GetTotalByDept (string deptName, List<string> years)
        {

            var details = (from req in entity.Requisitions
                           join reqD in entity.RequisitionDetails
                           on req.requisitionId equals reqD.requisitionId
                           where (req.departmentId == deptName && req.approvalDate != null)                      
                           select new ReqDetailsModel
                           {
                               ReqID = req.requisitionId,
                               ApprovalDate = req.approvalDate.Value,
                               ReqQty = reqD.requestQty,
                               DeptID = req.departmentId,

                           }).ToList();

           
            Dictionary<string, int> deptQty = new Dictionary<string, int>();

            foreach (string year in years)
            {
                int totalReq = 0;
                for (int i = 0; i < details.Count; i++)
                {                   
                        if (details[i].ApprovalDate.Year.ToString() == year)
                        {
                            totalReq += details[i].ReqQty;
                        }
                 }
                deptQty.Add(year, totalReq);                
            }

            return deptQty;
        }

        public Dictionary<string, int> GetTotalByDept (string deptName, string catg, List<string> years)
        {

            var details = (from req in entity.Requisitions
                           join reqD in entity.RequisitionDetails
                           on req.requisitionId equals reqD.requisitionId
                           join item in entity.Catalogues
                           on reqD.itemId equals item.itemId
                           where (req.departmentId == deptName
                           && item.category == catg && req.approvalDate != null)
                           select new ReqDetailsModel
                           {
                               ReqID = req.requisitionId,
                               ApprovalDate = req.approvalDate.Value,
                               ReqQty = reqD.requestQty,
                               DeptID = req.departmentId,
                               Category = item.category,

                           }).ToList();

           
            Dictionary<string, int> deptQty = new Dictionary<string, int>();

            foreach (string year in years)
            {
                int totalReq = 0;
                for (int i = 0; i < details.Count; i++)
                {
                    if (details[i].ApprovalDate.Year.ToString() == year)
                    {
                        totalReq += details[i].ReqQty;
                    }
                }
                deptQty.Add(year, totalReq);
            }

            return deptQty;
        }

        public Dictionary<int, int> GetTotalByDeptAndYear(string deptName, string year, List<int> selectedMonths)
        {

            var details = (from req in entity.Requisitions
                           join reqD in entity.RequisitionDetails
                           on req.requisitionId equals reqD.requisitionId
                           where (req.departmentId == deptName 
                           && req.approvalDate.Value.Year.ToString() == year)
                           select new ReqDetailsModel
                           {
                               ReqID = req.requisitionId,
                               ApprovalDate = req.approvalDate.Value,
                               ReqQty = reqD.requestQty,
                               DeptID = req.departmentId,

                           }).ToList();

            
            Dictionary<int, int> deptQty = new Dictionary<int, int>();
           
            if (selectedMonths != null)
            {
                
                foreach (int month in selectedMonths)
                {
                    int totalReq = 0;
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].ApprovalDate.Month == month)
                        {
                            totalReq += details[i].ReqQty;
                        }
                    }
                    deptQty.Add(month, totalReq);
                  
                }
            }
           
            return deptQty;
        }

        public Dictionary<int, int> GetTotalByDeptAndYear(string deptName, string year, List<int> selectedMonths, string item)
        {

            var details = (from req in entity.Requisitions
                           join reqD in entity.RequisitionDetails
                           on req.requisitionId equals reqD.requisitionId
                           join cat in entity.Catalogues 
                           on reqD.itemId equals cat.itemId
                           where (req.departmentId == deptName
                           && req.approvalDate.Value.Year.ToString() == year
                           && cat.category == item)
                           select new ReqDetailsModel
                           {
                               ReqID = req.requisitionId,
                               ApprovalDate = req.approvalDate.Value,
                               ReqQty = reqD.requestQty,
                               DeptID = req.departmentId,
                               Category = cat.category,

                           }).ToList();


            Dictionary<int, int> deptQty = new Dictionary<int, int>();
            if (selectedMonths != null)
            {
                foreach (int month in selectedMonths)
                {
                    int totalReq = 0;
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].ApprovalDate.Month == month)
                        {
                            totalReq += details[i].ReqQty;
                        }
                    }
                    deptQty.Add(month, totalReq);
                }
            }
           
            return deptQty;
        }


    }
}