using System;

namespace Swan.Test.Mocks
{
    public class User
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public Role Role { get; set; }

        public DateTime StartDate { get; set; }
    }

    public class Role
    {
        public string Name { get; set; }
    }

    public class UserDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime StartDate { get; set; }
    }
}