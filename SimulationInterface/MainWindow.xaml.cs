using System.Windows;
using SimulationInterface.Models;

namespace SimulationInterface
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private PatientModel? _currentPatient;

        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void LoadCaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(StudentIdBox.Text, out int studentId))
            {
                MessageBox.Show("Please enter a valid student ID.");
                return;
            }

            var patient = await _apiService.GetCaseForStudent(studentId);

            if (patient == null)
            {
                MessageBox.Show("No active case found for this student.");
                return;
            }

            _currentPatient = patient;
            DisplayPatient(patient);
        }

        private void DisplayPatient(PatientModel patient)
        {
            PatientNameText.Text = patient.Name;
            PatientInfoText.Text = $"Age: {patient.Age} | Sex: {patient.Sex} | Weight: {patient.Weight}";

            BPText.Text = $"{patient.SystolicBP}/{patient.DiastolicBP} mmHg";
            HRText.Text = $"{patient.HeartRate} bpm";
            SpO2Text.Text = $"{patient.OxygenSaturation} %";
            RRText.Text = $"{patient.RespiratoryRate} /min";
            TempText.Text = $"{patient.Temperature} °C";

            AllergiesList.Items.Clear();
            foreach (var a in patient.Allergies)
                AllergiesList.Items.Add(a.Allergy1);

            MedicationsList.Items.Clear();
            DrugComboBox.Items.Clear();
            foreach (var m in patient.Medications)
            {
                MedicationsList.Items.Add($"{m.Medication1} {m.Dosage}mg ({m.Administration})");
                DrugComboBox.Items.Add(m.Medication1);
            }

            DiagnosesList.Items.Clear();
            foreach (var d in patient.Diagnoses)
                DiagnosesList.Items.Add(d.Diagnosis1);
        }

        private void RegisterIntervention_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPatient == null)
            {
                MessageBox.Show("No case loaded.");
                return;
            }

            var drug = DrugComboBox.SelectedItem?.ToString();
            var dose = DoseBox.Text;
            var route = (RouteComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(drug) || string.IsNullOrEmpty(dose) || string.IsNullOrEmpty(route))
            {
                MessageBox.Show("Please fill in all intervention fields.");
                return;
            }

            if (!double.TryParse(dose, out double doseValue))
            {
                MessageBox.Show("Please enter a valid dose.");
                return;
            }

            foreach (var allergy in _currentPatient.Allergies)
            {
                if (allergy.Allergy1 != null && allergy.Allergy1.ToLower().Contains(drug.ToLower()))
                {
                    MessageBox.Show($"⚠️ WARNING: {drug} is contraindicated — patient has a documented allergy!\n\n{allergy.Allergy1}",
                        "Allergy Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            PhysiologicalEngine.ApplyEffect(_currentPatient, drug, doseValue, route);

            DisplayPatient(_currentPatient);

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {drug} {dose}mg {route}";
            EventLog.Items.Add(logEntry);
            EventLog.ScrollIntoView(logEntry);
        }
    }
}