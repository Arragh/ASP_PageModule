using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
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
            Dictionary<Guid, string> dictionary = new Dictionary<Guid, string>();
            foreach (var page in pages)
            {
                dictionary.Add(page.Id, page.PageTitle);
            }
            //ViewBag.Pages = model;

            return View(dictionary);
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
                    PageBody = SpecSymbolsPOST(model.PageBody),
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

        #region Редактирование страницы [GET]
        [HttpGet]
        public async Task<IActionResult> EditPage(Guid pageId)
        {
            Page page = await cmsDB.Pages.FirstAsync(p => p.Id == pageId);

            EditPageViewModel model = new EditPageViewModel()
            {
                PageId = pageId,
                PageTitle = page.PageTitle,
                PageBody = SpecSymbolsToGET(page.PageBody)
            };

            return View(model);
        }
        #endregion

        #region Редактирование страницы [POST]
        [HttpPost]
        public async Task<IActionResult> EditPage(EditPageViewModel model)
        {
            Page page = await cmsDB.Pages.FirstAsync(p => p.Id == model.PageId);

            page.PageTitle = model.PageTitle;
            page.PageBody = SpecSymbolsPOST(model.PageBody);

            cmsDB.Pages.Update(page);
            await cmsDB.SaveChangesAsync();

            return RedirectToAction("ViewPage", "Page", new { pageId = page.Id });
        }
        #endregion

        // Всё что ниже, потом надо будет вынести куда-то в отдельный класс-хелпер, для использования другими модулями

        #region Просмотр страницы
        public async Task<IActionResult> ViewPage(Guid pageId)
        {
            ViewBag.PageId = pageId;

            Page page = await cmsDB.Pages.FirstAsync(p => p.Id == pageId);

            ViewPageViewModel model = new ViewPageViewModel()
            {
                PageTitle = page.PageTitle,
                PageBody = BbCode(page.PageBody)
            };

            return View(model);
        }
        #endregion

        #region Замена опасных символов
        string SpecSymbolsPOST(string text)
        {
            return text.Replace("&", "&amp;")   // Очень важно, чтобы этот был в самом начале
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#x27;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\r\n", "<br>")
                       .Replace("\\", "&#x5C")
                       .Replace(" ", "&nbsp;");
        }

        string SpecSymbolsToGET(string text)
        {
            return text.Replace("&amp;", "&")
                       .Replace("&quot;", "\"")
                       .Replace("&#x27;", "'")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&#x5C", "\\")
                       .Replace("<br>", "\r\n")
                       .Replace("&nbsp;", " ");
        }
        #endregion

        #region BB-Code
        string BbCode(string text)
        {
            return text.Replace("[b]", "<b>")
                       .Replace("[/b]", "</b>")
                       .Replace("[i]", "<i>")
                       .Replace("[/i]", "</i>")
                       .Replace("[u]", "<u>")
                       .Replace("[/u]", "</u>");
        }
        #endregion
    }
}
