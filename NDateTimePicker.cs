#region
////////////////////////////////////////////////////////////////////
//                          _ooOoo_                               //
//                         o8888888o                              //
//                         88" . "88                              //
//                         (| ^_^ |)                              //
//                         O\  =  /O                              //
//                      ____/`---'\____                           //
//                    .'  \\|     |//  `.                         //
//                   /  \\|||  :  |||//  \                        //
//                  /  _||||| -:- |||||-  \                       //
//                  |   | \\\  -  /// |   |                       //
//                  | \_|  ''\---/''  |   |                       //
//                  \  .-\__  `-`  ___/-. /                       //
//                ___`. .'  /--.--\  `. . ___                     //
//              ."" '<  `.___\_<|>_/___.'  >'"".                  //
//            | | :  `- \`.;`\ _ /`;.`/ - ` : | |                 //
//            \  \ `-.   \_ __\ /__ _/   .-` /  /                 //
//      ========`-.____`-.___\_____/___.-`____.-'========         //
//                           `=---='                              //
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^        //
//            佛祖保佑       永不宕机     永无BUG                 //
////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// 2023-10-10  1.选择框标题
///             2.增加文件及文件夹选择
/// 2023-10-13  1.修复没清空上一次选择结果
/// 2023-10-17  1.增加快速设置选择类型
/// </summary>
public class NImportFile : Panel {
    private Button browser;
    private TextBox textBox;

    private int buttonWidth = 33;
    // 接收文件类型 "*.txt","*.png"
    private String[] accepts = null;
    // 选择的文件庵路径
    private String[] selectFilePath = null;
    // 是否多选
    private bool isMulti = false;

    public FileSelectedListener fileChooseListener = (String[] selectPath) => {

    };

    public String title = "请选择文件";
    public bool isChooserFile = true;


    public delegate void FileSelectedListener(String[] filePath);

    public NImportFile() {

        this.Width = 120;
        this.Height = 21;
        this.Padding = new Padding(1, 1, 1, 1);
        this.BackColor = Color.Transparent;

        textBox = new TextBox();
        browser = new Button();


        textBox.ReadOnly = true;
        textBox.Location = new Point(0, 0);
        textBox.TextAlign = HorizontalAlignment.Left;
        textBox.Size = new Size(this.Width - buttonWidth, this.Height);

        browser.Size = new Size(buttonWidth, this.Height);
        browser.AutoSize = false;
        browser.FlatStyle = FlatStyle.Flat;
        browser.Text = "∙∙∙";
        browser.TextAlign = ContentAlignment.MiddleCenter;
        browser.BringToFront();

        //textBox.Location = new Point

        this.Controls.Add(textBox);
        this.Controls.Add(browser);
        this.SizeChanged += NImportFile_SizeChanged;
        textBox.Click += TextBox_Click;
        browser.Click += Browser_Click;
    }

    private void Browser_Click(object sender, EventArgs e) {
        CommonDialog ofd2 = null;
        if (isChooserFile) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = title;
            ofd.Multiselect = isMulti;
            if (null == accepts || 0 == accepts.Length)
                ofd.Filter = "所有文件|*.*;*.*";
            else {
                List<String> list = new List<string>(accepts);
                for (int i = 0; i < accepts.Length; i++) {
                    list.Insert(list.Count - accepts.Length + i, accepts[i]);
                }
                ofd.Filter = "全部文件|" + String.Join(";", accepts) + "|" + String.Join("|", list.ToArray());
            }
            ofd2 = ofd;
        } else {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            ofd.RootFolder = Environment.SpecialFolder.Desktop;
            ofd.Description = title;
            ofd.ShowNewFolderButton = true;
            ofd2 = ofd;
        }
        selectFilePath = null;
        DialogResult dr = ofd2.ShowDialog();
        if (dr == DialogResult.Yes || dr == DialogResult.OK) {
            if (ofd2 is OpenFileDialog) {
                textBox.Text = ( (OpenFileDialog)ofd2 ).FileNames[0];
                selectFilePath = ( (OpenFileDialog)ofd2 ).FileNames;
            } else {
                textBox.Text = ( (FolderBrowserDialog)ofd2 ).SelectedPath;
                selectFilePath = new string[] { textBox.Text };
            }
            if (null != fileChooseListener) {
                fileChooseListener(selectFilePath);
            }
        }
    }

    private void TextBox_Click(object sender, EventArgs e) {
        textBox.SelectAll();
    }

    private void NImportFile_SizeChanged(object sender, EventArgs e) {
        this.textBox.Height = browser.Height = this.Height;
        this.textBox.Width = this.Width - buttonWidth;
        this.browser.Location = new Point(this.textBox.Width, 0);
    }

    public NImportFile setMulti(bool isMulti) {
        this.isMulti = isMulti;
        return this;
    }

    /// <summary>
    /// 添加文件过滤类型
    /// </summary>
    /// <param name="fileType">*.txt,*.png,*.xls 等</param>
    /// <returns></returns>
    public NImportFile addFileter(params String[] fileType) {
        this.accepts = fileType;
        return this;
    }


    public void setFilterExcel() {
        this.accepts = new String[] { "*.xls", "*.xlsx" };
    }

    public void setFilterImage() {
        this.accepts = new String[] { "*.png", "*.jpg", "*.tif", "*.jpeg", "*.jpe" };
    }

    public void setFilterText() {
        this.accepts = new String[] { "*.txt" };
    }

    public void openFileChooser() {
        Browser_Click(browser, new EventArgs());
    }

    public string[] SelectFilePath {
        get {
            return selectFilePath;
        }
    }
}