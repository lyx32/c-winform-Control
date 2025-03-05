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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ComboBox;

/// <summary>
/// 2022-11-22  1.调整item的高度
/// 2022-11-24  1.修复点击下拉不能输入的问题
/// 2022-11-29  1.取消上面的修改（ 上面的修改，不能滚轴滚动）
/// 2022-12-29  1.增加setOnEnter事件,并增加绑定按键
///             2.增加getSelectValueOrInputText方法
/// 2022-12-30  1.增加clearTextDataBind清空当前输入
///             2.增加绑定数据小于20。自动切换为选择模式（不能输入）
/// 2022-01-04  1.修复输入内容时，完全匹配的内容不再最上面的问题
/// 2023-01-10  1.添加requestFocus方法
/// 2023-01-12  1.修复InputMode=false时，通过各种切换，还可以进行输入的bug
///             2.增加isAutoSelect参数，当有输入或选择时（还没选），切换焦点，是否自动选中第一个
///             3.修复切换ReadOnly时，下拉按钮部分不变色的bug
/// 2023-01-17  1.取消Keys.Down事件，修复下拉第一个不是精准匹配的问题
/// 2023-03-09  1.修复展开下拉框不高亮选中项的bug（还有一个不会自动滚动到选中项的bug）
/// 2023-03-16  1.增加Count属性
/// 2023-03-30  1.修改isExecEvent=false时，也会触发选中事件
/// 2023-04-12  1.增加将焦点切换到下一个时的一些选中
/// 2023-04-18  1.修改clearInputAndChooseData();
/// 2023-04-19  1.取消一些不用代码
/// 2023-04-21  1.修复清理了数据，但是selectDataRow还在的情况
/// 2023-04-27  1.处理一些莫名其妙的并发
/// 2023-05-05  1.修复一些莫名其妙的情况下，不绘制下拉箭头
/// 2023-05-18  1.修复单击一直会全选的问题
/// 2023-05-24  1.应用物质程序的部门要求，增加上下箭头键，可以上下切换
/// 2023-05-29  1.增加上下方向键从当前筛选结果中切换
/// 2023-05-30  1.增加鼠标滚轴上下切换功能（最早以前就想处理了，没找到合适的方式）
///             2.增加失去焦点时，下表自动切换到第一列（实现不了从右往左选中，只能这样）
/// 2023-05-31  1.因为实现2023-05-30 1 所以取消了setSelectionValue，现在重新保留这个
/// 2023-06-15  1.修复仪setSelectionValue可能会多次触发自定义事件的bug
/// 2023-06-26  1.修复选中之后，再次展开选择区域，没有跳转到选中项的问题
/// 2023-07-04  1.修复value没有模糊查找
/// 2023-07-25  1.调整选中后，再次输入不清空选中
/// 2023-07-26  1.修复没有数据点下拉报错
/// 2023-07-27  1.修复只有一列数据报错的bug
/// 2023-07-28  1.修复显示列没有内容报错的bug
///             2.修复第一列和后面列高度不一致的问题
/// 2023-08-02  1.修复没有设置筛选字段时的默认筛选字段
/// 2023-08-07  1.绕过一个系统bug。没有行号不知道哪里的代码报错（ComboBox.DroppedDown SendMessage(NativeMethods.CB_SHOWDROPDOWN, value ? -1 : 0, 0);引发。）
/// 2023-08-08  1.增加是否启动上下键切换数据
/// 2023-08-11  1.修复2023-08-07 1的bug
/// 2023-08-31  1.优化一部分筛选效率
/// 2023-09-13  1.修复之前取消的try，这个不能取消会报错
///             2.优化筛选效率，目前不是很完美
/// 2023-09-15  1.增加筛选排序
/// 2023-09-21  1.增加是否精确筛选（=有则不显示like，=没有才显示like）
///             2.修复某些情况下，下拉panel会变成选中蓝色背景
/// 2023-09-25  1.增加本控件是否必选或者必填才能选中下一个
/// 2023-10-07  1.增加外部触发筛选效果的方法
/// 2023-10-09  1.修复切换下一个控件的bug
/// 2023-10-11  1.修复bindData慢的情况
/// 2023-10-25  1.修复某些情况下明明选中了，但是combobox.Text显示System.Data.DataRow，导致提示未选中的问题。
/// 2023-10-30  1.修改bindColumnName 为public
///             2.修复bug
/// 2023-11-29  1.修复没数据时点击下拉报错的bug
/// 2023-12-01  1.增加一个回车模式，（按回车才会触发输入联动筛选）
/// 2023-12-26  1.增加setNext设置全部控件
/// 2024-01-03  1.使用bindDataNew替换bindData，并取消bindDataNew方法。
/// 2024-01-04  1.发现2023-10-25 1的一种情况，备注了
///             2.修复2023-10-11 1遗漏
/// 2024-01-23  1.修复输入内容后，全选不在第一个
/// 2024-04-09  1.修复一个bug
/// 2024-04-10  1.修复一点细节
/// 2024-05-29  1.增加一个如果选择All，返回自定义内容的方法
/// 2024-06-11  1.修复addTextDataBind的table没数据报错
/// 2024-07-12  1.增加联动排序
/// 2024-10-22  1.修复不可编辑时，点击左边文本框区域，不会出现待选项（必须要点击右边箭头）
/// 2025-01-21  1.增加清空内容后，自动选择全选或为""的项（暂不启用）
/// </summary>
public class NComboBox3 : TextBox {
    private DataRow bindRow = null;
    public String bindColumnName = null;
    private ComboBox combobox = null;
    private int defualPadding = 2;
    private String valueField;
    private String[] showFields;
    private NPanel shadows = new NPanel();
    private bool bindDataSuccess = false;
    public bool isExecEvent = true;
    private String[] filterField;
    private bool isInputMode = true;
    private bool isExecTextChangedEvent = true;
    private Panel panel = new Panel();
    private Color readerOnlyColor = Color.FromArgb(240, 240, 240);
    private Pen linePen = new Pen(SystemColors.GrayText);
    private Brush brush = new SolidBrush(Color.FromArgb(51, 153, 255));
    private bool isNull = true;
    private String next = "";
    private EventHandler onEnterListener = null;
    private List<Keys> enterListenerBindKey = new List<Keys>();

    public bool isAutoSelect = false;
    public getShowText showTextListener = null;
    public selectionChanged selectionListener = null;

    private DataTable bindDataTable = null;
    private Dictionary<String, float> columnMaxWidth = new Dictionary<string, float>();
    private Dictionary<String, Object> thisTag = new Dictionary<string, object>();
    private Dictionary<String, int> filterTextMaxWidth = new Dictionary<string, int>();
    public String order = "";
    public String ascOrDesc = "";

    private int selectedIndex = 0;
    private DataRow selectedDataRow = null;

    private bool isAllowUpAndDown = true;
    // 2023-09-21   1
    /// <summary>
    /// 是否启用精确查找，是判断=，不判断like
    /// </summary>
    public bool isPreciseFilter = false;

    /// <summary>
    /// 2023-12-01
    /// 是否启用回车模式
    /// </summary>
    private bool isEnterMode = false;

    private long getFocusTime = 0;


    public delegate String getShowText(DataRow row);
    public delegate void selectionChanged(DataRow row, String value, String showText);

    public bool IsInputMode {
        get {
            return isInputMode;
        }

        set {
            this.isInputMode = value;
            if (!isInputMode)
                this.Controls.Add(shadows);
            else
                this.Controls.Remove(shadows);
        }
    }

    public int Count {
        get { if (null != combobox) { return combobox.Items.Count; } return 0; }
    }

    public bool IsAllowUpAndDown {
        get {
            return isAllowUpAndDown;
        }

        set {
            this.isAllowUpAndDown = value;
        }
    }

    public NComboBox3() {
        initWidget();
        initData();
        initListener();
    }

    private void initWidget() {

        this.GetNextControl(this, false);

        initCombobox();

        panel.BackColor = this.BackColor;
        panel.Size = new Size(15, this.Height);
        this.Controls.Add(panel);
        panel.Dock = DockStyle.Right;
        panel.Cursor = Cursors.Default;


        shadows.BackColor = Color.Transparent;
        shadows.Width = this.Width - this.panel.Width;
        shadows.Height = this.Height;
        shadows.Location = new Point(-1, -1);
        shadows.Cursor = Cursors.Default;
    }


    public bool MouseIsOver(Control control) {
        Type tt = control.GetType();
        FieldInfo fi = tt.GetField("mouseOver", BindingFlags.NonPublic | BindingFlags.Instance);
        try {
            return Convert.ToBoolean(fi.GetValue(control));
        } catch (Exception eee) {
        }
        return false;
    }

    private void initCombobox() {
        bool isFirst = null == combobox;
        if (!isFirst) {
            this.Controls.Remove(combobox);
            Type tt = combobox.GetType();
            FieldInfo fi = tt.GetField("itemsCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            try {
                fi.SetValue(combobox, new ObjectCollection(combobox));
            } catch (Exception eee) {
            }
        }
        combobox = new ComboBox();
        combobox.Size = new Size(0, 0);
        combobox.Location = new Point(-2, 0);
        combobox.Sorted = false;
        this.Controls.Add(combobox);
        combobox.Sorted = false;
        combobox.ItemHeight = 21;
        combobox.DropDownHeight = 240;
        Type t = combobox.GetType();
        MethodInfo mi = t.GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mi2 = t.GetMethod("UpdateStyles", BindingFlags.Instance | BindingFlags.NonPublic);

        mi.Invoke(combobox, new Object[] { ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true });
        mi2.Invoke(combobox, null);
        combobox.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true, null);
        combobox.DrawMode = DrawMode.OwnerDrawFixed;
        combobox.DrawItem += NComboBox_DrawItem;
        combobox.SelectedValueChanged += Combobox_SelectedValueChanged;
        combobox.LostFocus += Combobox_LostFocus; ;

    }


    private void initData() {
        shadows.SendToBack();
        showTextListener = delegate (DataRow row) {
            if (showFields.Length <= 1)
                return row[showFields[0]] + "";
            if (showFields.Length > 1)
                return "[" + row[showFields[0]] + "] " + row[showFields[1]];
            return row[valueField] + "";
        };
    }



    public void onEnter() {
        if (null != this.onEnterListener) {
            onEnterListener(this, new EventArgs());
        }
    }
    public NComboBox3 setOnEnter(EventHandler methodInvoker) {
        return setOnEnter(methodInvoker, Keys.Tab, Keys.Enter);
    }
    public NComboBox3 setOnEnter(EventHandler methodInvoker, params Keys[] keys) {
        this.onEnterListener = methodInvoker;
        enterListenerBindKey.Clear();
        enterListenerBindKey.AddRange(keys);
        return this;
    }
    public NComboBox3 setNext(params Control[] nextControls) {
        if (nextControls.Length > 0) {
            setNext(nextControls[0]);
            for (int i = 0; i < nextControls.Length; i++) {
                Control control = nextControls[i];
                if (i + 1 != nextControls.Length) {
                    if (control is NEditText)
                        ((NEditText)control).setNext(nextControls[i + 1]);
                    else if (control is NDateTimePicker)
                        ((NDateTimePicker)control).setNext(nextControls[i + 1]);
                    else if (control is NComboBox3)
                        ((NComboBox3)control).setNext(nextControls[i + 1]);
                }
            }
        }
        return this;
    }
    public NComboBox3 setNext(Control nextControl) {
        return setNext(true, nextControl.Name);
    }
    public NComboBox3 setNext(String nextControlName) {
        return setNext(true, nextControlName);
    }
    // 2023-09-25   1
    public NComboBox3 setNext(bool isNull, Control nextControl) {
        return setNext(isNull, nextControl.Name);
    }
    public NComboBox3 setNext(bool isNull, String nextControlName) {
        this.isNull = isNull;
        this.next = nextControlName;
        return this;
    }

    public String getBindValueField() {
        return valueField;
    }
    public String[] getBindShowFields() {
        return showFields;
    }



    public void addTextDataBind(DataRow row, String bindColumnName) {
        this.bindRow = null;
        this.bindColumnName = null;
        this.setSelectionAllDataValue(row[bindColumnName] + "");
        this.bindRow = row;
        this.bindColumnName = bindColumnName;
    }

    public void addTextDataBind(DataTable table, String bindColumnName) {
        if (table.Rows.Count > 0)
            addTextDataBind(table.Rows[0], bindColumnName);
    }

    public void clearTextDataBind() {
        this.bindRow = null;
        this.bindColumnName = null;
        this.setSelectionAllDataValue(null);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {

        if ((keyData == Keys.Enter || keyData == Keys.Tab)) {
            if (null != combobox.Items && combobox.Items.Count > 0) {
                if (combobox.DroppedDown && -1 == combobox.SelectedIndex)
                    combobox.SelectedIndex = 0;
            }
            combobox.DroppedDown = false;
            // 2023-08-11   1
            try {
                Object val = combobox.SelectedItem;
            } catch (ArgumentOutOfRangeException eee) {
                combobox.SelectedItem = null;
            }
            if (isEnterMode) {
                DataRow selRow = getSeletionItem();
                if (null == selRow || (null != selRow && !this.Text.Equals(showTextListener(selRow)))) {
                    filter(this.Text);
                    return true;                    
                }
            }
        }
        
        if (enterListenerBindKey.Contains(keyData)) {
            if (null != onEnterListener) {
                onEnterListener(this, new EventArgs());
                return true;
            }
        }
        if ((keyData == Keys.Enter || keyData == Keys.Tab)) {
            Form form = this.FindForm();
            if (null != form) {
                if (!String.IsNullOrEmpty(next)) {
                    Control[] controls = form.Controls.Find(next, true);
                    if (null != controls && 0 != controls.Length) {
                        // 2023-09-25   1
                        if (!isNull) {
                            if (null == this.getSeletionItem()) {
                                return true;
                            }
                        }
                        if (controls[0].CanFocus) {
                            controls[0].Select();
                            controls[0].Focus();
                            // 2023-04-12  1
                            if (controls[0] is NComboBox3)
                                ((NComboBox3)controls[0]).SelectAll();
                            else if (controls[0] is NEditText)
                                ((NEditText)controls[0]).SelectAll();
                            else if (controls[0] is NDateTimePicker)
                                ((NDateTimePicker)controls[0]).SelectAll();
                            else if (controls[0] is NDataGridView5)
                                ((NDataGridView5)controls[0]).getDataView().onMouseClick();
                            else if (controls[0] is NDataGridView3)
                                ((NDataGridView3)controls[0]).onMouseClick();
                            else {
                                // 2023-10-09   1
                                controls[0].Select();
                                controls[0].Focus();
                                if (controls[0] is TextBoxBase) {
                                    ((TextBoxBase)controls[0]).SelectAll();
                                }
                            }
                            return true;
                        }
                    }
                }
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void initListener() {


        this.GotFocus += (oo, ee) => {
            if (!this.IsInputMode) {
                shadows.Select();
                shadows.Focus();
            }
            getFocusTime = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            // 2023-05-05 1
            panel.Invalidate();
        };
        // 2023-05-31 2
        this.LostFocus += (oo, ee) => {
            if (!combobox.Focused && !combobox.DroppedDown) {
                this.Select(0, 0);
                this.ScrollToCaret();
            }
            // 2023-05-05 1
            panel.Invalidate();
        };
        
        // 2023-05-30  1
        this.MouseWheel += (oo, ee) => {
            this.OnKeyDown(new KeyEventArgs(ee.Delta < 0 ? Keys.Down : Keys.Up));
        };


        panel.Paint += (oo, ee) => {
            using (GraphicsPath path = new GraphicsPath()) {
                Rectangle downSize = new Rectangle(panel.Width / 2 - 3, panel.Height / 2 - 1, 6, 3);
                path.AddLine(new PointF(downSize.Left, downSize.Top), new PointF(downSize.Right, downSize.Top));
                path.AddLine(new PointF(downSize.Right, downSize.Top), new PointF(downSize.Left + downSize.Width / 2, downSize.Bottom));
                path.AddLine(new PointF(downSize.Left + downSize.Width / 2, downSize.Bottom), new PointF(downSize.Left, downSize.Top));
                // 2023-09-21   1
                ee.Graphics.FillRectangle(new SolidBrush(this.ReadOnly ? readerOnlyColor : this.BackColor), panel.DisplayRectangle);
                ee.Graphics.FillPath(Brushes.Black, path);
                ee.Graphics.DrawPath(Pens.Black, path);
                ee.Graphics.Dispose();
            }
        };
        this.SizeChanged += (oo, ee) => {
            shadows.Width = this.Width - this.panel.Width;
            shadows.Height = this.Height;
            panel.Invalidate();
        };


        this.MouseClick += delegate (object sender, MouseEventArgs e) {
            // 2023-05-18  1
            long cur = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            if (cur - getFocusTime < 555) {
                this.Select();
                this.Focus();
                this.SelectAll();
                getFocusTime = 0;
            }
        };
        this.MouseDoubleClick += (oo, ee) => {
            this.Select();
            this.Focus();
            this.SelectAll();
        };

        this.MouseMove += delegate (object sender, MouseEventArgs e) {
            this.Cursor = Cursors.IBeam;
        };
        EventHandler clickEvent = (oo, ee) => {
            if (ReadOnly || null == this.bindDataTable)
                return;
            if (this.bindDataTable.Rows.Count != 0 && combobox.Items.Count == 0) {
                combobox.Items.AddRange(bindDataTable.Select());
            }
            combobox.Select();
            combobox.Focus();
            combobox.DroppedDown = true;
            // 2023-06-26  1
            if (-1 != selectedIndex) {
                bool bck = isExecEvent;
                isExecEvent = false;
                // 2023-07-26   1
                if (combobox.Items.Count > selectedIndex)
                    combobox.SelectedIndex = selectedIndex;
                isExecEvent = bck;
            }
        };
        this.shadows.Click += (oo, ee) => {
            if (!isInputMode) {
                clickEvent(oo, ee);
            }
        };
        this.panel.Click += (oo, ee) => {
            clickEvent(oo,ee);
        };
        this.ReadOnlyChanged += (oo, ee) => {
            panel.Invalidate();
        };
        // 2023-05-05 1
        this.VisibleChanged += (oo, ee) => {
            panel.Invalidate();
        };

        // 2023-05-24 1
        this.KeyDown += (oo, ee) => {
            // 2023-08-08 1
            if (isAllowUpAndDown && !isEnterMode) {
                if (ee.KeyData == Keys.Up || ee.KeyData == Keys.Down) {
                    int idx = ee.KeyData == Keys.Up ? -1 : 1;
                    int newIndex = selectedIndex + idx;

                    if (this.bindDataTable.Rows.Count != 0 && combobox.Items.Count == 0) {
                        combobox.Items.AddRange(bindDataTable.Select());
                    }
                    if (newIndex >= 0 && newIndex < combobox.Items.Count) {
                        // 2023-05-29 1
                        bool isShowDownPanel = combobox.DroppedDown;
                        setSelectionCurrItemsIndex(newIndex);
                        if (isShowDownPanel)
                            combobox.DroppedDown = true;
                        this.SelectAll();
                        ee.Handled = true;
                    }
                    // 2025-01-21   1
                } else if(ee.KeyData == Keys.Back || ee.KeyData == (Keys.Control|Keys.X)) {
                    //if (!isSelectAll()) {
                    //    DataRow[] rows = this.bindDataTable.Select(this.valueField+"='' or "+this.valueField+"='all'");
                    //    if (rows.Length > 0) {
                    //        setSelectionValue(rows[0][this.valueField]);
                    //    }
                    //}
                }
            }
        };
    }

    // 2023-05-05 1
    protected override void OnPaint(PaintEventArgs e) {
        panel.Invalidate();
    }


    private void Combobox_SelectedValueChanged(object sender, EventArgs e) {
        if (ReadOnly || !bindDataSuccess || !isExecEvent)
            return;
        DataRow selRow = combobox.SelectedItem as DataRow;
        if (null == selRow)
            return;
        // 2023-04-27 1
        bool isExecEvent_back = isExecEvent;
        isExecEvent = false;
        isExecTextChangedEvent = false;
        selectedDataRow = selRow;
        selectedIndex = combobox.SelectedIndex;
        String text = showTextListener(selRow);
        float w = this.CreateGraphics().MeasureString(text, this.Font).Width;
        if (w > this.Width)
            text = text + "   ";
        this.Text = text;
        isExecTextChangedEvent = true;
        if (null != bindRow && null != bindColumnName)
            bindRow[bindColumnName] = selectedDataRow[valueField];
        try {
            if (null != selectionListener) {
                selectionListener(selRow, selRow[valueField] + "", this.Text);
            }
        } catch (Exception eee) {
            String dsdsds = "";
        }
        this.Select();
        this.Focus();
        this.SelectAll();


        isExecEvent = isExecEvent_back;
    }

    private void Combobox_LostFocus(object sender, EventArgs e) {
        this.BeginInvoke(new MethodInvoker(() => {
            bool txt = this.Focused;
            bool box = combobox.Focused;
            if (!txt && !box && combobox.DroppedDown && isAutoSelect) {
                if (combobox.Items.Count > 0)
                    combobox.SelectedIndex = 0;
                combobox.DroppedDown = false;
                // 2023-08-11   1.
                try {
                    Object val = combobox.SelectedItem;
                } catch (ArgumentOutOfRangeException eee) {
                    combobox.SelectedItem = null;
                }
            }
        }));
    }


    private void NComboBox_DrawItem(object sender, DrawItemEventArgs e) {

        if (e.Index == -1 || e.Index >= combobox.Items.Count || columnMaxWidth.Count == 0)
            return;
        DataRow val = ((DataRow)combobox.Items[e.Index]);

        e.DrawBackground();
        try {
            int lastRight = 1;
            Rectangle rect = e.Bounds;
            using (SolidBrush fontColor = new SolidBrush(e.ForeColor)) {
                if (null != selectedDataRow && selectedDataRow[valueField] == val[valueField]) {
                    SolidBrush brush = new SolidBrush(Color.FromArgb(51, 153, 255));
                    e.Graphics.FillRectangle(brush, e.Bounds);
                    fontColor.Color = Color.White;
                }
                // 2023-07-28   2
                float itemMaxHeight = 0F;
                for (int i = 0; i < showFields.Length; i++) {
                    String columnName = showFields[i];
                    string item = val[columnName] + "";
                    SizeF sizeF = e.Graphics.MeasureString(item, e.Font);
                    if (itemMaxHeight < sizeF.Height)
                        itemMaxHeight = sizeF.Height;
                }

                //循环各列
                for (int i = 0; i < showFields.Length; i++) {
                    String columnName = showFields[i];
                    string item = val[columnName] + "";
                    rect.X = lastRight + defualPadding;
                    rect.Width = (int)columnMaxWidth[columnName] + defualPadding;
                    lastRight = rect.Right;
                    e.Graphics.DrawString(item, e.Font, fontColor, rect.X, rect.Y + (Math.Abs(rect.Height - itemMaxHeight) / 2F));

                    if (i < showFields.Length - 1) {
                        e.Graphics.DrawLine(linePen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                    }
                }
            }
        } catch (Exception eeee) {
            String sdsdsd = "";
        }
    }


    private String backInputText = "";
    private List<Task> execTask = new List<Task>();
    protected override void OnTextChanged(EventArgs e) {

        if (!bindDataSuccess || !isExecTextChangedEvent || ReadOnly || !IsInputMode || !Visible || !isExecEvent || isEnterMode) return;

        if (this.bindDataTable == null || 0 == this.bindDataTable.Rows.Count)
            return;
        // 2023-09-13   2
        lock (this) {
            backInputText = this.Text;
        }
    

        // 第二版，处理数据量大，慢的问题。处理方式，再页面前面加以个筛选文本框，按回车再把前面的内容当作输入来筛选并展示
        filterChooserItem(this.Text);

    }

    // 2023-10-07   1
    public void filter(String filterText) {
        backInputText = filterText;
        filterChooserItem(filterText);
    }

    private void filterChooserItem(String text) {

        selectedIndex = -1;
        StringBuilder filters = new StringBuilder("");
        StringBuilder filters2 = new StringBuilder();
        long start = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        if (text.Length > 0) {
            List<DataRow> list = new List<DataRow>();

            if (null == filterField || 0 == filterField.Length) {
                // 2023-07-27   1
                filters.Append(valueField + " like '%" + text + "%' " + (filterField.Length > 1 ? "or " + showFields[1] + " like '%" + text + "%'" : "") + "    ");
                filters2.Append(valueField + "='" + text + "'    ");
            } else {
                //filters.Append(filterField[0] + " like '%" + text + "%' or ");
                for (int i = 0; i < filterField.Length; i++) {
                    filters.Append(filterField[i] + " like '%" + text + "%' or ");
                }

                filters2.Append(filterField[0] + "='" + text + "'    ");
            }
            if (filters.Length > 8)
                filters.Length -= 4;
        } else {
            filters.Append("1=1");
        }
        long start2 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        List<DataRow> filter_row = new List<DataRow>();
        List<DataRow> filter_all = new List<DataRow>();
        long start3 = 0L;
        long start4 = 0L;
        if (!backInputText.Equals(text)) return;
        // 2023-09-13   1
        try {
            // 2023-09-15   1
            DataRow[] filter_row2 = bindDataTable.Select(filters2.ToString(),Tools.isNullOrEmpty(this.order) ? getBindValueField():this.order+" "+this.ascOrDesc);
            start4 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
            filter_all.AddRange(filter_row2);

            if (!isPreciseFilter || isPreciseFilter && filter_row2.Length == 0) {
                // 2023-09-15   1
                filter_row.AddRange(bindDataTable.Select(filters.ToString(), Tools.isNullOrEmpty(this.order) ? getBindValueField() : this.order + " " + this.ascOrDesc));
                start3 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
                if (filter_row2.Length > 0) {
                    foreach (DataRow item1 in filter_row2) {
                        DataRow remove = null;
                        foreach (DataRow item2 in filter_row) {
                            if ((item1[valueField] + "").Trim().Equals((item2[valueField] + "").Trim())) {
                                remove = item2;
                                break;
                            }
                        }
                        if (null != remove) {
                            filter_row.Remove(remove);
                        }
                    }
                }
            }
            // 2024-01-23   1.
            if (filter_row.Count + filter_all.Count == bindDataTable.Rows.Count) {
                DataRow[] all = bindDataTable.Select(getBindValueField() + "='all'");
                if (all.Length > 0) {
                    filter_all.Remove(all[0]);
                    filter_all.Insert(0,all[0]);
                }
            }
        } catch (Exception eee) {
            if (eee is EvaluateException) {
                return;
            }
        }
        if (!backInputText.Equals(text)) return;
        filter_all.AddRange(filter_row);
        
        long start5 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        DataRow[] rows = filter_all.ToArray();
        Dictionary<String, int> columnNameMaxW = new Dictionary<string, int>();
        Dictionary<String, String> columnNameMaxT = new Dictionary<string, String>();
        Dictionary<String, float> columnWidths = new Dictionary<string, float>();
        Graphics g = this.CreateGraphics();
        foreach (DataRow row in rows) {
            for (int i = 0; i < showFields.Length; i++) {
                String column = showFields[i];
                if (!columnNameMaxW.ContainsKey(column)) {
                    columnNameMaxW.Add(column, 0);
                    columnNameMaxT.Add(column, row[column] + "");
                }
                int w = columnNameMaxW[column];
                // 2023-08-31   1
                int valByteCount = Encoding.Default.GetBytes(row[column] + "").Length;
                if (w < valByteCount) {
                    columnNameMaxW.Remove(column);
                    columnNameMaxT.Remove(column);
                    columnNameMaxW.Add(column, valByteCount);
                    columnNameMaxT.Add(column, row[column] + "");
                }
            }
        }

        foreach (String key in columnNameMaxW.Keys) {
            SizeF sizeF = g.MeasureString(columnNameMaxT[key], this.Font);
            columnWidths.Add(key, (int)(Math.Ceiling(sizeF.Width)));
        }
        g.Dispose();
        long start6 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        float maxDropDownWidth = 0.0F;
        foreach (String key in columnWidths.Keys) {
            maxDropDownWidth = maxDropDownWidth + columnWidths[key];
        }
        if (!backInputText.Equals(text)) return;
        columnMaxWidth = columnWidths;
        if (rows.Length == 0)
            combobox.DropDownWidth = this.Width;
        else
            combobox.DropDownWidth = (int)Math.Ceiling(maxDropDownWidth) + defualPadding + showFields.Length * 2 * defualPadding + SystemInformation.VerticalScrollBarWidth;
        if (combobox.DropDownWidth < this.Width)
            combobox.DropDownWidth = this.Width;
        long start7 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        if (!backInputText.Equals(text)) return;
        combobox.SelectedIndex = -1;
        combobox.Items.Clear();
        bool sorted = combobox.Sorted;
        combobox.Sorted = false;
        combobox.BeginUpdate();
        combobox.SuspendLayout();
        combobox.Items.AddRange(rows);
        combobox.ResumeLayout(false);
        combobox.EndUpdate();
        combobox.Sorted = sorted;
        combobox.Size = combobox.Size;
        if (text.Length == 0) {
            selectedDataRow = null;
            // 2023-08-11 1.
            combobox.SelectedItem = null;
            if (null != bindRow && null != bindColumnName)
                bindRow[bindColumnName] = "";
            return;
        }
        // 2023-08-11 1.
        if (rows.Length == 0) {
            selectedDataRow = null;
            combobox.SelectedItem = null;
        }
        // 2023-07-25 1.
        selectedDataRow = null;
        long start8 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        this.Cursor = Cursors.Default;
        combobox.Cursor = Cursors.Default;
        this.Parent.Cursor = this.Parent.Cursor;
        long start9 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        if (!combobox.DroppedDown) {
            if (!backInputText.Equals(text)) return;
            combobox.DroppedDown = true;
        }

        long start10 = Convert.ToInt64(DateTime.Now.ToString("HHmmssfff"));
        this.Cursor = Cursors.Default;
        combobox.Cursor = Cursors.Default;
        this.Parent.Cursor = this.Parent.Cursor;

        String sdsd = "(start2-start)=" + (start2 - start)
            + "\n (start3-start2)=" + (start3 - start2)
            + "\n (start4-start3)=" + (start4 - start3)
            + "\n (start5-start4)=" + (start5 - start4)
            + "\n (start6-start5)=" + (start6 - start5)
            + "\n (start7-start6)=" + (start7 - start6)
            + "\n (start8-start7)=" + (start8 - start7)
            + "\n (start9-start8)=" + (start9 - start8)
            + "\n (start10-start9)=" + (start10 - start9);

    }

    public DataTable getBindData() {
        return this.bindDataTable;
    }

    public NComboBox3 reBindData() {
        if (null == bindDataTable)
            return this;
        if (combobox.Items.Count != bindDataTable.Rows.Count) {
            if (combobox.Items.Count > 0)
                combobox.Items.Clear();
            combobox.Items.AddRange(bindDataTable.Select());
        }
        return this;
    }

    public NComboBox3 setOrderBy(String order) {
        return setOrderBy(order, "");
    }
    public NComboBox3 clearOrder() {
        this.order = this.ascOrDesc = "";
        return this;
    }
    public NComboBox3 setOrderBy(String order, String ascOrDesc) {
        this.order = order;
        this.ascOrDesc = ascOrDesc;
        return this;
    }

    public NComboBox3 bindDataBySort(DataTable table, String order) {
        return bindDataBySort(table,order,"");
    }
    public NComboBox3 bindDataBySort(DataTable table, String order, String ascOrDesc) {
        setOrderBy(order, ascOrDesc);
        return bindData(table);
    }
    public NComboBox3 bindData(DataTable table, params String[] filterField) {
        String val = "";
        List<String> shows = new List<string>();
        if (null == table)
            table = new DataTable();
        if (null != table && 0 != table.Rows.Count) {
            if (null != table && 0 != table.Rows.Count) {
                if (table.Columns.Count > 0) {
                    val = table.Columns[0].ColumnName;
                    shows.Add(table.Columns[0].ColumnName);
                }
                if (table.Columns.Count > 1) {
                    shows.Add(table.Columns[1].ColumnName);
                }
            }
        }
        return bindData(val, shows.ToArray(), table, filterField);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="valueField">value值</param>
    /// <param name="showField">显示值</param>
    /// <param name="table">数据源</param>
    /// <param name="filterField">允许进行筛选的列名</param>
    /// <returns></returns>
    public NComboBox3 bindData(String valueField, String[] showFields, DataTable table, params String[] filterField) {
        isEnterMode = false;
        bindDataSuccess = false;
        columnMaxWidth.Clear();
        if (this.IsDisposed)
            return this;
        Graphics g = this.CreateGraphics();
        // 2023-08-02   1
        if (0 == filterField.Length) {
            String[] filters = new String[showFields.Length];
            showFields.CopyTo(filters, 0);
            filterField = filters;
        }
        this.filterField = filterField;
        this.valueField = valueField;
        this.showFields = showFields;


        foreach (DataRow row in table.Rows) {
            for (int i = 0; i < showFields.Length; i++) {
                String column = showFields[i];
                String val = (row[column] + "").Trim();
                row[column] = val;
                int w = (int)(Math.Ceiling(g.MeasureString(val + "", this.Font).Width));
                int pw = columnMaxWidth.ContainsKey(column) ? (int)columnMaxWidth[column] : 0;
                // 2023-07-28   1
                if (w >= pw) {
                    columnMaxWidth.Remove(column);
                    columnMaxWidth.Add(column, w);
                }
            }
        }
        g.Dispose();
        float maxDropDownWidth = 0.0F;
        foreach (String key in columnMaxWidth.Keys) {
            maxDropDownWidth = maxDropDownWidth + columnMaxWidth[key];
        }

        this.bindDataTable = table;
        if (combobox.Items.Count > 0)
            combobox.Items.Clear();

        if (isAutoSelect) {
            combobox.Items.AddRange(bindDataTable.Select());
        }
     
        combobox.DropDownWidth = (int)Math.Ceiling(maxDropDownWidth) + defualPadding + showFields.Length * 2 * defualPadding + SystemInformation.VerticalScrollBarWidth;
        if (combobox.DropDownWidth < this.Width)
            combobox.DropDownWidth = this.Width;
        if (this.Focused) {
            this.BeginInvoke(new MethodInvoker(() => {
                this.SelectAll();
            }));
        }
        selectedIndex = -1;
        filterTextMaxWidth.Clear();
        selectedDataRow = null;
        bindDataSuccess = true;
        if (isAutoSelect) {
            setSelectionAllDataIndex(0);
        }
        return this;
    }
    /// <summary>
    /// 绑定数据，要输入Enter，才会触发联动
    /// </summary>
    /// <param name="table"></param>
    /// <param name="filterField"></param>
    /// <returns></returns>
    public NComboBox3 bindDataForEnter(DataTable table, params String[] filterField) {
        String val = "";
        List<String> shows = new List<string>();
        if (null == table)
            table = new DataTable();
        if (null != table && 0 != table.Rows.Count) {
            if (null != table && 0 != table.Rows.Count) {
                if (table.Columns.Count > 0) {
                    val = table.Columns[0].ColumnName;
                    shows.Add(table.Columns[0].ColumnName);
                }
                if (table.Columns.Count > 1) {
                    shows.Add(table.Columns[1].ColumnName);
                }
            }
        }
        return bindDataForEnter(val, shows.ToArray(), table, filterField);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="valueField">value值</param>
    /// <param name="showField">显示值</param>
    /// <param name="table">数据源</param>
    /// <param name="filterField">允许进行筛选的列名</param>
    /// <returns></returns>
    public NComboBox3 bindDataForEnter(String valueField, String[] showFields, DataTable table, params String[] filterField) {
        NComboBox3 combobox = bindData(valueField,showFields,table,filterField);
        isEnterMode = true;
        return combobox;
    }


    /// <summary>
    /// 根据下表选中所有数据中符合的数据
    /// </summary>
    /// <param name="index"></param>
    public void setSelectionCurrItemsIndex(int index) {
        if (this.combobox.Items.Count > index)
            setSelectionValue(valueField, ((DataRow)this.combobox.Items[index])[valueField], false);
    }
    /// <summary>
    /// 根据下表选中所有数据中符合的数据
    /// </summary>
    /// <param name="index"></param>
    public void setSelectionAllDataIndex(int index) {
        if (bindDataTable.Rows.Count > index)
            setSelectionValue(valueField, bindDataTable.Rows[index][valueField], true);
    }

    public void setSelectionCurrItemsValue(Object value) {
        setSelectionValue(valueField, value, false);
    }
    public void setSelectionCurrItemsValue(String key, Object value) {
        setSelectionValue(key, value, false);
    }
    public void setSelectionAllDataValue(Object value) {
        setSelectionValue(valueField, value, true);
    }
    public void setSelectionAllDataValue(String key, Object value) {
        setSelectionValue(key, value, true);
    }

    // 2023-05-31 1
    public void setSelectionValue(Object value) {
        setSelectionValue(valueField, value, true);
    }



    public void setSelectionValue(String key, Object value,bool isAllData) {
        try {
            if (Tools.isNullOrEmpty(value) || DBNull.Value.Equals(value) || null == bindDataTable) {
                this.selectedDataRow = null;
                combobox.SelectedIndex = selectedIndex = -1;
                isExecTextChangedEvent = false;
                this.Text = "";
                isExecTextChangedEvent = true;
                if (null != bindRow && null != bindColumnName)
                    bindRow[bindColumnName] = "";
                return;
            }

            if (bindDataTable.Rows.Count > 0 && !Tools.isNullOrEmpty(key)) {
                int idx = 0;
                // 2023-05-29 1
                List<DataRow> rows = new List<DataRow>();
                if (isAllData) {
                    foreach (DataRow item in bindDataTable.Rows) {
                        rows.Add(item);
                    }
                    if (this.combobox.Items.Count != rows.Count)
                        this.reBindData();
                } else {
                    foreach (DataRow item in this.combobox.Items) {
                        rows.Add(item);
                    }
                }
                foreach (DataRow row in rows) {
                    if (Tools.eq(Tools.trim(value), row[key])) {
                        selectedDataRow = row;
                        // 2023-06-15 1
                        bool oldVal = isExecEvent;
                        isExecEvent = false;
                        // 2023-05-29 1
                        combobox.SelectedIndex = selectedIndex = idx;
                        isExecTextChangedEvent = false;
                        this.Text = showTextListener(row);
                        isExecTextChangedEvent = true;
                        isExecEvent = oldVal;
                        if (null != bindRow && null != bindColumnName)
                            bindRow[bindColumnName] = selectedDataRow[valueField];
                        // 2023-03-30 1
                        if (null != selectionListener && isExecEvent) {
                            selectionListener(row, row[valueField] + "", this.Text);
                        }
                        break;
                    }
                    idx++;
                }
            }
        } catch (Exception eee) {
            String dsdsdsd = "";
        }
    }

    public void addItem(Object[] objs) {
        if (null == objs || 0 == objs.Length)
            return;
        Dictionary<String, Object> map = new Dictionary<string, object>();
        int i = 0;
        foreach (String key in showFields) {
            map.Add(key, objs[i]);
            i++;
        }
        addItem(map);
    }
    public void addItem(Dictionary<String, Object> vals) {
        if (null == bindDataTable || null == vals || 0 == vals.Count)
            return;
        DataRow row = this.bindDataTable.NewRow();
        foreach (String key in vals.Keys) {
            if (bindDataTable.Columns.Contains(key))
                row[key] = vals[key];
        }
        this.bindDataTable.Rows.Add(row);
        this.combobox.Items.Add(row);
    }


    public void start() {
        if (this.bindDataTable.Rows.Count != 0 && combobox.Items.Count == 0) {
            combobox.Items.AddRange(bindDataTable.Select());
        }
        combobox.DroppedDown = true;
    }

    public bool isSelectAll() {
        return isSelectVal("all");
    }
    public bool isSelectVal(String val) {
        if (null == getSeletionItem())
            return true;
        return (val + "").Equals(getSelectionValue(), StringComparison.CurrentCultureIgnoreCase);
    }

    // 2024-05-29   1
    public String getSelectionValueOrDefault(String isNullReturnValue) {
        String selOrInput = getSelectValueOrInputText();
        if (Tools.isNullOrEmpty(selOrInput)) {
            return isNullReturnValue;
        }
        return selOrInput;
    }
    public String getSelectionValue() {
        return getSelectionValue(valueField);
    }
    public String getSelectionValue(String columnName) {
        DataRow row = getSeletionItem();
        if (null == row)
            return "";
        return Tools.trim(row[columnName]);
    }
    public String getSelectionShowText() {
        DataRow row = getSeletionItem();
        if (null == row)
            return "";
        return showTextListener.Invoke(row);
    }

    /// <summary>
    /// 获取当前选中Value或者当前输入的Text
    /// </summary>
    /// <returns></returns>
    public String getSelectValueOrInputText() {
        String val = getSelectionValue();
        if (String.IsNullOrEmpty(val))
            val = this.Text.Replace("'", "");
        return val;
    }


    public DataRow getSeletionItem() {
        // 2023-04-21 1
        if (null != selectedDataRow && (selectedDataRow.RowState == DataRowState.Deleted || selectedDataRow.RowState == DataRowState.Detached)) {
            selectedDataRow = null;
            return null;
        }
        // 2023-10-25   1
        if (null == selectedDataRow) {
            String comText = "";
            // 2024-04-09   1
            // 触发条件：输入一个没有待选的内容，然后直接点查询，掉用getSelectValueOrInputText后，会就报错。
            // 但是如果按Tab切换了焦点，就在查询就不会报错
            try {
                comText = combobox.Text;
            } catch (ArgumentOutOfRangeException eee) {
                String sd = ""; 
            }
            if (!String.IsNullOrEmpty(comText) && comText.Contains("System.Data.DataRow")) {
                if (!String.IsNullOrEmpty(this.Text) && !this.Text.Equals(comText)) {
                    // 还未找到处理方法
                    // 发现一种情况，在查询完成后，调用了bandData。就会走到这里。目前时查询完成重新绑定后，在把之前选中的默认给他选中
                    String sdsds = "";
                    this.Text = "";
                }
            }
        }
        // 2023-08-07 1
        if (null == selectedDataRow) {
            combobox.SelectedItem = null;
            combobox.SelectedIndex = selectedIndex = -1;
        }
        return selectedDataRow;
    }

    public NComboBox3 setTag(String tag, Object obj) {
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

    public void clearInput() {
        this.isExecEvent = false;
        this.Text = "";
        combobox.SelectedItem = selectedDataRow = null;
        combobox.SelectedIndex = selectedIndex = -1;
        this.isExecEvent = true; ;
    }
    public void clearChooseData() {
        this.combobox.Items.Clear();
    }

    public void clearInputAndChooseData() {
        clearChooseData();
        clearInput();
        // 2023-04-18 1
        if (null != bindDataTable)
            bindDataTable.Clear();
    }

    protected override void Dispose(bool disposing) {
        this.Controls.Remove(combobox);
        Type tt = combobox.GetType();
        FieldInfo fi = tt.GetField("itemsCollection", BindingFlags.NonPublic | BindingFlags.Instance);
        try {
            fi.SetValue(combobox, new ObjectCollection(combobox));
        } catch (Exception eee) {
        }
        base.Dispose(disposing);
        linePen.Dispose();
        combobox.Dispose();
        brush.Dispose();
        System.GC.Collect();
    }
    public void requestFocus() {
        this.Select();
        this.Focus();
        this.SelectAll();
    }

}


public class NPanel : Panel {
    private const int WS_EX_TRANSPARENT = 0x20;
    public NPanel() {
        SetStyle(ControlStyles.Opaque, true);
    }

    private int opacity = 0;
    [DefaultValue(50)]
    public int Opacity {
        get {
            return this.opacity;
        }
        set {
            if (value < 0 || value > 100)
                throw new ArgumentException("value must be between 0 and 100");
            this.opacity = value;
        }
    }
    protected override CreateParams CreateParams {
        get {
            CreateParams cp = base.CreateParams;
            cp.ExStyle = cp.ExStyle | WS_EX_TRANSPARENT;
            return cp;
        }
    }
    protected override void OnPaint(PaintEventArgs e) {
        using (var brush = new SolidBrush(Color.FromArgb(this.opacity * 255 / 100, this.BackColor))) {
            e.Graphics.FillRectangle(brush, this.ClientRectangle);
        }
        base.OnPaint(e);
    }
}