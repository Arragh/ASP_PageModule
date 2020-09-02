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
                    PageBody = SpecSymbolsToView(model.PageBody), // Замена символов на их безопасные шестнадцатеричные значения
                    PageDate = DateTime.Now,
                    UserName = "Mnemonic" // Хардкод. Потом обязательно заменить !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                };

                await cmsDB.Pages.AddAsync(page);
                await cmsDB.SaveChangesAsync();

                return RedirectToAction("ViewPage", "Page", new { pageId = page.Id });
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
                PageBody = SpecSymbolsToEdit(page.PageBody) // Замена десятичных значений на символы, для удобства
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
            page.PageBody = SpecSymbolsToView(model.PageBody); // Замена символов на их безопасные шестнадцатеричные значения 

            cmsDB.Pages.Update(page);
            await cmsDB.SaveChangesAsync();

            return RedirectToAction("ViewPage", "Page", new { pageId = page.Id });
        }
        #endregion

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

            ViewBag.Title = page.PageTitle;

            return View(model);
        }
        #endregion

        #region Удаление страницы
        public async Task<IActionResult> DeletePage(Guid pageId, bool isChecked)
        {
            if (isChecked)
            {
                Page page = new Page { Id = pageId };

                cmsDB.Pages.Remove(page);
                await cmsDB.SaveChangesAsync();

                return RedirectToAction("Index", "Page");
            }

            return RedirectToAction("Index", "Page");
        }
        #endregion

        // Всё что ниже, потом надо будет вынести куда-то в отдельный файл, для использования другими модулями

        #region Замена опасных символов
        string SpecSymbolsToView(string text)
        {
            return text.Replace("&", "&amp;")   // Очень важно, чтобы замена & была в самом начале
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#x27;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\r\n", "<br>")
                       .Replace("\\", "&#x5C")
                       .Replace(" ", "&nbsp;");
        }

        string SpecSymbolsToEdit(string text)
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
            return text
                       // Жирный текст
                       .Replace("[b]", "<b>")
                       .Replace("[/b]", "</b>")
                       // Наклонный текст
                       .Replace("[i]", "<i>")
                       .Replace("[/i]", "</i>")
                       // Подчеркнутый текст
                       .Replace("[u]", "<u>")
                       .Replace("[/u]", "</u>")
                       // Вставка изображения ссылкой
                       .Replace("[img]", "<img style=\"max-width:1000px; height:auto;\" src=") // max-width надо как-нибудь привязать к ширине таблицы !!!!!!!!!!!!!!!!!!!!!!!
                       .Replace("[/img]", ">");
        }
        #endregion
    }
}
