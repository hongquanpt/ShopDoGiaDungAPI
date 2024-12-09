namespace ShopDoGiaDungAPI.DTO
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
