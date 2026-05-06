# System architecture project

Three different .NET applications have been made to make a digital patient journal simulation for training nursing students.
- A shared class library is made for the DTO's.
- Task 1 with login and case management is set up as a ASP.NET web application.
- Task 2 with the simulation for students is set up as a WPF Windows application.
- Task 3 with the teacher view of the simulation is set up as a MAUI application.
- Azure is used as a server and database.

## Task 1
Features:
- Teachers can log in, create/edit/delete cases and assign cases to students.
- Students can log in and see a few details of their assigned case.

Limitations:
- Only one case can be assigned to a student at any given time.

## Task 2
Features:
- Upon writing student-id in, the assigned case of the student will load.
- Main action is assigning medicine to patient.
- Some medicine will change the vitals (not all).
- You get a warning if patient is allergic to something and you give it to them.

Limitations:
- No actions other than assigning medicine.
- Some medicine, even when allergic to it, won't change vitals.
- No time-based tasks

## Task 3
Features:
- Teacher can write in student-id and see the actions of the student in Task 2 live.
- Teacher write your own comments, which will be added to a debrief.
- The debrief can be exported as a PDF-file, with student actions + teacher comments.
- Some rule-based checks, at least for giving medicine patient is allergic to.

Limitations:
- Can't view all students at once.
