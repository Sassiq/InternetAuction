using OnlineAuctionProject.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineAuctionProject.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Display(Name = "Email", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailReq")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }

        [Display(Name = "SenderType", ResourceType = typeof(Resource))]
        public string SenderType { get; set; }

        [Display(Name = "MessageText", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MessageTextReq")]
        public string MessageText { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? MessageDateAndTime { get; set; }
        public bool IsSeen { get; set; }

        [Display(Name = "RepliedBy", ResourceType = typeof(Resource))]
        public ApplicationUser RepliedBy { get; set; }

    }

    public class ReplyMessageViewModel
    {
        [Display(Name = "Email", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailReq")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "InvalidEmail")]
        public string Email { get; set; }

        [Display(Name = "MessageText", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MessageTextReq")]
        public string MessageText { get; set; }
    }

    public class EmailToAllUsers
    {
        [Display(Name = "Subject", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SubjectReq")]
        public string Subject { get; set; }

        [Display(Name = "EmailText", ResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailTextReq")]
        public string EmailText { get; set; }
    }
}