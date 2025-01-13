namespace Pizzeria.DTOs.Users
{
    public static class UserError
    {
        public static readonly Error InvalidCredentials = new("Validation", "Invalid credentials");
        public static readonly Error InvalidId = new("Validation", "Invalid User Id");

        public static readonly Error GetAll = new("UserService.GetAll", "Something went wrong during User Retrieval");
        public static readonly Error GetByEmail = new("UserService.GetByEmail", "Something went wrong during User Retrieval");
        public static readonly Error Create = new("UserService.Create", "Something went wrong during User Creation");
        public static readonly Error LogIn = new("UserService.GetByEmail", "Something went wrong during User LogIn");
    }
}
