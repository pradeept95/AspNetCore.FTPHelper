using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.FTPHelper.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.FTPHelper.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IFTPFileHelpers fTPFileHelpers;

        public IndexModel(IFTPFileHelpers fTPFileHelpers)
        {
            this.fTPFileHelpers = fTPFileHelpers;
        }
        public void OnGet()
        {
            this.fTPFileHelpers.DownloadFile("/po", "F:\\sapftp", ".CSV");
        }
    }
}
