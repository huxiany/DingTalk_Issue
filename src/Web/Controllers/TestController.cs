namespace EaseSource.AnDa.SMT.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using EaseSource.AnDa.SMT.Web.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// 生产工单API
    /// </summary>
    [Route("api/[controller]")]
    public class TestController : SMTControllerBase
    {
        private readonly ILogger<TestController> logger;

        public TestController(ILogger<TestController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Route("ImportData")]
        public IActionResult ImportData([FromBody] FileInfoViewModel specFile)
        {
            string base64Content = specFile.FileContent;

            return Success();
        }

        [HttpGet("")]
        [Route("Print")]
        public IActionResult Print()
        {
            return Success();
        }
    }
}