using Assignment_4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Assignment_4.Data
{
    public static class SeedData
    {
        public static async Task Initialize(Dat154Gr2Context db, UserManager<IdentityUser> userManager)
        {
            // If patients already exist, stop — already seeded
            if (db.Patients.Any())
                return;

            // -------------------------------------------------------
            // IDENTITY USERS + CUSTOM USERS
            // -------------------------------------------------------
            // For each account we:
            // 1. Create an IdentityUser via UserManager (handles password hashing, saves to AspNetUsers)
            // 2. Insert a matching row in CustomUsers (for role-based login redirect)

            var accounts = new[]
            {
                new { Email = "teacher1@sim.no", Password = "Teacher1!", Role = "Teacher" },
                new { Email = "teacher2@sim.no", Password = "Teacher2!", Role = "Teacher" },
                new { Email = "student1@sim.no", Password = "Student1!", Role = "Student" },
                new { Email = "student2@sim.no", Password = "Student2!", Role = "Student" },
                new { Email = "student3@sim.no", Password = "Student3!", Role = "Student" },
                new { Email = "admin@sim.no",    Password = "Admin123!", Role = "Admin"   },
            };

            foreach (var account in accounts)
            {
                // Only create Identity user if not already in AspNetUsers
                if (await userManager.FindByEmailAsync(account.Email) == null)
                {
                    var identityUser = new IdentityUser
                    {
                        UserName = account.Email,
                        Email = account.Email,
                        EmailConfirmed = true
                    };

                    // CreateAsync hashes the password and inserts into AspNetUsers
                    await userManager.CreateAsync(identityUser, account.Password);
                }

                // Only create CustomUser if not already there
                if (!db.CustomUsers.Any(u => u.Email == account.Email))
                {
                    db.CustomUsers.Add(new CustomUser
                    {
                        Email = account.Email,
                        Role = account.Role
                    });
                }
            }

            db.SaveChanges();

            // -------------------------------------------------------
            // PATIENTS
            // -------------------------------------------------------
            // StudentId is null — no student assigned yet at seed time
            // CASE-01 is IsActive = true by default

            var patients = new List<Patient>
            {
                new Patient
                {
                    Name             = "Kari Olsen",
                    Age              = "58",
                    Sex              = "Female",
                    Weight           = "78 kg",
                    SystolicBP       = 210,
                    DiastolicBP      = 120,
                    HeartRate        = 102,
                    RespiratoryRate  = 18,
                    OxygenSaturation = 97,
                    Temperature      = 37.2,
                    IsActive         = true,
                    StudentId        = null
                },
                new Patient
                {
                    Name             = "Lars Eriksen",
                    Age              = "44",
                    Sex              = "Male",
                    Weight           = "90 kg",
                    SystolicBP       = 128,
                    DiastolicBP      = 78,
                    HeartRate        = 110,
                    RespiratoryRate  = 26,
                    OxygenSaturation = 90,
                    Temperature      = 37.9,
                    IsActive         = false,
                    StudentId        = null
                },
                new Patient
                {
                    Name             = "Bjørn Haugen",
                    Age              = "74",
                    Sex              = "Male",
                    Weight           = "68 kg",
                    SystolicBP       = 135,
                    DiastolicBP      = 82,
                    HeartRate        = 118,
                    RespiratoryRate  = 16,
                    OxygenSaturation = 96,
                    Temperature      = 36.6,
                    IsActive         = false,
                    StudentId        = null
                }
            };

            db.ChangeTracker.Clear();

            db.Patients.AddRange(patients);
            db.SaveChanges();

            // Use the objects we already have — IDs are populated after SaveChanges
            var kariId = patients[0].Id;
            var larsId = patients[1].Id;
            var bjornId = patients[2].Id;
            // -------------------------------------------------------
            // DIAGNOSES
            // -------------------------------------------------------
            db.Diagnoses.AddRange(
                new Diagnosis { PatientId = kariId, Diagnosis1 = "Hypertensive emergency" },
                new Diagnosis { PatientId = kariId, Diagnosis1 = "Essential hypertension (10 years)" },
                new Diagnosis { PatientId = kariId, Diagnosis1 = "Type 2 diabetes mellitus (5 years)" },
                new Diagnosis { PatientId = kariId, Diagnosis1 = "Dyslipidaemia" },
                new Diagnosis { PatientId = larsId, Diagnosis1 = "Post-operative respiratory deterioration" },
                new Diagnosis { PatientId = larsId, Diagnosis1 = "Colorectal cancer stage II (surgically removed)" },
                new Diagnosis { PatientId = larsId, Diagnosis1 = "Mild obstructive sleep apnoea" },
                new Diagnosis { PatientId = bjornId, Diagnosis1 = "Acute hypoglycaemia (blood glucose 1.9 mmol/L)" },
                new Diagnosis { PatientId = bjornId, Diagnosis1 = "Type 1 diabetes mellitus (40 years, insulin pump)" },
                new Diagnosis { PatientId = bjornId, Diagnosis1 = "Chronic kidney disease stage 3" },
                new Diagnosis { PatientId = bjornId, Diagnosis1 = "Mild cognitive impairment" }
            );

            // -------------------------------------------------------
            // MEDICATIONS
            // -------------------------------------------------------
            db.Medications.AddRange(
                new Medication { PatientId = kariId, Medication1 = "Amlodipine", Dosage = 5, Administration = "Oral" },
                new Medication { PatientId = kariId, Medication1 = "Metformin", Dosage = 1000, Administration = "Oral" },
                new Medication { PatientId = kariId, Medication1 = "Atorvastatin", Dosage = 40, Administration = "Oral" },
                new Medication { PatientId = larsId, Medication1 = "Paracetamol", Dosage = 1000, Administration = "IV" },
                new Medication { PatientId = larsId, Medication1 = "Ketorolac", Dosage = 30, Administration = "IV" },
                new Medication { PatientId = larsId, Medication1 = "Enoxaparin", Dosage = 40, Administration = "Subcutaneous" },
                new Medication { PatientId = larsId, Medication1 = "Ondansetron", Dosage = 4, Administration = "IV" },
                new Medication { PatientId = larsId, Medication1 = "Morphine", Dosage = 4, Administration = "IV" },
                new Medication { PatientId = bjornId, Medication1 = "Insulin pump (continuous)", Dosage = 0, Administration = "Subcutaneous" },
                new Medication { PatientId = bjornId, Medication1 = "Aspirin", Dosage = 75, Administration = "Oral" },
                new Medication { PatientId = bjornId, Medication1 = "Bisoprolol", Dosage = 2.5, Administration = "Oral" },
                new Medication { PatientId = bjornId, Medication1 = "Atorvastatin", Dosage = 40, Administration = "Oral" }
            );

            // -------------------------------------------------------
            // ALLERGIES
            // -------------------------------------------------------
            db.Allergies.AddRange(
                new Allergies { PatientId = kariId, Allergy = "ACE Inhibitors (e.g. Ramipril, Enalapril) — causes angioedema" },
                new Allergies { PatientId = kariId, Allergy = "Penicillin — causes skin rash" },
                new Allergies { PatientId = larsId, Allergy = "NSAIDs (e.g. Ibuprofen, Ketorolac) — causes bronchospasm" },
                new Allergies { PatientId = larsId, Allergy = "Latex — causes contact dermatitis; use latex-free equipment" },
                new Allergies { PatientId = bjornId, Allergy = "No Known Drug Allergies (NKDA)" }
            );

            // -------------------------------------------------------
            // GOALS
            // -------------------------------------------------------
            db.Goals.AddRange(
                new Goal { PatientId = kariId, Goal1 = "Reduce systolic BP below 160 mmHg within 60 minutes" },
                new Goal { PatientId = kariId, Goal1 = "Reach target BP of 140-150/90-100 mmHg without dropping too fast" },
                new Goal { PatientId = kariId, Goal1 = "Check allergy list before administering any medication" },
                new Goal { PatientId = larsId, Goal1 = "Raise SpO2 to at least 94% within 10 minutes using supplemental oxygen" },
                new Goal { PatientId = larsId, Goal1 = "Order chest X-ray and ABG blood test within 20 minutes" },
                new Goal { PatientId = larsId, Goal1 = "Do NOT administer Ketorolac or any NSAID" },
                new Goal { PatientId = bjornId, Goal1 = "Raise blood glucose to at least 5 mmol/L within 15 minutes" },
                new Goal { PatientId = bjornId, Goal1 = "Suspend insulin pump immediately" },
                new Goal { PatientId = bjornId, Goal1 = "Log blood glucose every 15 minutes until stable" }
            );

            db.SaveChanges();
        }
    }
}