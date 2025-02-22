namespace NailsPublisher.Database;

public class EntityList
{
    public class Chat
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public bool IsChannel { get; set; }
        public List<User> Users { get; set; } = new List<User>();
    }
    public class User
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public long UserId { get; set; }
        public bool IsAdmin { get; set; }
        public Chat Chat { get; set; } = null!;
        public List<Post> Posts { get; set; } = new List<Post>();
    }
    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Step { get; set; } = null!;
        public string Description { get; set; }
        public short Price { get; set; }
        public User User { get; set; } = null!;
    }
}