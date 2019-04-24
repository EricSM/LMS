using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : CommonController
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            char[] charSplit = { ' ' };  // for use in delimiting the Semester into year and season

            var query = from s in db.Students.Where(s1 => s1.UId == uid)
                        join e in db.Enrolled
                        on s.UId equals e.StudentId
                        into studentEnrolled

                        from se in studentEnrolled
                        join c in db.Classes
                        on se.ClassId equals c.ClassId
                        into enrolledClasses

                        from ec in enrolledClasses
                        join co in db.Courses
                        on ec.CatalogId equals co.CatalogId
                        select new
                        {
                            subject = co.Subject,
                            number = co.Number,
                            name = co.Name,
                            season = ec.Semester.Split(charSplit, 2)[0],
                            year = ec.Semester.Split(charSplit, 2)[1],
                            grade = se.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            char[] charSplit = { ' ' };  // for use in delimiting the Semester into year and season

            var query = from s in db.Submissions.Where(s1 => s1.Student == uid)
                        join a in db.Assignments
                        on s.AId equals a.AId
                        into submitAssignments

                        from sa in submitAssignments
                        join ac in db.AssignmentCategories
                        on sa.Category equals ac.AssignCatId
                        into submitAssCat

                        from sac in submitAssCat
                        join c in db.Classes.Where(c1 => c1.Semester.Split(charSplit, 2)[0] == season &&
                        c1.Semester.Split(charSplit, 2)[1] == year.ToString())
                        on sac.ClassId equals c.ClassId
                        into submitClass

                        from sc in submitClass
                        join co in db.Courses.Where(co1 => co1.Subject == subject && co1.Number == num)
                        on sc.CatalogId equals co.CatalogId
                        select new
                        {
                            aname = sa.Name,
                            cname = sac.Name,
                            due = sa.DueDate,
                            score = sa.Submissions
                        };


            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            char[] charSplit = { ' ' };  // for use in delimiting the Semester into year and season
            
            // Check if this Assignment Submission already exists (with identical contents)
            var query = from s in db.Submissions.Where(s1 => s1.Contents == contents && s1.Student == uid)
                        join a in db.Assignments.Where(a1 => a1.Name == asgname)
                        on s.AId equals a.AId
                        into AssignmentSubmission

                        from asu in AssignmentSubmission
                        join ac in db.AssignmentCategories.Where(ac1 => ac1.Name == category)
                        on asu.Category equals ac.AssignCatId
                        into assCat

                        from aca in assCat
                        join c in db.Classes.Where(c1 => c1.Semester.Split(charSplit, 2)[0] == season &&
                        c1.Semester.Split(charSplit, 2)[1] == year.ToString())
                        on aca.ClassId equals c.ClassId
                        into submissionClass

                        from sc in submissionClass
                        join co in db.Courses.Where(co1 => co1.Number == num && co1.Subject == subject)
                        on sc.CatalogId equals co.CatalogId
                        select sc;


            // If submission already exists, return false
            if (query.Any())
                return Json(new { success = false });

            // if the Assignment hasn't been created by a professor with the specified parameters, return false
            var query2 = from a in db.Assignments.Where(a1 => a1.Name == asgname)
                         join ac in db.AssignmentCategories.Where(ac1 => ac1.Name == category)
                         on a.Category equals ac.AssignCatId
                         into assCat

                         from aca in assCat
                         join c in db.Classes.Where(c1 => c1.Semester.Split(charSplit, 2)[0] == season &&
                         c1.Semester.Split(charSplit, 2)[1] == year.ToString())
                         on aca.ClassId equals c.ClassId
                         into submissionClass

                         from sc in submissionClass
                         join co in db.Courses.Where(co1 => co1.Number == num && co1.Subject == subject)
                         on sc.CatalogId equals co.CatalogId
                         select a.AId;

            if (!query2.Any())
                return Json(new { success = false });
            
            // Otherwise, add new Submission to database and return true.
            else
            {
                uint assignmentID = 0;
                uint.TryParse(query2.ToString(), out assignmentID);

                // Create new Submission object
                Submissions submission = new Submissions();
                submission.Contents = contents;
                submission.Student = uid;
                submission.Time = DateTime.Now;
                submission.Score = 0;
                submission.AId = assignmentID;
                
                db.Submissions.Add(submission);
                db.SaveChanges();
                
                return Json(new { success = true });
            }
            
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            char[] charSplit = { ' ' };  // for use in delimiting the Semester into year and season

            // Check to see if this student is already in the class
            var query = from s in db.Students.Where(s1 => s1.UId == uid)
                        join e in db.Enrolled
                        on s.UId equals e.StudentId
                        into enrolledStudents

                        from es in enrolledStudents
                        join c in db.Classes.Where(c1 => c1.Semester.Split(charSplit, 2)[0] == season &&
                        c1.Semester.Split(charSplit, 2)[1] == year.ToString())
                        on es.ClassId equals c.ClassId
                        into enrolledInClass

                        from ec in enrolledInClass
                        join co in db.Courses.Where(co1 => co1.Number == num && co1.Subject == subject)
                        on ec.CatalogId equals co.CatalogId
                        select ec.ClassId;

            if (query.Any())
                return Json(new { success = false });

            else
            {
                uint classID = 0;
                uint.TryParse(query.ToString(), out classID);

                Enrolled enroll = new Enrolled();
                enroll.StudentId = uid;
                enroll.ClassId = classID;

                return Json(new { success = true });
            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {

            return Json(null);
        }

        /*******End code to modify********/

    }
}