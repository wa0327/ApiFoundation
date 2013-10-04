using System.Runtime.Serialization;

namespace ApiFoundation.Services
{
    [DataContract]
    internal class SecuredMessage
    {
        /// <summary>
        /// 時戳
        /// </summary>
        [DataMember(Name = "TimeStamp")]
        internal string Timestamp { get; set; }

        /// <summary>
        /// 密文
        /// </summary>
        [DataMember(Name = "EncryptedContent")]
        internal string CipherText { get; set; }

        /// <summary>
        /// 簽章
        /// </summary>
        [DataMember(Name = "ContentHash")]
        internal string Signature { get; set; }
    }
}