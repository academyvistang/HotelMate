using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Entitities;

namespace HotelMateWebV1.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Enter Username")] 
       // [MaxLength(10,ErrorMessage="Username must be a maximum of 10 characters.")]
        [Remote("ValidateUserName","Account",ErrorMessage="Sorry, This username already exists, please try another username.", HttpMethod="POST")]
        public string Username { get; set;}


        [Required(ErrorMessage = "Enter Password")] 
        public string Password { get; set; }

        //[Compare("Password", ErrorMessage="Password Mismatch")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Enter Email")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Incorrect Email")]
        public string Email { get; set; }

        public string UserPictureName { get; set; }

        public string CurrentUserName { get; set; }

        public decimal CurrentUserBalance { get; set; }

        public int LiveGamesCount { get; set; }

        public List<UserAccount> UserAccounts { get; set; }
    }
}