using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatOnline.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Message> Messages { get; set; }
        public IList<User> Users { get; set; }

        public Room()
        {
            Messages = new List<Message>();
            Users = new List<User>();
        }
    }
}
