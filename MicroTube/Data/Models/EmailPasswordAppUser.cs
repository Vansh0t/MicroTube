﻿namespace MicroTube.Data.Models
{
    public class EmailPasswordAppUser : AppUser
    {
        public EmailPasswordAuthentication Authentication { get; set; }
        public EmailPasswordAppUser(AppUser user,
            EmailPasswordAuthentication authentication) 
            : base(user.Id, user.Username, user.Email, user.PublicUsername, user.IsEmailConfirmed)
        {
            Authentication = authentication;
        }
    }
}