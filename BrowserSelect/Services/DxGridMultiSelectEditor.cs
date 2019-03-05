using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace BrowserSelect.Services
{
    public class DxGridMultiSelectEditor
    {
        private readonly GridView _view;
        private bool _lockEvents;

        public DxGridMultiSelectEditor(GridView view)
        {
            _view = view;
            _view.OptionsBehavior.EditorShowMode = EditorShowMode.MouseDownFocused;
            _view.MouseUp += GridView_OnMouseUp;
            _view.CellValueChanged += GridView_OnCellValueChanged;
            _view.MouseDown += GridView_OnMouseDown;
        }

        private void GridView_OnMouseUp(object sender, MouseEventArgs e)
        {
            bool inSelectedCell = GetInSelectedCell(e);
            if (inSelectedCell)
            {
                DXMouseEventArgs.GetMouseArgs(e).Handled = true;
                _view.ShowEditorByMouse();
            }
        }

        private void GridView_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (GetInSelectedCell(e))
            {
                GridHitInfo hi = _view.CalcHitInfo(e.Location);
                if (_view.FocusedRowHandle == hi.RowHandle)
                {
                    _view.FocusedColumn = hi.Column;
                    DXMouseEventArgs.GetMouseArgs(e).Handled = true;
                }
            }
        }

        private void GridView_OnCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            OnCellValueChanged(e);
        }


        private void OnCellValueChanged(CellValueChangedEventArgs e)
        {
            if (_lockEvents)
                return;
            _lockEvents = true;
            SetSelectedCellsValues(e.Value);
            _lockEvents = false;
        }

        private void SetSelectedCellsValues(object value)
        {
            try
            {
                _view.BeginUpdate();
                GridCell[] cells = _view.GetSelectedCells();
                foreach (GridCell cell in cells)
                    _view.SetRowCellValue(cell.RowHandle, cell.Column, value);
            }
            catch (Exception ex) { }
            finally { _view.EndUpdate(); }
        }

        private bool GetInSelectedCell(MouseEventArgs e)
        {
            GridHitInfo hi = _view.CalcHitInfo(e.Location);
            return hi.InRowCell && _view.IsCellSelected(hi.RowHandle, hi.Column);
        }


    }
}
