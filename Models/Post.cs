namespace SocialMedia.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public string Text { get; set; }

        public string? ImagePath { get; set; }

        public DateTime Date { get; set; }

        public List<Like> Likes { get; set; } = new();
    }
}