using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DailyStockDataCollector
{
    public partial class Form1 : Form
    {
        int screenNum = 1000;
        string code = "";
        static AutoResetEvent objAuto = new AutoResetEvent(false); // 이거 추가해야지 ReceiveTR 끝나고 code 업데이트 해서 안밀린다. 굳굳!!
        public Form1()
        {
            InitializeComponent();

            axKHOpenAPI1.CommConnect(); //start kiwoom api
            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrData; //TrData가 왔을 시

            투자자별매매동향Button.Click += Button_Click;
            프로그램매매동향Button.Click += Button_Click;

            dateTextBox.Text = DateTime.Now.ToString("yyyyMMdd");
            여기서부터다시시작textBox.Text = "";
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (sender == 투자자별매매동향Button)
            {
                listBox.Items.Add("투자자별매매동향 저장 클릭");
                Thread rqThread = new Thread(delegate ()
                {
                    string[] codeList = getCodeList("kosdaq", 여기서부터다시시작textBox.Text);
                    for (int i=0; i< codeList.Count();i++)
                    {
                        code = codeList[i];
                        Console.WriteLine(axKHOpenAPI1.GetMasterCodeName(codeList[i]));
                        axKHOpenAPI1.SetInputValue("일자", dateTextBox.Text);
                        axKHOpenAPI1.SetInputValue("종목코드", code);
                        axKHOpenAPI1.SetInputValue("금액수량구분", "2");
                        axKHOpenAPI1.SetInputValue("매매구분", "0");
                        axKHOpenAPI1.SetInputValue("단위구분", "1");
                        //sRQName에 종목코드를 추가해서 Receive event에서도 code 받을수 있게 하기
                        int result = axKHOpenAPI1.CommRqData("일별주가요청", "opt10059", 0, GetScreenNum());
                        Thread.Sleep(3750);
                        objAuto.WaitOne();
                    }
                });
                rqThread.Start();
            }
            else if (sender == 프로그램매매동향Button)
            {
                listBox.Items.Add("프로그램매매동향 저장 클릭");
                Thread rqThread = new Thread(delegate ()
                {
                    string[] codeList = getCodeList("kosdaq", 여기서부터다시시작textBox.Text);
                    for (int i = 0; i < codeList.Count(); i++)
                    {
                        string code = codeList[i];
                        axKHOpenAPI1.SetInputValue("시간일자구분", "2");
                        axKHOpenAPI1.SetInputValue("금액수량구분", "2");
                        axKHOpenAPI1.SetInputValue("종목코드", code);
                        axKHOpenAPI1.SetInputValue("날짜", dateTextBox.Text);
                        int result = axKHOpenAPI1.CommRqData("프로그램일별요청-" + code, "opt90013", 0, GetScreenNum());
                        Thread.Sleep(3750);
                        objAuto.WaitOne();
                    }
                });
                rqThread.Start();
            }

        }
        private void API_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            Console.WriteLine("sErrorCode" + e.sErrorCode);
            Console.WriteLine("sMessage" + e.sMessage);
            Console.WriteLine("sPrevNext" + e.sPrevNext);
            Console.WriteLine("sRecordName" + e.sRecordName);
            Console.WriteLine("sRQName" + e.sRQName);
            Console.WriteLine("sScrNo" + e.sScrNo);
            Console.WriteLine("sTrCode" + e.sTrCode);


            if (e.sRQName.Equals("일별주가요청")) 
            {
                listBox.Items.Add("Worked On: 일별주가요청 " + code);
                for (int i = 0; i < axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "일자"));
                    int retail = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "개인투자자"));
                    int foreigner = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "외국인투자자"));
                    int insitiute = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "기관계"));
                    if (retail > 0 || foreigner > 0 || insitiute > 0)
                    {
                        //Console.WriteLine("recorded_time: " + recorded_time);
                        //Console.WriteLine("Code: " + code);
                        //Console.WriteLine("date: " + date);
                        //Console.WriteLine("개인순매수: " + retail);
                        //Console.WriteLine("외인순매수: " + foreigner);
                        //Console.WriteLine("기관순매수: " + insitiute);
                        using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                        {
                            //이미 primary key가 있으면 ignore하고 아니면 insert하셈
                            cnn.Execute("insert or ignore into DailyCollectedData (recorded_time, code, date, retail, foreigner, insitiute) values (@recorded_time, @code, @date, @retail, @foreigner, @insitiute)", new { recorded_time, code, date, retail, foreigner, insitiute });
                        }
                    }
                    objAuto.Set();
                }
            }
            else if (e.sRQName.Contains("프로그램일별요청")) 
                // 여기서는 프로그램 수량 넣음
            {
                string code = e.sRQName.Split('-')[1];
                listBox.Items.Add("Worked On: 프로그램일별요청 " + code);
                for (int i = 0; i < axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "일자"));
                    int program = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "프로그램순매수수량"));
                    using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                    {
                        //이미 primary key가 있으면 ignore하고 아니면 insert하셈
                        cnn.Execute("update DailyCollectedData set  program = @program where (code = @code and date = @date)", new { program, code, date});
                    }
                }
                objAuto.Set();
            }
        }


        private int dataCleansing(string s)
        {
            if (s.Length > 0)
            {
                s = s.Trim();
                //if (s.Substring(0, 1).Equals("-"))
                //{
                //    s = s.Substring(1, s.Length - 1);
                //}
                return Int32.Parse(s);
            }
            else
            {
                return 0;
            }
        }
        
        private string[] getCodeList(string market="kosdaq", string interruptedCode="")
        {
            string mkt_parameter = "";
            if (market.Equals("kosdaq")) { mkt_parameter = "10"; }
            else if (market.Equals("kospi")) { mkt_parameter = "0"; }
            else { mkt_parameter = null; } // 모든 마켓
            string[] codeArray = axKHOpenAPI1.GetCodeListByMarket(mkt_parameter).Split(';'); // 마켓에 해당하는 종목 가져오기
            codeArray = codeArray.OrderBy(x => x).ToArray(); // Ascending
            codeArray = codeArray.Skip(1).ToArray(); // remove first element, which is blank
            if (interruptedCode.Equals("") == false)
            {
                int index = Array.FindIndex(codeArray, row => row == interruptedCode);
                string[] segment = codeArray.Skip(index).Take(codeArray.Count()).ToArray(); // 와 이거 하는데 정말 오래걸랬다. python은 그냥 array[index:] 이러면 끝인데...
                return segment;
            }
            return codeArray;
        }
        private string GetScreenNum()
        {
            if (screenNum >= 9999)
                screenNum = 1000;

            screenNum++;
            return screenNum.ToString();
        }
    }
}
