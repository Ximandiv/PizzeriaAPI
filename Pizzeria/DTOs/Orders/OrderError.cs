namespace Pizzeria.DTOs.Orders
{
    public static class OrderError
    {
        public readonly static Error InvalidItems = new("Validation", "Invalid Orders Items");
        public readonly static Error InvalidIds = new("Validation", "Invalid Order Ids");
        public readonly static Error InvalidParsing = new("Validation", "Invalid model to process as entity");
        public readonly static Error InvalidUpdate = new("Validation", "No changes were detected to update");
        public readonly static Error InvalidDelete = new("Validation", "Deletion was not processed for one or all entities");

        public readonly static Error NotFoundBulk = new("OrderService.GetOneFromUser", "One order was not found for bulk update");

        public readonly static Error GetAllFromUser = new("OrderService.GetAllFromUser", "Something went wrong during User's Orders retrieval");
        public readonly static Error GetOneFromUser = new("OrderService.GetOneFromUser", "Something went wrong during User's Orders retrieval");
        public readonly static Error Create = new("OrderService.Create", "Something went wrong during User's Orders creation");
        public readonly static Error Update = new("OrderService.Update", "Something went wrong during User's Orders update");
        public readonly static Error Delete = new("OrderService.delete", "Something went wrong during User's Orders deletion");
    }
}
