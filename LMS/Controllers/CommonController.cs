using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        protected Team25LMSContext db;

        public CommonController()
        {
            db = new Team25LMSContext();
        }

        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        public void UseLMSContext(Team25LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            using (db)
            {
                var query =
                  from d in db.Departments
                  select new { subject = d.Subject, name = d.Name };

                return Json(query.ToArray());
            }
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            JsonResult result;
            using (db)
            {
                var query = from d in db.Departments
                            select new
                            {
                                subject = d.Subject,
                                dname = d.Name,
                                courses = from c in db.Courses
                                          join e in db.Departments
                                          on c.Subject equals e.Subject
                                          where c.Subject == d.Subject
                                          select new
                                          {
                                              number = c.CatalogId,
                                              cname = c.Name
                                          }
                            };
                result = Json(query.ToArray());
            }
            return result;
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            char[] charSplit = { ' ' };  // for use in delimiting the Semester into year and season
            JsonResult result;
            using (db)
            {
                var query = from c in db.Courses
                            join c2 in db.Classes
                            on c.CatalogId equals c2.CatalogId
                            into CourseClass

                            from cc in CourseClass
                            join p in db.Professors
                            on cc.TeacherId equals p.UId
                            where c.Subject == subject && c.Number == number
                            select new
                            {
                                season = cc.Semester.Split(charSplit, 2)[0],
                                year = cc.Semester.Split(charSplit, 2)[1],
                                location = cc.Location,
                                start = cc.Start,
                                end = cc.End,
                                fname = p.FName,
                                lname = p.LName
                            };

                result = Json(query.ToArray());
            }
                return result;
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            char[] delimiter = { ' ' };
            string ans = "";
            using (db)
            {
                var query = from a in db.Assignments.Where(a1 => a1.Name == asgname)
                            join ac in db.AssignmentCategories.Where(ac1 => ac1.Name == category)
                            on a.Category equals ac.AssignCatId
                            into Assign

                            from ass in Assign
                            join c in db.Classes.Where(c1 => c1.Semester.Split(delimiter, 2)[0] == season &&
                                c1.Semester.Split(delimiter, 2)[1] == year.ToString())
                            on ass.ClassId equals c.ClassId
                            into AssClass

                            from asc in AssClass
                            join co in db.Courses
                            on asc.CatalogId equals co.CatalogId
                            where co.Subject == subject && co.Number == num
                            select new
                            {
                                contents = a.Contents
                            };
                ans = query.ToString();
            }
                return Content(ans);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            char[] delimiter = { ' ' };
            string ans = "";
            using (db)
            {
                var query = from su in db.Submissions
                            join s in db.Students.Where(s1 => s1.UId == uid)
                            on su.Student equals s.UId
                            into StudentSubmissions

                            from ss in StudentSubmissions
                            join a in db.Assignments.Where(a1 => a1.Name == asgname)
                            on su.AId equals a.AId
                            into StudSubAss

                            from ssa in StudSubAss
                            join ac in db.AssignmentCategories.Where(ac1 => ac1.Name == category)
                            on ssa.Category equals ac.AssignCatId
                            into SSACat

                            from sc in SSACat
                            join cl in db.Classes.Where(cl1 => cl1.Semester.Split(delimiter, 2)[0] == season &&
                                cl1.Semester.Split(delimiter, 2)[1] == year.ToString())
                            on sc.ClassId equals cl.ClassId
                            into ClassAssignments

                            from cla in ClassAssignments
                            join co in db.Courses.Where(co1 => co1.Number == num && co1.Subject == subject)
                            on cla.CatalogId equals co.CatalogId
                            into CourseAssignments
                            select new
                            {
                                submission = su.Contents
                            };
                
                ans = query.ToString();
            }
            return Content(ans);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {

            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}