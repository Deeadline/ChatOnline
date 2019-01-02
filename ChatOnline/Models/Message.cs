using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatOnline.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Content { get; set; }
        public User Sender { get; set; }
        public DateTime SendTime { get; set; }
        public virtual Room Room { get; set; }
        public Message() => SendTime = DateTime.Now;
    }
}
