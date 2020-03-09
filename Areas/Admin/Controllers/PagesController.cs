﻿using CMS.Models.Data;
using CMS.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CMS.Areas.Admin.Controllers
{
	public class PagesController : Controller
	{
		// GET Admin/Pages
		public ActionResult Index()
		{
			// Declaring list of PageVM
			List<PageVM> PagesList;

			using (DB db = new DB())
			{
				// Initializing list
				PagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
			}

			// Return pages to view
			return View(PagesList);
		}

		// GET Admin/Pages/AddPage
		[HttpGet]
		public ActionResult AddPage() {

			return View();
		}

		// POST Admin/Pages/AddPage
		[HttpPost]
		public ActionResult AddPage(PageVM model)
		{
			// Check whether form is valid
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			using (DB db = new DB())
			{
				string slug;
				// Initializing PageDTO
				PageDTO dto = new PageDTO();

				// When page URL is undefined Page title is assigned to Slug
				if (string.IsNullOrWhiteSpace(model.Slug))
				{
					slug = model.Title.Replace(" ", "-").ToLower();
				} else
				{
					slug = model.Slug.Replace(" ", "-").ToLower();
				}

				// Prevent adding same page
				if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
				{
					ModelState.AddModelError("", "Your Page Title or Page URL is already taken");
					return View(model);
				}

				dto.Title = model.Title;
				dto.Slug = slug;
				dto.Body = model.Body;
				dto.Sorting = 1000;
				dto.HasSidebar = model.HasSidebar;

				// Saving DTO
				db.Pages.Add(dto);
				db.SaveChanges();
			}
			TempData["SM"] = "You have added a new Page!";

			return RedirectToAction("AddPage");
		}

		// GET Admin/Pages/EditPage/Id
		[HttpGet]
		public ActionResult EditPage(int id)
		{
			// Declating PageVM
			PageVM model;

			using (DB db = new DB())
			{
				// Fetching Page by ID
				PageDTO dto = db.Pages.Find(id);
				if (dto == null)
				{
					return Content("Page does not exist!");
				}

				// Assigning fetched Page Data to variable model
				model = new PageVM(dto);
			}
			return View(model);
		}

		// POST Admin/Pages/EditPage
		[HttpPost]
		public ActionResult EditPage(PageVM model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			using (DB db = new DB())
			{
				int id = model.Id;
				string slug = "home";

				// Fetch page to edit it
				PageDTO dto = db.Pages.Find(id);
				if (model.Slug != "home")
				{
					if (string.IsNullOrWhiteSpace(model.Slug))
					{
						slug = model.Title.Replace(" ", "-").ToLower();
					}
					else
					{
						slug = model.Slug.Replace(" ", "-").ToLower();
					}
				}

				// Checking if page is unique
				if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
				{
					ModelState.AddModelError("", "Page Title or Page URL is already taken!");
				}

				// Parsing DTO
				dto.Title = model.Title;
				dto.Slug = slug;
				dto.Body = model.Body;
				dto.HasSidebar = model.HasSidebar;

				// Saving Data
				db.SaveChanges();
			}
			// Status variable assignment
			TempData["SM"] = "Page data successfully modified!";

			// Redirect
			return RedirectToAction("EditPage");
		}

		// GET Admin/Page/Details
		public ActionResult Details(int id)
		{
			// Declare PageVM
			PageVM model;
			using (DB db = new DB())
			{
				// Fetching Page by ID
				PageDTO dto = db.Pages.Find(id);

				// Check if Page exists
				if (dto == null)
				{
					return Content("Page does not exist!");
				}

				// Initializing PageVM
				model = new PageVM(dto);
			}
			return View(model);
		}

		// GET Admin/Pages/Delete/Id
		public ActionResult Delete(int id)
		{
			using (DB db = new DB())
			{
				// Fetch Page to delete
				PageDTO dto = db.Pages.Find(id);

				// Deleting selected page
				db.Pages.Remove(dto);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}
	}
}