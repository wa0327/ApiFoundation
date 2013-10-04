using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiFoundation.Web.Http.Controllers
{
    [DataContract]
    public class InvalidModelRequest
    {
        [DataMember(IsRequired = true), Required]
        public string BackyardId { get; set; }

        [DataMember(IsRequired = true), Required, MinLength(1)]
        public int[] CatSubIds { get; set; }
    }
}