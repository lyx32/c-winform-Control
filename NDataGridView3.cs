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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

/// <summary>
/// 2022-02-21  1.修改ComboBoxCell的下拉内容框不跟随文本框的宽度
///             2.修复CellValidata缓存问题，优化validataAllData的效率（使用isModify判断）
/// 2022-04-08  1.添加多行表头
/// 2022-04-09  1.处理合并表头的和并列边框也能拖动的问题，
/// 2022-04-13  1.处理合并表头从右往左拖动，合并列和新表头显示不完全bug，
/// 2022-04-15  1.合并表头边框错位及边框表现尽力一致
/// 2022-05-13  1.修复Combobox列宽度不够的ebug
/// 2022-10-14  1.增加文本框输入联动，类似NComboBox
/// 2022-10-20  1.修复类型NComboBox 被拖动滚动条才能看到是加载组件会自动跳回第一列的bug
/// 2022-11-02  1.修复按tab切换下一个编辑单元格的bug
/// 2022-11-16  1.新增下拉输入列是已选择或输入为主
///             2.增加下拉输入列触发最小输入下拉的长度
/// 2022-11-29  1.添加NDataGridViewComboxCell在DroppedDown之前时默认选中第一个（焦点来到这一列会显示选中第一个不通过键盘和鼠标，直接进入下一列时不会触发选中第一个的事件）
/// 2022-11-30  1.完善DataGridViewComboBoxCell 的默认选中及高亮等相关细节
/// 2022-12-14  1.修复combobox的报错
/// 2023-01-10  1.添加可输入的下拉框移除焦点后自动选择第一个
///             2.新增自动添加，删除的快捷键
/// 2023-03-01  1.修复添加可输入的下拉框时，筛选字段是设置的val和show的的bug
///             2.修复可输入的下拉框输入内容时，精确匹配的不再第一位的bug
/// 2023-03-10  1.修复一个bug
/// 2023-03-17  1.修复输入Convert会报错的内容是，无选中行的bug
/// 2023-03-28  1.取消一些不使用及有问题的老方法
/// 2023-04-11  1.调整设置背景色逻辑，增加点击排序后重新设置背景色
/// 2023-04-19  1.增加validataAllData是否强制验证。（主要用于其他地方修改不可见内容，但是输入验证列又没有变化，）
///             2.修改selectedAndEdit方法的事件触发
///             3.修复一个输入格式错误，validate已经拦截下来，但是在选中错误单元格时报错的bug
///             4.修复NDataGridView5获取焦点，自动触发点击事件（增加一行，并选中编辑）但是光标不在的bug
/// 2023-04-20  1.修改自动移除空行时，调用不触发选中行事件
///             2.修改自定义表头是否允许排序
/// 2023-04-21  1.修复输入doule的列，输入不是double，然后点击排序报错
///             2.修复一个addNewRowing返回null，但是还是添加了一行
/// 2023-05-09  1.处理一个遗留的bug
///             2.增加删除拦截
/// 2023-05-17  1.增加一点细节
///             2.修复有DataGridColumn列，但是dataTable没有列时，排序报错
/// 2023-05-22  1.修复细节
/// 2023-05-25  1.增加方向键切换可编辑单元格
///             2.发现一个问题，目前还没思路处理（可筛选下拉框，计算的高度不对的问题）
/// 2023-05-26  1.解决2023-05-25 2 的遗留问题
/// 2023-07-03  1.修复自定义Column没有无参构造函数导致DataGridView底层报错
/// 2023-07-11  1.增加 getModifyColumnNames 
/// 2023-08-21  1.处理设置剪切版内容失败提示
/// 2023-09-12  1.优化背景色绘制性能
/// 2023-09-19  1.增加自定义单元格背景色（性能优化版）
///             2.增加自定义单元格字体色（性能优化版）
///             3.增加移除所有空行
/// 2023-09-22  1.修复某种情况下拉框绘制会把以前点过的也一起绘制
///             2.修复给DtaaGridViewComboboxCell单独设置DataSource各种错乱问题
/// 2023-09-23  1.完善移除空行重载
/// 2023-09-25  1.完善获取被修改列
/// 2023-10-12  1.修复没数据时，点击排序。状态未重置的bug
/// 2023-10-16  1.调整全选表头的宽度
/// 2023-11-30  1.增加addHeder重载
/// 2023-12-01  1.修复点击dataView自动增加一行时，HSrcollOffise不会自动切换的问题
/// 2023-12-06  1.调整NDataGridView5合计内容格式化
/// 2023-12-29  1.调整王卓林电脑上报的错
/// 2024-01-02  1.优化一点点内存泄露的情况
/// 2024-01-03  1.增加读取滚动条显示与否
/// 2024-01-04  1.增加一个defaultCellEdit方法
/// 2024-01-17  1.增加getModifyColumnNames(DataRowState state, DataRow row)
/// 2024-01-24  1.修复在int，double列输入非int，double。然后直接保存报错的bug
///             2.修复1之后，单元格不能切换到下一个，并且无提示的问题
/// 2024-05-06  1.修复输入内容无法格式化（2024-01-24 1）的错误处理（部分validata需要处理类型设置未null后的默认值，如DateTime默认为DateTime.MinValue）
/// 2024-05-08  1.修复DGVComboBox显示System.Data.DataRow的情况，为全部解决
/// 2024-05-21  1.修复莫名其妙的bug（应该是改变背景色，或前景色引起的）
/// 2024-06-05  1.处理（2024-05-06 1）的一种情况（DataColumn是DateTime但是DataGridViewColumn是TextBox）
/// 2024-06-20  1.调整isNewModify()
/// 2024-06-24  1.增加方向键，是否允许直接切换单元格设置
/// 2024-06-27  1.修复Ctrl+C 复制不能复制鼠标选中的本文，只能复制当前单元格的问题
///             2.增加日期列，但是又不需要DateTimePicker组件的列（拓展的需求日期手工输入，但是又要按照日期验证）
/// 2024-07-01  1.修复渲染背景色时，没有dataRow报错的问题。
/// 2024-07-03  1.几乎完美的处理了addTextDateTimeHeader类型（有些代码可能有优化空间）
/// 2024-07-04  1.修复一个addTextDateTimeHeader格式报错的问题（我都不晓得运行逻辑是啥子了，反正这样就可以按照预期执行。等待后续和2024-07-03 1一起修复）
/// 2024-07-08  1.修复列，又是新时间列，又是输入下拉列。判断不正确的问题（具体是拓展的内容更单）
///             2.屏蔽一个底层错误，不晓得啥子原因报的，直接屏蔽。页面在操作一下就行了。
/// 2024-07-12  1.修复改变列宽，其他的列的联动下拉框显示出来的bug
/// 2024-07-16  1.取消不可输入不纳入是否修改验证
/// 2024-08-01  1.增加addCellDataSource方法，用于优化addHeaderDataSource。（列多了，没一行的dtasource都会重新赋值）
/// 2024-08-02  1.修复某些情况下可输入下拉列验证失败
/// 2024-08-13  1.优化显示行号性能（主要是addLineNumber的问题）但是以前1，2，3你把2删除了，他3不不会变成2。优化后会变成2
/// 2024-09-02  1.优化NDataGridView5自定义合计列合计列宽问题
///             2.取消序号列和表头列的单元格边框和外边框重叠问题
/// 2024-09-10  1.莫名奇妙报非线程安全。增加几个线程安全处理
/// 2024-09-12  1.同上
/// 2024-09-14  1.增加触发事件
///             2.处理关闭编辑，全选和复选还可以操作的bug
/// 2024-11-01  1.优化全选表头全选效率
/// 2024-11-20	1.添加自动清理已存在的可输入下拉框设置
/// 2024-12-19  1.增加getModifyDataRows方法
///             2.增加getModifyColumnNamesForLowerOrUpper方法
/// 2025-01-14  1.优化判断单元格是否可以编辑
///             2.增加可输入下拉框列获取下拉数据
/// 2025-02-21  1.修复一个诡异bug。处理单元格切换发现的问题
/// </summary>
public class NDataGridView3 : DataGridView {

    public Dictionary<String, Panel> input_combobox_columns = new Dictionary<string, Panel>();
    public Color editCellBackColor = System.Drawing.SystemColors.Window;
    public Color noEditCellBackColor = System.Drawing.Color.LemonChiffon;
    // 在最后一列是否允许自动添加一行
    public Boolean isAutoAppendRow = false;
    public bool isExecEvent = true;
    public bool isCheckboxColumnAutoSort = false;
    // 按左右建，是否切换单元格
    public bool leftAndRightNextCell = true;


    private bool _selectRowHeader = false;
    public DataTable sourceTable;
    private Dictionary<int, String> spaces = new Dictionary<int, string>();
    private Dictionary<String, Object> thisTag = new Dictionary<string, object>();
    private List<ColumnHeader> headers = new List<ColumnHeader>();
    private Dictionary<String, String> cellValueFormat = new Dictionary<string, string>();
    private long backSelectionTime = 0;
    private bool bindSelecttionEvent = false;
    public bool isPaint = true;
    //private bool isScrollBug = false;
    public bool isShowLineNumber = true;

    public validataCellValue validata = null;
    public SelectonRowChanged selectedRowlistener = null;

    public addRowing addNewRowing = null;
    public delRowing delNewRowing = null;

    public List<MutiHeader> mutis = new List<MutiHeader>();
    public Dictionary<String, String[]> datetimeFormart = new Dictionary<string, string[]>();

    private Regex datetimeFormart_int = new Regex("\\d{4}(.?\\d{2}){2}");
    private Regex noNumber = new Regex("\\D");

    public isCanEdit cellEdit = delegate (NDataGridView3 view, int row, int column) {
        return false;
    };
    public getCellColor cellCustomBackgroundColor = delegate (NDataGridView3 view, int row, int column) {
        return Color.Transparent;
    };
    public getCellColor cellCustomForeColor = delegate (NDataGridView3 view, int row, int column) {
        return Color.Transparent;
    };

    public ColumnHeaderVisibleChanged columnHeaderVisibleChanged = null;

    public bool isExecDefultAdd = false;
    public bool isExecDefultSub = false;


    public CanChangedStateListener canChangedStateListener = delegate (NDataGridView3 view, int columnIndex, bool checkState) {
        //DataTable table = view.getBindDataTable();
        //DGVCheckBoxColumn column = view.Columns[columnIndex] as DGVCheckBoxColumn;
        //Object val = checkState ? column.TrueValue : column.FalseValue;
        //view.isShowLineNumber = false;        
        //DataGridViewAutoSizeColumnMode old = column.AutoSizeMode;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        //view.isPaint = false;
        
        //view.SuspendLayout();
        //DataColumn dc = table.Columns[column.DataPropertyName];        
        //foreach (DataRow row in table.Rows) {
        //    row[dc] = val;
        //}
        //view.isPaint = true;
        //view.isShowLineNumber = true;
        //column.AutoSizeMode = old;
        //view.ResumeLayout(false);

        return true;
    };

    public bool SelectRowHeader {
        get {
            return _selectRowHeader;
        }

        set {
            this._selectRowHeader = value;
        }
    }

    public bool IsShowHScrollBar {
        get {
            foreach (Control ctrl in this.Controls) {
                if (ctrl is HScrollBar) {
                    return ctrl.Visible;
                }
            }
            return false;
        }
    }
    public bool IsShowVScrollBar {
        get {
            foreach (Control ctrl in this.Controls) {
                if (ctrl is VScrollBar) {
                    return ctrl.Visible;
                }
            }
            return false;
        }
    }

    public delegate CellValidata validataCellValue(NDataGridView3 view, int rowIndex, int columnIndex, Object value);
    public delegate void SelectonRowChanged(NDataGridView3 view, int rowIndex);
    public delegate Dictionary<String, Object> addRowing(NDataGridView3 view);
    public delegate bool delRowing(NDataGridView3 view, int row);
    public delegate bool isCanEdit(NDataGridView3 view, int row, int column);
    public delegate Color getCellColor(NDataGridView3 view, int row, int column);
    public delegate bool CanChangedStateListener(NDataGridView3 view, int columnIndex, bool checkState);
    public delegate bool comboxValueChangedListener(NDataGridView3 view, int row, int column, ComboBox combox, DataRow comboboxSelDataRow);
    public delegate bool checkChangedListener(NDataGridView3 view, int row, int column, bool checkChangedValue);
    public delegate void ColumnHeaderVisibleChanged(Object sender, DataGridViewColumnEventArgs e);



    public NDataGridView3() {
        spaces.Add(0, "");
        spaces.Add(1, " ");
        spaces.Add(2, "  ");
        spaces.Add(3, "   ");
        spaces.Add(4, "    ");
        spaces.Add(5, "     ");
        spaces.Add(6, "      ");
        spaces.Add(7, "       ");
        spaces.Add(8, "        ");
        spaces.Add(9, "         ");
        spaces.Add(10, "          ");
        base.DoubleBuffered = true;
    }
    public NDataGridView3 initWidget() {
        return initWidget(null);
    }
    public NDataGridView3 initWidget(DataTable pubInfo) {

        this.MultiSelect = false;
        

        this.TopLeftHeaderCell.Value = "No";
        this.TopLeftHeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        this.BackgroundColor = System.Drawing.SystemColors.Window;
        this.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        this.DefaultCellStyle.BackColor = System.Drawing.SystemColors.ButtonHighlight;
        //this.DefaultCellStyle.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        this.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        this.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;


        this.RowHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
        this.RowHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.ButtonHighlight;
        //this.RowHeadersDefaultCellStyle.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.RowHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
        this.RowHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
        this.RowHeadersDefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        this.RowHeadersDefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;

        this.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        this.AdvancedRowHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
        this.AdvancedRowHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
        this.AdvancedRowHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdvancedRowHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdjustedTopLeftHeaderBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
        this.AdjustedTopLeftHeaderBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
        this.AdjustedTopLeftHeaderBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdjustedTopLeftHeaderBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
        this.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
        this.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
        this.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
        this.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Single;
        this.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;


        this.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.SystemColors.ButtonHighlight;
        this.RowsDefaultCellStyle.BackColor = System.Drawing.SystemColors.ButtonHighlight;

        this.RowTemplate.Height = 21;
        this.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        //this.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
        this.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
        this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        this.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
        setInputListStyle();


        this.AutoGenerateColumns = false;
        this.ShowCellToolTips = false;
        this.ShowRowErrors = false;
        this.MultiSelect = false;
        this.AllowUserToResizeRows = false;
        this.AllowUserToAddRows = false;
        this.EnableHeadersVisualStyles = false;
        // 用来记录是否修改
        this.CellBeginEdit += NDataGridView_CellBeginEdit;
        // 用来验证是否为空，和输入长度
        this.CellEndEdit += NDataGridView_CellEndEdit;

        // 去空格
        this.CellFormatting += NDataGridView_CellFormatting;
        //this.DataSourceChanged += NDataGridView_DataSourceChanged;
        //this.DataBindingComplete += NDataGridView3_DataBindingComplete;
        this.RowStateChanged += (oo, ee) => {
            if (ee.StateChanged == DataGridViewElementStates.Displayed && isPaint && isShowLineNumber && this.RowCount > 0) {
                ee.Row.HeaderCell.Value = spaces[( this.RowCount + "" ).Length - ( ( ee.Row.Index + 1 ) + "" ).Length] + "" + ( ee.Row.Index + 1 );
            }
        };
        this.KeyPress += NDataGridView_KeyPress;
        this.CellClick += NDataGridView_CellClick;
        this.SelectionChanged += NDataGridView_SelectionChanged;
        this.MouseClick += NDataGridView3_MouseClick;
        this.CellPainting += gridview_CellPainting;
        this.MouseMove += NDataGridView3_MouseMove;
        this.CursorChanged += NDataGridView3_CursorChanged;
        this.ColumnWidthChanged += NDataGridView3_ColumnWidthChanged;
        //this.DataError += NDataGridView3_DataError;
        this.UserDeletingRow += (o, e) => {
            e.Cancel = true;
            if (null != delNewRowing)
                e.Cancel = !delNewRowing(this, e.Row.Index);
        };



        // 绑定一个DataTable
        if (0 != ColumnCount && null == sourceTable) {
            DataTable table = new DataTable();
            Type strType = typeof(String);
            Type decType = typeof(decimal);
            Type intType = typeof(int);
            foreach (ColumnHeader header in getHeaders()) {
                /*
                Type t = null;
                if (header.format == ColumnFormat.DOUBLE)
                    t = decType;
                else if (header.format == ColumnFormat.INT)
                    t = intType;
                else
                    t = strType;
                table.Columns.Add(header.dataName,t);
                */
                table.Columns.Add(header.dataName);
            }
            setDataSource(table);
        }

        this.DataError += (oo,ee) => {
            String name = this.Name;
            String columnName = this.Columns[ee.ColumnIndex].Name;
            Object val = this[ee.ColumnIndex, ee.RowIndex].Value;
            String msg = ee.Exception.Message;
            String sdsds = "";
        };


        this.CurrentCellChanged += delegate (Object o, EventArgs e) {
            if (SelectRowHeader) {
                NDataGridView3 view3 = this;
                if (null != view3.CurrentCell) {
                    view3.CurrentCell.Style.SelectionBackColor = view3.CurrentCell.Style.BackColor;
                    view3.CurrentCell.Style.SelectionForeColor = view3.RowHeadersDefaultCellStyle.ForeColor;
                }
            }
            if (input_combobox_columns.Count > 0) {
                foreach (String comboColumn in input_combobox_columns.Keys) {
                    Panel panel = input_combobox_columns[comboColumn];
                    panel.Width = 0;
                }
            }
        };



        this.EditingControlShowing += (ooooo, eeeee) => {

            if (eeeee.Control is TextBox) {
                String cName = this.CurrentCell.OwningColumn.DataPropertyName;

                if (input_combobox_columns.ContainsKey(cName)) {
                    DataGridViewCell cell = this.CurrentCell;
                    Panel panel = input_combobox_columns[cName];
                    DGVComboBox box = panel.Controls[0] as DGVComboBox;
                    // 2023-05-26 1.
                    Application.DoEvents();
                    Rectangle rect = this.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, true);
                    if (panel.Location.X != rect.X || panel.Location.Y != ( rect.Y + 2 - SystemInformation.BorderSize.Height - SystemInformation.BorderSize.Height ))
                        panel.Location = new Point(rect.X, rect.Y + 2 - SystemInformation.BorderSize.Height - SystemInformation.BorderSize.Height);

                    box.AttendColumnIndex = cell.ColumnIndex;
                    box.AttendRowIndex = cell.RowIndex;
                    box.AttendColumnName = cell.OwningColumn.DataPropertyName;
                    this.setTag("comr", cell.RowIndex);
                    this.setTag("comc", cell.ColumnIndex);
                    TextBox txt = eeeee.Control as TextBox;
                    //txt.TextChanged += Txt_TextChanged;
                    box.SelectedItem = null;
                    panel.Width = rect.Width - 1;
                    panel.Height = rect.Height - 1;
                    box.Text = txt.Text;
                    //DataRow bindRow = this.getDataRow(cell.RowIndex);
                    //box.refreshBindData((bindRow[this.Columns[cell.ColumnIndex].DataPropertyName]+"").Trim());
                    this.BeginInvoke(new MethodInvoker(() => {
                        // 2023-03-10-1 处理在没有焦点时，点击其他列，但是这里又异步获取了焦点，导致焦点错乱，输入报错的问题。
                        if (box.AttendColumnIndex != -1 && box.AttendRowIndex != -1) {
                            box.Focus();
                            box.Select();
                            box.SelectAll();
                        }
                        box.isInputing = false;
                    }));
                } else {
                    TextBox tb = eeeee.Control as TextBox;
                    int columnIndex = this.CurrentCell.ColumnIndex;
                    if (headers.Count > columnIndex) {
                        ColumnHeader header = headers[columnIndex];
                        if (0 != header.maxLength) {
                            tb.MaxLength = header.maxLength;
                        }
                    }
                    eeeee.CellStyle.BackColor = SystemColors.Window;
                }
            }
            //else if (eeeee.Control is ComboBox)
            //{
            //    ComboBox com = eeeee.Control as ComboBox;
            //    DataRow row = sourceTable.Rows[this.CurrentCell.RowIndex];
            //    Object val = row[this.CurrentCell.OwningColumn.DataPropertyName];
            //    if (null == val || DBNull.Value.Equals(val) || String.IsNullOrEmpty(val.ToString()))
            //        com.SelectedValue = null;
            //}
        };


        this.SizeChanged += (oo, ee) => {
            if (input_combobox_columns.Count > 0 && null != this.CurrentCell) {
                DataGridViewCell cell = this.CurrentCell;
                if (input_combobox_columns.ContainsKey(this.CurrentCell.OwningColumn.DataPropertyName)) {
                    Panel panel = input_combobox_columns[this.CurrentCell.OwningColumn.DataPropertyName];
                    Rectangle rect = this.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, true);
                    if (panel.Location.X != rect.X || panel.Location.Y != ( rect.Y + 2 - SystemInformation.BorderSize.Height - SystemInformation.BorderSize.Height ))
                        panel.Location = new Point(rect.X, rect.Y + 2 - SystemInformation.BorderSize.Height - SystemInformation.BorderSize.Height);
                }
            }
        };


        this.ControlRemoved += (oo, ee) => {
            if (ee.Control is Panel) {
                if (input_combobox_columns.Count > 0) {
                    DataGridViewCell cell = this.CurrentCell;
                    if (input_combobox_columns.ContainsKey(this.CurrentCell.OwningColumn.DataPropertyName)) {
                        Panel panel = input_combobox_columns[this.CurrentCell.OwningColumn.DataPropertyName];
                        DGVComboBox box = (DGVComboBox)panel.Controls[0];
                        if (box.Items.Count != 0 && box.DroppedDown) {
                            box.SelectedIndex = 0;
                        }
                        box.AttendColumnIndex = box.AttendRowIndex = -1;
                        box.AttendColumnName = box.Text = "";
                        box.isInputing = true;
                    }
                }
            }
        };

        this.cellEdit = delegate (NDataGridView3 view, int row, int column) {
            return defaultCellEdit(view, row, column);
        };

        return this;
    }



    //private void NDataGridView3_DataError(object sender, DataGridViewDataErrorEventArgs e) {

    //    String sdsdsd = "";
    //}

    protected override void WndProc(ref Message m) {
        if (isPaint) {
            try {
                base.WndProc(ref m);
            } catch (Exception eeeeee) {
               // 2024 - 07 - 08   2.
                if (!eeeeee.Message.Contains("操作无效，原因是它导致对 SetCurrentCellAddressCore 函数的可重入调用。"))
                    throw eeeeee;
            }
        }
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e) {
        if (isPaint)
            base.OnCellPainting(e);

    }

    protected override void OnPaintBackground(PaintEventArgs pevent) {
        if (isPaint)
            base.OnPaintBackground(pevent);
    }

    protected override void OnPaint(PaintEventArgs e) {
        if (isPaint)
            base.OnPaint(e);
    }

    protected override void NotifyInvalidate(Rectangle invalidatedArea) {
        if (isPaint)
            base.NotifyInvalidate(invalidatedArea);
    }


    #region 公布事件
    public void setStyle(ControlStyles style) {
        this.SetStyle(style, true);
        this.UpdateStyles();
    }

    public void targetCellClick(DataGridViewCellEventArgs e) {
        this.OnCellClick(e);
    }
    public void targetCellClick(int row, int column) {
        if (RowCount > row && ColumnCount > column)
            this.OnCellClick(new DataGridViewCellEventArgs(column,row));
    }

    public void targetCellEndEdit(int row, int column) {
        if (RowCount > row && ColumnCount > column)
            this.OnCellEndEdit(new DataGridViewCellEventArgs(column, row));
    }

    public void targetCellBeginEdit(int row, int column) {
        if (RowCount > row && ColumnCount > column)
            this.OnCellBeginEdit(new DataGridViewCellCancelEventArgs(column, row));
    }

    public void targetRowStateChanged(int row, DataGridViewElementStates state) {
        if (RowCount > row)
            this.OnRowStateChanged(row, new DataGridViewRowStateChangedEventArgs(this.Rows[row], state));
    }

    public void targetCellUpdate(int rowIndex, int columnIndex) {
        if (RowCount > rowIndex && ColumnCount>columnIndex)
            this.OnCellValueChanged(new DataGridViewCellEventArgs(columnIndex, rowIndex));
    }
    public void targetCellUpdate(int rowIndex, String columnName ) {
        if(RowCount>rowIndex)
            this.OnCellValueChanged(new DataGridViewCellEventArgs(Columns[columnName].Index, rowIndex));
    }
    //protected override void OnMouseWheel(MouseEventArgs e)
    //{
    //    //if (e.Delta < 0)
    //    //    SendKeys.Send("{DOWN}");
    //    //else
    //    //    SendKeys.Send("{UP}");
    //}
    #endregion



    #region 基本事件处理

    public String getHeaderCellText(int idx) {
        return spaces[( this.RowCount + "" ).Length - ( ( idx + 1 ) + "" ).Length] + "" + ( idx + 1 );
    }
    public int getHeaderCellTextWidth(int idx) {
        String text = getHeaderCellText(idx);
        return (int)Math.Ceiling(this.CreateGraphics().MeasureString(text, this.Font).Width);
    }


    public bool defaultCellEdit(NDataGridView3 view, int row, int column) {
        // 2025-01-14   1
        if (this.EditMode == DataGridViewEditMode.EditProgrammatically)
            return false;
        DataGridViewColumn col = view.Columns[column];
        if (col.Visible) {
            if (col is DGVCheckBoxColumn || col is DGVComboBox) {
                ColumnHeader header = view.getHeader(col);
                return header.isClick;
            }
            return !view.Columns[column].ReadOnly;
        }
        return false;
    }

    protected override void PaintBackground(Graphics g, Rectangle clipBounds, Rectangle gridBounds) {
        g.FillRectangle(new SolidBrush(this.BackgroundColor), gridBounds);
    }
    // 2023-04-20 2
    public void setSortColumn(bool isAllowSort) {
        foreach (DataGridViewColumn cName in this.Columns) {
            if (cName.HeaderCell is NDataGridViewHeaderCell) {
                ( (NDataGridViewHeaderCell)cName.HeaderCell ).isAllowSort = isAllowSort;
            }
        }
    }
    // 2023-04-20 2
    public void setNoSortColumn(params String[] columnNames) {
        Dictionary<String, bool> sortColumn = new Dictionary<string, bool>();
        foreach (String cName in columnNames) {
            sortColumn.Add(cName, true);
        }
        setSortColumn(sortColumn);
    }
    // 2023-04-20 2
    public void setSortColumn(params String[] columnNames) {
        Dictionary<String, bool> sortColumn = new Dictionary<string, bool>();
        foreach (String cName in columnNames) {
            sortColumn.Add(cName, false);
        }
        setSortColumn(sortColumn);
    }
    // 2023-04-20 2
    public void setSortColumn(Dictionary<String, bool> sortColumn) {
        foreach (String cName in sortColumn.Keys) {
            if (this.Columns.Contains(cName) && this.Columns[cName].HeaderCell is NDataGridViewHeaderCell)
                ( (NDataGridViewHeaderCell)this.Columns[cName].HeaderCell ).isAllowSort = sortColumn[cName];
        }
    }


    private void NDataGridView3_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e) {
        if (mutis.Count != 0) {
            foreach (MutiHeader item in mutis) {
                if (e.Column.Index == item.endIndex) {
                    int allWidth = 0;
                    for (int i = item.startIndex; i <= item.endIndex; i++) {
                        allWidth += Columns[i].Width;
                    }
                    int itemWidth = allWidth / ( ( item.endIndex - item.startIndex ) + 1 );
                    for (int i = item.startIndex; i <= item.endIndex; i++) {
                        Columns[i].Width = itemWidth;
                    }
                    break;
                }
            }
        }


        if (input_combobox_columns.Count > 0) {
            //2024-07-12    1.
            if (input_combobox_columns.ContainsKey(e.Column.DataPropertyName) && null != this.CurrentCell && this.CurrentCell.OwningColumn.DataPropertyName.Equals(e.Column.DataPropertyName)) {
                DataGridViewColumn column = e.Column;
                Rectangle rect = this.GetColumnDisplayRectangle(column.Index, true);
                input_combobox_columns[column.DataPropertyName].Width = rect.Width - 1;
                input_combobox_columns[column.DataPropertyName].Left = rect.X;
            } else {
                foreach (String columnName in input_combobox_columns.Keys) {
                    if (!columnName.Equals(e.Column.DataPropertyName))
                        input_combobox_columns[columnName].Width = 0;
                }
            }
            /*
            if (input_combobox_columns.ContainsKey(e.Column.DataPropertyName)) {
                input_combobox_columns[e.Column.DataPropertyName].Width = e.Column.Width - 1;
            } else {
                foreach (DataGridViewColumn column in this.Columns) {
                    if (input_combobox_columns.ContainsKey(column.DataPropertyName) ) {
                        Rectangle rect = this.GetColumnDisplayRectangle(column.Index, true);
                        input_combobox_columns[column.DataPropertyName].Width = rect.Width - 1;
                        input_combobox_columns[column.DataPropertyName].Left = rect.X;
                        if (null != this.CurrentCell && this.CurrentCell.ColumnIndex == column.Index)
                            break;
                    }
                }
            }
            */
        }
    }

    public void filter(String where) {
        filter(where, "");
    }
    public void filter(String where, String order) {
        DataTable table = getBindDataTable();
        if (null != table) {
            table.DefaultView.RowFilter = where;
            if (!String.IsNullOrEmpty(order))
                table.DefaultView.Sort = order;

        }
    }

    /// <summary>
    /// xxx desc or xxx asc
    /// </summary>
    /// <param name="order"></param>
    public void sort(String order) {
        DataTable table = getBindDataTable();
        if (null != table) {
            table.DefaultView.Sort = order;
        }
    }

    public void bindLineNumber() {
        int allLen = ( this.Rows.Count + "" ).Length;
        foreach (DataGridViewRow row in Rows) {
            int curLen = ( ( row.Index + 1 ) + "" ).Length;
            row.HeaderCell.Value = spaces[allLen - curLen] + "" + ( row.Index + 1 );
        }
    }

    public void bindLineNumberForAdd() {
        if (RowCount > 0) {
            DataGridViewRowHeaderCell fistCell = Rows[0].HeaderCell;
            // 如果是插入到第一行，那么则需要全部重新计算
            if (null == fistCell.Value || String.IsNullOrEmpty(fistCell.Value.ToString())) {
                bindLineNumber();
                return;
            }
            DataGridViewRowHeaderCell lastCell = Rows[RowCount - 1].HeaderCell;
            if (null == lastCell.Value || String.IsNullOrEmpty(lastCell.Value.ToString())) {
                lastCell.Value = RowCount + "";
                return;
            }
        }
    }



    public void onMouseClick() {
        NDataGridView3_MouseClick(this, new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 1));
    }



    private void NDataGridView3_MouseClick(object sender, MouseEventArgs e) {
        if (isAutoAppendRow && 0 == this.Rows.Count && EditMode == DataGridViewEditMode.EditOnEnter && Enabled) {
            if (null != addNewRowing) {
                Dictionary<String, Object> dic = addNewRowing(this);
                if (null != dic) {
                    addRow(dic);
                    if (Rows.Count > 0) {
                        foreach (DataGridViewColumn column in this.Columns) {
                            if (cellEdit(this, 0, column.Index)) {
                                selectedAndEdit(0, column.Index);
                                if (null != this.Parent && this.Parent is NDataGridView5) {
                                    ( this.Parent as NDataGridView5 ).getFootView().HorizontalScrollingOffset = 0;
                                }
                                break;
                            }
                        }
                    }
                }
            } else {
                addRow(new Dictionary<String, Object>());
            }
        }
    }

    private void NDataGridView_SelectionChanged(object sender, EventArgs e) {
        if (null != selectedRowlistener && bindSelecttionEvent && isExecEvent) {
            if (null != this.CurrentRow && null != this.CurrentCell && isExecEvent) {
                lock (this) {
                    long now = DateTime.Now.ToFileTimeUtc();
                    int rowIndex = this.CurrentRow.Index;
                    if (now - backSelectionTime > 500 && SelectedRows.Count > 0 && isExecEvent) {
                        backSelectionTime = DateTime.Now.ToFileTimeUtc();
                        selectedRowlistener(this, rowIndex);
                        backSelectionTime = DateTime.Now.ToFileTimeUtc();
                    }
                }
            }
        }
    }




    private void NDataGridView_CellClick(object sender, DataGridViewCellEventArgs e) {
        if (-1 != e.RowIndex && -1 != e.ColumnIndex) {
            if (this.EditMode == DataGridViewEditMode.EditOnEnter) {
                ColumnHeader header = getHeaders()[e.ColumnIndex];
                if (header.type == ColumnType.Checkbox && header.isClick && header.isExecDefaultEvent) {
                    DGVCheckBoxColumn tc = (DGVCheckBoxColumn)this.Columns[e.ColumnIndex];
                    DataGridViewCell cell = this.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (cell.Value.Equals(tc.TrueValue))
                        cell.Value = tc.FalseValue;
                    else
                        cell.Value = tc.TrueValue;

                    getDataRow(cell.OwningRow).EndEdit();
                    BeginInvoke(new MethodInvoker(() => {
                        if (this.CurrentCell == this.Rows[e.RowIndex].Cells[e.ColumnIndex])
                            this.CurrentCell.Value = this.CurrentCell.Value;
                    }));
                }
            }
        }
        if (e.ColumnIndex == -1) {
            if (SelectRowHeader) {
                NDataGridView3 view3 = this;
                view3.CurrentCell = view3.Rows[e.RowIndex].Cells[0];
            }
        }
    }





    #endregion



    #region 处理Ctrl+C 和Tab，回车自动到下一个可编辑的单元格去，如果存在可编辑的下拉框或复选框，则会有点问题。

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
        if (keyData == ( Keys.C | Keys.Control )) {
            if (null != this.CurrentCell) {
                if (this.CurrentCell.OwningColumn.GetType() == typeof(DGVTextBoxColumn)) {
                    // 2024-06-27   1
                    TextBox text = this.EditingControl as TextBox;
                    String copyText = this.CurrentCell.Value + "";
                    if (null != text) {
                        copyText = text.SelectedText;
                    }
                    // 2023-08-21   1
                    try {
                        Clipboard.SetData(DataFormats.Text, copyText);
                    } catch {
                        MessageBox.Show("复制失败，请使用鼠标右键复制！", "复制内容失败！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return true;
                }
            }
        } else if (( keyData == ( Keys.Control | Keys.Subtract ) || ( keyData == ( Keys.Alt | Keys.P ) ) ) && EditMode == DataGridViewEditMode.EditOnEnter && isExecDefultSub) {
            if (this.Rows.Count > 0 && SelectedRows.Count > 0 && null != this.CurrentCell)
                delRow(this.CurrentCell.RowIndex);
        } else if (( keyData == ( Keys.Control | Keys.Add ) || ( keyData == ( Keys.Alt | Keys.L ) ) ) && EditMode == DataGridViewEditMode.EditOnEnter && isExecDefultAdd) {
            if (null != addNewRowing) {
                Dictionary<String, Object> dic = addNewRowing(this);
                if (null != dic) {
                    addRow(dic);
                    foreach (ColumnHeader header in headers) {
                        if (header.type == ColumnType.TextBox && !header.column.ReadOnly) {
                            selectedAndEdit(this.Rows.Count - 1, header.column.Index);
                            break;
                        }
                    }
                }
            }
        } else if (( keyData == Keys.Enter || keyData == Keys.Tab || keyData == Keys.Left || keyData == Keys.Right ) && EditingControl != null) {

            DataGridViewCell cell = this.CurrentCell;
            int row = cell.RowIndex;
            bool isAdd = keyData != Keys.Left;
            int offset = isAdd ? 1 : -1;
            int column = cell.ColumnIndex + offset;
            // 2024-06-24   1
            if (!leftAndRightNextCell) {
                if (cell is DataGridViewTextBoxCell && ( keyData == Keys.Left || keyData == Keys.Right )) {
                    TextBox text = this.EditingControl as TextBox;
                    if (text.SelectedText == text.Text) {
                        text.SelectionLength = 0;
                        text.SelectionStart = 0;
                        return false;
                    }

                    if (text.SelectionStart == 0 && keyData == Keys.Left || text.SelectionStart == text.TextLength && keyData == Keys.Right)
                        return true;
                    else
                        return false;
                }
            }
            // 2023-05-25 1
            for (; row < this.RowCount && row >= 0; row = row + offset) {
                for (; column < headers.Count && column >= 0; column = column + offset) {
                    if (this.getHeader(column).type != ColumnType.Checkbox && this.getHeader(column).type != ColumnType.Button && this.Columns[column].Visible) {
                        if (cellEdit(this, row, column)) {
                            selectedAndEdit(row, column);
                            return true;
                        }
                    }
                }
                if (isAdd) {
                    if (( row + 1 ) < this.RowCount)
                        column = 0;
                    else {
                        if (!isAutoAppendRow) {
                            if (cell.ColumnIndex >= 0 && ( cell.ColumnIndex + 1 ) < this.Columns.Count)
                                column = cell.ColumnIndex + 1;
                            else
                                column = cell.ColumnIndex;
                            updateCurrentCellValue(this.Rows[row].Cells[column]);
                            return true;
                        }
                        if (null != addNewRowing) {
                            Dictionary<String, Object> dic = addNewRowing(this);
                            if (null != dic) {
                                addRow(dic);
                            }
                        }
                        column = 0;
                    }
                } else {
                    column = headers.Count - 1;
                }
            }
            // 这里return true 是因为如果在第一行第一个可编辑列里面按Left键，那么焦点会移除这个单元格，这里让他不移除
            // 最后一个列也一样
            if (keyData == Keys.Left || keyData == Keys.Right)
                return true;
        }



        // 2022-11-11 16：04 无法将焦点移交给DGVComboBox，放弃
        //else if (keyData == Keys.Down && EditingControl != null) {
        //    if (input_combobox_columns.ContainsKey(this.CurrentCell.OwningColumn.DataPropertyName)) {

        //        Panel panel = input_combobox_columns[this.CurrentCell.OwningColumn.DataPropertyName];
        //        if (panel.Controls[0].Focused)
        //        {
        //            ((DGVComboBox)panel.Controls[0]).targePreviewKeyDown(new KeyEventArgs(keyData));
        //            return true;
        //        }
        //        bool isShow = ((ComboBox)panel.Controls[0]).DroppedDown;
        //        panel.Controls[0].Select();
        //        panel.Controls[0].Focus();
        //        ((ComboBox)panel.Controls[0]).SelectedIndex = 0;
        //        if (isShow)
        //        ((ComboBox)panel.Controls[0]).DroppedDown = true;
        //        ((ComboBox)panel.Controls[0]).Text = DateTime.Now.ToFileTime() + "";
        //        return true;
        //    }
        //}
        return base.ProcessCmdKey(ref msg, keyData);
    }



    public void NDataGridView_KeyPress(object sender, KeyPressEventArgs e) {
        if (null != this.CurrentCell) {
            if (e.KeyChar == '\t' || e.KeyChar == '\r') {
                DataGridViewCell cell = this.CurrentCell;
                int row = cell.RowIndex;
                int column = cell.ColumnIndex;
                if (e.KeyChar == '\r' && isAutoAppendRow) {
                    column += 1;
                }

                for (; row < this.RowCount; row++) {
                    for (; column < headers.Count; column++) {
                        if (!headers[column].column.ReadOnly) {
                            selectedAndEdit(row, column);
                            e.Handled = true;
                            return;
                        }
                    }
                    if (( row + 1 ) < this.RowCount)
                        column = 0;
                    else {
                        if (!isAutoAppendRow) {
                            e.Handled = true;
                            return;
                        }

                        if (null != addNewRowing) {
                            Dictionary<String, Object> dic = addNewRowing(this);
                            if (null != dic) {
                                addRow(dic);
                            }
                        }
                        column = 0;
                    }
                }
            }
        }
    }

    private void NDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
        if (!isPaint)
            return;
        if (!e.FormattingApplied) {
            if (headers.Count <= e.ColumnIndex)
                return;
            Object newValue = Tools.trim(e.Value);
            if (headers[e.ColumnIndex].column.GetType() == typeof(DGVTextBoxColumn)) {
                DataGridViewColumn c = this.Columns[e.ColumnIndex];
                if (cellValueFormat.ContainsKey(c.Name.ToLower())) {
                    int i = 0;
                    double d = 0.0D;
                    if (!Tools.isNullOrEmpty(newValue) && ( Tools.isInt(newValue) || Tools.isDouble(newValue) ))
                        newValue = decimal.Parse(newValue + "").ToString(cellValueFormat[c.Name.ToLower()]);
                    e.Value = newValue;
                    // 2024-06-27   2
                } else if (datetimeFormart.ContainsKey(c.Name.ToLower())) {
                    String[] v_s = datetimeFormart[c.Name.ToLower()];
                    if (null != e.Value && !DBNull.Value.Equals(e.Value) && !String.IsNullOrEmpty(e.Value.ToString())) {
                        String validatVal = noNumber.Replace(e.Value + "", "");
                        String dateVal = "";
                        //yyyyMMddHHmmss
                        // yyyy-MM-dd HH:mm:ss
                        if (validatVal.Length == 8)
                            dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Substring(0, 10);
                        else if (validatVal.Length >= 14)
                            dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":").Substring(0, 19);
                        else
                            dateVal = e.Value + "";
                        if (!String.IsNullOrEmpty(dateVal)) {
                            DateTime tryDate = DateTime.MinValue;
                            if (DateTime.TryParse(dateVal, out tryDate)) {
                                e.Value = tryDate.ToString(v_s[1]);
                            }
                        }
                    }
                } else if (input_combobox_columns.ContainsKey(c.Name)) {
                    Panel panel = input_combobox_columns[c.Name];
                    DGVTextBoxColumn comboxColumn = c as DGVTextBoxColumn;
                    if (null != comboxColumn && comboxColumn.isComboBoxMaster) {
                        DGVComboBox box = panel.Controls[0] as DGVComboBox;
                        DataRow[] selRows = box.BindTable.Select(box.ValueMember + "='" + newValue + "'");
                        if (selRows.Length > 0) {
                            e.Value = selRows[0][box.DisplayMember];
                        }
                    }
                } else {
                    e.Value = newValue;
                }
            } else if (headers[e.ColumnIndex].column.GetType() == typeof(DGVComboBoxColumn)) {
                // 2025-02-21 这里很诡异，某些时候注释了也能正常。
                // 某些时候注释就部显示。
                // 具体体现合同录入，查询007分场租金的付款方式上
                // 初步怀疑和能否编辑又关,但是无关
                DGVComboBoxColumn column = (DGVComboBoxColumn)headers[e.ColumnIndex].column;
                DataTable table = column.DataSource as DataTable;
                if (null != table) {
                    DataRow[] rows = table.Select(column.ValueMember + "='" + newValue + "'");
                    if (null != rows && 0 != rows.Length)
                        e.Value = rows[0][column.DisplayMember];
                    else
                        e.Value = newValue.ToString();
                } else {
                    e.Value = newValue.ToString();
                }
            } else if (headers[e.ColumnIndex].column.GetType() == typeof(DGVDateTimeColumn)) {
                if (!Tools.isNullOrEmpty(e.Value)) {
                    DGVDateTimeColumn column = (DGVDateTimeColumn)headers[e.ColumnIndex].column;
                    int dateInt = 0;
                    if (int.TryParse(newValue.ToString(), out dateInt) && newValue.ToString().Length == 8)
                        newValue = newValue.ToString().Insert(4, "-").Insert(7, "-");
                    else if (Tools.isMatch(newValue, "^\\d{14}$"))
                        newValue = newValue.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":");

                    DateTime date = DateTime.MinValue;
                    if (DateTime.TryParse(newValue + "", out date)) {
                        e.Value = date.ToString(column.showFormat);
                    } else {
                        e.Value = newValue;
                    }
                } else {
                    e.Value = newValue;
                }
            }
            e.FormattingApplied = true;
        }
    }




    private void NDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
        if (0 != headers.Count && e.ColumnIndex < headers.Count) {
            e.Cancel = !cellEdit(this, e.RowIndex, e.ColumnIndex);
        }
    }



    public void NDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
        DataGridViewCell cell = this.Rows[e.RowIndex].Cells[e.ColumnIndex];
        if (null != cell) {
            if (cellValueFormat.ContainsKey(cell.OwningColumn.Name)) {
                Object val = cell.Value;
                if (Tools.isDouble(val) || Tools.isInt(val)) {
                    decimal newVal = Convert.ToDecimal(val);
                    cell.Value = newVal.ToString(cellValueFormat[cell.OwningColumn.Name]);
                }
            }

            CellValidata validata = validataLineData(e.RowIndex, e.ColumnIndex);
            if (!validata.validataSuccess)
                cell.ErrorText = validata.errorMsg;
            else
                cell.ErrorText = "";

            if (null == cell.Tag && validata.validataSuccess) {
                cell.Tag = validata;
            }

        }
    }

    #endregion



    #region 单元格内容，格式验证

    // 2023-04-19 1.
    public CellValidata validataAllData(Dictionary<String, bool> isForceValidata) {
        if (null == this.CurrentCell && this.Rows.Count == 0)
            return new CellValidata();
        if (null != this.CurrentCell) {
            // 输入的原始内容
            String cellOldValue = null == this.CurrentCell ? "" : this.CurrentCell.EditedFormattedValue + "";
            try {
                this.CurrentCell = null;
            } catch (Exception eee) {
                // 2024-01-24   2
                if (eee is InvalidOperationException) {
                    if (eee.Message.Contains("无法提交或取消单元格值更改")) {
                        // 2024-05-06   1
                        // 部分validata需要处理类型设置未null后，自动补齐的默认值，如DateTime默认为DateTime.MinValue
                        bool isShowDialog = true;
                        int oldColumnIndex = this.CurrentCell.ColumnIndex;
                        String oldColumnName = this.Columns[oldColumnIndex].DataPropertyName;
                        int oldRowIndex = this.CurrentCell.RowIndex;
                        ColumnHeader header = this.getHeader(oldColumnIndex);
                        DataRow dataRow = this.getDataRow(oldRowIndex);
                        if (null != dataRow) {
                            //2024-06-05    1
                            String currColumnName = this.Columns[oldColumnIndex].DataPropertyName.ToLower();
                            if (dataRow.Table.Columns[currColumnName].DataType == typeof(DateTime) && datetimeFormart.ContainsKey(currColumnName)) {
                                String[] v_s = datetimeFormart[currColumnName];
                                if (null != cellOldValue && !DBNull.Value.Equals(cellOldValue) && !String.IsNullOrEmpty(cellOldValue)) {
                                    String dateVal = "";
                                    String validatVal = noNumber.Replace(cellOldValue, "");

                                    //yyyyMMddHHmmss
                                    // yyyy-MM-dd HH:mm:ss
                                    if (validatVal.Length == 8)
                                        dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Substring(0, 10);
                                    else if (validatVal.Length >= 14)
                                        dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":").Substring(0, 19);
                                    else
                                        dateVal = cellOldValue;

                                    if (!String.IsNullOrEmpty(dateVal)) {
                                        DateTime tryDate = DateTime.MinValue;
                                        if (DateTime.TryParse(dateVal, out tryDate)) {
                                            dataRow[oldColumnName] = tryDate.ToString(v_s[0]);
                                            this.CurrentCell.Value = tryDate.ToString(v_s[1]);
                                            isShowDialog = false;
                                        }
                                    }
                                }
                            } else if (header.isNull) {
                                dataRow[oldColumnName] = DBNull.Value;
                                this.CurrentCell.Value = "";
                                isShowDialog = false;
                            }
                        }
                        if (isShowDialog) {
                            return new CellValidata(this.Columns[oldColumnIndex].HeaderText + "数据无效！", oldRowIndex, oldColumnIndex);
                        }
                    }
                }

            }
        }
        //if (null != this.CurrentCell) {
        //    if (this.CurrentCell.IsInEditMode || typeof(DGVCheckBoxColumn) == this.CurrentCell.OwningColumn.GetType()) {
        //        int r = this.CurrentCell.RowIndex;
        //        int c = this.CurrentCell.ColumnIndex;
        //        if (this.IsCurrentCellDirty) {
        //            if (typeof(Double) == this.CurrentCell.ValueType || typeof(double) == this.CurrentCell.ValueType) {
        //                Object v = this.CurrentCell.EditedFormattedValue;
        //                double vv = 0.0D;
        //                if (!double.TryParse(v + "", out vv)) {
        //                    return validataLineData(r, c);
        //                }
        //            }
        //        }
        //    }
        //}
        CellValidata validataResult = null;
        for (int rowIndex = 0; rowIndex < this.RowCount; rowIndex++) {
            DataRow dataRow = getDataRow(rowIndex);
            dataRow.EndEdit();
            for (int columnIndex = 0; columnIndex < this.headers.Count; columnIndex++) {
                ColumnHeader header = headers[columnIndex];
                if (!header.column.ReadOnly) {
                    bool isForceValidataItem = isForceValidata.ContainsKey(header.dataName) ? isForceValidata[header.dataName] : false;

                    if (isModify(rowIndex, header.column.Index) || isForceValidataItem) {
                        if (this.Rows[rowIndex].Cells[columnIndex].Tag is CellValidata && !isForceValidataItem) {
                            validataResult = this.Rows[rowIndex].Cells[columnIndex].Tag as CellValidata;
                            if (validataResult.validataSuccess)
                                continue;
                        }
                        if (datetimeFormart.ContainsKey(header.dataName.ToLower())) {
                            String[] v_s = datetimeFormart[header.dataName.ToLower()];
                            object val = dataRow[header.dataName];
                            if (null != val && !DBNull.Value.Equals(val) && !String.IsNullOrEmpty(val.ToString())) {
                                String validatVal = noNumber.Replace(val + "", "");
                                String dateVal = "";
                                //yyyyMMddHHmmss
                                // yyyy-MM-dd HH:mm:ss
                                if (validatVal.Length == 8)
                                    dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Substring(0, 10);
                                else if (validatVal.Length >= 14)
                                    dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":").Substring(0, 19);
                                else
                                    dateVal = val + "";
                                if (!String.IsNullOrEmpty(dateVal)) {
                                    DateTime tryDate = DateTime.MinValue;
                                    if (DateTime.TryParse(dateVal, out tryDate)) {
                                        String newVal = tryDate.ToString(v_s[0]);
                                        if (!newVal.Equals(val + ""))
                                            dataRow[header.dataName] = newVal;
                                    }
                                }
                            }
                        }

                        validataResult = validataLineData(rowIndex, columnIndex);
                        if (null == this.Rows[rowIndex].Cells[columnIndex].Tag || this.Rows[rowIndex].Cells[columnIndex].Tag is CellValidata)
                            this.Rows[rowIndex].Cells[columnIndex].Tag = validataResult;
                        if (!validataResult.validataSuccess)
                            return validataResult;
                    }
                    // 如果列是数字类型，则进一步判断是否输入了非数字，因为这种情况判断不了
                    // 2024-01-24   1
                    if (this.sourceTable.Columns[header.dataName].DataType == typeof(int)
                        || this.sourceTable.Columns[header.dataName].DataType == typeof(double)
                        || this.sourceTable.Columns[header.dataName].DataType == typeof(decimal)) {
                        Object val_e_f = this[columnIndex, rowIndex].EditedFormattedValue;
                        if (null != val_e_f && !DBNull.Value.Equals(val_e_f) && !"".Equals(val_e_f.ToString()) && !Tools.isDouble(val_e_f + "")) {
                            return new CellValidata(header.column.HeaderText + "格式无效！", rowIndex, columnIndex);
                        }
                    }
                }
            }
        }
        if (null == validataResult)
            validataResult = new CellValidata();
        return validataResult;
    }

    public CellValidata validataAllData() {
        Dictionary<String, bool> isForceValidata = new Dictionary<string, bool>();
        for (int columnIndex = 0; columnIndex < this.headers.Count; columnIndex++) {
            ColumnHeader header = headers[columnIndex];
            if (!header.column.ReadOnly) {
                isForceValidata.Add(header.dataName, false);
            }
        }
        return validataAllData(isForceValidata);
    }

    public CellValidata validataLineData(int rowIndex, int columnIndex) {
        DataGridViewCell cell = Rows[rowIndex].Cells[columnIndex];
        ColumnHeader header = headers[columnIndex];
        Object val = this.IsCurrentCellDirty ? cell.EditedFormattedValue : cell.FormattedValue;

        if (header.isNull) {
            if (Tools.trim(val).Length < header.minLength && 0 != Tools.trim(val).Length) {
                if (header.format == ColumnFormat.TEXT && datetimeFormart.ContainsKey(this.Columns[columnIndex].DataPropertyName.ToLower()))
                    return new CellValidata(header.column.HeaderText + "格式无效！", rowIndex, columnIndex);
                else
                    return new CellValidata(header.column.HeaderText + "最小长度为" + header.minLength + "！", rowIndex, columnIndex);
            } else {
                if (Tools.trim(val).Length > 0) {
                    if (header.format == ColumnFormat.INT) {
                        if (!Tools.isInt(val)) {
                            return new CellValidata(header.column.HeaderText + "只能接收数字型！", rowIndex, columnIndex);
                        }
                    } else if (header.format == ColumnFormat.DOUBLE) {
                        if (!Tools.isDouble(val) && !Tools.isInt(val)) {
                            return new CellValidata(header.column.HeaderText + "只能接收小数或整数类型！", rowIndex, columnIndex);
                        }
                        // 2024-07-08   1
                    } else if (header.format == ColumnFormat.TEXT && datetimeFormart.ContainsKey(this.Columns[columnIndex].DataPropertyName.ToLower()) && !input_combobox_columns.ContainsKey(this.Columns[columnIndex].DataPropertyName)) {
                        String[] v_s = datetimeFormart[cell.OwningColumn.DataPropertyName.ToLower()];
                        String validatVal = noNumber.Replace(val + "", "");
                        String dateVal = "";
                        //yyyyMMddHHmmss
                        // yyyy-MM-dd HH:mm:ss
                        if (validatVal.Length == 8)
                            dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Substring(0, 10);
                        else if (validatVal.Length >= 14)
                            dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":").Substring(0, 19);
                        else
                            dateVal = val + "";
                        if (!String.IsNullOrEmpty(dateVal)) {
                            DateTime tryDate = DateTime.MinValue;
                            if (!DateTime.TryParse(dateVal + "", out tryDate)) {
                                return new CellValidata(header.column.HeaderText + "无效！", rowIndex, columnIndex);
                            } else {
                                DataRow rowww = this.getDataRow(rowIndex);
                                if (null != rowww) {
                                    rowww[this.Columns[columnIndex].DataPropertyName] = tryDate.ToString(v_s[0]);
                                }
                            }
                        }
                    } else if (header.format == ColumnFormat.TEXT && !datetimeFormart.ContainsKey(this.Columns[columnIndex].DataPropertyName.ToLower()) && input_combobox_columns.ContainsKey(this.Columns[columnIndex].DataPropertyName)) {
                        Panel panel = input_combobox_columns[this.Columns[columnIndex].DataPropertyName];
                        DGVTextBoxColumn comboxColumn = this.Columns[columnIndex] as DGVTextBoxColumn;
                        if (null != comboxColumn && comboxColumn.isComboBoxMaster) {
                            DGVComboBox box = panel.Controls[0] as DGVComboBox;
                            if (box.SelectedIndex == -1 || null != box.SelectedItem) {
                                // 2024-08-02   1
                                if (box.BindTable.Select(box.ValueMember + "='" + val + "'").Length == 0) {
                                    // DataRow[] rows = (DataRow[])box.Items;
                                    return new CellValidata(comboxColumn.HeaderText + "选择无效！", rowIndex, columnIndex);
                                }
                            }
                        }
                    }
                }
            }
        } else {
            if (Tools.isNullOrEmpty(val)) {
                return new CellValidata(header.column.HeaderText + "不能为空！", rowIndex, columnIndex);
            }
            if (header.format == ColumnFormat.INT) {
                if (!Tools.isInt(val)) {
                    return new CellValidata(header.column.HeaderText + "只能接收数字型！", rowIndex, columnIndex);
                }
            } else if (header.format == ColumnFormat.DOUBLE) {
                if (!Tools.isDouble(val) && !Tools.isInt(val)) {
                    return new CellValidata(header.column.HeaderText + "只能接收小数或整数类型！", rowIndex, columnIndex);
                }
            } else if (header.format == ColumnFormat.TEXT && datetimeFormart.ContainsKey(this.Columns[columnIndex].DataPropertyName.ToLower()) && !input_combobox_columns.ContainsKey(this.Columns[columnIndex].DataPropertyName)) {
                String[] v_s = datetimeFormart[cell.OwningColumn.DataPropertyName.ToLower()];
                String validatVal = noNumber.Replace(val + "", "");
                String dateVal = "";
                //yyyyMMddHHmmss
                // yyyy-MM-dd HH:mm:ss
                if (validatVal.Length == 8)
                    dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Substring(0, 10);
                else if (validatVal.Length >= 14)
                    dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":").Substring(0, 19);
                else
                    dateVal = val + "";
                if (!String.IsNullOrEmpty(dateVal)) {
                    DateTime tryDate = DateTime.MinValue;
                    if (!DateTime.TryParse(dateVal + "", out tryDate)) {
                        return new CellValidata(header.column.HeaderText + "无效！", rowIndex, columnIndex);
                    } else {
                        DataRow rowww = this.getDataRow(rowIndex);
                        if (null != rowww) {
                            rowww[this.Columns[columnIndex].DataPropertyName] = tryDate.ToString(v_s[0]);
                        }
                    }
                }
            } else if (header.format == ColumnFormat.TEXT && !datetimeFormart.ContainsKey(this.Columns[columnIndex].DataPropertyName.ToLower()) && input_combobox_columns.ContainsKey(this.Columns[columnIndex].DataPropertyName)) {
                Panel panel = input_combobox_columns[this.Columns[columnIndex].DataPropertyName];
                DGVTextBoxColumn comboxColumn = this.Columns[columnIndex] as DGVTextBoxColumn;
                if (null != comboxColumn && comboxColumn.isComboBoxMaster) {
                    DGVComboBox box = panel.Controls[0] as DGVComboBox;
                    if (box.SelectedIndex == -1 || null != box.SelectedItem) {
                        return new CellValidata(comboxColumn.HeaderText + "选择无效！", rowIndex, columnIndex);
                    }
                }
            }
            if (Tools.trim(val).Length < header.minLength) {
                return new CellValidata(header.column.HeaderText + "长度最少" + header.minLength + "位！", rowIndex, columnIndex);
            }
            if (( Tools.trim(val) + "" ).Length > header.maxLength) {
                return new CellValidata(header.column.HeaderText + "长度不能超过" + header.maxLength + "位！", rowIndex, columnIndex);
            }
        }
        if (null != validata)
            return validata(this, rowIndex, columnIndex, val);
        return new CellValidata();
    }


    #endregion


    #region 添加header



    public NDataGridView3 addMutiHeader(String showName, int start, int end) {
        return addMutiHeader(showName, "", HorizontalAlignment.Center, start, end);
    }
    public NDataGridView3 addMutiHeader(String showName, String sortName, HorizontalAlignment align, int start, int end) {
        return addMutiHeader(new MutiHeader(showName, sortName, align, start, end));
    }
    public NDataGridView3 addMutiHeader(MutiHeader header) {
        this.ColumnHeadersHeight = 48;
        this.mutis.Add(header);
        return this;
    }

    public NDataGridView3 addHeader(String dataName, String showName) {
        return addHeader(dataName, showName, ColumnFormat.TEXT, true, 0, int.MaxValue);
    }

    public NDataGridView3 addHeader(String dataName, String showName, ColumnFormat format, int maxLength) {
        return addHeader(dataName, showName, format, true, 0, maxLength);
    }
    public NDataGridView3 addHeader(String dataName, String showName, ColumnFormat format, bool isNull, int minLength, int maxLength) {
        addColumnHeader(ColumnHeader.textbox(this, format, dataName, showName, isNull, minLength, maxLength));
        if (format == ColumnFormat.DOUBLE || showName.EndsWith("金额") || showName.EndsWith("价") || showName.Contains("含税") || showName.Contains("无税") || dataName.Contains("amt")) {
            addCellValueFormat(dataName, "0.00");
            setColumnAlign(DataGridViewContentAlignment.MiddleRight, dataName);
        }
        if ("gdsid".Equals(dataName) || "商品编码".Equals(showName))
            setColumnWidth(75, dataName);
        return this;
    }

    public NDataGridView3 addHeader(List<ColumnHeader> lists) {
        addColumnHeader(lists.ToArray());
        return this;
    }

    public NDataGridView3 addCheckBoxHeader(String dataName, String showName, bool isClick) {
        return addCheckBoxHeader(dataName, showName, isClick, true);
    }

    public NDataGridView3 addCheckBoxHeader(String dataName, String showName, bool isClick, String trueVal, String falseVal) {
        return addCheckBoxHeader(dataName, showName, isClick, true, trueVal, falseVal);
    }
    public NDataGridView3 addCheckBoxHeader(String dataName, String showName, bool isClick, bool isExecDefaultEvent) {
        return addCheckBoxHeader(dataName, showName, isClick, isExecDefaultEvent, "y", "n");
    }

    public NDataGridView3 addCheckBoxHeader(String dataName, String showName, bool isClick, bool isExecDefaultEvent, String trueVal, String falseVal) {
        addColumnHeader(ColumnHeader.checkbox(this, dataName, showName, isClick, isExecDefaultEvent, trueVal, falseVal));
        setColumnAlign(DataGridViewContentAlignment.MiddleCenter, dataName);
        setColumnWidth((int)Math.Ceiling(( showName.Length + 0.7D ) * 15.3D), dataName);
        return this;
    }


    public NDataGridView3 addAllSelectHeader(String dataName, String showName, bool isDefaultEvent) {
        return addAllSelectHeader(dataName, showName, isDefaultEvent, "y", "n");
    }

    public NDataGridView3 addAllSelectHeader(String dataName, String showName, bool isDefaultEvent, String trueVal, String falseVal) {
        return addAllSelectHeader(dataName, showName, isDefaultEvent, trueVal, falseVal, canChangedStateListener);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isDefaultEvent"></param>
    /// <param name="clickListener">return true表示需要执行选中事件，false不执行选中</param>
    /// <returns></returns>
    public NDataGridView3 addAllSelectHeader(String dataName, String showName, bool isDefaultEvent, CanChangedStateListener canChangedStateListener) {
        return addAllSelectHeader(dataName, showName, isDefaultEvent, "y", "n", canChangedStateListener);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isDefaultEvent"></param>
    /// <param name="trueVal"></param>
    /// <param name="falseVal"></param>
    /// <param name="clickListener">return true表示需要执行选中事件，false不执行选中</param>
    /// <returns></returns>
    public NDataGridView3 addAllSelectHeader(String dataName, String showName, bool isDefaultEvent, String trueVal, String falseVal, CanChangedStateListener clickListener) {
        if (showName.Length > 0)
            showName = "　 " + showName;
        ColumnHeader header = ColumnHeader.checkbox(this, dataName, showName, true, isDefaultEvent, trueVal, falseVal);
        header.column.HeaderCell = new DataGridViewCheckBoxHeaderCell(null == clickListener ? canChangedStateListener : clickListener);
        header.column.HeaderText = showName;
        addColumnHeader(header);
        if (showName.Trim().Length == 0)
            setColumnWidth(40, dataName);
        else
            setColumnWidth((int)Math.Ceiling(showName.Length * 14.3D), dataName);
        return this;
    }

    public NDataGridView3 addButtonHeader(String dataName, String showName) {
        addColumnHeader(ColumnHeader.button(this, dataName, showName));
        return this;
    }
    public NDataGridView3 addButtonHeader(String dataName, String showName, bool isClick) {
        addColumnHeader(ColumnHeader.button(this, dataName, showName, isClick));
        return this;
    }

    /// <summary>
    /// 有问题
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isNull"></param>
    /// <param name="showFormat"></param>
    /// <returns></returns>
    public NDataGridView3 addTextDateTimeHeader(String dataName, String showName, String showFormat, bool isNull) {
        return addTextDateTimeHeader(dataName, showName, showFormat, showFormat, isNull);
    }
    /// <summary>
    /// 有问题
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isNull"></param>
    /// <param name="showFormat"></param>
    /// <returns></returns>
    public NDataGridView3 addTextDateTimeHeader(String dataName, String showName, String valueFormat, String showFormat, bool isNull) {
        addColumnHeader(ColumnHeader.textbox(this, ColumnFormat.TEXT, dataName, showName, isNull, 8, 10));
        setColumnWidth((int)Math.Ceiling(showFormat.Replace(":", "").Length * 8.5D), dataName);
        datetimeFormart.Add(dataName.ToLower(), new string[] { valueFormat, showFormat });
        return this;
    }

    public NDataGridView3 addDateTimeHeader(String dataName, String showName, String showFormat) {
        return addDateTimeHeader(dataName, showName, showFormat, showFormat, true, false);
    }
    public NDataGridView3 addDateTimeHeader(String dataName, String showName, String showFormat, bool isClick) {
        return addDateTimeHeader(dataName, showName, showFormat, showFormat, true, isClick);
    }

    public NDataGridView3 addDateTimeHeader(String dataName, String showName, String valueFormat, String showFormat, bool isClick) {
        return addDateTimeHeader(dataName, showName, valueFormat, showFormat, true, isClick);
    }
    public NDataGridView3 addDateTimeHeader(String dataName, String showName, String valueFormat, String showFormat, bool isSelectMode, bool isClick) {
        addColumnHeader(ColumnHeader.datetime(this, dataName, showName, valueFormat, showFormat, isSelectMode, isClick));
        setColumnWidth((int)Math.Ceiling(showFormat.Replace(":", "").Length * 8.5D), dataName);
        return this;
    }

    #region 添加combox

    public NDataGridView3 addComboBoxHeader(String dataName, String showName, bool isClick) {
        addColumnHeader(ColumnHeader.combox(this, dataName, showName, isClick));
        return this;
    }

    /// <summary>
    /// 添加可输入并且可联动的combobox,已ComboBox为主
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isNull"></param>
    /// <param name="isInput"></param>
    /// <returns></returns>
    public NDataGridView3 addComboBoxHeader(String dataName, String showName, bool isNull, bool isInput) {
        return addComboBoxHeader(dataName, showName, isNull, isInput, true, 1);
    }
    /// <summary>
    /// 添加可输入并且可联动的combobox
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isNull"></param>
    /// <param name="isInput"></param>
    /// <param name="isComboBoxMaster">true 以combobox为主，false为输入的内容为主</param>
    /// <returns></returns>
    public NDataGridView3 addComboBoxHeader(String dataName, String showName, bool isNull, bool isInput, bool isComboBoxMaster) {
        return addComboBoxHeader(dataName, showName, isNull, isInput, isComboBoxMaster, 1);
    }
    /// <summary>
    /// 添加可输入并且可联动的combobox
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="showName"></param>
    /// <param name="isNull"></param>
    /// <param name="isInput"></param>
    /// <param name="isComboBoxMaster">true 以combobox为主，false为输入的内容为主</param>
    /// <param name="minTargetLenth">触发下拉的最小长度</param>
    /// <returns></returns>
    public NDataGridView3 addComboBoxHeader(String dataName, String showName, bool isNull, bool isInput, bool isComboBoxMaster, int minTargetLenth) {
        DGVTextBoxColumn column = (DGVTextBoxColumn)addColumnHeader(ColumnHeader.textbox(this, ColumnFormat.TEXT, dataName, showName, isNull, 1, 30));
        column.isComboBoxMaster = isComboBoxMaster;
        column.minTargetLength = minTargetLenth;
        return this;
    }


    public NDataGridView3 addComboBoxHeader(String dataName, String showName, DataTable dataSource, String valueField, String showField) {
        return addComboBoxHeader(dataName, showName, true, dataSource, valueField, showField, null);
    }

    public NDataGridView3 addComboBoxHeader(String dataName, String showName, bool isClick, DataTable dataSource, String valueField, String showField, comboxValueChangedListener listener) {
        ColumnHeader header = ColumnHeader.combox(this, dataName, showName, isClick);
        DGVComboBoxColumn column = (DGVComboBoxColumn)header.column;
        column.CellTemplate = new NDataGridViewComboxCell();
        column.Listener = listener;
        column.DataSource = dataSource;
        column.ValueMember = valueField;
        column.DisplayMember = showField;

        addColumnHeader(header);
        return this;
    }


    public NDataGridView3 addComboBoxHeader(String dataName, String headerName, String[] showName, DataTable dataSource, String valueField, String showField) {
        return addComboBoxHeader(dataName, headerName, showName, true, dataSource, valueField, showField, null);
    }

    public NDataGridView3 addComboBoxHeader(String dataName, String headerName, String[] showName, bool isClick, DataTable dataSource, String valueField, String showField, comboxValueChangedListener listener) {
        ColumnHeader header = ColumnHeader.combox(this, dataName, headerName, isClick);
        DGVComboBoxColumn column = (DGVComboBoxColumn)header.column;
        column.ShowColumns = showName;
        column.Listener = listener;
        column.CellTemplate = new NDataGridViewComboxCell();
        column.DataSource = dataSource;
        column.ValueMember = valueField;
        column.DisplayMember = showField;
        addColumnHeader(header);
        return this;
    }

    #endregion

    /// <summary>
    /// 添加DataGridView表头
    /// </summary>
    /// <returns></returns>
    public DataGridViewColumn addColumnHeader(params ColumnHeader[] headers) {
        foreach (ColumnHeader header in headers) {
            if (null == header.column.DataGridView) {
                this.Columns.Add(header.column);
                this.headers.Add(header);
            } else {
                addHeader(header.dataName, header.showName, header.format, header.isNull, header.minLength, header.maxLength);
            }
        }
        return this.Columns[this.Columns.Count - 1];
    }

    /// <summary>
    /// 如果dtaGrdView已经有数据，在调用这个方法会把每一行都重新设置datasource，所以会很慢
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String columnName, DataTable dataSource, String val, String show) {
        String[] showColumns = new string[] { show};
        return addHeaderDataSource(columnName, dataSource, val, show, showColumns);
    }

    /// <summary>
    /// 如果dtaGrdView已经有数据，在调用这个方法会把每一行都重新设置datasource，所以会很慢
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String columnName, DataTable dataSource, String val, String show, String[] showColumns) {
        DGVComboBoxColumn comboBoxColumn = (DGVComboBoxColumn)Columns[columnName];
        if (null == comboBoxColumn)
            return this;
        try {
            if (!val.Equals(comboBoxColumn.ValueMember, StringComparison.CurrentCultureIgnoreCase))
                comboBoxColumn.ValueMember = val;
            if (!show.Equals(comboBoxColumn.DisplayMember, StringComparison.CurrentCultureIgnoreCase))
                comboBoxColumn.DisplayMember = show;
            if (null != comboBoxColumn.DataSource && comboBoxColumn.DataSource is DataTable) {
                DataTable table = ( (DataTable)comboBoxColumn.DataSource );
                table.Rows.Clear();
                table.Columns.Clear();
                foreach (DataColumn column in dataSource.Columns) {
                    table.Columns.Add(column.ColumnName, column.DataType);
                }
                foreach (DataRow row in dataSource.Rows) {
                    table.ImportRow(row);
                }
                table.AcceptChanges();
            } else {
                comboBoxColumn.DataSource = dataSource;
            }
            comboBoxColumn.ShowColumns = showColumns;
        } catch (Exception eeeeeeee) {
            String sdsdsd = "";
        }
        return this;
    }
    public NDataGridView3 addHeaderDataSource(String columnName, DataTable dataSource, String val, String show, comboxValueChangedListener listener) {
        return addHeaderDataSource(columnName, dataSource, val, show, null, listener);
    }

    /// <summary>
    /// 如果dtaGrdView已经有数据，在调用这个方法会把每一行都重新设置datasource，所以会很慢,val,show,datasour 都会全部重置，并刷新。
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String columnName, DataTable dataSource, String val, String show, String[] showColumns, comboxValueChangedListener listener) {
        DGVComboBoxColumn comboBoxColumn = (DGVComboBoxColumn)Columns[columnName];
        if (null == comboBoxColumn)
            return this;
        if (comboBoxColumn.Listener != listener)
            comboBoxColumn.Listener = listener;
        try {
            if (!val.Equals(comboBoxColumn.ValueMember, StringComparison.CurrentCultureIgnoreCase))
                comboBoxColumn.ValueMember = val;
            if (!show.Equals(comboBoxColumn.DisplayMember, StringComparison.CurrentCultureIgnoreCase))
                comboBoxColumn.DisplayMember = show;
            comboBoxColumn.DataSource = dataSource;
            comboBoxColumn.ShowColumns = showColumns;
        } catch (Exception eeeeeeee) {
            String sdsdsd = "";
        }
        return this;
    }

    /// <summary>
    /// 如果dtaGrdView已经有数据，在调用这个方法会把每一行都重新设置datasource，所以会很慢
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public NDataGridView3 addCellDataSource(String columnName, int rowIndex, DataTable dataSource, String val, String show) {
        String[] showColumns = null;
        return addCellDataSource(columnName, rowIndex, dataSource, val, show, showColumns);
    }

    /// <summary>
    /// 如果dtaGrdView已经有数据，在调用这个方法会把每一行都重新设置datasource，所以会很慢
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public NDataGridView3 addCellDataSource(String columnName, int rowIndex, DataTable dataSource, String val, String show, String[] showColumns) {
        DataGridViewColumn comboBoxColumn = Columns[columnName];
        if (null == comboBoxColumn || !( comboBoxColumn is DGVComboBoxColumn ))
            return this;
        try {
            DGVComboBoxColumn comboColumn = ( (DGVComboBoxColumn)comboBoxColumn );
            NDataGridViewComboxCell comboCell = this[columnName, rowIndex] as NDataGridViewComboxCell;
            comboCell.ValueMember = val;
            comboCell.DisplayMember = show;
            comboColumn.ShowColumns = showColumns;
        } catch (Exception eeeeeeee) {
            String sdsdsd = "";
        }
        return this;
    }
    public NDataGridView3 addCellDataSource(String columnName, int rowIndex, DataTable dataSource, String val, String show, comboxValueChangedListener listener) {
        return addCellDataSource(columnName, rowIndex, dataSource, val, show, null, listener);
    }

    public NDataGridView3 addCellDataSource(String columnName, int rowIndex, DataTable dataSource, String val, String show, String[] showColumns, comboxValueChangedListener listener) {
        DataGridViewColumn comboBoxColumn = Columns[columnName];
        if (null == comboBoxColumn || !( comboBoxColumn is DGVComboBoxColumn ))
            return this;
        try {
            DGVComboBoxColumn comboColumn = ( (DGVComboBoxColumn)comboBoxColumn );
            if (comboColumn.Listener != listener)
                comboColumn.Listener = listener;


            NDataGridViewComboxCell comboCell = this[columnName, rowIndex] as NDataGridViewComboxCell;
            comboCell.ValueMember = val;
            comboCell.DisplayMember = show;
            comboColumn.ShowColumns = showColumns;
        } catch (Exception eeeeeeee) {
            String sdsdsd = "";
        }
        return this;
    }


    /// <summary>
    /// 专用于addComboBoxHeader(String dataName, String showName, bool isClick,bool isInput)
    /// </summary>
    /// <param name="columnNames"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <param name="showColumn"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String[] columnNames, DataTable dataSource, String val, String show, comboxValueChangedListener listener) {
        return addHeaderDataSource(columnNames, dataSource, val, show, null, listener);
    }


    /// <summary>
    /// 专用于addComboBoxHeader(String dataName, String showName, bool isClick,bool isInput)
    /// </summary>
    /// <param name="columnNames"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <param name="showColumn"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String[] columnNames, DataTable dataSource, String val, String show, String[] showColumn) {
        //DGVComboBoxColumn comboBoxColumn = (DGVComboBoxColumn)Columns[columnName];
        //if (null == comboBoxColumn)
        //    return this;
        //comboBoxColumn.Listener = listener;
        //comboBoxColumn.ValueMember = val;
        //comboBoxColumn.DisplayMember = show;
        //comboBoxColumn.DataSource = dataSource;
        //comboBoxColumn.
        //comboBoxColumn.ShowColumns = showColumn;
        return addHeaderDataSource(columnNames, dataSource, val, show, showColumn, showColumn, null);
    }

    /// <summary>
    /// 专用于addComboBoxHeader(String dataName, String showName, bool isClick,bool isInput)
    /// </summary>
    /// <param name="columnNames"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <param name="showColumns"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String[] columnNames, DataTable dataSource, String val, String show, String[] showColumns, comboxValueChangedListener listener) {
        return addHeaderDataSource(columnNames, dataSource, val, show, showColumns, showColumns, listener);
    }

    /// <summary>
    /// 专用于addComboBoxHeader(String dataName, String showName, bool isClick,bool isInput)
    /// </summary>
    /// <param name="columnNames"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <param name="showColumns"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String[] columnNames, DataTable dataSource, String val, String show, String[] filterColumns, String[] showColumns) {
        return addHeaderDataSource(columnNames, dataSource, val, show, filterColumns, showColumns, null);
    }
    /// <summary>
    /// 专用于addComboBoxHeader(String dataName, String showName, bool isClick,bool isInput)
    /// </summary>
    /// <param name="columnNames"></param>
    /// <param name="dataSource"></param>
    /// <param name="val"></param>
    /// <param name="show"></param>
    /// <param name="showColumn"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public NDataGridView3 addHeaderDataSource(String[] columnNames, DataTable dataSource, String val, String show, String[] filterColumns, String[] showColumn, comboxValueChangedListener listener) {

        if (null == listener) {
            listener = (NDataGridView3 view, int ri, int ci, ComboBox combox, DataRow dataRow) => {

                DataRow dRow = view.getDataRow(ri);
                if (null == dRow)
                    return true;

                combox.DroppedDown = false;
                dRow[view.Columns[ci].DataPropertyName] = dataRow[combox.ValueMember];
                view[ci, ri].ErrorText = "";
                //view.selectedAndEdit(ri, ci);

                return true;
            };
        }

        Panel panel = new Panel();

        DGVComboBox box = new DGVComboBox(this, listener);
        box.ValueMember = val;
        box.DisplayMember = show;
        box.FlatStyle = FlatStyle.Flat;
        box.bindData(showColumn, filterColumns, dataSource);
        box.Tag = dataSource;
        panel.Width = panel.Height = 0;
        panel.Controls.Add(box);
        box.Dock = DockStyle.Fill;
        this.Controls.Add(panel);

        box.Location = new Point(0, 0);

        foreach (String columnName in columnNames) {
        	// 2024-11-20	1.
            if (input_combobox_columns.ContainsKey(columnName)) {
                Panel p = input_combobox_columns[columnName];
                if (p.Controls[0] is DGVComboBox) {
                    DGVComboBox dgvCombobox = (DGVComboBox)p.Controls[0];
                    dgvCombobox.DataSource = null;
                    dgvCombobox.Dispose();
                    input_combobox_columns.Remove(columnName);
                }
            }
            input_combobox_columns.Add(columnName, panel);
        }
        return this;
    }



    #endregion

    #region 取值



    public DataGridViewRow getRow(int rowIndex) {
        if (this.Rows.Count <= rowIndex)
            return null;
        return this.Rows[rowIndex];
    }

    public DataGridViewCell getCell(int rowIndex, String columnName) {
        DataGridViewRow row = getRow(rowIndex);
        return row.Cells[columnName];
    }

    public DataGridViewCell getCell(int rowIndex, int columnIndex) {
        DataGridViewRow row = getRow(rowIndex);
        if (null != row && row.Cells.Count > columnIndex)
            return row.Cells[columnIndex];
        return null;
    }

    public Object getCellValue() {
        return getCellValue(this.SelectedRows[0].Index, this.SelectedCells[0].ColumnIndex);
    }

    public Object getCellValue(int columnIndex) {
        return getCellValue(this.SelectedRows[0].Index, this.Columns[columnIndex].Name);
    }
    public Object getCellValue(String columnName) {
        return getCellValue(this.SelectedRows[0].Index, columnName);
    }

    public Object getCellValue(int rowIndex, int columnIndex) {
        return getCellValue(rowIndex, this.Columns[columnIndex].Name);
    }


    public Object getCellValue(int rowIndex, String columnName) {
        if (this.Rows.Count > rowIndex) {
            DataGridViewCell cell = this.Rows[rowIndex].Cells[columnName];
            if (null != cell)
                return cell.Value;
        }
        return null;
    }

    public Object getDataRowValue(int rowIndex, int columnIndex) {
        DataRow row = getDataRow(rowIndex);
        if (null != row)
            return row[Columns[columnIndex].DataPropertyName];
        return null;
    }

    public Object getDataRowValue(int rowIndex, String columnName) {
        DataRow row = getDataRow(rowIndex);
        if (null != row)
            return row[columnName];
        return null;
    }

    public Object getDataRowValue(DataGridViewRow row, String columnName) {
        DataRow dataRow = getDataRow(row);
        if (null != row)
            return dataRow[columnName];
        return null;
    }

    public bool isDataValueModify(int row, int column) {
        DataRow dRow = getDataRow(row);
        if (dRow.RowState == DataRowState.Added || dRow.RowState == DataRowState.Deleted)
            return true;
        if (dRow.RowState == DataRowState.Modified) {
            Object newVal = dRow[Columns[column].DataPropertyName];
            Object oldVal = dRow[Columns[column].DataPropertyName, DataRowVersion.Original];
            if (null == newVal || DBNull.Value.Equals(newVal) || String.IsNullOrEmpty(newVal.ToString()))
                newVal = "";
            if (null == oldVal || DBNull.Value.Equals(oldVal) || String.IsNullOrEmpty(oldVal.ToString()))
                oldVal = "";
            return !newVal.ToString().Trim().Equals(oldVal.ToString().Trim());
        }
        return false;
    }

    // 2024-12-19  1
    public DataRow[] getModifyDataRows() {
        return getModifyDataRows(DataRowState.Added | DataRowState.Modified | DataRowState.Deleted);
    }
    public DataRow[] getModifyDataRows(DataRowState state) {
        if (null == this.sourceTable || 0 == this.sourceTable.Rows.Count)
            return new DataRow[0];
        List<DataRow> list = new List<DataRow>();
        foreach (DataRow row in this.sourceTable.Rows) {
            foreach (DataColumn column in this.sourceTable.Columns) {
                if (state.HasFlag(row.RowState)) {
                    if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Added) {
                        list.Add(row);
                        break;
                    } else {
                        Object now = row[column];
                        Object old = row[column, DataRowVersion.Original];
                        if (null == now || DBNull.Value.Equals(now))
                            now = "";
                        if (null == old || DBNull.Value.Equals(old))
                            old = "";
                        if (!now.ToString().Trim().Equals(old.ToString().Trim())) {
                            list.Add(row);
                            break;
                        }
                    }
                }
            }
        }
        return list.ToArray();
    }

    public String[] getModifyColumnNames() {
        return getModifyColumnNames(DataRowState.Modified | DataRowState.Added | DataRowState.Deleted, this.sourceTable);
    }

    // 2023-09-25   1
    public String[] getModifyColumnNames(DataRowState state) {
        return getModifyColumnNames(state, this.sourceTable);
    }
    // 2023-09-25   1
    public String[] getModifyColumnNames(DataTable table) {
        return getModifyColumnNames(DataRowState.Modified | DataRowState.Added | DataRowState.Deleted, table);
    }
    public String[] getModifyColumnNames(DataRowState state, DataTable table) {
        if (null == table || 0 == table.Rows.Count)
            return new String[0];
        List<String> list = new List<string>();
        foreach (DataRow row in table.Rows) {
            foreach (DataColumn column in table.Columns) {
                if (state.HasFlag(row.RowState)) {
                    Object now = row[column];
                    Object old = row[column, DataRowVersion.Original];
                    if (null == now || DBNull.Value.Equals(now))
                        now = "";
                    if (null == old || DBNull.Value.Equals(old))
                        old = "";
                    if (!now.ToString().Trim().Equals(old.ToString().Trim())) {
                        if (!list.Contains(column.ColumnName))
                            list.Add(column.ColumnName);
                    }
                }
            }
        }
        return list.ToArray();
    }

    public String[] getModifyColumnNames(DataGridViewRow row) {
        return getModifyColumnNames(getDataRow(row));
    }
    public String[] getModifyColumnNames(DataRow row) {
        return getModifyColumnNames(DataRowState.Modified | DataRowState.Added | DataRowState.Deleted, row);
    }

    public String[] getModifyColumnNames(DataRowState state, DataRow row) {
        if (null == row)
            return new String[0];
        List<String> list = new List<string>();
        foreach (DataColumn column in row.Table.Columns) {

            if (state.HasFlag(row.RowState)) {
                Object now = row[column];
                Object old = null;
                if (row.RowState == DataRowState.Added)
                    old = new Random().NextDouble() + "";
                else
                    old = row[column, DataRowVersion.Original];
                if (null == now || DBNull.Value.Equals(now))
                    now = "";
                if (null == old || DBNull.Value.Equals(old))
                    old = "";
                if (!now.ToString().Trim().Equals(old.ToString().Trim())) {
                    if (!list.Contains(column.ColumnName))
                        list.Add(column.ColumnName);
                }
            }
        }
        return list.ToArray();
    }

    // 2024-12-19 2
    public String[] getModifyColumnNamesForLowerOrUpper(DataRow row ,bool isLower) {
        String[] columnNames = getModifyColumnNames(row);
        for (int i = 0; i < columnNames.Length; i++) {
            columnNames[i] = isLower ? columnNames[i].ToLower() : columnNames[i].ToUpper();
        }
        return columnNames;
    }


    public DataRow getDataRow(int row) {
        return getDataRow(this.Rows[row]);
    }

    public DataRow getDataRow(DataGridViewRow row) {
        try {
            if (null != row.DataBoundItem) {
                DataRowView drv = ( (DataRowView)row.DataBoundItem );
                if (null != drv) {
                    drv.EndEdit();
                    return ( (DataRowView)row.DataBoundItem ).Row;
                }
            }
        } catch { }
        return null;
    }




    public NDataGridView3 set(int rowIndex, int columnIndex, Object val) {
        this.Rows[rowIndex].Cells[columnIndex].Value = Tools.trim(val);
        return this;
    }

    public NDataGridView3 set(int rowIndex, String columnName, Object val) {
        DataGridViewRow row = this.Rows[rowIndex];
        if (null != this.Columns[columnName])
            row.Cells[columnName].Value = val;
        return this;
    }
    public NDataGridView3 set(String columnName, Object val) {
        if (0 != this.SelectedRows.Count) {
            this.SelectedRows[0].Cells[columnName].Value = val;
        }
        return this;
    }

    /// <summary>
    /// 添加一行
    /// </summary>
    /// <param name="isValidata">是否验证录入内容</param>
    /// <param name="vals">key 为addHeader 中的columnName  value 是 要添加的值</param>
    /// <returns></returns>
    public Boolean addRow(int index, Dictionary<String, Object> vals) {
        if (null == sourceTable || null == vals)
            return false;
        DataRow row = sourceTable.NewRow();
        if (null != vals) {
            foreach (String columnName in vals.Keys) {
                // 判断绑定的DataTable 是否有这个列，有的话同步添加显示
                if (sourceTable.Columns.Contains(columnName)) {
                    row[columnName] = vals[columnName];
                    // 说明只是添加显示内容，不保存到数据库
                }
            }
        }
        if (-1 == index)
            sourceTable.Rows.Add(row);
        else
            sourceTable.Rows.InsertAt(row, index);

        int rowIndex = -1 == index ? this.RowCount - 1 : 0;
        return true;
    }

    /// <summary>
    /// 添加一行
    /// </summary>
    /// <param name="vals">key 为addHeader 中的columnName  value 是 要添加的值</param>
    /// <returns></returns>
    public Boolean addRow(Dictionary<String, Object> vals) {
        addRow(-1, vals);
        return true;
    }




    /// <summary>
    /// 删除一行，并将绑定的datatable同步修改
    /// </summary>
    /// <param name="rowIndex">删除一行</param>
    /// <returns></returns>
    public NDataGridView3 delRow(int rowIndex) {
        // 2023-05-09 2
        if (null != delNewRowing) {
            if (!delNewRowing(this, rowIndex)) {
                return this;
            }
        }
        this.Rows.RemoveAt(rowIndex);
        if (rowIndex < this.Rows.Count)
            selected(rowIndex, 0);
        else if (rowIndex - 1 >= 0)
            selected(rowIndex - 1, 0);
        return this;
    }


    /// <summary>
    /// 删除一行，并将绑定的datatable同步修改
    /// </summary>
    /// <param name="rowIndex">删除一行</param>
    /// <returns></returns>
    public NDataGridView3 delRow(DataGridViewRow row) {
        // 2023-05-09 2
        if (null != delNewRowing) {
            if (!delNewRowing(this, row.Index)) {
                return this;
            }
        }
        int rowIndex = row.Index;
        this.Rows.Remove(row);
        if (rowIndex < this.Rows.Count)
            selected(rowIndex, 0);
        else if (rowIndex - 1 >= 0)
            selected(rowIndex - 1, 0);
        return this;
    }

    /// <summary>
    /// 删除一行，并将绑定的datatable同步修改
    /// </summary>
    /// <param name="rowIndex">删除一行</param>
    /// <returns></returns>
    public NDataGridView3 delRowNoSelected(int rowIndex) {
        return delRowNoSelected(this.Rows[rowIndex]);
    }
    /// <summary>
    /// 删除一行，并将绑定的datatable同步修改
    /// </summary>
    /// <param name="rowIndex">删除一行</param>
    /// <returns></returns>
    public NDataGridView3 delRowNoSelected(DataGridViewRow row) {
        // 2023-05-09 2
        if (null != delNewRowing) {
            if (!delNewRowing(this, row.Index)) {
                return this;
            }
        }
        this.Rows.Remove(row);
        return this;
    }

    public NDataGridView3 setTag(String tag, Object obj) {
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
    public NDataGridView3 clearTag(String tag) {
        if (this.thisTag.ContainsKey(tag))
            thisTag.Remove(tag);
        return this;
    }
    public NDataGridView3 clearTag() {
        thisTag.Clear();
        return this;
    }

    public void clear() {

        this.ClearSelection();
    }
    public void clearAll() {
        clear();
        if (null != sourceTable && null != sourceTable.Rows && 0 != sourceTable.Rows.Count) {
            sourceTable.Clear();
        }
    }

    public void clearColumns() {
        this.datetimeFormart.Clear();
        this.cellValueFormat.Clear();
        this.Columns.Clear();
        this.headers.Clear();
    }
    public void clearColumns(int columnIndex) {
        clearColumns(this.Columns[columnIndex]);
    }

    public void clearColumns(String columnName) {
        clearColumns(this.Columns[columnName]);
    }
    public void clearColumns(DataGridViewColumn column) {
        ColumnHeader h = null;
        foreach (ColumnHeader header in headers) {
            if (header.column == column) {
                h = header;
                break;
            }
        }
        if (null != h) {
            this.Columns.Remove(h.column);
            headers.Remove(h);
        }
    }


    public NDataGridView3 bindDataTable(DataTable datasource) {

        return setDataSource(datasource);
    }



    public NDataGridView3 reBindDataSource(DataTable datasource) {
        lock (this) {
            bindSelecttionEvent = false;
            if (null != this.sourceTable && null != this.DataSource) {
                int rowIndex = this.SelectedRows.Count > 0 ? this.SelectedRows[0].Index : -1;
                int columnIndex = this.CurrentCell != null ? this.CurrentCell.ColumnIndex : -1;
                if (this.sourceTable != datasource)
                    this.sourceTable.Rows.Clear();
                foreach (DataRow row in datasource.Rows) {
                    Tools.addRow(sourceTable, row);
                    sourceTable.Rows[sourceTable.Rows.Count - 1].AcceptChanges();
                }
                if (-1 != rowIndex)
                    selectedAndEdit(rowIndex, -1 == columnIndex ? 0 : columnIndex);
            } else {
                this.DataSource = this.sourceTable = datasource;
            }
            bindSelecttionEvent = true;
            return this;
        }
    }
    public bool isBindDataing = false;

    public NDataGridView3 setDataSource(DataTable datasource) {
        lock (this) {
            bindSelecttionEvent = false;

            this.clear();
            if (this.sourceTable != datasource) {
                if (null != this.sourceTable) {
                    this.sourceTable.Rows.Clear();
                }
                foreach (ColumnHeader header in getHeaders()) {
                    if (header.type == ColumnType.Checkbox) {
                        if (header.column.HeaderCell is DataGridViewCheckBoxHeaderCell) {
                            ( (DataGridViewCheckBoxHeaderCell)header.column.HeaderCell ).IsChecked = false;
                        }
                    }
                }
            }

            this.sourceTable = datasource;
            if (VirtualMode)
                this.RowCount = datasource.Rows.Count;
            else {
                // 2023-04-21 1
                try {
                    isBindDataing = true;
                    this.DataSource = datasource;
                    if (this.RowCount == 0) {
                        this.DataSource = datasource;
                    }
                    Application.DoEvents();
                    // 处理某些bindData情况下，左边行号不撑开
                    //this.RowHeadersWidth = (int)(this.CreateGraphics().MeasureString( spaces[( datasource.Rows.Count + "" ).Length - 1] + "" + 1,this.Font).Width*1.9);
                    // 处理排序后等情况后，第一行行号居中显示的问题
                    targetRowStateChanged(0, DataGridViewElementStates.Displayed);
                } catch (Exception ee) {
                    if (ee is InvalidOperationException) {
                        if (null != this.CurrentCell && this.IsCurrentCellDirty) {
                            if (typeof(Double) == this.CurrentCell.ValueType || typeof(double) == this.CurrentCell.ValueType
                                || typeof(Int32) == this.CurrentCell.ValueType || typeof(int) == this.CurrentCell.ValueType) {
                                Object v = this.CurrentCell.EditedFormattedValue;
                                if (!Tools.isDouble(v)) {
                                    this.CurrentCell.Value = 0;
                                    this.DataSource = datasource;
                                }
                            }
                        }
                    }
                }
            }
            bindSelecttionEvent = true;
            this.OnSelectionChanged(new EventArgs());
            return this;
        }
    }



    public bool isNull() {
        return null == sourceTable;
    }

    public bool isViewEmpty() {
        return this.Rows.Count == 0;
    }

    public bool isDataTableEmpty() {
        return 0 == sourceTable.Rows.Count;
    }

    public bool isNullOrEmpty() {
        return null == sourceTable || 0 == sourceTable.Rows.Count;
    }

    public DataTable getBindDataTable() {
        return sourceTable;
    }

    public List<ColumnHeader> getHeaders() {
        return this.headers;
    }


    public ColumnHeader getHeader(DataGridViewColumn column) {
        foreach (ColumnHeader item in headers) {
            if (item.column == column)
                return item;
        }
        return null;
    }
    public ColumnHeader getHeader(DataGridViewCell cell) {
        return getHeader(cell.OwningColumn);
    }

    public ColumnHeader getHeader(int columnIndex) {
        return headers[columnIndex];
    }
    public ColumnHeader getHeader(String columnName) {
        foreach (ColumnHeader item in headers) {
            if (Tools.eq(item.dataName, columnName))
                return item;
        }
        return null;
    }



    #endregion


    #region 辅助方法

    public NDataGridView3 setWidth(int w) {
        if (this.ColumnCount > 0) {
            this.Columns[this.ColumnCount - 1].Width = w;
        }
        return this;
    }

    public int findRowIndex(Object primaryKeyVal) {
        DataTable table = getBindDataTable();
        if (null == table)
            return -1;
        return table.DefaultView.Find(primaryKeyVal);
    }
    public int findRowIndex(Object[] primaryKeyVal) {
        DataTable table = getBindDataTable();
        if (null == table)
            return -1;
        return table.DefaultView.Find(primaryKeyVal);
    }

    // 放弃修改
    public NDataGridView3 rejectChanges() {
        if (null != sourceTable)
            sourceTable.RejectChanges();
        return this;
    }
    // 接收修改
    public NDataGridView3 acceptChanges() {
        if (null != sourceTable)
            sourceTable.AcceptChanges();
        return this;
    }

    public NDataGridView3 removeEmptyRow(params String[] notNullColumnName) {

        return removeAllEmptyRow(true, notNullColumnName);
    }

    // 2023-09-19   3
    public NDataGridView3 removeAllEmptyRow(bool isHeaderAndFooter, params String[] notNullColumnName) {
        return removeAllEmptyRow(DataRowState.Added, isHeaderAndFooter, notNullColumnName);
    }

    // 2023-09-19   1
    public NDataGridView3 removeAllEmptyRow(DataRowState state, params String[] notNullColumnName) {

        return removeAllEmptyRow(state, true, notNullColumnName);
    }

    // 2023-09-23   1
    /// <summary>
    /// 移动空行
    /// </summary>
    /// <param name="state">要移除那些状态行</param>
    /// <param name="isHeaderAndFooter">是否只移除头和尾</param>
    /// <param name="notNullColumnName">哪些字段为空</param>
    /// <returns></returns>
    public NDataGridView3 removeAllEmptyRow(DataRowState state, bool isHeaderAndFooter, params String[] notNullColumnName) {
        if (null == notNullColumnName || 0 == notNullColumnName.Length) {
            List<String> columns = new List<string>();
            foreach (ColumnHeader header in headers) {
                if (!header.isNull && header.type == ColumnType.TextBox)
                    columns.Add(header.dataName);
            }
            notNullColumnName = columns.ToArray();
        }
        if (isHeaderAndFooter) {
            while (0 != this.Rows.Count) {
                DataGridViewRow row = getRow(0);
                DataRow drow = this.getDataRow(row);
                if (!state.HasFlag(drow.RowState))
                    break;
                int emptyCount = 0;
                foreach (String columnName in notNullColumnName) {
                    if (Tools.isNullOrEmpty(row.Cells[columnName].Value))
                        emptyCount++;
                }
                if (emptyCount == notNullColumnName.Length) {
                    // 2023-04-20  1
                    this.delRowNoSelected(row.Index);
                } else {
                    break;
                }
            }
            while (0 != this.Rows.Count) {
                DataGridViewRow row = getRow(this.Rows.Count - 1);
                DataRow drow = this.getDataRow(row);
                if (!state.HasFlag(drow.RowState))
                    break;
                int emptyCount = 0;
                foreach (String columnName in notNullColumnName) {
                    if (Tools.isNullOrEmpty(row.Cells[columnName].Value))
                        emptyCount++;
                }
                if (emptyCount == notNullColumnName.Length) {
                    // 2023-04-20  1
                    this.delRowNoSelected(row.Index);
                } else {
                    break;
                }
            }
        } else {
            List<DataGridViewRow> removeList = new List<DataGridViewRow>();
            for (int i = 0; i < this.RowCount; i++) {
                DataGridViewRow item = this.Rows[i];
                DataRow row = this.getDataRow(item);
                if (state.HasFlag(row.RowState)) {
                    int emptyCount = 0;
                    foreach (String columnName in notNullColumnName) {
                        if (Tools.isNullOrEmpty(item.Cells[columnName].Value))
                            emptyCount++;
                    }
                    if (emptyCount == notNullColumnName.Length) {
                        this.delRowNoSelected(item);
                        i--;
                    }
                }
            }
        }
        return this;
    }

    public String getCellValueFormat(String columnName, String defFormat) {
        if (cellValueFormat.ContainsKey(columnName))
            return cellValueFormat[columnName];
        return defFormat;
    }

    public NDataGridView3 addCellValueFormat(String columnName, String format) {
        columnName = columnName.ToLower();
        if (cellValueFormat.ContainsKey(columnName))
            cellValueFormat.Remove(columnName);
        cellValueFormat.Add(columnName, format);
        return this;
    }

    public NDataGridView3 addCellValFormat(String format, params String[] columnNames) {
        foreach (String cName in columnNames) {
            if (cellValueFormat.ContainsKey(cName))
                cellValueFormat.Remove(cName);
            cellValueFormat.Add(cName, format);
        }
        return this;
    }

    public NDataGridView3 removeCellValueFormat(params String[] columnNames) {
        foreach (String cName in columnNames) {
            if (cellValueFormat.ContainsKey(cName))
                cellValueFormat.Remove(cName);
        }
        return this;
    }

    public NDataGridView3 setColumnWidth(int width, params String[] columnNames) {
        foreach (String columnName in columnNames) {
            foreach (DataGridViewColumn column in Columns) {
                if (column.Name.Equals(columnName)) {
                    this.Columns[columnName].Width = width;
                    break;
                }
            }
        }
        return this;
    }

    public NDataGridView3 updateCurrentCellValue(DataGridViewCell cell) {
        if (null != this.CurrentCell) {
            if (typeof(Double) == this.CurrentCell.ValueType || typeof(double) == this.CurrentCell.ValueType || typeof(int) == this.CurrentCell.ValueType || typeof(Int32) == this.CurrentCell.ValueType) {
                Object v = this.CurrentCell.EditedFormattedValue;
                double vv = 0.0D;
                int vvv = 0;
                if (double.TryParse(v + "", out vv) || int.TryParse(v + "", out vvv)) {
                    this.CurrentCell = null;
                    this.CurrentCell = cell;
                }
            } else {
                this.CurrentCell = null;
                this.CurrentCell = cell;
            }
        }
        return this;
    }
    public NDataGridView3 setColumnLeft(params String[] columnNames) {
        return setColumnAlign(DataGridViewContentAlignment.MiddleLeft, columnNames);
    }
    public NDataGridView3 setColumnCenter(params String[] columnNames) {
        return setColumnAlign(DataGridViewContentAlignment.MiddleCenter, columnNames);
    }
    public NDataGridView3 setColumnRight(params String[] columnNames) {
        return setColumnAlign(DataGridViewContentAlignment.MiddleRight, columnNames);
    }

    public NDataGridView3 setColumnAlign(DataGridViewContentAlignment align, params String[] columnNames) {
        foreach (String columnName in columnNames) {
            foreach (DataGridViewColumn column in Columns) {
                if (column.Name.Equals(columnName)) {
                    column.DefaultCellStyle.Alignment = column.HeaderCell.Style.Alignment = align;
                    this.Columns[columnName].DefaultCellStyle.Alignment = this.Columns[columnName].HeaderCell.Style.Alignment = align;
                    break;
                }
            }
        }
        return this;
    }



    public NDataGridView3 selectRow(String columnName, Object columnValue) {
        foreach (DataGridViewRow row in this.Rows) {
            DataRow dataRow = this.getDataRow(row);
            if (Tools.eqIgnoreCase(columnValue + "", dataRow[columnName])) {
                return selected(row.Index, this.Columns.Contains(columnName) ? this.Columns[columnName].Index : 0);
            }
            //DataGridViewCell cell = row.Cells[columnName];
            //if (Tools.eq(columnValue + "", cell.Value)) {
            //    return selected(cell.RowIndex, cell.ColumnIndex);
            //}
        }
        return this;
    }

    public NDataGridView3 selected(int rowIndex, int columnIndex, bool isTargetEvent) {
        ClearSelection();
        DataGridViewRow row = getRow(rowIndex);
        if (null != row) {
            this.SelectionChanged -= NDataGridView_SelectionChanged;
            bool isFail = false;
            // 输入的原始内容
            String cellOldValue = null == this.CurrentCell ? "" : this.CurrentCell.EditedFormattedValue + "";
            try {
                this.CurrentCell = null;
            } catch (Exception eee) {
                isFail = true;
                // 2024-01-24   2
                if (eee is InvalidOperationException) {
                    if (eee.Message.Contains("无法提交或取消单元格值更改")) {
                        // 2024-05-06   1
                        // 部分validata需要处理类型设置未null后，自动补齐的默认值，如DateTime默认为DateTime.MinValue
                        bool isShowDialog = true;
                        int oldColumnIndex = this.CurrentCell.ColumnIndex;
                        String oldColumnName = this.Columns[oldColumnIndex].DataPropertyName;
                        int oldRowIndex = this.CurrentCell.RowIndex;
                        ColumnHeader header = this.getHeader(oldColumnIndex);
                        DataRow dataRow = this.getDataRow(oldRowIndex);
                        if (null != dataRow) {
                            //2024-06-05    1
                            String currColumnName = this.Columns[oldColumnIndex].DataPropertyName.ToLower();
                            if (dataRow.Table.Columns[currColumnName].DataType == typeof(DateTime) && datetimeFormart.ContainsKey(currColumnName)) {
                                String[] v_s = datetimeFormart[currColumnName];
                                if (null != cellOldValue && !DBNull.Value.Equals(cellOldValue) && !String.IsNullOrEmpty(cellOldValue)) {
                                    String dateVal = "";
                                    String validatVal = noNumber.Replace(cellOldValue, "");

                                    //yyyyMMddHHmmss
                                    // yyyy-MM-dd HH:mm:ss
                                    if (validatVal.Length == 8)
                                        dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Substring(0, 10);
                                    else if (validatVal.Length >= 14)
                                        dateVal = validatVal.ToString().Insert(4, "-").Insert(7, "-").Insert(10, " ").Insert(13, ":").Insert(16, ":").Substring(0, 19);
                                    else
                                        dateVal = cellOldValue;

                                    if (!String.IsNullOrEmpty(dateVal)) {
                                        DateTime tryDate = DateTime.MinValue;
                                        if (DateTime.TryParse(dateVal, out tryDate)) {
                                            dataRow[oldColumnName] = tryDate.ToString(v_s[0]);
                                            this.CurrentCell.Value = tryDate.ToString(v_s[1]);
                                            isShowDialog = false;
                                        }
                                    }
                                }
                            } else if (header.isNull) {
                                dataRow[oldColumnName] = DBNull.Value;
                                this.CurrentCell.Value = "";
                                isShowDialog = false;
                            }
                        }
                        if (isShowDialog)
                            MessageBox.Show("当前数据[" + this.CurrentCell.EditedFormattedValue + "]无效！", this.Columns[oldColumnIndex].HeaderText + "数据无效！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        try {
                            this.CurrentCell = null;
                            isFail = false;
                        } catch {

                        }
                    }
                }
                if (isFail) {
                    // 2023-03-17.1
                    this.CurrentCell.Selected = true;
                    this.CurrentCell.OwningRow.Selected = true;
                    this.SelectionChanged += NDataGridView_SelectionChanged;
                    // 2023-04-19 2
                    if (isTargetEvent)
                        this.OnSelectionChanged(new EventArgs());
                    return this;
                }
            }
            row.Selected = true;
            row.Cells[columnIndex].Selected = true;
            this.CurrentCell = row.Cells[columnIndex];
            this.SelectionChanged += NDataGridView_SelectionChanged;
            // 2023-04-19 2
            if (isTargetEvent)
                this.OnSelectionChanged(new EventArgs());
        } else {
            this.FirstDisplayedScrollingColumnIndex = 0;
        }
        return this;
    }

    public NDataGridView3 selected(int rowIndex, int columnIndex) {
        return selected(rowIndex, columnIndex, true);
    }

    public NDataGridView3 selectedAndEdit(int rowIndex, int columnIndex) {
        try {
            if (this.Rows.Count == 0)
                NDataGridView3_MouseClick(this, new MouseEventArgs(MouseButtons.Left, 1, 5, 5, 0));
            // 2023-04-19 3
            if (null != this.CurrentCell) {
                if (rowIndex == this.CurrentCell.RowIndex && columnIndex == this.CurrentCell.ColumnIndex) {
                    // 2023-04-19  4
                    if (EditMode == DataGridViewEditMode.EditOnEnter) {
                        this.BeginEdit(true);
                    }
                    return this;
                }
            }
            selected(rowIndex, columnIndex);
            if (EditMode == DataGridViewEditMode.EditOnEnter) {
                this.BeginEdit(true);
            }
        } catch (Exception ee) {
            // 说明是日期列，
            if (ee.StackTrace.Contains("InitializeEditingControlValue")) {
                DataGridViewCell cell = this.Rows[rowIndex].Cells[columnIndex];
                this.CurrentCell = cell;
                // 2024-05-21   1
                try {
                    cell.Selected = true;
                } catch (Exception eee) {
                    String sdsds = "";
                }
            } else {
                throw ee;
            }
        }
        return this;
    }



    public Boolean isOnlyInsert() {
        bool isInert = false;
        int r = -1;
        int c = -1;
        this.EndEdit();
        if (null != this.sourceTable) {
            foreach (DataRow row in this.sourceTable.Rows) {
                row.EndEdit();
                if (row.RowState == DataRowState.Added) {
                    isInert = true;
                } else if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Unchanged) {
                    isInert = false;
                    break;
                }
            }
        }
        return isInert;
    }

    public Boolean isContainsInsert() {
        bool isInert = false;
        int r = -1;
        int c = -1;
        this.EndEdit();
        if (null != this.sourceTable) {
            foreach (DataRow row in this.sourceTable.Rows) {
                row.EndEdit();
                if (row.RowState == DataRowState.Added) {
                    isInert = true;
                    break;
                }
            }
        }
        return isInert;
    }

    public bool isNewModify(DataTable table, params String[] columns) {
        bool isModify = false;
        if (null != table) {
            List<DataColumn> validataColumns = new List<DataColumn>();
            if (null == columns || 0 == columns.Length) {
                foreach (DataColumn column in table.Columns) {
                    validataColumns.Add(column);
                }
            } else {
                foreach (String columnName in columns) {
                    if (table.Columns.Contains(columnName))
                        validataColumns.Add(table.Columns[columnName]);
                }
            }

            foreach (DataRow row in table.Rows) {
                row.EndEdit();
                if (row.RowState == DataRowState.Deleted)
                    return true;
                else if (row.RowState == DataRowState.Added) {
                    bool isRealModify = false;
                    foreach (DataColumn column in validataColumns) {
                        Object now = row[column];
                        if (null == now || DBNull.Value.Equals(now))
                            now = "";
                        if (!"".Equals(now.ToString().Trim())) {
                            isRealModify = true;
                            break;
                        }
                    }
                    if (isRealModify) {
                        isModify = true;
                        break;
                    }
                }
                if (row.RowState == DataRowState.Modified) {
                    bool isRealModify = false;
                    foreach (DataColumn column in validataColumns) {
                        Object now = row[column];
                        Object old = row[column, DataRowVersion.Original];
                        if (null == now || DBNull.Value.Equals(now))
                            now = "";
                        if (null == old || DBNull.Value.Equals(old))
                            old = "";
                        if (!now.ToString().Trim().Equals(old.ToString().Trim())) {
                            isRealModify = true;
                            // 2024-06-20   1
                            if (Tools.isDouble(now.ToString().Trim()) && Tools.isDouble(old.ToString().Trim())) {
                                if (Convert.ToDecimal(now.ToString().Trim()) == Convert.ToDecimal(old.ToString().Trim()))
                                    isRealModify = false;
                            }
                            if (isRealModify) {
                                break;
                            }
                        }
                    }
                    if (isRealModify) {
                        isModify = true;
                        break;
                    }
                }
            }
        }
        return isModify;
    }

    public bool isNewModify(params String[] columns) {
        int r = -1;
        int c = -1;
        this.CommitEdit(DataGridViewDataErrorContexts.Commit);
        this.EndEdit();
        if (null != this.CurrentCell && EditMode == DataGridViewEditMode.EditOnEnter) {
            if (this.CurrentCell.IsInEditMode || typeof(DGVCheckBoxColumn) == this.CurrentCell.OwningColumn.GetType()) {
                r = this.CurrentCell.RowIndex;
                c = this.CurrentCell.ColumnIndex;
                if (typeof(Double) == this.CurrentCell.ValueType || typeof(double) == this.CurrentCell.ValueType || typeof(Decimal) == this.CurrentCell.ValueType || typeof(decimal) == this.CurrentCell.ValueType) {
                    Object v = this.CurrentCell.EditedFormattedValue;
                    double vv = 0.0D;
                    if (!double.TryParse(v + "", out vv)) {
                        return true;
                    }
                }
                if (typeof(int) == this.CurrentCell.ValueType || typeof(Int32) == this.CurrentCell.ValueType) {
                    Object v = this.CurrentCell.EditedFormattedValue;
                    int vv = 0;
                    if (!int.TryParse(v + "", out vv)) {
                        return true;
                    }
                }
                if (this.IsCurrentCellDirty) {
                    // 2024-07-04   1.放开try，让下层去处理日期格式有问题的数据
                    try {
                        this.CurrentCell = null;
                        selectedAndEdit(r, c);
                    } catch (InvalidOperationException eee) {
                        if (eee.Message.Contains("无法提交或取消单元格值更改")) {
                            return true;
                        }
                    }
                }
            }
        }
        if (null == columns || 0 == columns.Length) {
            List<String> validataList = new List<string>();
            foreach (ColumnHeader header in this.getHeaders()) {
                /*
                if (header.type == ColumnType.TextBox) {
                    if (!header.isNull)
                        validataList.Add(header.dataName);
                    else if (header.isNull && (0 != header.minLength || int.MaxValue != header.maxLength))
                        validataList.Add(header.dataName);
                } else if (header.type != ColumnType.Button) {
                    if (header.isClick)
                        validataList.Add(header.dataName);
                }
                */
                // 2024-07-16   1
                if (header.type != ColumnType.Button) {
                    validataList.Add(header.dataName);
                }
            }
            columns = validataList.ToArray();
        }

        return isNewModify(this.sourceTable, columns);
    }


    public bool isNewModify(DataRow row, params String[] columns) {
        bool isModify = false;
        int r = -1;
        int c = -1;
        this.CommitEdit(DataGridViewDataErrorContexts.Commit);
        this.EndEdit();
        if (null != this.CurrentCell && EditMode == DataGridViewEditMode.EditOnEnter) {
            if (this.CurrentCell.IsInEditMode || typeof(DGVCheckBoxColumn) == this.CurrentCell.OwningColumn.GetType()) {
                r = this.CurrentCell.RowIndex;
                c = this.CurrentCell.ColumnIndex;
                if (typeof(Double) == this.CurrentCell.ValueType || typeof(double) == this.CurrentCell.ValueType || typeof(Decimal) == this.CurrentCell.ValueType || typeof(decimal) == this.CurrentCell.ValueType) {
                    Object v = this.CurrentCell.EditedFormattedValue;
                    double vv = 0.0D;
                    if (!double.TryParse(v + "", out vv)) {
                        return true;
                    }
                }
                if (this.IsCurrentCellDirty) {
                    //try {
                    this.CurrentCell = null;
                    selectedAndEdit(r, c);
                    //} catch (InvalidOperationException eee) {
                    //    if (eee.Message.Contains("无法提交或取消单元格值更改")) {
                    //        MessageBox.Show("数据无效！", "当前单元格数据["+ this.CurrentCell.EditedFormattedValue + "]无效！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //        if (this.sourceTable == row.Table) {
                    //            this.selectedAndEdit(r,c);
                    //        }
                    //        return false;
                    //    }
                    //}
                }
            }
        }
        if (null != this.sourceTable) {
            List<DataColumn> validataColumns = new List<DataColumn>();
            if (null == columns || 0 == columns.Length) {
                foreach (DataColumn column in sourceTable.Columns) {
                    validataColumns.Add(column);
                }
            } else {
                foreach (String columnName in columns) {
                    if (this.sourceTable.Columns.Contains(columnName))
                        validataColumns.Add(sourceTable.Columns[columnName]);
                }
            }
            row.EndEdit();
            if (row.RowState == DataRowState.Deleted)
                isModify = true;
            else if (row.RowState == DataRowState.Added) {
                foreach (DataColumn column in validataColumns) {
                    Object now = row[column];
                    if (null == now || DBNull.Value.Equals(now))
                        now = "";
                    if (!"".Equals(now.ToString().Trim())) {
                        isModify = true;
                    }
                }
            } else if (row.RowState == DataRowState.Modified) {
                foreach (DataColumn column in validataColumns) {
                    Object now = row[column];
                    Object old = row[column, DataRowVersion.Original];
                    if (null == now || DBNull.Value.Equals(now))
                        now = "";
                    if (null == old || DBNull.Value.Equals(old))
                        old = "";
                    if (!now.ToString().Trim().Equals(old.ToString().Trim())) {
                        isModify = true;
                    }
                }
            }
        }
        return isModify;
    }


    public Boolean isModify() {
        bool isModify = false;
        int r = -1;
        int c = -1;
        this.CommitEdit(DataGridViewDataErrorContexts.Commit);
        this.EndEdit();
        if (null != this.CurrentCell && EditMode == DataGridViewEditMode.EditOnEnter) {
            if (this.CurrentCell.IsInEditMode || typeof(DGVCheckBoxColumn) == this.CurrentCell.OwningColumn.GetType()) {
                r = this.CurrentCell.RowIndex;
                c = this.CurrentCell.ColumnIndex;
                if (typeof(Double) == this.CurrentCell.ValueType || typeof(double) == this.CurrentCell.ValueType || typeof(Decimal) == this.CurrentCell.ValueType || typeof(decimal) == this.CurrentCell.ValueType) {
                    Object v = this.CurrentCell.EditedFormattedValue;
                    double vv = 0.0D;
                    if (!double.TryParse(v + "", out vv)) {
                        return true;
                    }
                }
                if (this.IsCurrentCellDirty) {
                    this.CurrentCell = null;
                    selectedAndEdit(r, c);
                }
            }
        }
        if (null != this.sourceTable) {
            foreach (DataRow row in this.sourceTable.Rows) {
                row.EndEdit();
                if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Added)
                    return true;
                if (row.RowState == DataRowState.Modified) {
                    bool isRealModify = false;
                    foreach (DataColumn column in this.sourceTable.Columns) {
                        Object now = row[column];
                        Object old = row[column, DataRowVersion.Original];
                        if (null == now || DBNull.Value.Equals(now))
                            now = "";
                        if (null == old || DBNull.Value.Equals(old))
                            old = "";
                        if (!now.ToString().Equals(old.ToString())) {
                            isRealModify = true;
                            break;
                        }
                    }
                    if (isRealModify) {
                        isModify = true;
                        break;
                    }
                }
            }
        }
        return isModify;
    }


    public Boolean isModify(int rowIndex, int columnIndex) {
        return isModify(Rows[rowIndex], columnIndex);
    }
    public Boolean isModify(int rowIndex, String columnName) {
        return isModify(Rows[rowIndex], Columns[columnName].Index);
    }
    public Boolean isModify(DataGridViewRow row, String columnName) {
        return isModify(row, Columns[columnName].Index);
    }

    public Boolean isModify(DataGridViewRow row, int columnIndex) {
        DataRow dRow = getDataRow(row);
        if (null == dRow)
            return true;
        if (dRow.RowState == DataRowState.Added || dRow.RowState == DataRowState.Deleted)
            return true;
        if (dRow.RowState == DataRowState.Modified) {
            Object newV = dRow[Columns[columnIndex].DataPropertyName];
            Object oldV = dRow[Columns[columnIndex].DataPropertyName, DataRowVersion.Original];
            return !( ( newV + "" ).Equals(oldV + "") );
        }
        return false;
    }


    public bool isValueModify() {
        return isModify();
    }
    // 2023-04-11 1
    public void resetBackgroundColor(DataGridViewRow row) {
        //foreach (ColumnHeader header in getHeaders()) {
        resetBackgroundColor(row.Index);
    }
    // 2023-04-11 1
    public void resetBackgroundColor(int rowIndex) {
        if (this.RowCount > rowIndex) {
            this.InvalidateRow(rowIndex);
        }
    }
    // 2023-04-11 1
    public void resetBackgroundColor(int rowIndex, int columnIndex) {
        if (this.RowCount > rowIndex && this.ColumnCount > columnIndex) {
            this.InvalidateCell(columnIndex, rowIndex);
        }
    }
    // 2023-04-11 1
    public void resetBackgroundColor(int rowIndex, String columnName) {
        if (this.RowCount > rowIndex && this.Columns.Contains(columnName)) {
            DataGridViewColumn column = this.Columns[columnName];
            resetBackgroundColor(rowIndex, column.Index);
        }
    }
    // 2023-04-11 1
    public void resetBackgroundColor() {
        foreach (DataGridViewRow row in this.Rows) {
            resetBackgroundColor(row.Index);
        }
    }

    /// <summary>
    /// 启用编辑，单元格变色必须要在setDataSource之前调用
    /// </summary>
    public NDataGridView3 startEdit() {
        editCellBackColor = System.Drawing.SystemColors.Window;
        resetBackgroundColor();
        RowTemplate.ReadOnly = ReadOnly = false;
        EditMode = DataGridViewEditMode.EditOnEnter;
        // 2023-12-29   1
        //if (!this.InvokeRequired) {
        //    BeginInvoke(new MethodInvoker(() => {
        resetBackgroundColor();
        //    }));
        //}
        return this;
    }

    /// <summary>
    /// 设置为单据列表背景色
    /// </summary>
    public NDataGridView3 setSingleStyle() {
        this.editCellBackColor = System.Drawing.SystemColors.Window;
        this.noEditCellBackColor = System.Drawing.SystemColors.ButtonHighlight;
        return this;
    }
    /// <summary>
    /// 设置为明细列表背景色
    /// </summary>
    public NDataGridView3 setInputListStyle() {
        this.editCellBackColor = System.Drawing.SystemColors.Window;
        this.noEditCellBackColor = System.Drawing.Color.LemonChiffon;
        return this;
    }
    /// <summary>
    /// 设置为明细列表背景色
    /// </summary>
    public NDataGridView3 setNormalStyle() {
        this.editCellBackColor = System.Drawing.SystemColors.Window;
        this.noEditCellBackColor = System.Drawing.SystemColors.ButtonFace;
        return this;
    }

    /// <summary>
    /// 关闭编辑模式，单元格变色必须要在setDataSource之前调用
    /// </summary>
    public NDataGridView3 closeEdit() {
        isExecDefultAdd = isExecDefultSub = false;
        editCellBackColor = noEditCellBackColor;
        resetBackgroundColor();
        RowTemplate.ReadOnly = ReadOnly = false;
        EditMode = DataGridViewEditMode.EditProgrammatically;
        //BeginInvoke(new MethodInvoker(() => {
        resetBackgroundColor();
        //}));
        return this;
    }


    #endregion



    #region 样式处理





    bool isSetDefaultCursor = false;


    private void NDataGridView3_CursorChanged(object sender, EventArgs e) {
        if (isSetDefaultCursor && this.Cursor == Cursors.SizeWE) {
            this.Cursor = Cursors.Default;
        }

    }


    private void NDataGridView3_MouseMove(object sender, MouseEventArgs e) {
        isSetDefaultCursor = false;
        if (mutis.Count > 0) {
            foreach (Rectangle item in splitPoints.Values) {
                if (item.Left <= e.X && e.X <= item.Right && item.Top < e.Y && e.Y < item.Bottom) {
                    this.Cursor = Cursors.Default;
                    isSetDefaultCursor = true;
                    break;
                }
            }
        }
    }

    private int _muti_top = 0, _muti_left = 0, _muti_height = 0, _muti_right = 0;
    private Dictionary<int, Rectangle> splitPoints = new Dictionary<int, Rectangle>();
    public void gridview_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) {
        DataGridView dgv = (DataGridView)( sender );


        if (SelectRowHeader) {
            if (e.ColumnIndex == -1 && null != dgv.CurrentCell && dgv.CurrentCell.RowIndex == e.RowIndex) {
                e.CellStyle.BackColor = dgv.RowHeadersDefaultCellStyle.SelectionBackColor;
                e.CellStyle.ForeColor = dgv.RowHeadersDefaultCellStyle.SelectionForeColor;
            }
        }

        if (e.RowIndex != -1 && e.ColumnIndex != -1) {
            if (e.RowIndex >= this.RowCount || e.ColumnIndex >= this.ColumnCount || null == getDataRow(e.RowIndex))
                return;
            // 2023-09-19  1
            Color customBackgroundColor = cellCustomBackgroundColor(this, e.RowIndex, e.ColumnIndex);
            if (Color.Transparent == customBackgroundColor) {
                if (cellEdit(this, e.RowIndex, e.ColumnIndex)) {
                    e.CellStyle.BackColor = editCellBackColor;
                } else {
                    e.CellStyle.BackColor = noEditCellBackColor;
                }
            } else {
                e.CellStyle.BackColor = customBackgroundColor;
            }
            // 2023-09-19   2
            Color customForeColor = cellCustomForeColor(this, e.RowIndex, e.ColumnIndex);
            if (Color.Transparent != customForeColor) {
                e.CellStyle.ForeColor = customForeColor;
            }
        }

        if (!isPaint)
            return;
        int columnIndex = e.ColumnIndex;
        if (e.RowIndex == -1 && mutis.Count > 0) {
            foreach (MutiHeader header in mutis) {
                if (e.ColumnIndex >= header.startIndex && e.ColumnIndex <= header.endIndex) {
                    if (e.ColumnIndex == header.startIndex) {
                        _muti_top = e.CellBounds.Top;
                        _muti_left = e.CellBounds.Left;
                        _muti_height = e.CellBounds.Height;

                    } else {
                        // 判断是合并列，但是这里页面没显示完全，没有从和并列的第一列开始绘制，
                        if (dgv.FirstDisplayedScrollingColumnIndex != 0) {
                            if (dgv.FirstDisplayedScrollingColumnIndex > header.startIndex && dgv.FirstDisplayedScrollingColumnIndex <= header.endIndex) {
                                _muti_left = 0;
                                for (int i = 0; i < header.startIndex; i++) {
                                    _muti_top = e.CellBounds.Top;
                                    _muti_left += Columns[i].Width;
                                    _muti_height = e.CellBounds.Height;
                                }
                                _muti_left = _muti_left - dgv.HorizontalScrollingOffset + dgv.RowHeadersWidth;
                            }
                        }
                    }
                    int width = 0;//总长度
                    for (int i = header.startIndex; i <= header.endIndex; i++) {
                        width += dgv.Columns[i].Width;
                    }

                    Rectangle rect = new Rectangle(_muti_left, _muti_top, width, e.CellBounds.Height);
                    using (Brush backColorBrush = new SolidBrush(e.CellStyle.BackColor)) {
                        e.Graphics.FillRectangle(backColorBrush, rect);
                    }
                    using (Pen gridLinePen = new Pen(dgv.GridColor)) //画笔颜色
                    {
                        SolidBrush headerTextColor = new SolidBrush(e.CellStyle.ForeColor);
                        // 绘制上边框
                        e.Graphics.DrawLine(gridLinePen, _muti_left, _muti_top, _muti_left + width, _muti_top);
                        // 绘制添加的表头和原本表头上下分割横线
                        e.Graphics.DrawLine(gridLinePen, _muti_left, _muti_top + _muti_height / 2, _muti_left + width - 1, _muti_top + _muti_height / 2);
                        // 绘制原本表头下边框
                        e.Graphics.DrawLine(gridLinePen, _muti_left, _muti_top + _muti_height - 1, _muti_left + width - 1, _muti_top + _muti_height - 1);
                        _muti_right = 0;
                        // 绘制左做边框
                        //e.Graphics.DrawLine(gridLinePen, _muti_left , _muti_top, _muti_left , _muti_top + _muti_height);
                        for (int i = header.startIndex; i <= header.endIndex; i++) {

                            if (i == header.startIndex)
                                _muti_right += dgv.Columns[i].Width - 1; //分隔区域首列
                            else if (i < header.endIndex) {
                                _muti_right += dgv.Columns[i].Width; //分隔区域首列
                                if (splitPoints.ContainsKey(i))
                                    splitPoints.Remove(i);
                                splitPoints.Add(i, new Rectangle(_muti_left + _muti_right - 4, _muti_top, 17, _muti_height / 2));
                            } else {
                                _muti_right += dgv.Columns[i].Width;
                            }
                            // 绘制合并表头和原本表头的上下分割竖线，这里-3是为了和原边框下面的空白保持一致，目前没有发现这个3可以从哪里取
                            e.Graphics.DrawLine(gridLinePen, _muti_left + _muti_right, _muti_height / 2, _muti_left + _muti_right, _muti_height - 3);
                            //splitPoints.Add(new Point( _muti_left + _muti_right - 4, _muti_left + _muti_right - 4+12));
                        }
                        // 绘制最后一列右边框，这里 top+3和 bottom-6是为了和原边框上下空白保持一致，目前没有发现可以从哪里取
                        e.Graphics.DrawLine(gridLinePen, _muti_left + _muti_right, _muti_top + 3, _muti_left + _muti_right, _muti_height - 6);
                        SizeF sf = e.Graphics.MeasureString(header.headerText, e.CellStyle.Font);
                        float lstr = ( width - sf.Width ) / 2F;
                        if (header.align == HorizontalAlignment.Left)
                            lstr = 0;
                        else if (header.align == HorizontalAlignment.Right)
                            lstr = ( width - sf.Width );
                        float rstr = ( _muti_height / 2F - sf.Height ) / 2;
                        e.Graphics.DrawString(header.headerText, e.CellStyle.Font, headerTextColor, _muti_left + lstr, _muti_top + rstr + 1);

                        width = 0;
                        _muti_right = 0;
                        // 绘制原表头
                        for (int i = header.startIndex; i <= header.endIndex; i++) {
                            string columnValue = dgv.Columns[i].HeaderText;
                            _muti_right = dgv.Columns[i].Width;
                            sf = e.Graphics.MeasureString(columnValue, e.CellStyle.Font);
                            lstr = ( _muti_right - sf.Width ) / 2F;
                            if (header.align == HorizontalAlignment.Left)
                                lstr = 0;
                            else if (header.align == HorizontalAlignment.Right)
                                lstr = ( width - sf.Width );
                            rstr = ( _muti_height / 2F - sf.Height ) / 2F;
                            e.Graphics.DrawString(columnValue, e.CellStyle.Font, headerTextColor, _muti_left + width + lstr, _muti_top + _muti_height / 2 + rstr + 1, StringFormat.GenericDefault);
                            width += dgv.Columns[i].Width;
                        }
                    }
                    e.Handled = true;
                }
            }
        }
    }


    #endregion



    public enum ColumnFormat {
        TEXT, INT, DOUBLE
    }
    public enum ColumnType {
        TextBox, Checkbox, Combobox, Button, DateTime
    }


    public class CellValidata {
        public String errorMsg = "";
        public bool validataSuccess = true;
        public int rowIndex = 0;
        public int columnIndex = 0;

        public CellValidata() {
            errorMsg = "";
            validataSuccess = true;
        }
        public CellValidata(String msg, int rowIndex, int columnIndex) {
            this.errorMsg = msg;
            this.validataSuccess = false;
            this.rowIndex = rowIndex;
            this.columnIndex = columnIndex;
        }
    }


    public class ColumnHeader {
        public DataGridViewColumn column;
        public ColumnType type;
        public ColumnFormat format;
        public String dataName;
        public String showName;
        public Boolean isNull;
        public int minLength;
        public int maxLength;
        public String valueFormat;
        public String showFormat;
        public string trueVal;
        public String falseVal;
        public bool isSelectMode;
        public bool isClick = true;
        public bool isExecDefaultEvent = true;

        public static ColumnHeader datetime(NDataGridView3 view, String dataName, String showName, String valueFormat, String showFormat, bool isClick) {
            return datetime(view, dataName, showName, valueFormat, showFormat, true, isClick);
        }
        public static ColumnHeader datetime(NDataGridView3 view, String dataName, String showName, String valueFormat, String showFormat, bool isSelectMode, bool isClick) {

            ColumnHeader header = new ColumnHeader(ColumnType.DateTime, ColumnFormat.TEXT, dataName, showName);
            header.isNull = true;
            header.valueFormat = valueFormat;
            header.showFormat = showFormat;
            header.isSelectMode = isSelectMode;
            header.isClick = header.isExecDefaultEvent = isClick;
            header.column = getColumn(view, header);
            return header;
        }
        public static ColumnHeader checkbox(NDataGridView3 view, String dataName, String showName, bool isClick) {
            return checkbox(view, dataName, showName, isClick, isClick);
        }
        public static ColumnHeader checkbox(NDataGridView3 view, String dataName, String showName, bool isClick, bool isExecDefaultEvent) {
            ColumnHeader header = new ColumnHeader(ColumnType.Checkbox, ColumnFormat.TEXT, dataName, showName);
            header.isNull = true;
            header.isClick = isClick;
            header.isExecDefaultEvent = isExecDefaultEvent;
            header.column = getColumn(view, header);
            return header;
        }
        public static ColumnHeader checkbox(NDataGridView3 view, String dataName, String showName, bool isClick, bool isExecDefaultEvent, String trueVal, String falseVal) {
            ColumnHeader header = new ColumnHeader(ColumnType.Checkbox, ColumnFormat.TEXT, dataName, showName);
            header.isNull = true;
            header.isClick = isClick;
            header.isExecDefaultEvent = isExecDefaultEvent;
            header.trueVal = trueVal;
            header.falseVal = falseVal;
            header.column = getColumn(view, header);
            return header;
        }

        public static ColumnHeader combox(NDataGridView3 view, String dataName, String showName) {
            return combox(view, dataName, showName, true);
        }
        public static ColumnHeader combox(NDataGridView3 view, String dataName, String showName, bool isClick) {
            ColumnHeader header = new ColumnHeader(ColumnType.Combobox, ColumnFormat.TEXT, dataName, showName);
            header.isNull = true;
            header.isClick = header.isExecDefaultEvent = isClick;
            header.column = getColumn(view, header);
            return header;
        }
        public static ColumnHeader combox(NDataGridView3 view, String dataName, String showName, comboxValueChangedListener listener) {
            ColumnHeader header = new ColumnHeader(ColumnType.Combobox, ColumnFormat.TEXT, dataName, showName);
            header.isNull = true;
            header.isClick = header.isExecDefaultEvent = true;
            header.column = getColumn(view, header);
            return header;
        }

        public static ColumnHeader button(NDataGridView3 view, string dataName, string showName) {
            return button(view, dataName, showName, true);
        }
        public static ColumnHeader button(NDataGridView3 view, string dataName, string showName, bool isClick) {
            ColumnHeader header = new ColumnHeader(ColumnType.Button, ColumnFormat.TEXT, dataName, showName);
            header.isNull = true;
            header.isClick = isClick;
            header.column = getColumn(view, header);
            return header;
        }

        public static ColumnHeader textbox(NDataGridView3 view, string dataName, string showName) {
            return textbox(view, ColumnFormat.TEXT, dataName, showName, true, 0, int.MaxValue);
        }

        public static ColumnHeader textbox(NDataGridView3 view, ColumnFormat format, string dataName, string showName, int maxLength) {
            return textbox(view, format, dataName, showName, true, 0, maxLength);
        }

        public static ColumnHeader textbox(NDataGridView3 view, ColumnFormat format, string dataName, string showName, bool isNull, int minLength, int maxLength) {

            ColumnHeader header = new ColumnHeader(ColumnType.TextBox, format, dataName, showName);
            header.isNull = isNull;
            header.minLength = minLength;
            header.maxLength = maxLength;
            header.column = getColumn(view, header);
            return header;
        }



        private ColumnHeader(ColumnType type, ColumnFormat format, string dataName, string showName) {
            this.type = type;
            this.format = format;
            this.dataName = dataName;
            this.showName = showName;
        }




        public static DataGridViewColumn getColumn(NDataGridView3 view, ColumnHeader header) {

            bool readOnly = false;
            if (header.type == ColumnType.TextBox)
                readOnly = ( 0 == header.minLength && int.MaxValue == header.maxLength );
            else if (header.type == ColumnType.Checkbox)
                readOnly = true;
            else
                readOnly = !header.isClick;

            DataGridViewColumn column = null;
            if (header.type == ColumnType.Checkbox) {
                DGVCheckBoxColumn column2 = new DGVCheckBoxColumn(view);
                column2.DefaultCellStyle.Alignment = column2.HeaderCell.Style.Alignment = column2.DefaultCellStyle.Alignment = column2.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column2.FalseValue = header.falseVal;
                column2.TrueValue = header.trueVal;
                column2.SortMode = DataGridViewColumnSortMode.NotSortable;
                column2.HeaderCell = new NDataGridViewHeaderCell();
                column = column2;
            } else if (header.type == ColumnType.Combobox) {
                column = new DGVComboBoxColumn(view);
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.HeaderCell = new NDataGridViewHeaderCell();
                column.CellTemplate = new NDataGridViewComboxCell();
            } else if (header.type == ColumnType.Button) {
                DGVButtonColumn column2 = new DGVButtonColumn(view);
                column2.CellTemplate = new NDataGridViewButtonCell();
                column2.DefaultCellStyle.Alignment = column2.HeaderCell.Style.Alignment = column2.DefaultCellStyle.Alignment = column2.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column2.HeaderText = header.showName;
                column2.Text = header.showName;
                column2.SortMode = DataGridViewColumnSortMode.NotSortable;
                column2.UseColumnTextForButtonValue = true;
                column = column2;
            } else if (header.type == ColumnType.DateTime) {
                DGVDateTimeColumn column2 = new DGVDateTimeColumn(view);
                column2.SortMode = DataGridViewColumnSortMode.NotSortable;
                column2.valueFormat = header.valueFormat;
                column2.showFormat = header.showFormat;
                column2.showUpDown = !header.isSelectMode;
                column2.Width = 90;
                column2.HeaderCell = new NDataGridViewHeaderCell();
                column = column2;

            } else if (header.type == ColumnType.TextBox) {
                DGVTextBoxColumn column2 = new DGVTextBoxColumn(view);
                column2.SortMode = DataGridViewColumnSortMode.NotSortable;
                column2.MaxInputLength = header.maxLength;
                column2.HeaderCell = new NDataGridViewHeaderCell();
                column = column2;
            }
            //column.SortMode = DataGridViewColumnSortMode.Programmatic;
            column.Tag = "desc";
            column.DataPropertyName = column.Name = header.dataName;
            column.HeaderText = header.showName;
            column.ReadOnly = readOnly;

            return column;
        }

    }

    public class DataTeimeHeader {

    }
}


#region 多行表头
public class MutiHeader {
    public String headerText;
    public int startIndex = 0;
    public int endIndex = 0;
    public String sortName;
    public HorizontalAlignment align;

    public MutiHeader(String headerText, int start, int end) {
        this.headerText = headerText;
        this.startIndex = start;
        this.endIndex = end;
    }
    public MutiHeader(String headerText, String sortName, HorizontalAlignment align, int start, int end) {
        this.headerText = headerText;
        this.sortName = sortName;
        this.align = align;
        this.startIndex = start;
        this.endIndex = end;
    }
}

#endregion

#region 自定义表头


public class NDataGridViewHeaderCell : DataGridViewColumnHeaderCell {
    public SortOrder order = SortOrder.None;
    public bool isPaint = true;
    private bool sorting = false;
    // 是否允许排序
    public bool isAllowSort = true;



    public NDataGridViewHeaderCell() {

    }

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
        if (isPaint)
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
    }


    protected override void OnMouseClick(DataGridViewCellMouseEventArgs e) {
        if (isAllowSort && Cursor.Current != Cursors.SizeWE && !sorting) {
            NDataGridView3 dataView = (NDataGridView3)this.DataGridView;
            DataTable old = dataView.getBindDataTable();
            // 2023-05-17  1
            if (!old.Columns.Contains(dataView.Columns[e.ColumnIndex].DataPropertyName)) {
                sorting = false;
                return;
            }

            sorting = true;
            SortOrder o = order;
            if (o == SortOrder.None || o == SortOrder.Descending)
                o = SortOrder.Ascending;
            else
                o = SortOrder.Descending;
            drawImg(e.RowIndex, e.ColumnIndex, o);


            foreach (DataGridViewColumn item in this.DataGridView.Columns) {
                if (item.HeaderCell is NDataGridViewHeaderCell && item.HeaderCell != this) {
                    ( (NDataGridViewHeaderCell)item.HeaderCell ).order = SortOrder.None;
                }
            }

            if (null == old || 0 == old.Rows.Count) {
                sorting = false;
                return;
            }
            String ascOrDesc = "";
            if (o == SortOrder.Ascending) {
                ascOrDesc = "asc";
            } else {
                ascOrDesc = "desc";
            }

            DataRow[] selRows = old.Select(old.DefaultView.RowFilter, this.OwningColumn.Name + " " + ascOrDesc, DataViewRowState.CurrentRows);
            DataTable newT = old.Clone();
            foreach (DataRow item in selRows) {
                newT.ImportRow(item);
            }
            dataView.setDataSource(newT);


            this.order = o;
            this.DataGridView.InvalidateCell(this);
            dataView.targetRowStateChanged(0, DataGridViewElementStates.Displayed);
            sorting = false;
        }
    }


    private void drawImg(int r, int c, SortOrder order) {
        if (order == SortOrder.Ascending || order == SortOrder.Descending) {

            int height = this.ContentBounds.Height;
            if (height == 0)
                height = this.Size.Height;

            float mt = this.ContentBounds.Top + height / 2 - 5 + 1;
            float mb = this.ContentBounds.Top + height / 2 + 5;

            if (order == SortOrder.Descending) {
                float m = mt;
                mt = mb;
                mb = m;
            }
            Rectangle oo = this.DataGridView.GetCellDisplayRectangle(c, r, true);
            PointF point1 = new PointF(oo.X + oo.Width - 8F, mt);
            PointF point2 = new PointF(oo.X + oo.Width - 11.5F, mb);
            PointF point3 = new PointF(oo.X + oo.Width - 4.5F, mb);
            PointF[] pntArr = { point1, point2, point3 };
            this.DataGridView.CreateGraphics().FillPolygon(Brushes.Black, pntArr);
        }
    }
}
#endregion

#region 全选表头

public class DataGridViewCheckBoxHeaderCell : NDataGridViewHeaderCell {
    public Point chkboxLocal;
    public Size chkSize;
    private bool isChecked = false;
    Point cellLocal = new Point();
    CheckBoxState _cbState = CheckBoxState.UncheckedNormal;
    public NDataGridView3.CanChangedStateListener listener;

    public bool IsChecked {
        get {
            return isChecked;
        }

        set {
            isChecked = value;
            this.DataGridView.InvalidateCell(this);
        }
    }

    public DataGridViewCheckBoxHeaderCell(NDataGridView3.CanChangedStateListener listener) {
        this.listener = listener;
    }

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        Point p = new Point();

        Size s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
        if (this.OwningColumn.HeaderText.Length > 0)
            p.X = cellBounds.Location.X + ( s.Width / 2 );
        else
            p.X = cellBounds.Location.X + ( cellBounds.Width - s.Width ) / 2;
        p.Y = ( cellBounds.Height - s.Height ) / 2 + cellBounds.Location.Y;
        cellLocal = cellBounds.Location;
        chkboxLocal = p;
        chkSize = s;
        if (IsChecked)
            _cbState = CheckBoxState.CheckedNormal;
        else
            _cbState = CheckBoxState.UncheckedNormal;
        CheckBoxRenderer.DrawCheckBox(graphics, chkboxLocal, _cbState);
    }

    protected override void OnMouseClick(DataGridViewCellMouseEventArgs e) {
        if (clickCheckbox(e.Location) && this.DataGridView.EditMode == DataGridViewEditMode.EditOnEnter) {
            if (listener != null) {
                if (listener((NDataGridView3)this.DataGridView, e.ColumnIndex, !isChecked)) {
                    NDataGridView3 view = this.DataGridView as NDataGridView3;
                    //DataGridViewCheckBoxColumn column = this.OwningColumn as DataGridViewCheckBoxColumn;
                    //view.isShowLineNumber = false;
                    //DataGridViewAutoSizeColumnMode old = column.AutoSizeMode;
                    //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    //view.isPaint = false;
                    //view.SuspendLayout();
                    //foreach (DataGridViewRow row in this.DataGridView.Rows) {
                    //    ( (NDataGridView3)this.DataGridView ).getDataRow(row)[this.OwningColumn.DataPropertyName] = !isChecked ? column.TrueValue : column.FalseValue;
                    //}
                    //view.isPaint = true;
                    //view.isShowLineNumber = true;
                    //column.AutoSizeMode = old;
                    //view.ResumeLayout(false);
                    DataTable table = view.getBindDataTable().Copy();
                    DataGridViewCheckBoxColumn column = this.OwningColumn as DataGridViewCheckBoxColumn;
                    Object val = !IsChecked ? column.TrueValue : column.FalseValue;                    
                    DataColumn dc = table.Columns[column.DataPropertyName];
                    table.BeginLoadData();
                    foreach (DataRow row in table.Rows) {                        
                        row[dc] = val;                        
                    }
                    table.EndLoadData();
                    bool isOldState = IsChecked;
                    view.bindDataTable(table);
                    IsChecked = !isOldState;
                }
            }
            //this.DataGridView.Refresh();
            this.DataGridView.Capture = true;
            this.DataGridView.InvalidateCell(this);
        }
    }

    public bool clickCheckbox(Point clickPoint) {
        Point p = new Point(clickPoint.X + cellLocal.X, clickPoint.Y + cellLocal.Y);
        return ( p.X >= chkboxLocal.X && p.X <= chkboxLocal.X + chkSize.Width && p.Y >= chkboxLocal.Y && p.Y <= chkboxLocal.Y + chkSize.Height );
    }
}

#endregion

#region 文本框列
public class DGVTextBoxColumn : DataGridViewTextBoxColumn {
    private NDataGridView3 view = null;
    public bool isComboBoxMaster = false;
    public int minTargetLength = 1;

    public override bool Visible {
        get {
            return base.Visible;
        }
        set {
            bool oldVal = base.Visible;
            base.Visible = value;
            if (oldVal != value && null != view.columnHeaderVisibleChanged)
                view.columnHeaderVisibleChanged(view, new DataGridViewColumnEventArgs(this));

        }
    }
    public DGVTextBoxColumn() : base() {
        this.view = (NDataGridView3)this.DataGridView;
    }
    public DGVTextBoxColumn(NDataGridView3 view) : base() {
        this.view = view;
    }

    // 2025-01-14   2
    public bool isComboBoxModel() {
        return view.input_combobox_columns.ContainsKey(this.Name);
    }
    // 2025-01-14   2
    public DataTable getComboBoxDataTable() {
        if (isComboBoxModel()) {
            Panel p = view.input_combobox_columns[this.Name];
            if (p.Controls[0] is DGVComboBox) {
                DGVComboBox dgvCombobox = (DGVComboBox)p.Controls[0];
                return dgvCombobox.Tag as DataTable;
            }
        }
        return null;
    }
    // 2025-01-14   2
    public String getComboBoxValueMember() {
        if (isComboBoxModel()) {
            Panel p = view.input_combobox_columns[this.Name];
            if (p.Controls[0] is DGVComboBox) {
                DGVComboBox dgvCombobox = (DGVComboBox)p.Controls[0];
                return dgvCombobox.ValueMember;
            }
        }
        return null;
    }
    // 2025-01-14   2
    public String getComboBoxDisplayMember() {
        if (isComboBoxModel()) {
            Panel p = view.input_combobox_columns[this.Name];
            if (p.Controls[0] is DGVComboBox) {
                DGVComboBox dgvCombobox = (DGVComboBox)p.Controls[0];
                return dgvCombobox.DisplayMember;
            }
        }
        return null;
    }

}
#endregion

#region 复选框列
public class DGVCheckBoxColumn : DataGridViewCheckBoxColumn {
    private NDataGridView3 view = null;

    public override bool Visible {
        get {
            return base.Visible;
        }
        set {
            bool oldVal = base.Visible;
            base.Visible = value;
            if (oldVal != value && null != view.columnHeaderVisibleChanged)
                view.columnHeaderVisibleChanged(view, new DataGridViewColumnEventArgs(this));
        }
    }
    public DGVCheckBoxColumn(NDataGridView3 view) : base() {
        this.view = view;
    }
    public DGVCheckBoxColumn() {
        view = (NDataGridView3)this.DataGridView;
    }

}
#endregion

#region 时间列
public class DGVDateTimeColumn : DataGridViewColumn {
    private NDataGridView3 view = null;
    public String valueFormat = "yyyyMMdd";
    public String showFormat = "yyyy-MM-dd";
    public bool showUpDown = false;

    public override bool Visible {
        get {
            return base.Visible;
        }
        set {
            bool oldVal = base.Visible;
            base.Visible = value;
            if (oldVal != value && null != view.columnHeaderVisibleChanged)
                view.columnHeaderVisibleChanged(view, new DataGridViewColumnEventArgs(this));
        }
    }
    public DGVDateTimeColumn() : base(new DataGridViewDateTimeCell()) {
        this.view = (NDataGridView3)this.DataGridView;
    }
    public DGVDateTimeColumn(NDataGridView3 view) : base(new DataGridViewDateTimeCell()) {
        this.view = view;
    }

}

public class DataGridViewDateTimeCell : DataGridViewTextBoxCell {
    private CalendarEditingControl ctl;
    private bool isUnInstall = true;
    public DataGridViewDateTimeCell() : base() {
        this.Style.Format = "d";
    }

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
        //if (isUnInstall)
        //{
        // Set the value of the editing control to the current cell value.
        base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
        ctl = DataGridView.EditingControl as CalendarEditingControl;

        DGVDateTimeColumn column = (DGVDateTimeColumn)OwningColumn;
        ctl.Format = DateTimePickerFormat.Custom;
        ctl.CustomFormat = column.showFormat;
        ctl.ShowUpDown = column.showUpDown;
        if (Tools.isNullOrEmpty(this.Value)) {
            //ctl.Text = DateTime.Now.ToString(column.valueFormat);
            ctl.Value = DateTime.Now;
        } else {
            Object nVal = Value;
            if (Value.ToString().Length == 8 && Value.ToString().IndexOf("-") == -1)
                nVal = Value.ToString().Insert(4, "-").Insert(7, "-");
            if (Value.ToString().Length > 10 && Value.ToString().IndexOf(":") == -1)
                nVal = Value.ToString().Insert(10, " ").Insert(13, ":").Insert(16, ":");
            DateTime dt = Convert.ToDateTime(nVal);
            ctl.Value = dt;
            //ctl.Text = dt.ToString(column.showFormat);
        }
        isUnInstall = false;
        //}
    }

    public override void DetachEditingControl() {
        this.Value = ctl.Value.ToString(((DGVDateTimeColumn)OwningColumn).valueFormat);
        //ctl.Dispose();
        isUnInstall = true;
        base.DetachEditingControl();
    }


    public override Type EditType {
        get {
            return typeof(CalendarEditingControl);
        }
    }

    public override Type ValueType {
        get {
            return typeof(String);
        }
    }



    public override object DefaultNewRowValue {
        get {
            if (null == this.OwningColumn)
                return null; ;
            return DateTime.Now.ToString(((DGVDateTimeColumn)this.OwningColumn).showFormat);
        }
    }
}

class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl {
    DataGridView dataGridView;
    private bool valueChanged = false;
    int rowIndex;

    public CalendarEditingControl() {
        this.Format = DateTimePickerFormat.Long;
    }

    // Implements the IDataGridViewEditingControl.EditingControlFormattedValue
    // property.
    public object EditingControlFormattedValue {
        get {
            if (dataGridView.CurrentCell.OwningColumn is DGVDateTimeColumn) {
                return this.Value.ToString();

            }
            return this.Value;
        }
        set {
            if (value is String) {

                this.Value = DateTime.Parse((String)value);

            } else if (value is DateTime)
                this.Value = (DateTime)value;
        }
    }

    // Implements the
    // IDataGridViewEditingControl.GetEditingControlFormattedValue method.
    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) {
        return EditingControlFormattedValue;
    }

    // Implements the
    // IDataGridViewEditingControl.ApplyCellStyleToEditingControl method.
    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) {
        this.Font = dataGridViewCellStyle.Font;
        // 2024-05-21   1
        try {
            this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
        } catch (Exception eeee) {
            String dsddd = "";
        }
        try {
            this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
        } catch (Exception eeee) {
            String dsddd = "";
        }
    }

    // Implements the IDataGridViewEditingControl.EditingControlRowIndex
    // property.
    public int EditingControlRowIndex {
        get {
            return rowIndex;
        }
        set {
            rowIndex = value;
        }
    }

    // Implements the IDataGridViewEditingControl.EditingControlWantsInputKey
    // method.
    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey) {
        // Let the DateTimePicker handle the keys listed.
        switch (key & Keys.KeyCode) {
            case Keys.Left:
            case Keys.Up:
            case Keys.Down:
            case Keys.Right:
            case Keys.Home:
            case Keys.End:
            case Keys.PageDown:
            case Keys.PageUp:
                return true;
            default:
                return !dataGridViewWantsInputKey;
        }
    }

    // Implements the IDataGridViewEditingControl.PrepareEditingControlForEdit
    // method.
    public void PrepareEditingControlForEdit(bool selectAll) {
        // No preparation needs to be done.
    }

    // Implements the IDataGridViewEditingControl
    // .RepositionEditingControlOnValueChange property.
    public bool RepositionEditingControlOnValueChange {
        get {
            return false;
        }
    }

    // Implements the IDataGridViewEditingControl
    // .EditingControlDataGridView property.
    public DataGridView EditingControlDataGridView {
        get {
            return dataGridView;
        }
        set {
            dataGridView = value;
        }
    }

    // Implements the IDataGridViewEditingControl
    // .EditingControlValueChanged property.
    public bool EditingControlValueChanged {
        get {
            return valueChanged;
        }
        set {
            valueChanged = value;
        }
    }

    // Implements the IDataGridViewEditingControl
    // .EditingPanelCursor property.
    public Cursor EditingPanelCursor {
        get {
            return base.Cursor;
        }
    }

    protected override void OnValueChanged(EventArgs eventargs) {
        // Notify the DataGridView that the contents of the cell
        // have changed.
        valueChanged = true;
        this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
        base.OnValueChanged(eventargs);
    }
}

#endregion

#region 下拉列

public class DGVComboBoxColumn : DataGridViewComboBoxColumn {

    private NDataGridView3 view = null;

    private String[] showColumns;
    public DataTable bindTable;
    private NDataGridView3.comboxValueChangedListener listener;


    public override bool Visible {
        get {
            return base.Visible;
        }
        set {
            bool oldVal = base.Visible;
            base.Visible = value;
            if (oldVal != value && null != view.columnHeaderVisibleChanged)
                view.columnHeaderVisibleChanged(view, new DataGridViewColumnEventArgs(this));
        }
    }

    public DGVComboBoxColumn(NDataGridView3 view) : this(view, null) {

    }

    public DGVComboBoxColumn(NDataGridView3 view, String[] show) {
        this.view = view;
        this.ShowColumns = show;
    }

    public string[] ShowColumns {
        get {
            return showColumns;
        }

        set {
            this.showColumns = value;
        }
    }

    public NDataGridView3.comboxValueChangedListener Listener {
        get {
            return listener;
        }

        set {
            this.listener = value;
        }
    }
}

#endregion

#region 下拉单元格

public class NDataGridViewComboxCell : DataGridViewComboBoxCell {
    private bool isInstall = false;
    private String selValue = "";
    private int defualPadding = 2;
    private int backWidth = 0;
    public bool isBindComplete = true;
    private Dictionary<String, float> columnMaxWidth = new Dictionary<string, float>();



    public NDataGridViewComboxCell() {
        this.FlatStyle = FlatStyle.Flat;
        // 这一行表示是否在未编辑的情况下，显示下拉按钮
        this.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
    }

    public override void DetachEditingControl() {

        ComboBox comboBox = this.DataGridView.EditingControl as ComboBox;
        comboBox.SelectedValueChanged -= ComboBox_SelectedValueChanged;
        String cccc = comboBox.SelectedValue + "";
        base.DetachEditingControl();

        isInstall = false;
    }


    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
        if (isInstall)
            return;
        base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

        ComboBox comboBox = this.DataGridView.EditingControl as ComboBox;
        DataRow viewRow = ( (DataTable)this.DataGridView.DataSource ).Rows[rowIndex];
        comboBox.Tag = new int[] { rowIndex, this.ColumnIndex };
        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        comboBox.DrawItem += ComboBox_DrawItem;
        comboBox.SelectedValueChanged += ComboBox_SelectedValueChanged;
        DGVComboBoxColumn column = (DGVComboBoxColumn)this.OwningColumn;
        if (backWidth != comboBox.Width && null != column.ShowColumns) {
            Object ds = this.DataSource;
            DataTable table = (DataTable)ds;
            if (null == table)
                return;
            foreach (DataRow row in table.Rows) {
                float lineMaxWidth = 0.0F;
                foreach (String fieldName in column.ShowColumns) {
                    string item = Convert.ToString(row[fieldName]);
                    SizeF sizeF = comboBox.CreateGraphics().MeasureString(item, comboBox.Font);//返回显示项字符串的大小
                    lineMaxWidth += sizeF.Width;
                    if (columnMaxWidth.ContainsKey(fieldName)) {
                        if (columnMaxWidth[fieldName] < sizeF.Width)
                            columnMaxWidth[fieldName] = sizeF.Width;
                    } else {
                        columnMaxWidth.Add(fieldName, sizeF.Width);
                    }
                }
            }
            backWidth = comboBox.Width;
            float maxDropDownWidth = 0.0F;
            foreach (String key in columnMaxWidth.Keys) {
                maxDropDownWidth = maxDropDownWidth + columnMaxWidth[key];
            }
            this.DropDownWidth = (int)Math.Ceiling(maxDropDownWidth) + defualPadding + column.ShowColumns.Length * 2 * defualPadding + SystemInformation.VerticalScrollBarWidth;
        }
        selValue = this.Value + "";
        comboBox.SelectedValue = selValue;
        ThreadPool.QueueUserWorkItem((Object obj) => {
            if (comboBox.IsDisposed)
                return;
            if (null == initialFormattedValue || DBNull.Value.Equals(initialFormattedValue) || String.IsNullOrEmpty(initialFormattedValue.ToString())) {
                if (comboBox.InvokeRequired) {
                    comboBox.Invoke(new Action(() => {
                        if (null == this.DataSource || ( (DataTable)this.DataSource ).Rows.Count == 0)
                            comboBox.SelectedIndex = -1;
                        else {
                            comboBox.SelectedIndex = -1;
                            try {
                                comboBox.SelectedValue = ( (DataTable)this.DataSource ).Rows[0][this.ValueMember];
                            } catch { }
                        }
                    }));
                } else {
                    if (null == this.DataSource || ( (DataTable)this.DataSource ).Rows.Count == 0)
                        comboBox.SelectedIndex = -1;
                    else {
                        comboBox.SelectedIndex = -1;
                        try {
                            comboBox.SelectedValue = ( (DataTable)this.DataSource ).Rows[0][this.ValueMember];
                        } catch { }
                    }
                }
            }
            if (comboBox.IsDisposed)
                return;
            if (null != this.DataGridView && null != this.DataGridView.CurrentCell) {
                if (rowIndex == this.DataGridView.CurrentCell.RowIndex && ColumnIndex == this.DataGridView.CurrentCell.ColumnIndex) {
                    // 2023-09-22   2
                    // 因为拓展合同，关联合同的列。会弹出2次，但是这里不能这样判断，因为如果不DroppedDown，则不会触发绘制，不触发绘制，则不会去计算columnMaxWidth
                    // 所以就必须要点2次。第二次手工点，底层会触发DroppedDown，才会触发绘制。
                    //if (columnMaxWidth.Count != 0) {
                    // this.DataGridView.BeginInvoke(new MethodInvoker(()=> {
                    // 2024-09-12   1
                    if (!comboBox.InvokeRequired) {
                        comboBox.DroppedDown = true;
                    }

                    //  }));
                    //} 
                }
            }
        });
        isInstall = true;

    }



    private void ComboBox_SelectedValueChanged(object sender, EventArgs e) {
        if (!isBindComplete)
            return;
        ComboBox comboBox = (ComboBox)sender;
        DGVComboBoxColumn column = (DGVComboBoxColumn)this.OwningColumn;
        DataRow row = comboBox.SelectedItem is DataRowView ? ( (DataRowView)comboBox.SelectedItem ).Row : comboBox.SelectedItem as DataRow;

        if (null != column.Listener && null != row) {
            selValue = row[column.ValueMember] + "";
            int[] rc = (int[])comboBox.Tag;
            if (null != rc) {
                column.Listener((NDataGridView3)( this.DataGridView ), rc[0], rc[1], comboBox, row);
            }
        }
    }

    private void ComboBox_DrawItem(object sender, DrawItemEventArgs e) {
        if (e.Index == -1 || null == this.DataGridView || null == this.DataGridView.EditingControl || e.Index >= Items.Count)
            return;

        ComboBox comboBox = this.DataGridView.EditingControl as ComboBox;
        if (String.IsNullOrEmpty(comboBox.DisplayMember))
            return;

        // 2023-09-22   1
        // 不知道为什么，感觉用给单元格单独设置DataSource的时候，会把所有点过的下拉单元格全部重新绘制一次。
        if (this.RowIndex != DataGridView.CurrentCell.RowIndex)
            return;
        DGVComboBoxColumn column = (DGVComboBoxColumn)this.OwningColumn;
        Object val = this.Items[e.Index];
        DataRow valRow = null;
        String text = "";
        if (val is DataRow)
            valRow = ( (DataRow)val );
        else if (val is DataRowView)
            valRow = ( (DataRowView)val ).Row;
        else
            text = val.ToString();

        if (valRow.RowState == DataRowState.Deleted || valRow.RowState == DataRowState.Detached)
            return;
        if (null != valRow) {
            text = valRow[this.DisplayMember] + "";
        }

        // 计算字符串尺寸（以像素为单位）
        SizeF ss = e.Graphics.MeasureString(text, e.Font);
        // 水平居中
        float left = (float)Math.Ceiling(( e.Bounds.Width - ss.Width ) / 2 - ( this.DisplayStyle == DataGridViewComboBoxDisplayStyle.Nothing ? 0 : 10 ));
        if (left < 0)
            left = 1f;
        // 输出
        e.DrawBackground();



        if (null != selValue && !String.IsNullOrEmpty(selValue) && selValue.Equals(valRow[this.ValueMember])) {
            SolidBrush brush = new SolidBrush(Color.FromArgb(51, 153, 255));
            e.Graphics.FillRectangle(brush, e.Bounds);
        }
        if (e.State.ToString().Contains(DrawItemState.ComboBoxEdit.ToString())) {
            e.Graphics.DrawString(text, e.Font, new SolidBrush(e.ForeColor), e.Bounds);
            e.DrawFocusRectangle();
        } else {
            if (null == column.ShowColumns) {
                Rectangle rect = new Rectangle((int)left, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.DrawString(text, e.Font, new SolidBrush(e.ForeColor), rect);
            } else {
                int lastRight = 1;
                DataRow row = ( (DataRowView)Items[e.Index] ).Row;
                Rectangle rect = e.Bounds;
                // 2023-09-22   2
                if (!columnMaxWidth.ContainsKey(column.ShowColumns[0])) {
                    DataTable table = (DataTable)this.DataSource;
                    foreach (DataRow rrr in table.Rows) {
                        float lineMaxWidth = 0.0F;
                        foreach (String fieldName in column.ShowColumns) {
                            string item = Convert.ToString(rrr[fieldName]);
                            SizeF sizeF = comboBox.CreateGraphics().MeasureString(item, comboBox.Font);//返回显示项字符串的大小
                            lineMaxWidth += sizeF.Width;
                            if (columnMaxWidth.ContainsKey(fieldName)) {
                                if (columnMaxWidth[fieldName] < sizeF.Width)
                                    columnMaxWidth[fieldName] = sizeF.Width;
                            } else {
                                columnMaxWidth.Add(fieldName, sizeF.Width);
                            }
                        }
                    }
                    backWidth = comboBox.Width;
                    float maxDropDownWidth = 0.0F;
                    foreach (String key in columnMaxWidth.Keys) {
                        maxDropDownWidth = maxDropDownWidth + columnMaxWidth[key];
                    }
                    this.DropDownWidth = (int)Math.Ceiling(maxDropDownWidth) + defualPadding + column.ShowColumns.Length * 2 * defualPadding + SystemInformation.VerticalScrollBarWidth;
                    comboBox.DroppedDown = true;
                }

                using (Pen linePen = new Pen(SystemColors.GrayText)) {
                    //循环各列
                    for (int i = 0; i < column.ShowColumns.Length; i++) {
                        String columnName = column.ShowColumns[i];
                        string item = row[columnName] + "";
                        rect.X = lastRight + defualPadding;
                        rect.Width = (int)columnMaxWidth[columnName] + defualPadding;
                        lastRight = rect.Right;
                        e.Graphics.DrawString(item, e.Font, new SolidBrush(e.ForeColor), rect);

                        if (i < column.ShowColumns.Length - 1) {
                            e.Graphics.DrawLine(linePen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                        }
                    }
                }
            }
        }
    }

}

#endregion

#region 按钮单元格

public class DGVButtonColumn : DataGridViewButtonColumn {
    private NDataGridView3 view = null;

    public override bool Visible {
        get {
            return base.Visible;
        }
        set {
            bool oldVal = base.Visible;
            base.Visible = value;
            if (oldVal != value && null != view.columnHeaderVisibleChanged)
                view.columnHeaderVisibleChanged(view, new DataGridViewColumnEventArgs(this));
        }
    }
    public DGVButtonColumn(NDataGridView3 view) : base() {
        this.view = view;
    }

}

public class NDataGridViewButtonCell : DataGridViewButtonCell {


    public bool Enabled = true;

    // Override the Clone method so that the Enabled property is copied.
    public override object Clone() {
        NDataGridViewButtonCell cell = (NDataGridViewButtonCell)base.Clone();
        cell.Enabled = this.Enabled;
        return cell;
    }


    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
        // The button cell is disabled, so paint the border,
        // background, and disabled button for the cell.
        if (!this.Enabled) {
            // Draw the cell background, if specified.
            if (( paintParts & DataGridViewPaintParts.Background ) == DataGridViewPaintParts.Background) {
                SolidBrush cellBackground = new SolidBrush(cellStyle.BackColor);
                graphics.FillRectangle(cellBackground, cellBounds);
                cellBackground.Dispose();
            }

            // Draw the cell borders, if specified.
            if (( paintParts & DataGridViewPaintParts.Border ) == DataGridViewPaintParts.Border) {
                PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
            }

            // Calculate the area in which to draw the button.
            Rectangle buttonArea = cellBounds;
            Rectangle buttonAdjustment = this.BorderWidths(advancedBorderStyle);
            buttonArea.X += buttonAdjustment.X;
            buttonArea.Y += buttonAdjustment.Y;
            buttonArea.Height -= buttonAdjustment.Height;
            buttonArea.Width -= buttonAdjustment.Width;

            // Draw the disabled button.
            ButtonRenderer.DrawButton(graphics, buttonArea, PushButtonState.Disabled);

            // Draw the disabled button text.
            if (this.FormattedValue is String) {
                TextRenderer.DrawText(graphics, (string)this.FormattedValue, this.DataGridView.Font, buttonArea, SystemColors.GrayText);
            }
        } else {
            // The button cell is enabled, so let the base class
            // handle the painting.
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }
    }

}
#endregion


class DGVComboBox : ComboBox {
    public bool isInputing = false;
    private String selValue = "";
    private String attendColumnName = "";
    private int attendRowIndex;
    private int attendColumnIndex;

    private DataTable bindTable = null;
    private NDataGridView3 view = null;
    public NDataGridView3.comboxValueChangedListener listener = null;
    public String[] filterColumns = null;
    public String[] showColumns = null;
    private NDictionary<String, int> columnWidths = new NDictionary<string, int>();
    private NDictionary<String, Object> tags = new NDictionary<string, object>();



    public DGVComboBox(NDataGridView3 view, NDataGridView3.comboxValueChangedListener listener) {
        this.view = view;
        this.listener = listener;
        this.DrawMode = DrawMode.OwnerDrawFixed;
        this.SelectedIndexChanged += NComboBox2_SelectedIndexChanged;
    }

    public void targetPreviewKeyDown(KeyEventArgs e) {
        this.OnKeyDown(e);
    }

    public void targetMouseClick(MouseEventArgs e) {
        base.OnMouseClick(e);
    }



    private void NComboBox2_SelectedIndexChanged(object sender, EventArgs e) {
        ComboBox comboBox = (ComboBox)sender;
        if (null == comboBox.SelectedItem || -1 == comboBox.SelectedIndex)
            return;
        isInputing = true;
        DataRow row = comboBox.SelectedItem is DataRow ? comboBox.SelectedItem as DataRow : ( comboBox.SelectedItem as DataRowView ).Row;
        DataRow viewBindRow = view.getDataRow(attendRowIndex);
        if (null != viewBindRow && null != row)
            viewBindRow[AttendColumnName] = row[ValueMember];

        selValue = row[ValueMember] + "";

        if (null != listener) {
            listener(view, attendRowIndex, attendColumnIndex, this, row);
        }
        comboBox.Text = row[DisplayMember] + "";
        comboBox.DroppedDown = false;
        this.BeginInvoke(new MethodInvoker(() => {
            comboBox.Text = row[DisplayMember] + "";
            comboBox.SelectAll();
            isInputing = false;
        }));
    }

    public void targetOnTextUpdate(String text) {
        textUpdate(text);
    }


    public void refreshBindData(String text) {

        List<DataRow> list = new List<DataRow>();

        DataRow[] rows = null;
        if (!String.IsNullOrEmpty(text)) {
            StringBuilder filterString = new StringBuilder();
            foreach (String filter in filterColumns) {
                filterString.Append(filter + " like '%" + text + "%' or ");
            }
            //String filter = this.ValueMember + " like '%" + text + "%' or " + this.DisplayMember + " like '%" + text + "%'";
            filterString.Length -= 3;
            rows = BindTable.Select(filterString.ToString());
        } else {
            rows = BindTable.Select();
        }
        if (null == rows || 0 == rows.Length) {
            return;
        }
        int first = 0;
        int rdm = new Random().Next(0, rows.Length - 1);
        int end = rows.Length - 1;
        // 如果重新刷新的数据和当前数据一直，则不重新绑定
        if (rows.Length == this.Items.Count) {
            DataRow item_first = (DataRow)this.Items[first];
            DataRow item_rdm = (DataRow)this.Items[rdm];
            DataRow item_end = (DataRow)this.Items[end];
            if (rows[first][ValueMember] == item_first[ValueMember]
                && rows[rdm][ValueMember] == item_rdm[ValueMember]
                && rows[end][ValueMember] == item_end[ValueMember])
                return;
        }
        calWidth(rows);
        list.AddRange(rows);
        this.Items.Clear();
        this.Items.AddRange(list.ToArray());
    }

    private void calWidth(DataRow[] rows) {

        Dictionary<String, int> columnNameMaxW = new Dictionary<string, int>();
        Dictionary<String, String> columnNameMaxT = new Dictionary<string, String>();

        foreach (DataColumn column in BindTable.Columns) {
            foreach (DataRow item in rows) {
                if (!columnNameMaxW.ContainsKey(column.ColumnName)) {
                    columnNameMaxW.Add(column.ColumnName, 0);
                    columnNameMaxT.Add(column.ColumnName, item[column.ColumnName] + "");
                }
                int w = columnNameMaxW[column.ColumnName];
                // 2023-08-31   1
                int valByteCount = Encoding.Default.GetBytes(item[column.ColumnName] + "").Length;
                if (w < valByteCount) {
                    columnNameMaxW.Remove(column.ColumnName);
                    columnNameMaxT.Remove(column.ColumnName);
                    columnNameMaxW.Add(column.ColumnName, valByteCount);
                    columnNameMaxT.Add(column.ColumnName, item[column.ColumnName] + "");
                }
            }
        }
        Graphics g = this.CreateGraphics();
        columnWidths.Clear();
        foreach (String key in columnNameMaxW.Keys) {
            SizeF sizeF = g.MeasureString(columnNameMaxT[key], this.Font);
            columnWidths.Add(key, (int)( Math.Ceiling(sizeF.Width) ));
        }
        g.Dispose();


        //Graphics g = this.CreateGraphics();
        //columnWidths.Clear();
        //foreach (DataColumn column in bindTable.Columns) {
        //    columnWidths.add(column.ColumnName, 0);
        //    foreach (DataRow item in rows) {
        //        int w = columnWidths[column.ColumnName];
        //        int vw = (int)Math.Ceiling(g.MeasureString(item[column] + "", this.Font).Width) + 3;
        //        if (w < vw) {
        //            columnWidths[column.ColumnName] = vw;
        //        }
        //    }
        //}
        //g.Dispose();

        float maxDropDownWidth = 0.0F;
        foreach (String key in columnWidths.Keys) {
            maxDropDownWidth = maxDropDownWidth + columnWidths[key];
        }
        this.DropDownWidth = (int)Math.Ceiling(maxDropDownWidth) + SystemInformation.VerticalScrollBarWidth + SystemInformation.VerticalScrollBarWidth;

    }

    public Object getTag(String key) {
        return tags.get(key, null);
    }

    public DGVComboBox setTag(String key, Object tag) {
        tags.Add(key, tag);
        return this;
    }
    public void bindData(DataTable table) {
        List<String> showColumns = new List<string>();
        if (table.Columns.Count > 0)
            showColumns.Add(table.Columns[0].ColumnName);
        if (table.Columns.Count > 1)
            showColumns.Add(table.Columns[1].ColumnName);
        bindData(showColumns.ToArray(), table);
    }
    public void bindData(String[] showColumns, DataTable table) {
        List<String> filterColumns = new List<string>();
        if (table.Columns.Count > 0)
            filterColumns.Add(table.Columns[0].ColumnName);
        if (table.Columns.Count > 1)
            filterColumns.Add(table.Columns[1].ColumnName);
        bindData(showColumns, filterColumns.ToArray(), table);
    }
    public void bindData(String[] showColumns, String[] filterColumns, DataTable table) {
        this.showColumns = showColumns;
        this.filterColumns = filterColumns;
        this.bindTable = table;
        DataRow[] rows = table.Select();
        calWidth(rows);
        this.Items.AddRange(rows);
        if (null == filterColumns || 0 == filterColumns.Length)
            filterColumns = new String[] { this.ValueMember, this.DisplayMember };
    }

    private void textUpdate(String text) {
        if (isInputing || -1 == AttendColumnIndex)
            return;

        DGVTextBoxColumn column = view.Columns[AttendColumnIndex] as DGVTextBoxColumn;
        if (null != column) {
            if (this.Text.Trim().Length < column.minTargetLength)
                return;
        }
        isInputing = true;
        this.Cursor = Cursors.Default;
        List<DataRow> list = new List<DataRow>();

        DataRow[] rows = null;
        if (!String.IsNullOrEmpty(text)) {
            StringBuilder filterString = new StringBuilder();
            StringBuilder filterString2 = new StringBuilder();
            foreach (String filter in filterColumns) {
                filterString.Append("" + filter + "='" + text + "' or ");
                filterString2.Append(filter + " like '%" + text + "%' or ");
            }
            filterString.Length -= 3;
            filterString2.Length -= 3;
            DataRow[] _rows = BindTable.Select(filterString.ToString());
            DataRow[] _rows2 = BindTable.Select(filterString2.ToString());

            List<DataRow> filter_all = new List<DataRow>();
            List<DataRow> filter_all2 = new List<DataRow>();
            filter_all.AddRange(_rows);
            filter_all2.AddRange(_rows2);
            if (_rows2.Length > 0) {
                foreach (DataRow item1 in _rows) {
                    DataRow remove = null;
                    foreach (DataRow item2 in filter_all2) {
                        if (( item1[ValueMember] + "" ).Trim().Equals(( item2[ValueMember] + "" ).Trim())) {
                            remove = item2;
                            break;
                        }
                    }
                    if (null != remove) {
                        filter_all2.Remove(remove);
                    }
                }
            }
            filter_all.AddRange(filter_all2.ToArray());
            rows = filter_all.ToArray();
            _rows = _rows2 = null;
        } else {
            rows = BindTable.Select();
        }

        if (null == rows || 0 == rows.Length) {
            this.DroppedDown = false;
            isInputing = false;
            return;
        }

        if (text.Length == 0) {
            DataRow viewBindRow = view.getDataRow(attendRowIndex);
            viewBindRow[AttendColumnName] = null;
            this.SelectedIndex = -1;
            isInputing = false;
            return;
        }

        list.AddRange(rows);
        this.SelectedIndexChanged -= NComboBox2_SelectedIndexChanged;
        this.Items.Clear();
        calWidth(rows);
        this.Items.AddRange(list.ToArray());
        if (this.Text.Length >= 0)
            this.SelectionStart = this.Text.Length;
        Cursor viewCursor = view.Cursor;
        this.Cursor = Cursors.Default;
        view.Cursor = Cursors.Default;
        this.Parent.Cursor = Cursors.Default;
        this.BeginInvoke(new MethodInvoker(() => {
            this.SelectedIndexChanged += NComboBox2_SelectedIndexChanged;

            // 这里有一个遗留bug，当this.DroppedDown = true之后，会莫名触发选中index=0的值，然后就会将combobox.Text修改为System.Data.DataRow（但不会触发IndexChanged事件，虽然上一句代码绑定了IndexChanged事件）。
            // 如果当前输入的是第一个字符，并且字符是s的话。那么这个System.Data.DataRow将修改不了，如果把选中清空在输入s，还是会重复。
            // 如果把System.Data.DataRow全部选中，然后直接输入s才能正常显示下拉联动
            if (this.Items.Count > 0 && !this.DroppedDown) {
                // 2024-05-08   1 
                // 2024-05-08 发现，只要执行了下面这一句this.DroppedDown = true;，就会自动选中index=0的项，并且不会触发上面的IndexChanged事件。
                // 目前只有让输入s的不显示备选列表，但是如果要输入system部门。就会和System.Data.DataRow重复，又会显示成System.Data.DataRow，目前无解
                // 并且输入sy之后会全选System.Data.DataRow，然后在输入st之后，就会清空内容。如此循环
                if (!"s".Equals(text, StringComparison.CurrentCultureIgnoreCase))
                    this.DroppedDown = true;
            }
            // 如果这里强行将combobox.Text的值改为text的话。那么System.Data.DataRow也会出现，但是会发现被改变（改变肉眼可见，大约在500ms左右）
            //this.Text = text;
            //this.SelectionStart = this.Text.Length;
        }));
        isInputing = false;
        this.Cursor = Cursors.Default;
        view.Cursor = viewCursor;
        this.Parent.Cursor = Cursors.Default;

    }

    protected override void OnTextUpdate(EventArgs e) {
        String text = this.Text.Replace("'", "");
        if (!isInputing) {
            textUpdate(text);
        }


        if (AttendRowIndex > view.RowCount)
            return;
        DataRow viewBindRow = view.getDataRow(AttendRowIndex);
        DataGridViewColumn column = view.Columns[AttendColumnIndex];

        if (null != viewBindRow && column is DGVTextBoxColumn && !( (DGVTextBoxColumn)column ).isComboBoxMaster) {
            viewBindRow[AttendColumnName] = this.Text;
        } else {
            viewBindRow[AttendColumnName] = null;
        }
    }

    protected override void OnDrawItem(DrawItemEventArgs e) {

        if (e.Index == -1 || e.Index >= this.Items.Count)
            return;
        DataRow val = ( (DataRow)this.Items[e.Index] );


        e.DrawBackground();

        int lastRight = 1;
        Rectangle rect = e.Bounds;
        Rectangle backRect = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);

        if (null != selValue && !String.IsNullOrEmpty(selValue) && selValue.Equals(val[this.ValueMember])) {
            SolidBrush brush = new SolidBrush(Color.FromArgb(51, 153, 255));
            e.Graphics.FillRectangle(brush, e.Bounds);
        }
        //循环各列
        SolidBrush fontColor = new SolidBrush(e.ForeColor);
        using (Pen linePen = new Pen(SystemColors.GrayText)) {
            for (int i = 0; i < showColumns.Length; i++) {
                String columnName = showColumns[i];
                string item = val[columnName] + "";
                rect.X = lastRight + 2;
                rect.Width = (int)columnWidths[columnName] + 2;
                lastRight = rect.Right;
                SizeF si = e.Graphics.MeasureString(item, e.Font);
                e.Graphics.DrawString(item, e.Font, fontColor, rect.X, rect.Y + ( Math.Abs(rect.Height - si.Height) / 2F ));

                if (i < showColumns.Length - 1) {
                    e.Graphics.DrawLine(linePen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                }
            }
        }

        fontColor.Dispose();

    }



    public string AttendColumnName {
        get {
            return attendColumnName;
        }
        set {
            this.attendColumnName = value;
        }
    }

    public int AttendRowIndex {
        get {
            return attendRowIndex;
        }
        set {
            this.attendRowIndex = value;
        }
    }

    public int AttendColumnIndex {
        get {
            return attendColumnIndex;
        }
        set {
            this.attendColumnIndex = value;
        }
    }

    public DataTable BindTable { get => bindTable; }
}