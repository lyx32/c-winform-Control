using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

/// <summary>
/// 2022-10-17 添加addDataBind方法
///            添加身份证类型
/// 2022-12-12 添加回车触发事件，并修改触发事件类型
/// 2022-12-28 修改输入达到ManLength之后，在中间输入光标回到最后的bug
/// 2022-12-29 修改onSetEnter，增加绑定按键
/// 2023-01-12 增加setNext(Control) 方法
/// 2023-01-16 修复一个输入bug
///            修复addTextBind之后，文本框置空，DataRow没置空的bug
///            修复addTextBind之后，数据为double类型，但是把值删了会报错的bug（现在是置为DBNull.Value）
/// 2023-02-14 增加不允许输入的关键字
/// 2023-04-12  1.增加将焦点切换到下一个时的一些选中
/// 2023-07-17  1.增加有焦点是否自动全选
/// 2023-07-21  1.修复设置输入为deimal，但是赋值是int，某些情况下会丢失的bug
/// 2023-08-15  1.增加负数
/// 2023-09-25  1.增加本控件是否必选或者必填才能选中下一个
/// 2023-10-09  1.修复切换下一个控件的bug
/// 2023-12-26  1.增加setNext设置全部控件
/// 2024-06-11  1.修复addTextDataBind的table没数据报错
/// 2025-01-20  1.增加是否允许负数录入
/// 2025-01-15  1.修复输入格式为0000.00中，当前内容为1.23。光标在1后面输入.光标没有后移的问题
///             2.取消日期选择模式
/// 2025-01-20  1.增加是否允许负数录入
/// 2025-01-21  1.修复输入小数字符可以录入.的问题
/// 2025-02-17  1.增加targetTextChanged方法
/// </summary>
public class NEditText : TextBox {

    private String bindColumnName;
    private DataRow bindRow;

    public SqlDbType maxType = SqlDbType.Char;
    public bool isAllowShowICO = false;
    // 2023-07-17 1.
    public bool isAutoSelectAll = true;
    private bool isSelectAll = false;
    private String format = "";
    private Regex replaceNoDecimal = new Regex("[^0-9\\.]");
    private int inputType = INPUT_TYPE_TEXT;
    public static readonly int INPUT_TYPE_INT = 0;
    public static readonly int INPUT_TYPE_DECIMAL = 1;
    public static readonly int INPUT_TYPE_ENGLISH = 2;
    public static readonly int INPUT_TYPE_ENGLISH_INT = 3;
    public static readonly int INPUT_TYPE_ENGLISH_DECIMAL = 4;
    public static readonly int INPUT_TYPE_TEXT = 5;
    public static readonly int INPUT_TYPE_SFZ = 6;
    // 2025-01-20   1
    public bool isAllowNegative = true;
    private bool isNull = true;
    private String next = "";
    private EventHandler onEnterListener = null;
    private List<Keys> enterListenerBindKey = new List<Keys>();

    private Regex isInt = new Regex("^-?\\d+$");
    private Regex isDecimal = new Regex("^-?\\d+(\\.\\d+)$");
    private Regex isDecimal2 = new Regex("^-?\\d+\\.$");
    private Regex isEnglish = new Regex("^[A-Za-z]+$");
    private Regex isEnglishAndInt = new Regex("^[A-Za-z0-9]+$");
    private Regex isEnglishAndDecimal = new Regex("^[A-Za-z0-9\\.]+$");
    private Regex isSFZ = new Regex("^\\d{17}[\\dxX]$");


    // 16 是Ctrl+V 03 是Ctrl+C 18 是Ctrl+X \b 是退格   01 是Ctrl+A
    private String[] noFilter = new String[] { "\u0001", "\u0003", "\u0018", "\u0016", "\b" };
    private String[] noFilter2 = new String[] { "\u0003", "\u0018", "\b" };
    private String[] noFilter3 = new String[] { "x", "X" };
    private String[] keywords = new String[] { "'", "--" };
    private Dictionary<String, Object> thisTag = new Dictionary<string, object>();


    public NEditText() {
        this.GetNextControl(this, false);
        this.Enter += NEditText_Enter;
        this.MouseUp += NEditText_MouseUp;
        this.TextChanged += NEditText_TextChanged;
        this.KeyPress += NEditText_KeyPress;
        this.MouseMove += NEditText_MouseMove;
        this.MouseLeave += NEditText_MouseEnter;
        this.MouseEnter += NEditText_MouseEnter;
        this.MouseClick += NEditText_MouseClick;
        if (null != this.FindForm()) {
            this.FindForm().FormClosing += NEditText_FormClosing;
            this.FindForm().MouseMove += NEditText_MouseMove;
        }
    }




    public void addTextDataBind(DataRow row, String bindColumnName) {
        this.bindRow = row;
        this.bindColumnName = bindColumnName;
        this.TextChanged -= NEditText_TextChanged;
        this.Text = row[bindColumnName] + "";
        this.TextChanged += NEditText_TextChanged;
    }

    public void addTextDataBind(DataTable table, String bindColumnName) {
        if (table.Rows.Count > 0)
            addTextDataBind(table.Rows[0], bindColumnName);
    }

    public void clearTextDataBind() {
        this.bindRow = null;
        this.bindColumnName = null;
        backText = "";
    }





    private void NEditText_FormClosing(object sender, FormClosingEventArgs e) {
        ( (Form)sender ).MouseMove -= NEditText_MouseMove;
    }


    private void NEditText_MouseEnter(object sender, EventArgs e) {
        draw();
    }

    private void NEditText_MouseClick(object sender, MouseEventArgs e) {
        Object _size = getTag("size");
        Object _point = getTag("point");


        if (this.TextLength != 0 && isAllowShowICO && Enabled && !ReadOnly) {
            draw();
            SizeF size = (SizeF)_size;
            PointF point = (PointF)_point;
            if (e.X > point.X && e.X < point.X + size.Width && e.Y > point.Y && e.Y < point.Y + size.Height) {
                lock (this) {
                    this.Text = "";
                    this.Cursor = (Cursor)this.getTag("cursor");


                }
            }
        }
    }

    private void NEditText_MouseMove(object sender, MouseEventArgs e) {
        Object _size = getTag("size");
        Object _point = getTag("point");
        if (this.TextLength != 0 && isAllowShowICO && Enabled && !ReadOnly) {
            SizeF size = (SizeF)_size;
            PointF point = (PointF)_point;
            if (e.X > point.X && e.X < point.X + size.Width && e.Y > point.Y && e.Y < point.Y + size.Height) {
                if (null == this.getTag("cursor")) {
                    lock (this) {
                        if (null == this.getTag("cursor"))
                            this.setTag("cursor", this.Cursor);
                    }
                }
                this.Cursor = Cursors.Arrow;
            } else {
                this.Cursor = (Cursor)this.getTag("cursor");
            }
        }
    }



    protected override void WndProc(ref Message m) {
        base.WndProc(ref m);
        if (m.Msg == 0x000F)
            this.draw();
        else if (m.Msg == 0x114 || m.Msg == 0x115 || m.Msg == 0x20A)
            this.Refresh();
    }

    private void draw() {
        if (isAllowShowICO && Enabled && !ReadOnly) {
            Graphics g = this.CreateGraphics();
            SizeF size = g.MeasureString("×", this.Font);
            PointF point = new PointF(this.Width - size.Width - 2, ( this.Height - size.Height ) / 2 - 2);
            RectangleF rect = new RectangleF();
            if (this.TextLength != 0) {
                g.FillRectangle(new Pen(this.BackColor).Brush, new Rectangle((int)point.X, (int)point.Y, (int)size.Width, (int)size.Height));
                g.DrawString("×", this.Font, Brushes.Black, point);

                rect.Location = point;
                rect.Size = new SizeF(size.Width, size.Height);

            }
            this.setTag("rect", rect);
            this.setTag("point", point);
            this.setTag("size", size);
        }
    }


    private void NEditText_KeyPress(object sender, KeyPressEventArgs e) {

        if (inputType == INPUT_TYPE_INT) {
            // 2023-08-15 1
            // 如果输入的是-，判断是否允许输入
            if (( isAllowNegative && e.KeyChar == '-' ) && ( this.TextLength == 0 || ( this.SelectedText == this.Text && this.SelectionLength == this.TextLength ) || this.TextLength > 0 && this.SelectionStart == 0 )) {
                e.Handled = false;
            } else {
                e.Handled = !Char.IsNumber(e.KeyChar) && !Tools.ins(e.KeyChar.ToString(), noFilter);
            }
        } else if (inputType == INPUT_TYPE_DECIMAL) {
            // 2023-08-15 1
            // 如果输入的是-，判断是否允许输入
            // 是不是从第一位开始输入
            // 2025-01-21 1
            if (( this.TextLength == 0 || ( this.SelectedText == this.Text && this.SelectionLength == this.TextLength ) || this.TextLength > 0 && this.SelectionStart == 0 )) {
                e.Handled = !( isAllowNegative && e.KeyChar == '-' || Char.IsNumber(e.KeyChar) || Tools.ins(e.KeyChar.ToString(), noFilter) );
            } else {
                e.Handled = !Char.IsNumber(e.KeyChar) && !Tools.ins(e.KeyChar.ToString(), noFilter);
                if (e.Handled) {
                    if (e.KeyChar == '.') {
                        e.Handled = this.Text.Contains(".");
                        // 2025-01-15   1
                        if (this.Text.Contains(".")) {
                            if ('.' == this.Text[this.SelectionStart]) {
                                this.SelectionStart++;
                            }
                        }
                    }
                }
            }
        } else if (inputType == INPUT_TYPE_ENGLISH) {
            e.Handled = !Char.IsLower(e.KeyChar) && !Char.IsUpper(e.KeyChar) && !Tools.ins(e.KeyChar.ToString(), noFilter);
        } else if (inputType == INPUT_TYPE_ENGLISH_INT) {
            e.Handled = !Char.IsNumber(e.KeyChar) && !Char.IsLower(e.KeyChar) && !Char.IsUpper(e.KeyChar) && !Tools.ins(e.KeyChar.ToString(), noFilter);
        } else if (inputType == INPUT_TYPE_ENGLISH_DECIMAL) {
            e.Handled = !Char.IsNumber(e.KeyChar) && !Char.IsLower(e.KeyChar) && !Char.IsUpper(e.KeyChar) && !Tools.ins(e.KeyChar.ToString(), noFilter);
            if (e.Handled) {
                if (e.KeyChar == '.') {
                    e.Handled = this.Text.Contains(".");
                }
            }
        } else if (inputType == INPUT_TYPE_SFZ) {
            e.Handled = !Char.IsNumber(e.KeyChar) && !Tools.ins(e.KeyChar.ToString(), noFilter);
            if (e.Handled) {
                if (this.TextLength == 17) {
                    if (e.KeyChar == 'x' || e.KeyChar == 'X') {
                        e.Handled = this.Text.Contains("x") || this.Text.Contains("X");
                    }
                }
            }
        } else {
            if (Tools.ins(e.KeyChar.ToString(), keywords))
                e.Handled = true;
        }
    }
    // 2025-02-17   1
    public void targetTExtChanged() {
        this.OnTextChanged(new EventArgs());
    }

    public void onEnter() {
        if (null != this.onEnterListener) {
            onEnterListener(this, new EventArgs());
        }
    }

    public NEditText setOnEnter(EventHandler methodInvoker) {
        return setOnEnter(methodInvoker, Keys.Tab, Keys.Enter);
    }
    public NEditText setOnEnter(EventHandler methodInvoker, params Keys[] keys) {
        this.onEnterListener = methodInvoker;
        enterListenerBindKey.AddRange(keys);
        return this;
    }


    public NEditText setNext(params Control[] nextControls) {
        if (nextControls.Length > 0) {
            setNext(nextControls[0]);
            for (int i = 0; i < nextControls.Length; i++) {
                Control control = nextControls[i];
                if (i + 1 != nextControls.Length) {
                    if (control is NEditText)
                        ( (NEditText)control ).setNext(nextControls[i + 1]);
                    else if (control is NDateTimePicker)
                        ( (NDateTimePicker)control ).setNext(nextControls[i + 1]);
                    else if (control is NComboBox3)
                        ( (NComboBox3)control ).setNext(nextControls[i + 1]);
                }
            }
        }
        return this;
    }

    public NEditText setNext(Control nextControl) {
        return setNext(true, nextControl.Name);
    }
    public NEditText setNext(String nextControlName) {
        return setNext(true, nextControlName);
    }
    // 2023-09-25   1
    public NEditText setNext(bool isNull, Control nextControl) {
        return setNext(isNull, nextControl.Name);
    }
    public NEditText setNext(bool isNull, String nextControlName) {
        this.isNull = isNull;
        this.next = nextControlName;
        return this;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {

        if (enterListenerBindKey.Contains(keyData) && null != onEnterListener) {
            onEnterListener(this, new EventArgs());
            return true;
        }
        if (( keyData == Keys.Enter || keyData == Keys.Tab )) {
            Form form = this.FindForm();
            Control parent = this.Parent;
            if (null != form) {
                if (!String.IsNullOrEmpty(next)) {
                    Control[] controls = form.Controls.Find(next, true);
                    if (null != controls && 0 != controls.Length) {
                        // 2023-09-25   1
                        if (!isNull) {
                            if (null == Text || String.IsNullOrEmpty(Text)) {
                                return true;
                            }
                        }
                        if (controls[0].CanFocus) {
                            // 2023-04-12  1
                            if (controls[0] is NComboBox3)
                                ( (NComboBox3)controls[0] ).requestFocus();
                            else if (controls[0] is NEditText)
                                ( (NEditText)controls[0] ).requestFocus();
                            else if (controls[0] is NDateTimePicker)
                                ( (NDateTimePicker)controls[0] ).requestFocus();
                            else if (controls[0] is NDataGridView5)
                                ( (NDataGridView5)controls[0] ).getDataView().onMouseClick();
                            else if (controls[0] is NDataGridView3)
                                ( (NDataGridView3)controls[0] ).onMouseClick();
                            else {
                                // 2023-10-09   1
                                controls[0].Select();
                                controls[0].Focus();
                                if (controls[0] is TextBoxBase) {
                                    ( (TextBoxBase)controls[0] ).SelectAll();
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

    public NEditText setInputType(int inputType) {
        this.inputType = inputType;

        return this;
    }

    /// <summary>
    /// 设置数据格式0000.0000
    /// </summary>
    /// <param name="format">00 表示不足用0补位</param>
    /// <returns></returns>
    public NEditText setFormat(String format) {
        this.format = format;
        if (format.Contains("."))
            this.MaxLength = 65535;
        else {
            if (!String.IsNullOrEmpty(format))
                this.MaxLength = format.Length;
            else
                this.MaxLength = 65535;
        }
        return this;
    }


    private void NEditText_KeyDown(object sender, KeyEventArgs e) {

    }
    private string backText = "";
    private void NEditText_TextChanged(object sender, EventArgs e) {
        string str = this.Text.Trim();
        if (String.IsNullOrEmpty(str)) {
            if (null != bindRow && null != bindColumnName) {
                try {
                    bindRow[bindColumnName] = this.Text;
                } catch {
                    bindRow[bindColumnName] = DBNull.Value;
                }
            }
            return;
        }
        int lent = System.Text.ASCIIEncoding.Default.GetByteCount(str);
        if (maxType == SqlDbType.VarChar || maxType == SqlDbType.NVarChar) {
            lent = (int)Math.Ceiling(lent/2D);
        }
        int max = this.MaxLength;
        if (lent > max) {
            byte[] bb = System.Text.ASCIIEncoding.Default.GetBytes(str);
            String text = System.Text.ASCIIEncoding.Default.GetString(bb, 0, max);
            if ((text + "").EndsWith("?"))
                text = text.Substring(0, text.Length - 1);
            int start = this.SelectionStart;
            this.Text = text;
            this.SelectionStart = start;
            //  this.SelectionStart = max;
        }
        Boolean isReturn = false;
        if (NEditText.INPUT_TYPE_INT == inputType) {
            if (!isInt.Match(str).Success) {
                // 2023-08-15 1
                if (!"-".Equals(this.Text)) {
                    BeginInvoke(new MethodInvoker(() => {
                        this.Text = "";
                    }));
                    isReturn = true;
                }
            }
        } else if (NEditText.INPUT_TYPE_DECIMAL == inputType) {            
            if (!isInt.Match(str).Success && !isDecimal.Match(str).Success && !isDecimal2.Match(str).Success) {
                // 2023-08-15 1
                if (!"-".Equals(this.Text)) {
                    BeginInvoke(new MethodInvoker(() => {
                        this.Text = "";
                    }));
                    isReturn = true;
                }
            }
        } else if (NEditText.INPUT_TYPE_ENGLISH_DECIMAL == inputType) {
            if (!isEnglishAndDecimal.Match(str).Success) {
                BeginInvoke(new MethodInvoker(() => {
                    this.Text = "";
                }));
                isReturn = true;
            }
        } else if (NEditText.INPUT_TYPE_ENGLISH_INT == inputType) {
            if (!isEnglishAndInt.Match(str).Success) {
                BeginInvoke(new MethodInvoker(() => {
                    this.Text = "";
                }));
                isReturn = true;
            }
        } else if (NEditText.INPUT_TYPE_SFZ == inputType && 18 == this.TextLength) {
            if (!isInt.Match(str).Success && !isSFZ.Match(str).Success) {
                BeginInvoke(new MethodInvoker(() => {
                    this.Text = "";
                }));
                isReturn = true;
            }
        }

        if (String.IsNullOrEmpty(this.Text) || isReturn)
            return;
        if (!String.IsNullOrEmpty(format) && format.Contains(".")) {
            bool isOwe = str.StartsWith("-");


            if (!Tools.isNullOrEmpty(str)) {

                str = replaceNoDecimal.Replace(str, "");


                StringBuilder builder = new StringBuilder();
                int zeroQLength = format.Substring(0, format.IndexOf(".")).Length;
                int zeroHLength = format.Substring(format.IndexOf(".") + 1).Length;
                int _dianIndex = str.IndexOf(".");
                int currLength = this.SelectionStart;
                int inputQLength = 0;
                int inputHLength = 0;
                // 如果是皆为补0的则显示0
                if (format.Contains(".0")) {
                    builder.Append(str);
                    if (-1 != _dianIndex) {
                        inputQLength = str.Substring(0, _dianIndex).Length;
                        inputHLength = str.Substring(_dianIndex + 1).Length;
                    } else {
                        inputQLength = str.Length;
                        _dianIndex = zeroQLength + 1;
                    }

                    // 说明我出入的是.前面的则验证长度
                    // 多了截取
                    if (inputQLength <= zeroQLength && str.Contains(".") && inputHLength < zeroHLength) {
                        for (int i = inputHLength; i < zeroHLength; i++) {
                            builder.Append("0");
                        }
                    }

                    // 说明是输入完整后，一个一个删除，现在把.删除了。这个时候需要把.保留。然后删除.前面一位
                    // 2023-07-21   1.
                    if (-1 == str.IndexOf(".") && -1 != backText.IndexOf(".") && this.Text.Equals(backText.Replace(".", ""))) {
                        builder.Length = 0;
                        String[] backTexts = backText.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                        if (backTexts.Length > 1) {
                            builder.Append(backTexts[0].Substring(0, backTexts[0].Length - 1) + "." + backTexts[1]);
                            currLength -= 1;
                            // 说明只有.xx了
                            if (0 == builder.ToString().IndexOf(".")) {
                                String msg = builder.ToString();
                                if (Convert.ToDecimal("0" + msg) == 0M) {
                                    builder.Length = 0;
                                    currLength = 0;
                                }
                            }
                        }
                    } else {
                        if (inputQLength > zeroQLength) {
                            builder.Remove(zeroQLength, inputQLength - zeroQLength);
                            str = builder.ToString();
                            if (-1 != str.IndexOf(".")) {
                                inputQLength = str.Substring(0, _dianIndex).Length;
                                inputHLength = str.Substring(_dianIndex + 1).Length;
                            } else {
                                inputQLength = str.Length;
                            }
                        }
                        if (inputQLength == zeroQLength && str.Length == zeroQLength) {
                            builder.Append(".");
                            for (int i = 0; i < zeroHLength; i++) {
                                builder.Append("0");
                            }
                            currLength += 1;
                        } else if (inputHLength > zeroHLength) {
                            builder.Length -= inputHLength - zeroHLength;
                        }
                        //// 处理0.00，删除第一位0后，格式有误，固定强制添加一个0在前面
                        //if (0 == inputQLength && inputHLength > 0 && 0 == _dianIndex)
                        //{
                        //    builder.Insert(0, "0");
                        //    currLength += 1;
                        //}
                    }
                }
                if (isOwe && !builder.ToString().StartsWith("-"))
                    builder.Insert(0, "-");
                this.TextChanged -= NEditText_TextChanged;
                this.Text = builder.ToString();
                if (null != bindRow && null != bindColumnName) {
                    //try {
                    //    bindRow[bindColumnName] = this.Text;
                    //} catch {
                    //    bindRow[bindColumnName] = DBNull.Value;
                    //}
                    Type valType = null;
                    bool valIsDecimal = false;
                    if (null != bindRow.Table) {
                        valType = bindRow.Table.Columns[bindColumnName].DataType;
                        if (valType == typeof(Decimal) || valType == typeof(Double) || valType == typeof(float)
                            || valType == typeof(long) || valType == typeof(int) || valType == typeof(short) || valType == typeof(Int64) || valType == typeof(Int32)) {
                            valIsDecimal = true;
                        }
                    }
                    if ("-".Equals(this.Text.Trim()) && valIsDecimal) {
                        bindRow[bindColumnName] = "-0";
                    } else {
                        bindRow[bindColumnName] = this.Text;
                    }
                }
                if (this.Focused) {
                    this.SelectionStart = currLength;
                }
                backText = Text;
                this.TextChanged += NEditText_TextChanged;
            }
        } else {
            Type valType = null;
            bool valIsDecimal = false;
            if (null != bindRow && null != bindColumnName) {
                if (null != bindRow.Table) {
                    valType = bindRow.Table.Columns[bindColumnName].DataType;
                    if (valType == typeof(Decimal) || valType == typeof(Double) || valType == typeof(float)
                        || valType == typeof(long) || valType == typeof(int) || valType == typeof(short) || valType == typeof(Int64) || valType == typeof(Int32)) {
                        valIsDecimal = true;
                    }
                }
                if ("-".Equals(this.Text.Trim()) && valIsDecimal) {
                    bindRow[bindColumnName] = "-0";
                } else {
                    bindRow[bindColumnName] = this.Text;
                }
            }
            //if (null != bindRow && null != bindColumnName)
            //    bindRow[bindColumnName] = this.Text;

        }

    }

    private void NEditText_MouseUp(object sender, MouseEventArgs e) {
        if (isAutoSelectAll) {
            lock (this) {
                if (this.isSelectAll) {
                    this.isSelectAll = false;
                    this.SelectAll();
                }
            }
        }
    }

    private void NEditText_Enter(object sender, EventArgs e) {
        this.isSelectAll = true;
    }



    public bool isInputInt() {
        return Tools.isInt(this.Text);
    }
    public bool isInputDouble() {
        return Tools.isDouble(this.Text);
    }


    public NEditText setTag(String tag, Object obj) {
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

    public void requestFocus() {
        this.Select();
        this.Focus();
        if (isAutoSelectAll)
            this.SelectAll();
        else {
            Application.DoEvents();
            this.Select(this.TextLength,0);            
        }
    }
}

