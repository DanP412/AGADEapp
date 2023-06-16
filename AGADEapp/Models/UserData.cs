﻿using System.Reflection.Metadata;

namespace AGADEapp.Models
{
    public class UserData
    {
        public int Id { get; set; }

        // Zadeklarowana relacja do User
        public int UserId { get; set; } // Klucz obcy
        public User User { get; set; } = null!; // Referencja do User
    }
}
