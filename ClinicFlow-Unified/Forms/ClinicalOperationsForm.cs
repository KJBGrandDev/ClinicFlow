using ClinicFlow_Unified.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace ClinicFlow_Unified.Forms
{
    public partial class ClinicalOperationsForm : Form
    {
        public ClinicalOperationsForm()
        {
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
        }
    }
}
