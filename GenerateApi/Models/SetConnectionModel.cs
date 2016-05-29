using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GenerateApi.Models
{
    public class SetConnectionModel
    {
        [Display(Name ="Server名稱")]
        [Required(ErrorMessage ="{0}必須輸入")]
        public string Server { get; set; }

        [Display(Name = "DB名稱")]
        [Required(ErrorMessage = "{0}必須輸入")]
        public string Db { get; set; }

        [Display(Name = "DB使用者名稱")]
        [Required(ErrorMessage = "{0}必須輸入")]
        public string User { get; set; }

        [Display(Name = "DB密碼")]
        [Required(ErrorMessage = "{0}必須輸入")]
        [DataType(DataType.Password)]
        public string Pwd { get; set; }
    }
}