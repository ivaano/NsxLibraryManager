﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models;

public class BaseLanguage
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(2)]
    public required string LanguageCode { get; set; }
}