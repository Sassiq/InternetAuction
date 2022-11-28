using System.ComponentModel.DataAnnotations;
using OnlineAuctionProject.Resources;
using System.ComponentModel;

namespace OnlineAuctionProject.Models
{
    public class ManageUserViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PasswordReq")]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(Resource))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "NewPasswordReq")]
        [StringLength(100, ErrorMessageResourceName = "PasswordLength", ErrorMessageResourceType = typeof(Resource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resource))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resource))]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordError")]
        public string ConfirmPassword { get; set; }
    }

    public class EditProfileModel
    {
        [Display(Name = "FirstName", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "FirstNameReq")]
        public string FirstName { get; set; }

        [Display(Name = "LastName", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "LastNameReq")]
        public string LastName { get; set; }

        [Display(Name = "Gender", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "GenderReq")]
        public string Gender { get; set; }

        [Display(Name = "Email", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailReq")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }

        [Display(Name = "PhNumber", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PhNumberReq")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CountryReq")]
        public string Country { get; set; }

        [Display(Name = "City", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CityReq")]
        public string City { get; set; }

        [Display(Name = "Street", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StreetReq")]
        public string Street { get; set; }

        [Display(Name = "ZipCode", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ZipCodeReq")]
        public string ZipCode { get; set; }

        [Display(Name = "AccountNumber", ResourceType = typeof(Resource))]
        [DataType(DataType.CreditCard)]
        public string AccountNumber { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "UsernameReq")]
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PasswordReq")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Display(Name = "FirstName", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "FirstNameReq")]
        public string FirstName { get; set; }

        [Display(Name = "LastName", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "LastNameReq")]
        public string LastName { get; set; }

        [Display(Name = "Gender", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "GenderReq")]
        public string Gender { get; set; }

        [Display(Name = "Email", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailReq")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }

        [Display(Name = "PhNumber", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PhNumberReq")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CountryReq")]
        public string Country { get; set; }

        [Display(Name = "City", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "CityReq")]
        public string City { get; set; }

        [Display(Name = "Street", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StreetReq")]
        public string Street { get; set; }

        [Display(Name = "ZipCode", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ZipCodeReq")]
        public string ZipCode { get; set; }

        [Display(Name = "Username", ResourceType = typeof(Resource))]
        [StringLength(25, MinimumLength = 5, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "UsernameLength")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "UsernameReq")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessageResourceName = "PasswordLength", ErrorMessageResourceType = typeof(Resource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "PasswordReq")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resource))]
        [System.Web.Mvc.Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "ConfirmPasswordError")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "AccountNumber", ResourceType = typeof(Resource))]
        [DataType(DataType.CreditCard)]
        public string AccountNumber { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Display(Name = "Email", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailReq")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }
    }
}
