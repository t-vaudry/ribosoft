using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ribosoft.Models;

namespace Ribosoft.Controllers
{
    /*! \class HomeController
     * \brief This class is the home page controller, used to return the View of the page.
     */
    [AllowAnonymous]
    public class HomeController : Controller
    {
        /*! \fn Index
         * \brief HTTP GET request for home page
         * \return View for home index
         */
        public IActionResult Index()
        {
            return View();
        }

        /*! \fn About
         * \brief HTTP GET request for about page
         * \return View for about index
         */
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.\n";

            return View();
        }

        /*! \fn Contact
         * \brief HTTP GET request for contact page
         * \return View for contact index
         */
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /*! \fn Error
         * \brief HTTP GET request for error page
         * \return View for error index
         */
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
