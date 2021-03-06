﻿using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Web.Mvc;
using System.Web.Security;
using ChatApp.Filter;
using FormsAuthenticationExtensions;
using Model;

namespace ChatApp.Controllers
{
    [Authorize]
    [NitishAuthentication]

    public class UsersController : Controller
    {
        private ChatAppContext db = new ChatAppContext();

        [Authorize(Roles="Admin")]
        public ActionResult Index()
        {
            var users = db.Users.Include(u => u.Role);
            return View(users.ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

 
        //public ActionResult Create()
        //{
        //    ViewBag.RoleId = new SelectList(db.Roles, "ID", "RoleName");
        //    return View();
        //}

        //// POST: Users/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ID,Firstname,Lastname,Username,Password,ProfileImage,Status,EmailAddress,PhoneNumber,RoleId,Confirmation")] User user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        user.ID = Guid.NewGuid();
        //        //Encrypting password:
        //        string EncryptedPassword =Encryption.encrypt(user.Password);
        //        user.Password = EncryptedPassword;
        //        //Encrypting ConfirmPassword:
        //        string EncryptedCOnfirmPassword = Encryption.encrypt(user.Confirmation);
        //        user.Confirmation= EncryptedCOnfirmPassword;

        //        db.Users.Add(user);
        //        db.SaveChanges();
        //        return RedirectToAction("Login");
        //    }

        //    ViewBag.RoleId = new SelectList(db.Roles, "ID", "RoleName", user.RoleId);
        //    return View(user);
            
        //}


        //Edit Method has been changed...
        //beacase only Authenticated Users can access this action,and every one can change his own Info
        //So that Edit methode DOES NOT need take id in parameters!

        //public ActionResult Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    User user = db.Users.Find(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.RoleId = new SelectList(db.Roles, "ID", "RoleName", user.RoleId);
        //    return View(user);
        //}

        public ActionResult Edit()
        {
            var ticketData = ((FormsIdentity)HttpContext.User.Identity).Ticket.GetStructuredUserData();
            string usrid = ticketData["UserID"];
            var id = Guid.Parse(usrid);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            //ViewBag.RoleId = new SelectList(db.Roles, "ID", "RoleName", user.RoleId);
            Model.AddEditUserViewmodel vm = new AddEditUserViewmodel();
            vm.UserViewModel = user;
           
            return View(vm);
        }


        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Model.AddEditUserViewmodel model)
        {
            if (ModelState.IsValid)
            {
                //Check wether User has Change his ProfileImage OR not??  :
                if (model.ImageFile !=null )
                {

                    //deleting current profileimage from folder:
                    string filePath = Server.MapPath(model.UserViewModel.ProfileImage);
                    if (System.IO.File.Exists(filePath))
                          System.IO.File.Delete(filePath);

                    //seve new profile Imag:
                    string filename = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                    string extention = Path.GetExtension(model.ImageFile.FileName);
                    filename = filename + DateTime.Now.ToString("yymmssfff") + extention;
                    model.ImagePath = "~/UserProfileImage/" + filename; ;
                    filename = Path.Combine(Server.MapPath("~/UserProfileImage/"), filename);
                    model.ImageFile.SaveAs(filename);
                    model.UserViewModel.ProfileImage = model.ImagePath;

                    //Update profile Image on the header :

                        string cookieTitle = model.UserViewModel.Username;
                        Response.Cookies[cookieTitle].Value = model.ImagePath;

                    
                }

                try
                {
                    //Edit Statments goes here:
                    db.Entry(model.UserViewModel).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MyClasses", "Classes");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "ویرایش اطلاعات با خطا مواجه شده است.ممکن است حجم عکس شما بیشتر از 2 مگابایت باشد.دوباره تلاش کنید");
                    return View(model);
                }
               
            }
            return View(model);

        }









        //=Delete Account

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        //****************************************************************************
        //****************************************************************************
        public User FindUserById(Guid? id)
        {
            if (id == null)
                  return null;

            User user = db.Users.Find(id);

            if (user == null)
                  return null;


            return user;
        }


      

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

       
    }
}
