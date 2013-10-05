using System.Runtime.Serialization;

namespace ApiFoundation.Services
{
    public class EncryptedMessage
    {
        /// <summary>
        /// 時戳
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// 密文
        /// </summary>
        public string CipherText { get; set; }

        /// <summary>
        /// 簽章
        /// </summary>
        public string Signature { get; set; }
    }
}