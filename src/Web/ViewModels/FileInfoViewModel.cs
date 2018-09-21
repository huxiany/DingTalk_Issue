namespace EaseSource.AnDa.SMT.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class FileInfoViewModel
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileContent { get; set; }

        /// <summary>
        /// 临时授权码，从页面上传入，方便发送钉钉消息
        /// </summary>
        public string AuthCode { get; set; }
    }
}
