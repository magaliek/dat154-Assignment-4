using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Dispatching;
using SharedLogic;

namespace Task3;

public sealed class LineItem
{
    public string Text { get; set; } = "";
}

public partial class MainPage : ContentPage
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
    private readonly ObservableCollection<LineItem> _eventLines = new();
    private readonly ObservableCollection<LineItem> _allergyLines = new();
    private readonly ObservableCollection<LineItem> _medLines = new();
    private readonly ObservableCollection<LineItem> _diagnosisLines = new();
    private readonly Dictionary<int, SimulationActionDto> _actionById = new();
    private readonly Dictionary<int, TeacherObservationDto> _observationById = new();
    private readonly HashSet<int> _knownActionIds = new();
    private int? _activeSessionId;
    private int? _loadedStudentCustomUserId;
    private IDispatcherTimer? _actionPollTimer;
    private bool _loadingSession;
    private bool _pollInFlight;
    private CaseDto? _loadedCase;

    public MainPage()
    {
        InitializeComponent();
        var baseUri = ApiBaseUrl.AsHttpClientBaseAddress();
        var h = new HttpClientHandler();
        if (ApiBaseUrl.IsLocalhost(baseUri))
            h.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _http = new HttpClient(h) { BaseAddress = baseUri };
        EventLogView.ItemsSource = _eventLines;
        AllergiesView.ItemsSource = _allergyLines;
        MedicationsView.ItemsSource = _medLines;
        DiagnosesView.ItemsSource = _diagnosisLines;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_activeSessionId.HasValue)
            StartActionPollTimer();
    }

    protected override void OnDisappearing()
    {
        StopActionPollTimer();
        base.OnDisappearing();
    }

    private async void OnLoadCaseClicked(object? sender, EventArgs e) => await TryLoadCaseAsync();

    private async void OnAddObservationClicked(object? sender, EventArgs e)
    {
        if (!_activeSessionId.HasValue)
        {
            await DisplayAlertAsync("", "Load a case first", "OK");
            return;
        }

        var text = ObservationEntry.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        var sid = _activeSessionId.Value;
        var body = JsonSerializer.Serialize(new AddObservationDto { Text = text }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var resp = await _http.PostAsync($"api/sessions/{sid}/observations", new StringContent(body, Encoding.UTF8, "application/json"));
        ObservationEntry.Text = "";
        if (resp.IsSuccessStatusCode)
        {
            await PollObservationsAsync(sid, incremental: true);
            MainThread.BeginInvokeOnMainThread(RebuildDebriefEditor);
        }
    }

    private async Task TryLoadCaseAsync()
    {
        if (_loadingSession)
            return;

        if (!int.TryParse(StudentIdEntry.Text?.Trim(), out var studentCustomUserId))
        {
            await DisplayAlertAsync("", "Enter the student id (same as in Task 2 — CustomUsers id).", "OK");
            return;
        }

        _loadingSession = true;
        try
        {
            var resolveResp = await _http.GetAsync($"api/sessions/for-student/{studentCustomUserId}");
            var resolveBody = await resolveResp.Content.ReadAsStringAsync();
            if (!resolveResp.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("", TryReadApiError(resolveBody) ?? "No simulation data for this student yet. They must load a case in Task 2 first.", "OK");
                ClearSessionAndUi();
                return;
            }

            int sessionId;
            try
            {
                sessionId = JsonSerializer.Deserialize<int>(resolveBody, _json);
            }
            catch
            {
                await DisplayAlertAsync("", "Unexpected server response from the API.", "OK");
                ClearSessionAndUi();
                return;
            }

            _loadedStudentCustomUserId = studentCustomUserId;

            if (!await LoadSessionForIdAsync(sessionId))
            {
                await DisplayAlertAsync("", "Load failed", "OK");
                ClearSessionAndUi();
            }
        }
        finally
        {
            _loadingSession = false;
        }
    }

    private string? TryReadApiError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        if (body[0] == '"')
        {
            try
            {
                return JsonSerializer.Deserialize<string>(body, _json);
            }
            catch
            {
                return body;
            }
        }

        return body;
    }

    private async Task<bool> LoadSessionForIdAsync(int sessionId)
    {
        var resp = await _http.GetAsync($"api/sessions/{sessionId}/patient-summary");
        var json = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return false;

        var dto = JsonSerializer.Deserialize<CaseDto>(json, _json);
        if (dto == null)
            return false;

        _activeSessionId = sessionId;
        _loadedCase = dto;
        _knownActionIds.Clear();
        _actionById.Clear();
        _observationById.Clear();
        _eventLines.Clear();

        await PollActionsAsync(sessionId, resetEventLog: true);
        await PollObservationsAsync(sessionId, incremental: false);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ApplyCase(dto);
            RebuildDebriefEditor();
            StartActionPollTimer();
        });

        return true;
    }

    private void ClearSessionAndUi()
    {
        StopActionPollTimer();
        _activeSessionId = null;
        _loadedStudentCustomUserId = null;
        _loadedCase = null;
        _knownActionIds.Clear();
        _actionById.Clear();
        _observationById.Clear();
        _eventLines.Clear();
        DebriefEditor.Text = "";
        ClearPatientUi();
    }

    private void ClearPatientUi()
    {
        PatientNameLabel.Text = "No case loaded";
        PatientInfoLabel.Text = "";
        BpLabel.Text = "--/-- mmHg";
        HrLabel.Text = "-- bpm";
        Spo2Label.Text = "-- %";
        RrLabel.Text = "-- /min";
        TempLabel.Text = "-- °C";
        _allergyLines.Clear();
        _medLines.Clear();
        _diagnosisLines.Clear();
    }

    private void ApplyCase(CaseDto c)
    {
        PatientNameLabel.Text = c.Name;
        PatientInfoLabel.Text = $"Age: {c.Age} | Sex: {c.Sex} | Weight: {c.Weight}";
        BpLabel.Text = $"{c.SystolicBP}/{c.DiastolicBP} mmHg";
        HrLabel.Text = $"{c.HeartRate} bpm";
        Spo2Label.Text = $"{c.OxygenSaturation} %";
        RrLabel.Text = $"{c.RespiratoryRate} /min";
        TempLabel.Text = $"{c.Temperature} °C";

        _allergyLines.Clear();
        foreach (var a in c.Allergies)
            _allergyLines.Add(new LineItem { Text = a.Allergy });

        _medLines.Clear();
        foreach (var m in c.Medications)
            _medLines.Add(new LineItem { Text = $"{m.Medication1} {m.Dosage}mg ({m.Administration})" });

        _diagnosisLines.Clear();
        foreach (var d in c.Diagnoses)
            _diagnosisLines.Add(new LineItem { Text = d.Diagnosis1 });
    }

    private void StartActionPollTimer()
    {
        if (!_activeSessionId.HasValue)
            return;

        StopActionPollTimer();
        _actionPollTimer = Dispatcher.CreateTimer();
        _actionPollTimer.Interval = TimeSpan.FromSeconds(2);
        _actionPollTimer.Tick += OnPollTick;
        _actionPollTimer.Start();
    }

    private void StopActionPollTimer()
    {
        if (_actionPollTimer != null)
        {
            _actionPollTimer.Tick -= OnPollTick;
            _actionPollTimer.Stop();
            _actionPollTimer = null;
        }
    }

    private async void OnPollTick(object? sender, EventArgs e)
    {
        if (!_activeSessionId.HasValue || _pollInFlight)
            return;
        _pollInFlight = true;
        try
        {
            if (_loadedStudentCustomUserId.HasValue)
            {
                var resolveResp = await _http.GetAsync($"api/sessions/for-student/{_loadedStudentCustomUserId.Value}");
                if (resolveResp.IsSuccessStatusCode)
                {
                    var resolveBody = await resolveResp.Content.ReadAsStringAsync();
                    var latestSid = JsonSerializer.Deserialize<int>(resolveBody, _json);
                    if (latestSid != _activeSessionId.Value)
                    {
                        await LoadSessionForIdAsync(latestSid);
                        return;
                    }
                }
            }

            var sid = _activeSessionId.Value;
            await PollActionsAsync(sid, resetEventLog: false);
            await PollObservationsAsync(sid, incremental: true);
            MainThread.BeginInvokeOnMainThread(RebuildDebriefEditor);
        }
        finally
        {
            _pollInFlight = false;
        }
    }

    private static string FormatActionLine(SimulationActionDto a) =>
        $"{a.OccurredAt.LocalDateTime:HH:mm:ss} {a.Kind} {a.Drug} {a.DoseMg} {a.Route}";

    private static string FormatDebriefDateTime(DateTimeOffset dto) =>
        dto.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    private void RebuildDebriefEditor()
    {
        if (!_activeSessionId.HasValue || _loadedCase == null)
        {
            DebriefEditor.Text = "";
            return;
        }

        var sb = new StringBuilder();
        var stu = _loadedStudentCustomUserId?.ToString() ?? "?";
        sb.AppendLine($"Student id: {stu} · {_loadedCase.Name}");
        sb.AppendLine();

        foreach (var a in _actionById.Values.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id))
        {
            sb.AppendLine($"Student action · {FormatDebriefDateTime(a.OccurredAt)} · {a.Kind} {a.Drug} {a.DoseMg} {a.Route}");
            foreach (var dev in (a.Deviations ?? []).OrderBy(x => x.Id))
                sb.AppendLine($"  {dev.RuleName}: {dev.Message}");
        }

        if (_observationById.Count > 0)
            sb.AppendLine();

        foreach (var o in _observationById.Values.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id))
            sb.AppendLine($"Teacher observation · {FormatDebriefDateTime(o.CreatedAt)} · {o.Text}");

        DebriefEditor.Text = sb.ToString();
    }

    private async Task PollActionsAsync(int sessionId, bool resetEventLog)
    {
        try
        {
            var resp = await _http.GetAsync($"api/sessions/{sessionId}/actions");
            if (!resp.IsSuccessStatusCode)
                return;

            var list = JsonSerializer.Deserialize<List<SimulationActionDto>>(await resp.Content.ReadAsStringAsync(), _json) ?? [];
            var ordered = list.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).ToList();

            if (resetEventLog)
            {
                _eventLines.Clear();
                _knownActionIds.Clear();
                _actionById.Clear();
                foreach (var a in ordered)
                {
                    _actionById[a.Id] = a;
                    _knownActionIds.Add(a.Id);
                    _eventLines.Add(new LineItem { Text = FormatActionLine(a) });
                }
            }
            else
            {
                foreach (var a in ordered)
                {
                    _actionById[a.Id] = a;
                    if (_knownActionIds.Add(a.Id))
                        _eventLines.Add(new LineItem { Text = FormatActionLine(a) });
                }
            }
        }
        catch
        {
            // ignore
        }
    }

    private async Task PollObservationsAsync(int sessionId, bool incremental)
    {
        try
        {
            var resp = await _http.GetAsync($"api/sessions/{sessionId}/observations");
            if (!resp.IsSuccessStatusCode)
                return;

            var list = JsonSerializer.Deserialize<List<TeacherObservationDto>>(await resp.Content.ReadAsStringAsync(), _json) ?? [];
            var ordered = list.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToList();

            if (!incremental)
                _observationById.Clear();

            foreach (var o in ordered)
                _observationById[o.Id] = o;
        }
        catch
        {
            // ignore
        }
    }

    private async void OnExportPdfClicked(object? sender, EventArgs e)
    {
        var text = DebriefEditor.Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            await DisplayAlertAsync("", "Nothing to export yet", "OK");
            return;
        }

        if (!_activeSessionId.HasValue || !_loadedStudentCustomUserId.HasValue)
        {
            await DisplayAlertAsync("", "Load a case first (enter student id).", "OK");
            return;
        }

        try
        {
            var stu = _loadedStudentCustomUserId.Value;
            await using var ms = new MemoryStream();
            DebriefPdfExporter.Write(ms, $"Debrief · student {stu}", text);
            ms.Position = 0;

            var result = await FileSaver.Default.SaveAsync($"debrief-student-{stu}.pdf", ms, CancellationToken.None);
            if (result.IsSuccessful && !string.IsNullOrEmpty(result.FilePath))
                await DisplayAlertAsync("", $"Saved to:\n{result.FilePath}", "OK");
            else if (result.IsCancelled)
                return;
            else
                await DisplayAlertAsync("", result.Exception?.Message ?? "Save failed", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("", ex.Message, "OK");
        }
    }
}
