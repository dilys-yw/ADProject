using System;
using System.ComponentModel.DataAnnotations;

namespace ADProjGp2_LogicUStationeryStore.Models
{
    public class ChangePasswordModel
    {       
        [Required(ErrorMessage = "Username is Required")]
        public string userName { get; set; }
        [Required(ErrorMessage = "Previous Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string oldpassword { get; set; }

        [Required(ErrorMessage = "New Password is Required")]
        [DataType(DataType.Password)]
        [Display(Name = "new Password")]
        public string newpassword { get; set; }
        public string replymsg { get; set; }
    }
}
