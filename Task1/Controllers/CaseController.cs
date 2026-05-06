using Assignment_4.Mapping;
using Assignment_4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment_4.Controllers
{
    public class CaseController : Controller
    {
        private readonly Dat154Gr2Context _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CaseController(Dat154Gr2Context db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult TeacherCases()
        {
            var vm = new TeacherCasesViewModel
            {
                Patients = _db.Patients.OrderBy(p => p.Id).ToList(),
                Students = _db.CustomUsers.Where(u => u.Role == "Student").OrderBy(u => u.Id).ToList()
            };
            return View("CasesTeachers", vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(CaseFormViewModel.ForNewCase());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaseFormViewModel model)
        {
            if (model.Id != 0)
                return BadRequest();
            NormalizeClinicalLists(model);
            if (!ModelState.IsValid)
                return View(model);

            var patient = new Patient();
            model.ApplyToPatient(patient);
            patient.StudentId = null;
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            ReplaceClinicalData(patient.Id, model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(TeacherCases));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _db.Patients
                .AsSplitQuery()
                .Include(p => p.Diagnoses)
                .Include(p => p.Goals)
                .Include(p => p.Allergies)
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
                return NotFound();
            return View(CaseFormViewModel.FromPatient(patient));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CaseFormViewModel model)
        {
            if (model.Id <= 0)
                return BadRequest();
            NormalizeClinicalLists(model);
            if (!ModelState.IsValid)
                return View(model);

            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == model.Id);
            if (patient == null)
                return NotFound();

            model.ApplyToPatient(patient);
            ReplaceClinicalData(patient.Id, model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(TeacherCases));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await DeletePatientAndRelatedAsync(id);
            return RedirectToAction(nameof(TeacherCases));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignStudent(int patientId, string? studentCustomUserId)
        {
            var patient = _db.Patients.FirstOrDefault(p => p.Id == patientId);
            if (patient == null)
                return RedirectToAction(nameof(TeacherCases));

            if (string.IsNullOrWhiteSpace(studentCustomUserId))
            {
                patient.StudentId = null;
            }
            else if (int.TryParse(studentCustomUserId, out var sid))
            {
                foreach (var other in _db.Patients.Where(p => p.StudentId == sid && p.Id != patientId))
                    other.StudentId = null;
                patient.StudentId = sid;
                // Simulation API only serves active cases; activating on assign avoids "no active case" after assignment.
                patient.IsActive = true;
            }

            _db.SaveChanges();
            return RedirectToAction(nameof(TeacherCases));
        }

        public async Task<IActionResult> StudentCases()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser?.Email is not { } email)
                return Challenge();
            var user = _db.CustomUsers.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return View("CasesStudents", new StudentCasePageViewModel());

            var patient = await _db.Patients
                .AsSplitQuery()
                .Include(p => p.Diagnoses)
                .Include(p => p.Goals)
                .Include(p => p.Medications)
                .Include(p => p.Allergies)
                .FirstOrDefaultAsync(p => p.StudentId == user.Id);

            return View("CasesStudents", new StudentCasePageViewModel
            {
                StudentCustomUserId = user.Id,
                Patient = patient
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id)
        {
            var patient = _db.Patients.FirstOrDefault(p => p.Id == id);
            if (patient != null)
            {
                patient.IsActive = !patient.IsActive;
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(TeacherCases));
        }

        [HttpGet("api/case/{studentId}")]
        public IActionResult GetCaseForStudent(int studentId)
        {
            var assigned = _db.Patients
                .Where(p => p.StudentId == studentId)
                .Select(p => new { p.Id, p.IsActive })
                .FirstOrDefault();

            if (assigned == null)
                return NotFound(
                    "No case is assigned to this student id. Use the numeric id from the web app (CustomUsers), not your email.");

            if (!assigned.IsActive)
                return NotFound(
                    "A case is assigned to this student, but it is deactivated. The simulation only loads active cases, so nothing will open until the teacher activates it again in All Cases.");

            var patient = _db.Patients
                .Include(p => p.Medications)
                .Include(p => p.Allergies)
                .Include(p => p.Diagnoses)
                .Include(p => p.Goals)
                .First(p => p.Id == assigned.Id);

            return Ok(CaseMapper.ToDto(patient));
        }

        private static void NormalizeClinicalLists(CaseFormViewModel model)
        {
            model.Diagnoses ??= new List<string>();
            model.Goals ??= new List<string>();
            model.Allergies ??= new List<string>();
            model.Medications ??= new List<MedicationFormRow>();

            if (model.Diagnoses.Count == 0)
                model.Diagnoses.Add("");
            if (model.Goals.Count == 0)
                model.Goals.Add("");
            if (model.Allergies.Count == 0)
                model.Allergies.Add("");
            if (model.Medications.Count == 0)
                model.Medications.Add(new MedicationFormRow());
        }

        private async Task DeletePatientAndRelatedAsync(int patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
            if (patient == null)
                return;

            var sessions = await _db.SimulationSessions.Where(s => s.PatientId == patientId).ToListAsync();
            _db.SimulationSessions.RemoveRange(sessions);

            _db.Diagnoses.RemoveRange(_db.Diagnoses.Where(d => d.PatientId == patientId));
            _db.Goals.RemoveRange(_db.Goals.Where(g => g.PatientId == patientId));
            _db.Medications.RemoveRange(_db.Medications.Where(m => m.PatientId == patientId));
            _db.Allergies.RemoveRange(_db.Allergies.Where(a => a.PatientId == patientId));

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
        }

        private void ReplaceClinicalData(int patientId, CaseFormViewModel model)
        {
            _db.Diagnoses.RemoveRange(_db.Diagnoses.Where(d => d.PatientId == patientId));
            _db.Goals.RemoveRange(_db.Goals.Where(g => g.PatientId == patientId));
            _db.Medications.RemoveRange(_db.Medications.Where(m => m.PatientId == patientId));
            _db.Allergies.RemoveRange(_db.Allergies.Where(a => a.PatientId == patientId));

            foreach (var text in model.Diagnoses.Where(s => !string.IsNullOrWhiteSpace(s)))
                _db.Diagnoses.Add(new Diagnosis { PatientId = patientId, Diagnosis1 = text.Trim() });

            foreach (var text in model.Goals.Where(s => !string.IsNullOrWhiteSpace(s)))
                _db.Goals.Add(new Goal { PatientId = patientId, Goal1 = text.Trim() });

            foreach (var text in model.Allergies.Where(s => !string.IsNullOrWhiteSpace(s)))
                _db.Allergies.Add(new Allergies { PatientId = patientId, Allergy = text.Trim() });

            foreach (var m in model.Medications.Where(m => !string.IsNullOrWhiteSpace(m.Name)))
            {
                var route = string.IsNullOrWhiteSpace(m.Route) ? "Oral" : m.Route.Trim();
                _db.Medications.Add(new Medication
                {
                    PatientId = patientId,
                    Medication1 = m.Name.Trim(),
                    Dosage = m.Dosage,
                    Administration = route
                });
            }
        }
    }
}
