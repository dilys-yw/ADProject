using System;
using System.ComponentModel.DataAnnotations;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    // object created for Login API - http://localhost/LoginServicesApp/Service1.svc/LoginGet?username=" + username + "&password=" + password
    public class LoginObjectModel
    {       
        public string employeeId { get; set; }
        public string title { get; set; }
        public string employeeName { get; set; }
        public string departmentId { get; set; }
        public string email { get; set; }
        public string superiorName { get; set; }

        public string superiorID { get; set; }

        [Required(ErrorMessage = "Username is Required")]
        public string userName { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
        public string role { get; set; }
        
        public string departmentName { get; set; }
        public string contactPerson { get; set; }
        public string phone { get; set; }
        public string faxNumber { get; set; }
    }
}
