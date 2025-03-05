#region
////////////////////////////////////////////////////////////////////
//                           _ooOoo_                               //
//                          o8888888o                              //
//                          88" . "88                              //
//                          (| ^_^ |)                              //
//                          O\  =  /O                              //
//                       ____/'---'\____                           //
//                     .'  \\|     |//  '.                         //
//                    /  \\|||  :  |||//  \                        //
//                   /  _||||| -:- |||||-  \                       //
//                   |   | \\\  -  /// |   |                       //
//                   | \_|  ''\---/''  |   |                       //
//                   \  .-\__  '-'  ___/-. /                       //
//                 ___'. .'  /--.--\  '. . ___                     //
//               .""'<  '.___\_<|>_/___.'  >'"".                   //
//             | | :  '- \'.;'\ _ /';.'/ - ' : | |                 //
//             \  \ '-.   \_ __\ /__ _/   .-' /  /                 //
//      ========'-.____'-.___\_____/___.-'____.-'========          //
//                           '=---='                               //
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^         //
//             佛祖保佑       永不宕机     永无BUG            	   //
/////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static NDataGridView3;

/// <summary>
/// 2022-02-22  1.优化下面统计
///             2.新增统计单独显示Label
/// 2022-02-4   1.我都不晓得fix bug没有。底层逻辑太混乱了。使用filter和order会触发DataBindComplete。
/// 2022-04-29  1.修复xp下dateTime列月份前面少个0，日期不显示的bug
/// 2022-10-20  1.修改按tab时，footView滚动条不联动
/// 2022-10-28  1.修复某些组件显示时，会自动跳回第一列去的bug
/// 2022-11-11  1.修复多个NDataGridView5存在时，切换丢失汇总表头说明的bug
/// 2023-03-14  1.修复一个计算汇总的一个bug
/// 2023-03-27  1.增加获取焦点，自动触发点击（有些情况会自动增加一行）
/// 2023-07-03  1.修复bug
/// 2023-07-12  1.增加bindFootData支持多行合计
/// 2023-07-14  1.调整-1列，行号列宽度问题
/// 2023-07-26  1.增加多行合计，最后一行单独表头
/// 2023-08-08  1.修复一个数据列和合计列行头宽度的问题
/// 2023-11-30  1.修复自动合计，合计列超过不同列宽度，不会自动适应的问题
/// 2023-12-01  1.修复横向滚动条在最后边，然后将dataView列宽拖小，上面单元格没对齐的问题
/// 2023-12-06  1.调整合计格式化
/// 2023-12-22  1.修复隐藏合计，数据没有滚动条
/// 2024-01-02  1.增加如果超过最大合计行则先似乎滚动条（合计列有纵向滚动条，但是数据没有纵向滚动该条。那么数据的最后1列宽度就有问题。目前替换不了footView的纵向滚动条，调整dataView最后一列的列宽也有问题。目前目前可能只有做一个NDataGridView6才能解决这个问题）
/// 2024-04-19  1.调整手工设置合计时，没有取数据区列的Visible
/// 2024-06-20  1.修复dataView列改变下标，footView不自动改变的问题
/// 2024-08-15  1.修复NDataGridView3调整行号显示性能因为的foorView总计行和dataView行号宽度不一致的问题
/// 2024-09-02  1.修复自定义合计列列宽问题
///             2.修复单元格边框和footView，dataView外边框重叠问题
/// 2024-09-07  1.微改一些合计列显示小问题。强迫症
///             2.调整部分代码样式
///             3.处理一个诡异bug
/// 2024-09-10  1.调整dataView和footView中间间距
///             2.继续修复09-07 3的bug
/// 2024-09-18  1.将2024-01-02   1应用到自定义合计上
/// 2025-01-15  1.修复行号列和合计列超出默认宽度问题
/// 2025-02-17  1.修复在卡券结账单正常，但是在拓展管理就不正常的情况（合计View高度少1px问题）
/// 2025-02-24  1.修复数据未初始化完，就被外部移除引起的bug
/// </summary>
public class NDataGridView5 : Panel {

    private NDataGridView3 dataView;
    private NDataGridView3 footView;

    private bool statistion = false;
    private String headText = "";
    private String summaryText = "";
    private Label toolTipView = null;

    private List<String> stColumns = new List<string>();

    private Dictionary<String, Object> thisTag = new Dictionary<string, object>();


    private int defaultFootViewHeight = 21;
    private int defaultFootViewItemHeight = 21;
    private int maxFootRowCount = 7;

    private bool showFootView = true;

    public bool ShowFootView {
        get {
            return showFootView;
        }

        set {
            footView.Visible = this.showFootView = value;
            if (!value) {
                dataView.ScrollBars = ScrollBars.Both;
            } else {
                dataView.ScrollBars = ScrollBars.Vertical;
            }
        }
    }

    public int MaxFootRowCount { get => maxFootRowCount; set => maxFootRowCount = value; }

    public NDataGridView5() {
        this.Padding = new Padding(1);
        dataView = new NDataGridView3();
        footView = new NDataGridView3();
        toolTipView = new Label();



        footView.Height = defaultFootViewHeight;
        this.BorderStyle = BorderStyle.None;
        footView.Dock = DockStyle.Bottom;

        dataView.Dock = DockStyle.Fill;
        dataView.BorderStyle = footView.BorderStyle = BorderStyle.FixedSingle;

        dataView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        footView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        footView.Height = defaultFootViewHeight;
        dataView.ScrollBars = ScrollBars.Vertical;
        footView.ScrollBars = ScrollBars.Horizontal;
        toolTipView.BringToFront();
        toolTipView.Height = 30;
        toolTipView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        toolTipView.BackColor = System.Drawing.SystemColors.Control;
        toolTipView.Visible = false;

        footView.isShowLineNumber = false;


        this.Controls.Add(dataView);
        this.Controls.Add(footView);
        this.Controls.Add(toolTipView);


    }
    public void initWidget() {
        initWidget(null);
    }
    public void initWidget(DataTable pubInfo) {

        dataView.initWidget(pubInfo);
        footView.initWidget(pubInfo);

        dataView.Name = this.Name + "_dataView";
        footView.Name = this.Name + "_footView";





        dataView.AllowUserToAddRows = false;

        dataView.BorderStyle = BorderStyle.None;
        dataView.ScrollBars = ScrollBars.Vertical;


        dataView.BorderStyle = footView.BorderStyle = BorderStyle.FixedSingle;

        footView.ShowCellToolTips = false;
        footView.AllowUserToAddRows = false;
        footView.AllowUserToResizeColumns = false;
        footView.AllowUserToResizeRows = false;
        footView.Width = dataView.Width - 17;
        footView.ScrollBars = ScrollBars.Horizontal;
        footView.ColumnHeadersVisible = false;
        footView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        footView.AllowUserToResizeColumns = false;
        footView.RowHeadersDefaultCellStyle.Padding = new Padding(1);
        footView.AllowUserToAddRows = false;
        footView.ColumnHeadersVisible = false;

        footView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;


        footView.Dock = DockStyle.Bottom;
        dataView.Dock = DockStyle.Fill;
        footView.Height = defaultFootViewHeight;

        dataView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        footView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        footView.ScrollBars = ScrollBars.Horizontal;
        footView.Margin = new Padding(footView.Margin.Left, 5, footView.Margin.Right, footView.Margin.Bottom);



        toolTipView.BringToFront();
        toolTipView.Height = 30;
        toolTipView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        toolTipView.BackColor = System.Drawing.SystemColors.Control;
        toolTipView.Visible = false;

        footView.isShowLineNumber = false;



        this.BackColor = dataView.BackgroundColor = footView.RowHeadersDefaultCellStyle.SelectionBackColor = footView.RowsDefaultCellStyle.SelectionBackColor = Color.White;
        footView.RowHeadersDefaultCellStyle.SelectionForeColor = footView.RowsDefaultCellStyle.SelectionForeColor = dataView.RowHeadersDefaultCellStyle.ForeColor;




        footView.setStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint);

        footView.setSingleStyle();
        footView.closeEdit();

        dataView.columnHeaderVisibleChanged += (Object oo, DataGridViewColumnEventArgs ee) => {
            if (footView.Columns.Contains(ee.Column.DataPropertyName))
                footView.Columns[ee.Column.DataPropertyName].Visible = ee.Column.Visible;
        };

        dataView.ColumnWidthChanged += delegate (object sender, DataGridViewColumnEventArgs e) {
            if (footView.ColumnCount >= e.Column.Index && footView.ColumnCount != 0) {
                footView.Columns[e.Column.Name].Width = e.Column.Width;
                this.OnResize(new EventArgs());
                this.OnScroll(new ScrollEventArgs(ScrollEventType.SmallDecrement, footView.HorizontalScrollingOffset));
            }
        };
        dataView.Scroll += (oo, ee) => {
            if (ee.NewValue >= 0 && ee.ScrollOrientation == ScrollOrientation.HorizontalScroll && footView.Visible) {
                if (dataView.HorizontalScrollingOffset != footView.HorizontalScrollingOffset)
                    dataView.HorizontalScrollingOffset = footView.HorizontalScrollingOffset;
            }
        };

        dataView.Paint += (oo, ee) => {
            using (Pen pen = new Pen(new SolidBrush(dataView.BackgroundColor))) {
                ee.Graphics.DrawLine(pen, new Point(1, dataView.Height - 1), new Point(dataView.Width - 2, dataView.Height - 1));
            }
        };
     

        footView.Scroll += delegate (Object sender, ScrollEventArgs e) {
            if (e.NewValue >= 0 && e.ScrollOrientation == ScrollOrientation.HorizontalScroll) {
                try {
                    dataView.HorizontalScrollingOffset = e.NewValue;
                } catch { }
            }
        };
        this.SizeChanged += (oo, ee) => {
            setFootViewInfo();
        };


        this.VisibleChanged += (oo, ee) => {
            if (this.Visible) {
                setFootViewInfo();
            }
        };

        dataView.ColumnHeaderMouseClick += delegate (object sender, DataGridViewCellMouseEventArgs e) {
            DataGridViewColumn column = dataView.Columns[e.ColumnIndex];
            DataGridViewColumnHeaderCell header = column.HeaderCell;
            if (header is DataGridViewCheckBoxHeaderCell) {
                DataGridViewCheckBoxHeaderCell checkboxHeader = (DataGridViewCheckBoxHeaderCell)header;
                if (checkboxHeader.clickCheckbox(e.Location))
                    return;
            }
        };


        //dataView.CellFormatting += (oo, ee) => {
        //    if (ee.ColumnIndex == dataView.ColumnCount - 1 && dataView.RowCount>dataView.DisplayedRowCount(false)) {
        //        ee.Value = ee.Value + "　 ";
        //        dataView.Columns[ee.ColumnIndex].HeaderText = dataView.getHeader(ee.ColumnIndex).showName + "　 ";
        //        //dataView.Columns[ee.ColumnIndex].Width = dataView.getHeader(ee.ColumnIndex).showName + "　 ";
        //    }
        //};

        dataView.CellEndEdit += delegate (Object o, DataGridViewCellEventArgs e) {
            if (this.statistion && 0 != stColumns.Count) {
                String cName = dataView.Columns[e.ColumnIndex].DataPropertyName;
                if (stColumns.Contains(cName) && dataView.Columns[e.ColumnIndex].Visible) {
                    if (dataView.isModify(e.RowIndex, e.ColumnIndex)) {
                        calculateFoot();
                    }
                }
            }
        };
        dataView.CurrentCellChanged += (o, e) => {
            // 2023-07-03 1
            if (null != dataView.CurrentCell && footView.RowCount > 0) {
                footView.CurrentCell = footView[dataView.CurrentCell.ColumnIndex, 0];
            }
        };
        dataView.DataBindingComplete += (oo, ee) => {
            if (ee.ListChangedType == ListChangedType.Reset || ee.ListChangedType == ListChangedType.ItemAdded || ee.ListChangedType == ListChangedType.ItemDeleted) {
                if (this.statistion && 0 != stColumns.Count) {
                    if (ee.ListChangedType == ListChangedType.Reset)
                        calculateFoot();
                }
            }
        };
        dataView.UserDeletedRow += delegate (object sender, DataGridViewRowEventArgs e) {
            if (this.statistion && 0 != stColumns.Count) {
                calculateFoot();
            }
        };
        dataView.UserAddedRow += delegate (object sender, DataGridViewRowEventArgs e) {
            if (this.statistion && 0 != stColumns.Count) {
                DataRow row = dataView.getDataRow(e.Row);
                bool isRefresh = false;
                foreach (String column in stColumns) {
                    if (dataView.Columns[column].Visible) {
                        Object obj = row[column];
                        if (null != obj && !DBNull.Value.Equals(obj) && Tools.isDouble(obj)) {
                            isRefresh = true;
                            break;
                        }
                    }
                }
                if (isRefresh)
                    calculateFoot();
            }
        };

        dataView.MouseEnter += delegate (object sender, EventArgs e) {
            toolTipView.Visible = false;
        };
        // 2024-06-20 1
        dataView.ColumnDisplayIndexChanged += (oo, ee) => {
            if (footView.Columns.Contains(ee.Column.DataPropertyName))
                footView.Columns[ee.Column.DataPropertyName].DisplayIndex = ee.Column.DisplayIndex;
        };

        toolTipView.MouseEnter += delegate (object sender, EventArgs e) {
            toolTipView.Visible = false;
        };

        footView.CellPainting += delegate (object sender, DataGridViewCellPaintingEventArgs e) {
            footView.ClearSelection();
            if (e.PaintParts == DataGridViewPaintParts.Focus && e.ColumnIndex != -1) {
                e.Handled = true;
            }
        };

        footView.Click += delegate (Object o, EventArgs e) {
            dataViewfocus();
        };
        footView.GotFocus += delegate (Object o, EventArgs e) {
            dataViewfocus();
        };
        this.GotFocus += delegate (Object o, EventArgs e) {
            if (0 == dataView.RowCount) {

                dataView.Select();
                dataView.Focus();
                dataView.onMouseClick();

                if (1 == dataView.RowCount) {
                    for (int i = 0; i < dataView.ColumnCount; i++) {
                        if (dataView.cellEdit(dataView, 0, i)) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                dataView.selectedAndEdit(0, i);
                            }));
                            break;
                        }
                    }
                }
            }
        };

        footView.MouseLeave += delegate (object o, EventArgs e) {
            toolTipView.Visible = false;
        };

        footView.CellMouseEnter += delegate (object sender, DataGridViewCellEventArgs e) {
            bool isShow = e.ColumnIndex == -1 || stColumns.Contains(footView.Columns[e.ColumnIndex].DataPropertyName);
            Point loc = new Point(0, footView.Location.Y - toolTipView.Height);

            if (isShow) {
                if (e.ColumnIndex == -1) {
                    toolTipView.Text = footView.Rows[0].HeaderCell.ToolTipText;
                    SizeF size = toolTipView.CreateGraphics().MeasureString(toolTipView.Text, toolTipView.Font);
                    toolTipView.Width = (int)( size.Width * 1.2 );
                    toolTipView.Height = (int)( size.Height * 1.2 );
                    loc.X = footView.Location.X + 1;
                } else {
                    Rectangle rect1 = footView.GetColumnDisplayRectangle(e.ColumnIndex, true);
                    String showText = "";
                    if (dataView.mutis.Count != 0) {
                        foreach (MutiHeader header in dataView.mutis) {
                            if (e.ColumnIndex >= header.startIndex && e.ColumnIndex <= header.endIndex) {
                                showText = header.headerText + "\n";
                                break;
                            }
                        }
                    }
                    toolTipView.Text = showText + footView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText;
                    SizeF size = toolTipView.CreateGraphics().MeasureString(toolTipView.Text, toolTipView.Font);
                    toolTipView.Width = (int)( size.Width * 1.2 );
                    toolTipView.Height = (int)( size.Height * 1.7 );
                    if (e.ColumnIndex == footView.Columns.Count - 1)
                        loc.X = rect1.Right - toolTipView.Width - 1;
                    else
                        loc.X = rect1.Left - Math.Abs(footView.Columns[e.ColumnIndex].Width - toolTipView.Width) / 2;
                }
                loc.Y = footView.Location.Y - toolTipView.Height;
                toolTipView.Location = loc;
            }
            toolTipView.Visible = isShow;
        };


    }

    private void setFootViewInfo() {
        int showColumnCount = this.dataView.DisplayedColumnCount(false);
        int columnCount = 0;
        foreach (DataGridViewColumn column in this.dataView.Columns) {
            if (column.Visible)
                columnCount++;
        }
        // 2024-09-10   2
        if (footView.RowCount > 0 && !String.IsNullOrEmpty(headText)) {
            foreach (DataGridViewRow row in footView.Rows) {
                footView.Rows[row.Index].HeaderCell.Value = headText;
            }
            if (footView.RowCount > 1) {
                footView.Rows[footView.RowCount - 1].HeaderCell.Value = summaryText;
            }
        }
        if (showColumnCount == columnCount)
            this.footView.Height = defaultFootViewHeight;
        else {
            this.footView.Height = defaultFootViewHeight + SystemInformation.HorizontalScrollBarHeight + 1;
        }
    }


    private void dataViewfocus() {
        dataView.Focus();
        dataView.Select();
    }

    public NDataGridView3 getDataView() {
        return dataView;
    }

    public NDataGridView3 getFootView() {
        return footView;
    }



    public NDataGridView5 bindDataTable(DataTable datasource) {
        if (datasource.Rows.Count > 0)
            this.dataView.isExecEvent = false;
        dataView.setDataSource(datasource);
        footView.Visible = ShowFootView;
        if (datasource.Rows.Count > 0)
            this.dataView.isExecEvent = true;
        if (this.dataView.RowCount > 0)
            this.dataView.selected(0, 0);
        return this;
    }





    /// <summary>
    /// 显示合计列
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String headText, params String[] columnNames) {
        this.statistion = true;
        this.headText = headText;
        this.stColumns.Clear();
        this.stColumns.AddRange(columnNames);

        footView.clearAll();
        footView.clearColumns();

        footView.addHeader(dataView.getHeaders());
        Control parent = this;
        // 2025-02-24   1
        while (null != parent) {
            parent = parent.Parent;
            if (parent is Form) {
                break;
            }
            if (null == parent)
                return this;
        }
        this.BeginInvoke(new MethodInvoker(() => {
            if (this.IsDisposed)
                return;
            DataTable table = new DataTable();
            Dictionary<String, Object> vals = new Dictionary<string, Object>();
            for (int i = 0; i < dataView.ColumnCount; i++) {
                footView.Columns[i].Width = dataView.Columns[i].Width;
                footView.Columns[i].Frozen = dataView.Columns[i].Frozen;
                footView.Columns[i].Visible = dataView.Columns[i].Visible;
                footView.Columns[i].DefaultCellStyle.Alignment = dataView.Columns[i].DefaultCellStyle.Alignment;
                table.Columns.Add(footView.Columns[i].DataPropertyName);
                if (stColumns.Contains(footView.Columns[i].DataPropertyName))
                    vals.Add(footView.Columns[i].DataPropertyName, "0");
            }

            Tools.addRow(table, vals);
            footView.setDataSource(table);
            int viewHeight = 0;

            foreach (DataGridViewRow row in footView.Rows) {
                // 2025-02-17   1
                if (row.Height + row.DividerHeight > defaultFootViewItemHeight)
                    viewHeight += row.Height + row.DividerHeight;
                else
                    viewHeight += defaultFootViewItemHeight;
            }
            //  2024-01-02   1
            if (footView.RowCount > maxFootRowCount) {
                footView.ScrollBars = ScrollBars.Both;
            } else {
                footView.ScrollBars = ScrollBars.Horizontal;
            }
            footView.Height = defaultFootViewHeight = viewHeight;
            if (footView.RowCount > 0)
                footView.Rows[0].HeaderCell.Value = headText;

            if (footView.RowCount > 0) {
                foreach (String cName in columnNames) {
                    footView.Rows[0].Cells[cName].ToolTipText = dataView.getHeader(cName).showName + "合计：" + vals[cName];
                }
                footView.Rows[0].HeaderCell.Value = headText;
                footView.Rows[0].HeaderCell.ToolTipText = "共0行";
                if (footView.RowHeadersWidth >= dataView.RowHeadersWidth)
                    dataView.RowHeadersWidth = footView.RowHeadersWidth;
                else
                    footView.RowHeadersWidth = dataView.RowHeadersWidth;
            }
        }));
        return this;
    }

    public void calculateFoot() {
        if (!statistion || 0 == stColumns.Count)
            return;

        DataTable table = dataView.getBindDataTable();
        DataTable footTable = footView.getBindDataTable();
        if (null == footTable || footView.Rows.Count == 0)
            return;
        if (null == table)
            return;
        Dictionary<String, Object> vals = new Dictionary<string, Object>();

        foreach (String cName in stColumns) {
            if (!table.Columns.Contains(cName))
                continue;
            if (!dataView.Columns[cName].Visible)
                continue;
            Object cv = null;
            if (table.Columns[cName].DataType == typeof(String)) {
                decimal deci = 0.0M;
                List<DataRow> rows = new List<DataRow>();
                if (!String.IsNullOrEmpty(table.DefaultView.RowFilter))
                    rows.AddRange(table.Select(table.DefaultView.RowFilter,"",DataViewRowState.CurrentRows));
                else {
                    rows.AddRange(table.Select("", "", DataViewRowState.CurrentRows));
                }
                foreach (DataRow row in rows) {
                    Object obj = row[cName];
                    // 2023-03-14.1 修复输入的不是数字类型的bug
                    if (null != obj && !DBNull.Value.Equals(obj) && !String.IsNullOrEmpty(obj.ToString()) && Tools.isDouble(obj.ToString()))
                        deci += Convert.ToDecimal(obj);
                }
                cv = deci;
            } else {
                cv = table.Compute("SUM(" + cName + ")", table.DefaultView.RowFilter);
            }
            if (null == cv || DBNull.Value.Equals(cv))
                cv = "0.0";
            String format = footView.getCellValueFormat(cName, "0.00");
            vals.Add(cName, Convert.ToDecimal(cv).ToString(format));
        }

        int fw = (int)Math.Ceiling(footView.CreateGraphics().MeasureString(headText, footView.Font).Width);
        int dw = fw;
        if (dataView.Rows.Count > 0) {
            dw = dataView.getHeaderCellTextWidth(dataView.Rows.Count - 1);
        }
        int hw = (int)( ( fw > dw ? fw : dw ) * 1.5 ) + 17;
        if (hw < 62)
            hw = 62;


        foreach (String cName in vals.Keys) {
            footTable.Rows[0][cName] = vals[cName];
            // 列宽
            int columnW = dataView.Columns[cName].Width;
            using (Graphics g = footView.CreateGraphics()) {
                // 内容宽
                int valueW = (int)Math.Ceiling(g.MeasureString(footView.Rows[0].Cells[cName].FormattedValue + "", footView.Columns[cName].InheritedStyle.Font).Width) + 5;
                if (valueW > columnW)
                    dataView.Columns[cName].Width = valueW;
            }
            footView.Rows[0].Cells[cName].ToolTipText = dataView.getHeader(cName).showName + "合计：" + vals[cName];
        }
        dataView.RowHeadersWidth = footView.RowHeadersWidth = hw;


        if (footView.RowCount > 0) {
            footView.Rows[0].HeaderCell.Value = headText;
            footView.Rows[0].HeaderCell.ToolTipText = "共" + dataView.RowCount + "行";
        }

    }


    /// <summary>
    /// 显示自定义页脚
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String headText, Dictionary<String, Object> vals) {
        return bindFootTable(headText, headText, vals);
    }


    /// <summary>
    /// 显示自定义页脚
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String headText, List<Dictionary<String, Object>> vals) {

        return bindFootTable(headText, headText, vals);
    }

    /// <summary>
    /// 显示自定义页脚
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String headText, DataTable dataTable) {
        return bindFootTable(headText, headText, dataTable);
    }



    /// <summary>
    /// 显示自定义页脚
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String itemSumaryText, String summaryText, Dictionary<String, Object> vals) {
        List<Dictionary<String, Object>> list = new List<Dictionary<String, Object>>();
        list.Add(vals);
        return bindFootTable(itemSumaryText, summaryText, list);
    }


    /// <summary>
    /// 显示自定义页脚
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String itemSumaryText, String summaryText, List<Dictionary<String, Object>> vals) {

        DataTable table = new DataTable();
        foreach (String key in vals[0].Keys) {
            table.Columns.Add(key, vals[0][key].GetType());
        }
        foreach (Dictionary<String, Object> item in vals) {
            Tools.addRow(table, item);
        }
        return bindFootTable(itemSumaryText, summaryText, table);
    }

    /// <summary>
    /// 显示自定义页脚
    /// </summary>
    /// <param name="headText"></param>
    /// <param name="columnNames"></param>
    /// <returns></returns>
    public NDataGridView5 bindFootTable(String itemSumaryText, String summaryText, DataTable dataTable) {

        // 2024-09-07 3
        // 必须要异步，这个有个很诡异的bug。如果同一页面上有多个NDataView5使用合计。那么只有最有一个生效。
        // 所以这里全部要改成异步，才能正常。
        footView.BeginInvoke(new MethodInvoker(() => {

            this.statistion = false;
            this.headText = itemSumaryText;
            this.summaryText = summaryText;
            this.stColumns = new List<string>();
            footView.clearAll();
            footView.clearColumns();
            footView.ColumnHeadersVisible = false;
            footView.addHeader(dataView.getHeaders());
            DataTable table = dataView.getBindDataTable();
            DataTable footTable = Tools.copyDataTableStruts(table);
            for (int i = 0; i < dataView.ColumnCount; i++) {
                footView.Columns[i].Width = dataView.Columns[i].Width;
                footView.Columns[i].Frozen = dataView.Columns[i].Frozen;
                footView.Columns[i].Visible = dataView.Columns[i].Visible;
                footView.Columns[i].DefaultCellStyle.Alignment = dataView.Columns[i].DefaultCellStyle.Alignment;
            }

            foreach (DataRow row in dataTable.Rows) {
                Tools.addRow(footTable, row);
            }

            footView.setDataSource(footTable);
            int viewHeight = 0;
            foreach (DataGridViewRow row in footView.Rows) {
                if (maxFootRowCount > row.Index) {
                    if (row.Height + row.DividerHeight > defaultFootViewItemHeight)
                        viewHeight += row.Height + row.DividerHeight;
                    else
                        viewHeight += defaultFootViewItemHeight;
                }
                footView.Rows[row.Index].HeaderCell.Value = headText;
            }
            if (viewHeight < defaultFootViewItemHeight)
                viewHeight = defaultFootViewItemHeight;
            if (footView.RowCount > 1) {
                viewHeight -= ( footView.RowCount - 1 );
                footView.Rows[footView.RowCount - 1].HeaderCell.Value = summaryText;
            }

            //  2024-01-02   1
            if (footView.RowCount > maxFootRowCount) {
                footView.ScrollBars = ScrollBars.Both;
            } else {
                footView.ScrollBars = ScrollBars.Horizontal;
            }

            footView.Height = defaultFootViewHeight = viewHeight;
            footView.ClearSelection();

            int dataViewHeaderWidth = 0;
            int footViewHeaderWidth = 0;


            // 2023-07-14  1
            //dataViewHeaderWidth = dataView.RowHeadersWidth;
            // 2025-01-15   1
            // 这里取消上面的计算方法，改用这个。1位长度占6宽，默认基础宽度是33.
            int data0 = ( dataView.RowCount + "" ).Length * 6 + 33;
            int data1 = 0;
            if (dataView.Rows.Count > 0) {
                data1 = dataView.getHeaderCellTextWidth(dataView.Rows.Count - 1) + 33;
            }
            dataViewHeaderWidth = data0 > data1 ? data0 : data1;
            int foot0 = footView.Rows[0].HeaderCell.PreferredSize.Width;
            int foot1 = footView.Rows[footView.RowCount - 1].HeaderCell.PreferredSize.Width;
            footViewHeaderWidth = foot0 > foot1 ? foot0 : foot1;
            int allWidth = 0;
            if (dataViewHeaderWidth > footViewHeaderWidth)
                allWidth = dataViewHeaderWidth;
            else
                allWidth = footViewHeaderWidth;
            //      allWidth += 7;
            if (allWidth < 62)
                allWidth = 62;
            // 2023-08-08 1
            dataView.RowHeadersWidth = footView.RowHeadersWidth = allWidth;


            int showColumnCount = this.dataView.DisplayedColumnCount(false);
            int columnCount = 0;
            foreach (DataGridViewColumn column in this.dataView.Columns) {
                if (column.Visible)
                    columnCount++;
            }
            if (showColumnCount == columnCount)
                this.footView.Height = defaultFootViewHeight;
            else {
                this.footView.Height = defaultFootViewHeight + SystemInformation.HorizontalScrollBarHeight + 1;
            }
            String sdsd = footView.Rows[footView.RowCount - 1].HeaderCell.Value+"";
        }));
        return this;
    }


    public bool containsKey(String tag) {
        return this.thisTag.ContainsKey(tag);
    }

    public NDataGridView5 setTag(String tag, Object obj) {
        if (this.thisTag.ContainsKey(tag))
            thisTag.Remove(tag);
        thisTag.Add(tag, obj);
        return this;
    }
    public Object getTag(String tag) {
        if (thisTag.ContainsKey(tag))
            return thisTag[tag];
        return null;
    }
    public NDataGridView5 clearTag(String tag) {
        if (this.thisTag.ContainsKey(tag))
            thisTag.Remove(tag);
        return this;
    }
    public NDataGridView5 clearTag() {
        thisTag.Clear();
        return this;
    }


    public void clearAllInfo() {
        dataView.clearAll();
        dataView.clearColumns();

        footView.clearAll();
        footView.clearColumns();

        this.statistion = false;
        stColumns = new List<string>();

    }
}