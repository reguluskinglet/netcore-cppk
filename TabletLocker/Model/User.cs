﻿using System;

namespace TabletLocker.Model
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public string PersonNumber { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}