using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SfDataGridDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void On_DataGrid_AutoGeneratingRelations(object sender, AutoGeneratingRelationsArgs e)
        {
            e.GridViewDefinition.DataGrid.AutoGenerateRelations = true;
        }

        private void On_DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DoSelection();
        }

        private void DoSelection()
        {
            var empToSelect = ((ViewModel)this.DataContext).Employees.FirstOrDefault(emp => emp.EmployeeID == 17);

            if (empToSelect != null)
            {

                this.dataGrid.SelectedItem = empToSelect;

                var employeeRowIndex = this.dataGrid.ResolveToRowIndex(this.dataGrid.SelectedIndex);


                this.ExpandAndSelectDetailsView(this.dataGrid, employeeRowIndex, 22, "Orders");

#pragma warning disable CS4014
                dataGrid.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var orderGrid = dataGrid.GetDetailsViewGrid(this.dataGrid.ResolveToRecordIndex(employeeRowIndex), "Orders");

                    if (orderGrid != null)
                    {

                        var ordersRowindex = orderGrid.ResolveToRowIndex(orderGrid.SelectedIndex);

                        this.ExpandAndSelectDetailsView(orderGrid, ordersRowindex, 2, "Sales");


                        dataGrid.SelectionController.GetType().GetMethod("ScrollToRowIndex", System.Reflection.BindingFlags.Instance
                            | System.Reflection.BindingFlags.NonPublic).Invoke(dataGrid.SelectionController, new object[] { dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex });
                    }
                });
#pragma warning restore CS4014
            }
        }

        private void ExpandAndSelectDetailsView(SfDataGrid dataGrid, int parentRowIndex, int childRowIndex, string relationalColumn)
        {
            //Checks whether the given index is parent row index or not. 
            bool IsDetailsViewIndex = dataGrid.IsInDetailsViewIndex(parentRowIndex);
            if (IsDetailsViewIndex == true)
                return;
            //Gets the record of the parent row index.
            var record = dataGrid.View.Records[dataGrid.ResolveToRecordIndex(parentRowIndex)];
            //Gets the DetailsViewManager by using Reflection.
            var propertyInfo = dataGrid.GetType().GetField("DetailsViewManager", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            DetailsViewManager detailsViewManager = propertyInfo.GetValue(dataGrid) as DetailsViewManager;
            // Checks whether the parent record has the child grid or not by getting the child source and its count.
            var childSource = detailsViewManager.GetChildSource(record.Data, relationalColumn);
            var GetChildSourceCount = detailsViewManager.GetType().GetMethod("GetChildSourceCount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var ChildSourceCount = GetChildSourceCount.Invoke(detailsViewManager, new object[] { childSource });
            if ((int)ChildSourceCount == 0)
                return;

            //Checks whether the record is Expanded or Collapsed.
            //When it is in the collapsed state, you need to expand the particular DetailsView based on the index.
            if (!record.IsExpanded) dataGrid.ExpandDetailsViewAt(dataGrid.ResolveToRecordIndex(parentRowIndex));
            //Gets the index of the DetailsView.
            int index = 0;
            foreach (var def in dataGrid.DetailsViewDefinition)
            {
                if (def.RelationalColumn == relationalColumn)
                {
                    index = dataGrid.DetailsViewDefinition.IndexOf(def);
                    index = parentRowIndex + index + 1;
                }
            }
            //Brings the parent row in the view. 
            var rowcolumnIndex = new RowColumnIndex(index, 1);
            dataGrid.ScrollInView(rowcolumnIndex);
            //Gets the DetailsViewDataGrid by passing the corresponding rowindex and relation name.
            var detailsViewDataGrid = dataGrid.GetDetailsViewGrid(dataGrid.ResolveToRecordIndex(parentRowIndex), relationalColumn);
            //Checks whether the given index is currently in view or not.
            //When the specified index is not currently in view, you can bring that row in to the view by using the SfDataGrid.ScrollInView method.
            var firstline = dataGrid.GetVisualContainer().ScrollRows.GetVisibleLines().FirstOrDefault(line => line.Region == ScrollAxisRegion.Body);
            var lastline = dataGrid.GetVisualContainer().ScrollRows.GetVisibleLines().LastOrDefault(line => line.Region == ScrollAxisRegion.Body);
            if (firstline.LineIndex >= index || lastline.LineIndex <= index)
            {
                //Brings the details view grid in to the view and sets the child grid's SelectedIndex as the ChildRowIndex.
                if (record != null && record.IsExpanded)
                {
                    if (detailsViewDataGrid == null)
                    {
                        detailsViewManager.BringIntoView(index);
                        detailsViewDataGrid = dataGrid.GetDetailsViewGrid(dataGrid.ResolveToRecordIndex(parentRowIndex), relationalColumn);
                    }
                }
            }

            if (detailsViewDataGrid != null)
                detailsViewDataGrid.SelectedIndex = childRowIndex;
        }
    }
}
