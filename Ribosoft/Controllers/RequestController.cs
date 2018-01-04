using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ribosoft.Models;

namespace Ribosoft.Controllers
{
    public class RequestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new RequestViewModel()
            {
                TargetRegions = new TargetRegion[] {
                    new TargetRegion(1, "5'UTR", false),
                    new TargetRegion(2, "Open Reading Frame (ORF)", false),
                    new TargetRegion(3, "3'UTR", false)
                }
            });
        }

        [HttpPost]
        public IActionResult Index(RequestViewModel model)
        {
            return Content($"Structure: {model.RibozymeStructure}\nSequence: {model.InputSequence}\nTest: {model.TargetRegions[0].Selected}");
        }
    }
}