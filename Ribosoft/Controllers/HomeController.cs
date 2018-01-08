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
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            SampleDllCall sdc = new SampleDllCall();

            IntPtr outputPtr = IntPtr.Zero;
            int size;

            sdc.Fold("AUGUCUUAGGUGAUACGUGC", out outputPtr, out size);

            if (outputPtr == IntPtr.Zero) {
                // the pointer doesn't point to good memory...
                ViewData["Message"] = "nullptr";
                return View();
            }

            FoldOutput[] decodedData = new FoldOutput[size];

            for (int i = 0; i < size; ++i, outputPtr += i * Marshal.SizeOf<FoldOutput>())
            {
                decodedData[i] = Marshal.PtrToStructure<FoldOutput>(outputPtr);
            }

            ViewData["Message"] = "Your application description page.\n" + decodedData[0].Structure;

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
