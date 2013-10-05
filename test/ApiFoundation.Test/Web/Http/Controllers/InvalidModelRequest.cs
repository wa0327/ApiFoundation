using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ApiFoundation.Web.Http.Controllers
{
    [DataContract]
    public class InvalidModelRequest
    {
        [DataMember(IsRequired = true), Required]
        public int? UserId { get; set; }

        [DataMember(IsRequired = true), Required]
        public string BackyardId { get; set; }

        [DataMember(IsRequired = true), Required, MinLength(1)]
        public int[] CatSubIds { get; set; }
    }
}