using ClinicFlow_Unified.Utilities;
#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Services;
namespace ClinicFlow_Unified.Forms;

public partial class PatientHistoryForm : Form
{
    private readonly int _patientId;
    private readonly PatientHistoryService _service = new();
    private List<PatientEncounterRecord> _encounters = [];

    public PatientHistoryForm(int patientId)
    {
        _patientId = patientId;
        InitializeComponent();
            ClinicTheme.ApplyForm(this);
        ConfigureGrids();
        WireEvents();
        LoadPatientRecord();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            int totalWidth = _mainSplit.Width;
            int desiredTimelineWidth = Math.Max(360, (int)(totalWidth * 0.28));
            if (desiredTimelineWidth > 280 && desiredTimelineWidth < totalWidth - 400)
            {
                _mainSplit.SplitterDistance = desiredTimelineWidth;
            }
        }
        catch { }
    }

    private void ConfigureGrids()
    {
        // 1. Timeline Grid
        _timelineGrid.AutoGenerateColumns = false;
        _timelineGrid.Columns.Clear();

        _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PatientEncounterRecord.FormattedDate),
            HeaderText = "Visit Date / Time",
            Width = 145
        });

        _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PatientEncounterRecord.Diagnosis),
            HeaderText = "Diagnosis",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        // 2. Rx Grid for Selected Encounter
        _rxGrid.AutoGenerateColumns = false;
        _rxGrid.Columns.Clear();

        _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastPrescriptionItem.MedicineName),
            HeaderText = "Medication / Drug Name",
            Width = 280
        });

        _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastPrescriptionItem.Quantity),
            HeaderText = "Qty",
            Width = 60,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastPrescriptionItem.Instructions),
            HeaderText = "Sig / Dosage Instructions",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastPrescriptionItem.FulfillmentChoice),
            HeaderText = "Dispense Location",
            Width = 140
        });

        // 3. Laboratory Grid for Selected Encounter
        _labGrid.AutoGenerateColumns = false;
        _labGrid.Columns.Clear();

        _labGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastLabRequestItem.TestCode),
            HeaderText = "Code",
            Width = 70
        });

        _labGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastLabRequestItem.TestName),
            HeaderText = "Laboratory Test",
            Width = 220
        });

        _labGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastLabRequestItem.RequestStatus),
            HeaderText = "Status",
            Width = 100
        });

        _labGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(PastLabRequestItem.ResultText),
            HeaderText = "Result Findings",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private void WireEvents()
    {
        _timelineGrid.SelectionChanged += TimelineGrid_SelectionChanged;
        _refreshButton.Click += (_, _) => LoadPatientRecord();
        _closeButton.Click += (_, _) => Close();
    }

    private void LoadPatientRecord()
    {
        var result = _service.LoadPatientRecord(_patientId);
        if (!result.Success)
        {
            MessageBox.Show(result.Message, "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (result.Summary != null)
        {
            _lblPatientTitle.Text = $"Patient EHR: {result.Summary.FullName} ({result.Summary.PatientNumber})";
            _lblPatientDetails.Text = $"Age: {result.Summary.Age}  •  Sex: {result.Summary.Sex}  •  Contact: {result.Summary.ContactNumber}  •  Address: {result.Summary.Address}  •  Lifetime Completed Visits: {result.Summary.TotalVisitsCount}";
        }

        _encounters = result.Encounters;
        _timelineGrid.DataSource = _encounters;

        if (_encounters.Count > 0)
        {
            DisplayEncounterSheet(_encounters[0]);
        }
        else
        {
            ClearEncounterSheet();
        }
    }

    private void TimelineGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_timelineGrid.CurrentRow?.DataBoundItem is PatientEncounterRecord encounter)
        {
            DisplayEncounterSheet(encounter);
        }
    }

    private void DisplayEncounterSheet(PatientEncounterRecord enc)
    {
        _lblEncounterInfo.Text = $"Clinical Encounter: {enc.FormattedDate}  •  Attending: {enc.DoctorName} ({enc.Specialty})";
        _txtChiefComplaint.Text = enc.ChiefComplaint;
        _txtDiagnosis.Text = enc.Diagnosis;
        _txtClinicalNotes.Text = string.IsNullOrWhiteSpace(enc.ClinicalNotes) ? "None recorded." : enc.ClinicalNotes;
        _txtRecommendation.Text = string.IsNullOrWhiteSpace(enc.DoctorRecommendation) ? "None recorded." : enc.DoctorRecommendation;

        _rxGrid.DataSource = enc.Prescriptions;
        _labGrid.DataSource = enc.LabRequests;
    }

    private void ClearEncounterSheet()
    {
        _lblEncounterInfo.Text = "No previous encounters recorded for this patient.";
        _txtChiefComplaint.Clear();
        _txtDiagnosis.Clear();
        _txtClinicalNotes.Clear();
        _txtRecommendation.Clear();
        _rxGrid.DataSource = null;
        _labGrid.DataSource = null;
    }
}