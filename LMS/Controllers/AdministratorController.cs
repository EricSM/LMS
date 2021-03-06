﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Administrator")]
  public class AdministratorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Department(string subject)
    {
      ViewData["subject"] = subject;
      return View();
    }

    public IActionResult Course(string subject, string num)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }

    /*******Begin code to modify********/

    /// <summary>
    /// Returns a JSON array of all the courses in the given department.
    /// Each object in the array should have the following fields:
    /// "number" - The course number (as in 5530)
    /// "name" - The course name (as in "Database Systems")
    /// </summary>
    /// <param name="subject">The department subject abbreviation (as in "CS")</param>
    /// <returns>The JSON result</returns>
    public IActionResult GetCourses(string subject)
    {
      using (db)
      {
        var query = from c in db.Courses
                    where c.Subject == subject
                    select new { number = c.Number, name = c.Name };
        return Json(query.ToArray());
      }
    }


    


    /// <summary>
    /// Returns a JSON array of all the professors working in a given department.
    /// Each object in the array should have the following fields:
    /// "lname" - The professor's last name
    /// "fname" - The professor's first name
    /// "uid" - The professor's uid
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <returns>The JSON result</returns>
    public IActionResult GetProfessors(string subject)
    {
      using (db)
      {
        var query = from p in db.Professors
                    where p.Subject == subject
                    select new {
                      lname = p.LName,
                      fname = p.FName,
                      uid = p.UId
                    };
        return Json(query.ToArray());
      }
    }



    /// <summary>
    /// Creates a course.
    /// A course is uniquely identified by its number + the subject to which it belongs
    /// </summary>
    /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
    /// <param name="number">The course number</param>
    /// <param name="name">The course name</param>
    /// <returns>A JSON object containing {success = true/false}.
    /// false if the course already exists, true otherwise.</returns>
    public IActionResult CreateCourse(string subject, int number, string name)
    {
      using (db)
      {
        // Check if this course already exists
        var query = from c in db.Courses
                    where c.Subject == subject && c.Number == (ushort) number
                    select c;

        // If course already exists, return false
        if (query.Any())
          return Json(new { success = false });
        // Otherwise, add new course to database and return true.
        else
        {
          // Retrive department of the new course
          Departments dept = db.Departments.FirstOrDefault(d => d.Subject == subject);

          // Create new Course object
          Courses course = new Courses();
          course.Subject = subject;
          course.Number = (ushort)number;
          course.Name = name;
          course.SubjectNavigation = dept;

          db.Courses.Add(course);
          db.SaveChanges();

          return Json(new { success = true });
        }
      }
    }



    /// <summary>
    /// Creates a class offering of a given course.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="number">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="start">The start time</param>
    /// <param name="end">The end time</param>
    /// <param name="location">The location</param>
    /// <param name="instructor">The uid of the professor</param>
    /// <returns>A JSON object containing {success = true/false}. 
    /// false if another class occupies the same location during any time 
    /// within the start-end range in the same semester, or if there is already
    /// a Class offering of the same Course in the same Semester,
    /// true otherwise.</returns>
    public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
    {
      var startTime = new TimeSpan(start.Hour, start.Minute, start.Second);
      var endTime = new TimeSpan(end.Hour, end.Minute, end.Second);

      using (db)
      {
        // Check if this class conflicts with other classes in the same semester or in the same room and time.
        var query = from cl in db.Classes
                    join co in db.Courses
                    on cl.CatalogId equals co.CatalogId
                    where cl.Semester == season + " " + year && ((co.Subject == subject && co.Number == (ushort)number) ||
                    (((cl.Start >= startTime && cl.Start <= endTime) || (cl.End >= startTime && cl.End <= endTime)) && cl.Location == location))
                    select cl;

        // If there are conflicts return false
        if (query.Any())
          return Json(new { success = false });
        // Otherwise add new class to database and return true
        else
        {
          // Retrieve the professor and course of the class
          Professors professor = db.Professors.FirstOrDefault(p => p.UId == instructor);
          Courses course = db.Courses.FirstOrDefault(c => c.Subject == subject && c.Number == (ushort)number);

          // Create new Class object
          Classes newClass = new Classes();
          newClass.Semester = season + " " + year;
          newClass.Start = startTime;
          newClass.End = endTime;
          newClass.Location = location;
          newClass.Teacher = professor;
          newClass.Catalog = course;

          db.Classes.Add(newClass);
          db.SaveChanges();

          return Json(new { success = true });
        }
      }
    }


    /*******End code to modify********/

  }
}