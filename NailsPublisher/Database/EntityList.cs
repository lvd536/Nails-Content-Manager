namespace NailsPublisher.Database;

public class EntityList
{
    public class Chat
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public bool IsChannel { get; set; }
        public int LastDateMessageId { get; set; }
        public List<User> Users { get; set; } = new List<User>();
    }
    public class User
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public long ChannelId { get; set; }
        public long UserId { get; set; }
        public bool IsAdmin { get; set; }
        public Chat Chat { get; set; } = null!;
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<OpenDate> OpenDates { get; set; } = new List<OpenDate>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public short Price { get; set; } = 0;
        public string Step { get; set; } = "Finally";
        public string Description { get; set; } = String.Empty;
        public DateTime Date { get; set; }
        public User User { get; set; } = null!;
    }

    public class OpenDate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsOpen { get; set; }
        public User User { get; set; } = null!;
        public DateTime Date { get; set; }
    }
    
    public class Product
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Price { get; set; } = 0;
        public int Step { get; set; } = 5;
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsPurchased { get; set; }
        public User User { get; set; } = null!;
    }
}