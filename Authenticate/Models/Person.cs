namespace Authenticate.Models
{
    record Person
    {
        public string Email { get; init; }
        public string Password { get; private set; }
        public Role Role { get; private set; } 

        public Person(string email, string password, Role role)
        {
            Email = email;
            Password = password;
            Role = role;
        }
    }

    class Role
    {
        public string Name { get; }
        public Role(string name) => Name = name;
    }
}
