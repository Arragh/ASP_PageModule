using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASP_PageModule.Models.Page;
using ASP_PageModule.Models.Service;
using ASP_PageModule.ViewModels.Page;
using Microsoft.AspNetCore.Mvc;

namespace ASP_PageModule.Controllers
{
    public class PageController : Controller
    {
        CmsContext cmsDB;
        public PageController(CmsContext context)
        {
            cmsDB = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPage(AddPageViewModel model)
        {
            Page page = new Page()
            {
                Id = Guid.NewGuid(),
                PageTitle = model.PageTitle,
                PageBody = model.PageBody,
                PageDate = DateTime.Now,
                UserName = "Mnemonic" // Хардкод. Потом обязательно заменить !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            };

            await cmsDB.Pages.AddAsync(page);
            await cmsDB.SaveChangesAsync();

            return RedirectToAction("Index", "Page");
        }
    }
}
