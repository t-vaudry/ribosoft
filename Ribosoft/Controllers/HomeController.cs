using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ribosoft.Models;

namespace Ribosoft.Controllers
{
    public class HomeController : Controller
    {
        [DllImport("RibosoftAlgo")]
        extern static String fold(String seq);

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            String seq = "AUUGCUAGCUAGCAUCGUAGCUGUACUGCAUGACUGAUGGCGGCUAGC";
            String second_struct = fold(seq);
            ViewData["Message"] = "Your application description page.\n" + second_struct;

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
