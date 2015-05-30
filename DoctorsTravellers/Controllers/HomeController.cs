using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using DoctorsTravellers.Models;
using System.Data;
using System.Web.Script.Serialization;


namespace DoctorsTravellers.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult HomePage()
        {

            return View();

        }

        [HttpGet]
        public ActionResult SignIn()
        {
            return View("SignIn");
        }

        [HttpPost]
        public ActionResult SignIn(FormCollection collection)
        {
            HomePageServices hps = new HomePageServices();
            int result = hps.CheckIfRegisteredUserHandler(collection);

            string username = collection.Get("username");
            ViewBag.username = result;
            return View("SignInResult");
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View("SignUp");
        }

        [HttpPost]
        public ActionResult SignUp(FormCollection collection)
        {
            HomePageServices hps = new HomePageServices();
            //TODO Check for duplicate username and email
            //TODO encript password
            int result = hps.RegisterInfoPostHandler(collection);

            //TODO if duplicate username/email - show error msg

            // for now assume everything is good
            // if everything is good save username in this session
            Session["userName"] = collection.Get("username");
            return View("HomePage");
        }

        public ActionResult QuestionPage(int qid, string hashtags)
        {

            return View();

        }

        public ActionResult test()//SIMULATES BROWZER REQUESTS BUT WITHOUT BROWZER
        {
            //return RedirectToAction("QuestionSearch", new { question = "Im #pregnant and travelling with a #child and an #elder what #medication should I take with me? we are going to #Ibiza " });//This line is for testing 
            //return RedirectToAction("QuestionPost", new { question = "Im #pregnant and travelling with a #child and an #elder what #medication should I take with me? we are going to #Ibiza " });//This line is for testing 
            //return RedirectToAction("ResponsePost", new { qid = 22, response = "you will need lots of sleeping pills good luck!" });//This line is for testing 
            return RedirectToAction("QuestionPage");//This line is for testing
        }

        public ActionResult QuestionSearch(string question)//SEARCH QUESTION AND RETURNS QUESTION LIST TO SERVER
        {
            HomePageServices hps = new HomePageServices();
            var result = hps.QuestionSearchHandelr(question);
            var temp = new JavaScriptSerializer().Serialize(result);


            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult QuestionPost(string question)//STORES QUESTION TO THE DATA BASE AND RETURNS QID
        {
            int qid = -1;
            HomePageServices hps = new HomePageServices();
            qid = hps.QuestionPostHandelr(question);

            return Json(new { status = "Your Question Has Been Posted!", qid = qid }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ResponsePost(int qid, string response)//STORES RESPONSE TO THE DATA BASE AND RETURNS RID
        {
            int rid = -1;
            HomePageServices hps = new HomePageServices();
            rid = hps.ResponsePostHandelr(qid, response);
            return Json(new { response = rid, table = "response" + qid.ToString() });
        }


    }
}
