using System.Windows;
using SharedLogic;

namespace Task2
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private CaseDto? _currentPatient;
        private int? _sessionId;

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

            var (patient, loadError) = await _apiService.GetCaseForStudent(studentId);

            if (patient == null)
            {
                MessageBox.Show(loadError ?? "Could not load case for this student.");
                return;
            }

            _currentPatient = patient;
            _sessionId = await _apiService.StartSessionAsync(patient.Id, studentId);
            DisplayPatient(patient);
        }

        private void DisplayPatient(CaseDto patient)
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
                AllergiesList.Items.Add(a.Allergy);

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

        private async void RegisterIntervention_Click(object sender, RoutedEventArgs e)
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
                if (!string.IsNullOrEmpty(allergy.Allergy) && allergy.Allergy.ToLower().Contains(drug.ToLower()))
                {
                    MessageBox.Show($"⚠️ WARNING: {drug} is contraindicated — patient has a documented allergy!\n\n{allergy.Allergy}",
                        "Allergy Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            PhysiologicalEngine.ApplyEffect(_currentPatient, drug, doseValue, route);

            DisplayPatient(_currentPatient);

            if (_sessionId.HasValue)
                await _apiService.PostActionAsync(_sessionId.Value, drug, doseValue, route ?? "");

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logEntry = $"[{timestamp}] {drug} {dose}mg {route}";
            EventLog.Items.Add(logEntry);
            EventLog.ScrollIntoView(logEntry);
        }
    }
}