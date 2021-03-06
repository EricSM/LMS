﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Professor")]
  public class ProfessorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Students(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
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

    public IActionResult Categories(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
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

    public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      ViewData["uid"] = uid;
      return View();
    }

    /*******Begin code to modify********/


    /// <summary>
    /// Returns a JSON array of all the students in a class.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "dob" - date of birth
    /// "grade" - the student's grade in this class
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
    {
      // Untested
      using (db)
      {
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.Enrolled
                    into enrolled

                    from e in enrolled
                    join s in db.Students
                    on e.StudentId equals s.UId
                    select new
                    {
                      fname = s.FName,
                      lname = s.LName,
                      uid = s.UId,
                      dob = s.Dob,
                      grade = e.Grade
                    };
        return Json(query.ToArray());
      }
    }



    /// <summary>
    /// Returns a JSON array with all the assignments in an assignment category for a class.
    /// If the "category" parameter is null, return all assignments in the class.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The assignment category name.
    /// "due" - The due DateTime
    /// "submissions" - The number of submissions to the assignment
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class, 
    /// or null to return assignments from all categories</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
    {
      // Untested
      using (db)
      {
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.AssignmentCategories
                    into categories

                    from ca in categories
                    where category == null ? true : ca.Name == category
                    join a in db.Assignments
                    on ca.AssignCatId equals a.Category
                    into assignments

                    from assign in assignments
                    select new
                    {
                      aname = assign.Name,
                      cname = ca.Name,
                      due = assign.DueDate,
                      submissions = assign.Submissions.Count()
                    };

        return Json(query.ToArray());
      }
    }


    /// <summary>
    /// Returns a JSON array of the assignment categories for a certain class.
    /// Each object in the array should have the folling fields:
    /// "name" - The category name
    /// "weight" - The category weight
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
    {
      // Untested
      using (db)
      {
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.AssignmentCategories
                    into categories

                    from cat in categories
                    select new
                    {
                      name = cat.Name,
                      weight = cat.Weight
                    };

        return Json(query.ToArray());
      }
    }

    /// <summary>
    /// Creates a new assignment category for the specified class.
    /// If a category of the given class with the given name already exists, return success = false.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false} </returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
    {
      // Untested
      using (db)
      {
        // Check if this category already exists in this class
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.AssignmentCategories
                    into categories

                    from cat in categories
                    where cat.Name == category
                    select cat;

        // if category already exists, return false
        if (query.Any())
          return Json(new { success = false });
        else
        {
          // retieve class of new category
          Classes myClass = (from co in db.Courses
                             where co.Subject == subject && co.Number == num
                             select co.Classes
                             into classes

                             from cl in classes
                             where cl.Semester == season + " " + year
                             select cl).FirstOrDefault();

          // create new assignment category object
          AssignmentCategories newCat = new AssignmentCategories();
          newCat.Class = myClass;
          newCat.Name = category;
          newCat.Weight = (uint)catweight;

          // add it to the database
          db.AssignmentCategories.Add(newCat);
          db.SaveChanges();

          return Json(new { success = true });
        }
      }
    }

    /// <summary>
    /// Creates a new assignment for the given class and category.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="asgpoints">The max point value for the new assignment</param>
    /// <param name="asgdue">The due DateTime for the new assignment</param>
    /// <param name="asgcontents">The contents of the new assignment</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {
      // Untested
      using (db)
      {
        // Check if the assignment already exists in this class category
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.AssignmentCategories
                    into categories

                    from cat in categories
                    where cat.Name == category
                    select cat.Assignments
                    into assigns

                    from a in assigns
                    where a.Name == asgname
                    select a;

        // if category already exists, return false
        if (query.Any())
          return Json(new { success = false });
        else
        {
          // retieve class of new category
          AssignmentCategories myCat = (from co in db.Courses
                                        where co.Subject == subject && co.Number == num
                                        select co.Classes
                                        into classes

                                        from cl in classes
                                        where cl.Semester == season + " " + year
                                        select cl.AssignmentCategories
                                        into categories

                                        from cat in categories
                                        where cat.Name == category
                                        select cat).FirstOrDefault();

          // create new assignment object
          Assignments newAssign = new Assignments();
          newAssign.Name = asgname;
          newAssign.Points = (uint)asgpoints;
          newAssign.Contents = asgcontents;
          newAssign.DueDate = asgdue;
          newAssign.CategoryNavigation = myCat;

          // add it to the database
          db.Assignments.Add(newAssign);
          db.SaveChanges();

          return Json(new { success = true });
        }
      }
    }


    /// <summary>
    /// Gets a JSON array of all the submissions to a certain assignment.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "time" - DateTime of the submission
    /// "score" - The score given to the submission
    /// 
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
    {
      // Untested
      using (db)
      {
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.AssignmentCategories
                    into categories

                    from ca in categories
                    where ca.Name == category
                    join a in db.Assignments
                    on ca.AssignCatId equals a.Category
                    into assignments

                    from assign in assignments
                    where assign.Name == asgname
                    select assign.Submissions
                    into submissions

                    from subs in submissions
                    join stu in db.Students
                    on subs.Student equals stu.UId
                    into stuJoinSubs

                    from j in stuJoinSubs
                    select new
                    {
                      fname = j.FName,
                      lname = j.LName,
                      uid = j.UId,
                      time = subs.Time,
                      score = subs.Score
                    };

        return Json(query.ToArray());
      }
    }


    /// <summary>
    /// Set the score of an assignment submission
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <param name="uid">The uid of the student who's submission is being graded</param>
    /// <param name="score">The new score for the submission</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
    {
      // Untested
      using (db)
      {
        // retreive student's submission
        var query = from co in db.Courses
                    where co.Subject == subject && co.Number == num
                    select co.Classes
                    into classes

                    from cl in classes
                    where cl.Semester == season + " " + year
                    select cl.AssignmentCategories
                    into categories

                    from ca in categories
                    where ca.Name == category
                    join a in db.Assignments
                    on ca.AssignCatId equals a.Category
                    into assignments

                    from assign in assignments
                    where assign.Name == asgname
                    select assign.Submissions
                    into submissions

                    from subs in submissions
                    where subs.Student == uid
                    select subs;

        Submissions s = query.SingleOrDefault();

        // update submission if it exists
        if (s != null)
          s.Score = (uint?)score;
        else
        {
          return Json(new { success = false });
        }
        db.SaveChanges();
      }
      return Json(new { success = true });
    }


    /// <summary>
    /// Returns a JSON array of the classes taught by the specified professor
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester in which the class is taught
    /// "year" - The year part of the semester in which the class is taught
    /// </summary>
    /// <param name="uid">The professor's uid</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
      // Untested
      using (db)
      {
        var query = from co in db.Courses
                    join cl in db.Classes
                    on co.CatalogId equals cl.CatalogId
                    into coJoinCl

                    from j1 in coJoinCl
                    join p in db.Professors
                    on j1.TeacherId equals p.UId
                    into cJoinP

                    from j2 in cJoinP
                    where j2.UId == uid
                    select new
                    {
                      subject = co.Subject,
                      number = co.Number,
                      name = co.Name,
                      season = j1.Semester.Split()[0],
                      year = int.Parse(j1.Semester.Split()[1])
                    };

        return Json(query.ToArray());
      }
    }


    /*******End code to modify********/

  }
}