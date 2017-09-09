using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Web.Mvc;
namespace BizSparkSupport.MVC.ViewModels
{
    public class RegisterModel
    {
        public long UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Remote("EmailExists", "StartUp", HttpMethod = "POST", ErrorMessage = "Email address already registered.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "error Message ")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }
        public string Hash { get; set; }
        public string Mobile { get; set; }
        public string RememberToken { get; set; }
        public string VerificationToken { get; set; }
        public Nullable<bool> IsVerified { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
        public long CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNumber { get; set; }
    }
}