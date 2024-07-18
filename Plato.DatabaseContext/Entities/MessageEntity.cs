﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plato.DatabaseContext.Entities
{
    [Table("Messages")]
    [PrimaryKey("Username")]
    public class MessageEntity : BaseEntity
    {
        public required string Username { get; set; }

        public required string Message { get; set; }

        public required int Order { get; set; }
    }
}