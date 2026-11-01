using ClinicFlow_Unified.Utilities;
#nullable enable

using System.Windows.Forms;
namespace ClinicFlow_Unified.Forms
{
    public class BaseClinicForm : Form
    {
        public BaseClinicForm()
        {
            ClinicTheme.ApplyForm(this);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is DataGridView dgv)
            {
                ClinicTheme.ApplyGrid(dgv);
            }
        }
    }
}
