using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASP_PageModule.Models.Page;
using ASP_PageModule.Models.Service;
using ASP_PageModule.ViewModels.Page;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_PageModule.Controllers
{
    public class PageController : Controller
    {
        CmsContext cmsDB;
        public PageController(CmsContext context)
        {
            cmsDB = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Page> pages = await cmsDB.Pages.ToListAsync();
            Dictionary<Guid, string> model = new Dictionary<Guid, string>();
            foreach (var page in pages)
            {
                model.Add(page.Id, page.PageTitle);
            }
            ViewBag.Pages = model;

            return View(model);
        }

        #region Создание страницы [GET]
        [HttpGet]
        public IActionResult AddPage()
        {
            return View();
        }
        #endregion

        #region Создание страницы [POST]
        [HttpPost]
        public async Task<IActionResult> AddPage(AddPageViewModel model)
        {
            if (ModelState.IsValid)
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

            return View(model);
        }
        #endregion

        #region Просмотр страницы
        public async Task<IActionResult> ViewPage(Guid pageId)
        {
            Page page = await cmsDB.Pages.FirstAsync(p => p.Id == pageId);

            ViewPageViewModel model = new ViewPageViewModel()
            {
                PageTitle = page.PageTitle,
                PageBody = page.PageBody
            };

            return View(model);
        }
        #endregion

    }
}
